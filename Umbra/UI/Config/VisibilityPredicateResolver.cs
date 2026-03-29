using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves <see cref="UmbraHideIfAttribute{T}"/> declarations into per-frame visibility
/// predicates. Keeps HideIf logic isolated from both tree traversal and control rendering.
/// </summary>
internal static class VisibilityPredicateResolver
{
    private static readonly ConcurrentDictionary<HideIfAccessorCacheKey, HideIfAccessorBinding> s_accessorCache = new();
    private static readonly ConcurrentDictionary<HideIfAccessorCacheKey, byte> s_invalidAccessorWarnings = new();

    /// <summary>
    /// Cache key for one owner-type/member-name HideIf accessor shape.
    /// </summary>
    /// <param name="OwnerType">The runtime owner type that declares the referenced member.</param>
    /// <param name="MemberName">The referenced property or field name.</param>
    private readonly record struct HideIfAccessorCacheKey(Type OwnerType, string MemberName);

    /// <summary>
    /// Cached accessor metadata for a resolved HideIf member.
    /// </summary>
    /// <param name="isValid">Whether the referenced member was found successfully.</param>
    /// <param name="getValue">The cached accessor that reads the current member value from an owner instance.</param>
    private sealed class HideIfAccessorBinding(bool isValid, Func<object, object?> getValue)
    {
        internal bool IsValid { get; } = isValid;
        internal Func<object, object?> GetValue { get; } = getValue;
    }

    /// <summary>
    /// Resolves a cached <see cref="IHideIfAttribute"/> into a <see cref="Func{Boolean}"/> that
    /// returns <see langword="true"/> when the parameter should be visible (i.e. the hide
    /// condition is NOT met).
    /// </summary>
    /// <remarks>
    /// The raw property or field accessor is compiled once per owner-type/member-name pair and
    /// cached in <see cref="s_accessorCache"/> so repeated config UI builds do not pay expression
    /// compilation or <see cref="PropertyInfo.GetValue(object)"/> reflection costs. The cached
    /// accessor is then bound to the closed-over <paramref name="owner"/> instance.
    /// <para>
    /// Callers that have already determined <paramref name="hideIf"/> is
    /// <see langword="null"/> should short-circuit with <c>static () =&gt; true</c> and skip
    /// this call entirely to avoid the function-call overhead for the common no-condition case.
    /// </para>
    /// <para>
    /// When an explicit comparison value is present, equality is evaluated with
    /// <see cref="object.Equals(object?, object?)"/> against the boxed runtime value returned by
    /// the accessor. This keeps the comparison path simple and works for the primitive, string,
    /// enum, and nullable values typically used by config-backed
    /// <see cref="Umbra.Config.Attributes.UmbraHideIfAttribute{T}"/> (<c>[UmbraHideIf]</c>) conditions.
    /// </para>
    /// <para>
    /// Invalid <see cref="Umbra.Config.Attributes.UmbraHideIfAttribute{T}"/> (<c>[UmbraHideIf]</c>)
    /// bindings are warned only once per owner-type/member-name pair.
    /// This avoids repeating the same warning every time a panel or drawer is rebuilt while still
    /// surfacing the configuration issue to the developer.
    /// </para>
    /// </remarks>
    /// <param name="hideIf">
    /// The pre-cached hide-condition data from <see cref="ParameterMetadata.HideIf"/>, or
    /// <see langword="null"/> when the parameter carries no hide condition.
    /// </param>
    /// <param name="owner">The configuration object instance that owns the parameter.</param>
    /// <returns>
    /// A predicate that returns <see langword="true"/> when the control should be rendered;
    /// always returns <see langword="true"/> when <paramref name="hideIf"/> is <see langword="null"/>.
    /// </returns>
    internal static Func<bool> Build(IHideIfAttribute? hideIf, object owner)
    {
        if (hideIf is null) return static () => true;

        var memberName = hideIf.MemberName;
        var hasValue = hideIf.HasValue;
        var compareValue = hideIf.BoxedValue;
        var ownerType = owner.GetType();

        var accessor = s_accessorCache.GetOrAdd(
            new HideIfAccessorCacheKey(ownerType, memberName),
            static key => CreateAccessorBinding(key.OwnerType, key.MemberName));

        if (!accessor.IsValid)
        {
            WarnInvalidAccessorOnce(new HideIfAccessorCacheKey(ownerType, memberName));
            return static () => true;
        }

        var getValue = accessor.GetValue;

        // No explicit value: treat member as bool — visible while NOT true.
        if (!hasValue) return () => getValue(owner) is not true;

        return () => !Equals(getValue(owner), compareValue);
    }

    /// <summary>
    /// Creates and caches the compiled accessor binding for one owner-type/member-name pair.
    /// </summary>
    /// <param name="ownerType">The runtime owner type declaring the referenced member.</param>
    /// <param name="memberName">The property or field name referenced by the HideIf attribute.</param>
    /// <returns>A cached accessor binding describing how to read the current member value.</returns>
    private static HideIfAccessorBinding CreateAccessorBinding(Type ownerType, string memberName)
    {
        var targetProp = ownerType.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var targetField = ownerType.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (targetProp is null && targetField is null)
            return new HideIfAccessorBinding(false, static _ => null);

        var rawType = (targetProp?.PropertyType ?? targetField!.FieldType)!;
        var getRaw = BuildRawAccessor(ownerType, targetProp, targetField);
        if (rawType.IsGenericType && rawType.GetGenericTypeDefinition() == typeof(Parameter<>))
            return new HideIfAccessorBinding(true, owner => (getRaw(owner) as IParameter)?.GetValue());

        return new HideIfAccessorBinding(true, getRaw);
    }

    /// <summary>
    /// Logs a warning once for an invalid HideIf accessor binding.
    /// </summary>
    /// <param name="key">The cached owner-type/member-name binding that failed resolution.</param>
    private static void WarnInvalidAccessorOnce(HideIfAccessorCacheKey key)
    {
        if (!s_invalidAccessorWarnings.TryAdd(key, 0))
            return;

        Logger.Warning(
            $"ConfigDrawer: HideIf member '{key.MemberName}' not found on {key.OwnerType.Name}; condition ignored.");
    }

    /// <summary>
    /// Builds the cached raw member accessor for a HideIf property or field.
    /// </summary>
    /// <remarks>
    /// Expression compilation is performed once per member and cached by
    /// <see cref="CreateAccessorBinding(Type, string)"/>. If expression compilation fails, the
    /// resolver falls back to reflection-based value access so visibility checks still work.
    /// </remarks>
    /// <param name="ownerType">The runtime owner type declaring the referenced member.</param>
    /// <param name="targetProp">The target property when the member is a property; otherwise <see langword="null"/>.</param>
    /// <param name="targetField">The target field when the member is a field; otherwise <see langword="null"/>.</param>
    /// <returns>A delegate that reads the boxed current member value from an owner instance.</returns>
    private static Func<object, object?> BuildRawAccessor(Type ownerType, PropertyInfo? targetProp, FieldInfo? targetField)
    {
        try
        {
            var ownerParam = Expression.Parameter(typeof(object), "owner");
            var typedOwner = Expression.Convert(ownerParam, ownerType);
            Expression rawAccess = targetProp is not null
                ? Expression.Property(typedOwner, targetProp)
                : Expression.Field(typedOwner, targetField!);

            return Expression.Lambda<Func<object, object?>>(
                Expression.Convert(rawAccess, typeof(object)),
                ownerParam).Compile();
        }
        catch
        {
            return targetProp is not null
                ? owner => targetProp.GetValue(owner)
                : owner => targetField!.GetValue(owner);
        }
    }

}
