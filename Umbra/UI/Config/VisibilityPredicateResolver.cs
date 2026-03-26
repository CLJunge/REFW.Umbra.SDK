using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves <see cref="HideIfAttribute{T}"/> declarations into per-frame visibility
/// predicates. Keeps HideIf logic isolated from both tree traversal and control rendering.
/// </summary>
internal static class VisibilityPredicateResolver
{
    private static readonly ConcurrentDictionary<HideIfAccessorCacheKey, HideIfAccessorBinding> s_accessorCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object?, object?, bool>> s_typedEqualsCache = new();
    private static readonly MethodInfo s_createTypedEqualsCoreMethod = typeof(VisibilityPredicateResolver)
        .GetMethod(nameof(CreateTypedEqualsCore), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    /// Cache key for one owner-type/member-name HideIf accessor shape.
    /// </summary>
    /// <param name="OwnerType">The runtime owner type that declares the referenced member.</param>
    /// <param name="MemberName">The referenced property or field name.</param>
    private readonly record struct HideIfAccessorCacheKey(Type OwnerType, string MemberName);

    /// <summary>
    /// Cached accessor metadata for a resolved HideIf member.
    /// </summary>
    /// <param name="IsValid">Whether the referenced member was found successfully.</param>
    /// <param name="ValueType">The effective value type compared by the visibility predicate.</param>
    /// <param name="GetValue">The cached accessor that reads the current member value from an owner instance.</param>
    private sealed class HideIfAccessorBinding(bool isValid, Type valueType, Func<object, object?> getValue)
    {
        internal bool IsValid { get; } = isValid;
        internal Type ValueType { get; } = valueType;
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
    /// When an explicit comparison value is present, a typed
    /// <see cref="EqualityComparer{T}.Default"/> comparer delegate is compiled once per concrete
    /// value type and cached in <see cref="s_typedEqualsCache"/> so that per-frame equality checks
    /// use <see cref="IEquatable{T}"/> rather than virtual <see cref="object.Equals(object)"/>
    /// dispatch without recompiling a new comparer for each individual hidden node.
    /// If <see cref="IParameter.GetValue"/> returns <see langword="null"/> at runtime (e.g. when
    /// a <c>Parameter&lt;T&gt;</c> value has been cleared via <c>SetWithoutNotify</c>), the
    /// compiled comparer is <em>not</em> invoked; the predicate instead treats
    /// <see langword="null"/> as equal to <paramref name="hideIf"/>'s comparison value only when
    /// that comparison value is itself <see langword="null"/>, preventing an
    /// <see cref="InvalidCastException"/> from unboxing <see langword="null"/> to a value type.
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
            Logger.Warning($"ConfigDrawer: HideIf member '{memberName}' not found on {ownerType.Name}; condition ignored.");
            return static () => true;
        }

        var getValue = accessor.GetValue;

        // No explicit value: treat member as bool — visible while NOT true.
        if (!hasValue) return () => getValue(owner) is not true;

        if (compareValue is null)
            return () => getValue(owner) is not null;

        if (!CanUseTypedEquals(accessor.ValueType, compareValue))
            return () => !Equals(getValue(owner), compareValue);

        var typedEquals = s_typedEqualsCache.GetOrAdd(accessor.ValueType, CreateTypedEquals);

        // Guard against null: getValue() may return null when Parameter<T>.Value has been
        // cleared (e.g. via SetWithoutNotify). Unboxing null to a value type inside the
        // compiled comparer would throw InvalidCastException, breaking the per-frame UI loop.
        // Treat null as equal to compareValue only when compareValue is also null.
        return () =>
        {
            var val = getValue(owner);
            if (val is null) return compareValue is not null;
            return !typedEquals(val, compareValue);
        };
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
            return new HideIfAccessorBinding(false, typeof(object), static _ => null);

        var rawType = (targetProp?.PropertyType ?? targetField!.FieldType)!;
        var getRaw = BuildRawAccessor(ownerType, targetProp, targetField);
        if (rawType.IsGenericType && rawType.GetGenericTypeDefinition() == typeof(Parameter<>))
        {
            var valueType = rawType.GetGenericArguments()[0];
            return new HideIfAccessorBinding(true, valueType, owner => (getRaw(owner) as IParameter)?.GetValue());
        }

        return new HideIfAccessorBinding(true, rawType, getRaw);
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

    /// <summary>
    /// Determines whether <paramref name="compareValue"/> can use a cached typed comparer for
    /// <paramref name="valueType"/>.
    /// </summary>
    /// <param name="valueType">The effective value type read by the HideIf accessor.</param>
    /// <param name="compareValue">The boxed comparison value declared by the HideIf attribute.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="compareValue"/> is compatible with
    /// <paramref name="valueType"/> and a typed cached comparer can be used; otherwise
    /// <see langword="false"/>.
    /// </returns>
    private static bool CanUseTypedEquals(Type valueType, object compareValue)
    {
        if (valueType.IsInstanceOfType(compareValue))
            return true;

        var underlyingNullableType = Nullable.GetUnderlyingType(valueType);
        return underlyingNullableType is not null && underlyingNullableType.IsInstanceOfType(compareValue);
    }

    /// <summary>
    /// Creates a cached typed comparer that compares two boxed runtime values using
    /// <see cref="EqualityComparer{T}.Default"/> for the concrete <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    /// The cached delegate unboxes both incoming <c>object?</c> values to <c>T</c> and calls
    /// <see cref="EqualityComparer{T}.Equals(T, T)"/>, which respects
    /// <see cref="IEquatable{T}"/> implementations and avoids virtual
    /// <see cref="object.Equals(object)"/> dispatch on each frame.
    /// <para>
    /// The delegate itself is created once per <paramref name="valueType"/> by closing the
    /// generic <see cref="CreateTypedEqualsCore{T}"/> helper and caching the resulting delegate
    /// in <see cref="s_typedEqualsCache"/>.
    /// </para>
    /// </remarks>
    /// <param name="valueType">
    /// The concrete <c>T</c> used to parameterise the comparer; derived from the member's
    /// declared type or from the inner generic argument of <c>Parameter&lt;T&gt;</c>.
    /// </param>
    /// <returns>
    /// A cached comparer delegate that returns <see langword="true"/> when the two runtime values
    /// are equal according to <see cref="EqualityComparer{T}.Default"/>.
    /// </returns>
    private static Func<object?, object?, bool> CreateTypedEquals(Type valueType)
    {
        try
        {
            return (Func<object?, object?, bool>)s_createTypedEqualsCoreMethod
                .MakeGenericMethod(valueType)
                .Invoke(null, null)!;
        }
        catch
        {
            return static (left, right) => Equals(left, right);
        }
    }

    /// <summary>
    /// Creates the closed generic comparer implementation for one concrete HideIf value type.
    /// </summary>
    /// <typeparam name="T">The concrete value type used by the HideIf member.</typeparam>
    /// <returns>
    /// A delegate that compares two boxed values using <see cref="EqualityComparer{T}.Default"/>.
    /// </returns>
    private static Func<object?, object?, bool> CreateTypedEqualsCore<T>()
        => static (left, right) => EqualityComparer<T>.Default.Equals((T)left!, (T)right!);
}
