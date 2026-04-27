using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Umbra.Config;
using Umbra.Logging;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves conditional attribute member references into cached per-frame predicates.
/// </summary>
/// <remarks>
/// This helper centralizes member-access caching for condition-driven UI state such as hide and disable rules so those features share one accessor-resolution pipeline.
/// </remarks>
internal static class ConditionalMemberPredicateResolver
{
    private static readonly ConcurrentDictionary<ConditionalAccessorCacheKey, ConditionalAccessorBinding> _accessorCache = new();
    private static readonly ConcurrentDictionary<ConditionalWarningCacheKey, byte> _invalidAccessorWarnings = new();

    /// <summary>
    /// Cache key for one owner-type/member-name conditional accessor shape.
    /// </summary>
    private readonly record struct ConditionalAccessorCacheKey(Type OwnerType, string MemberName);

    /// <summary>
    /// Cache key for one logged invalid conditional member warning.
    /// </summary>
    private readonly record struct ConditionalWarningCacheKey(Type OwnerType, string MemberName, string ConditionName);

    /// <summary>
    /// Cached accessor metadata for a resolved conditional member.
    /// </summary>
    private sealed class ConditionalAccessorBinding(bool isValid, Func<object, object?> getValue)
    {
        internal bool IsValid { get; } = isValid;

        internal Func<object, object?> GetValue { get; } = getValue;
    }

    /// <summary>
    /// Builds a predicate that returns <see langword="true"/> when the referenced member currently matches the declared condition.
    /// </summary>
    /// <param name="memberName">The referenced property or field name.</param>
    /// <param name="hasValue"><see langword="true"/> when the condition uses explicit value comparison; otherwise, Boolean semantics are used.</param>
    /// <param name="compareValue">The boxed comparison value for explicit-value conditions.</param>
    /// <param name="owner">The configuration object that owns the annotated member.</param>
    /// <param name="conditionName">The logical condition name used for diagnostics.</param>
    /// <returns>A predicate that returns <see langword="true"/> when the condition currently matches.</returns>
    internal static Func<bool> BuildIsMatchPredicate(
        string memberName,
        bool hasValue,
        object? compareValue,
        object owner,
        string conditionName)
    {
        var ownerType = owner.GetType();
        var accessor = _accessorCache.GetOrAdd(
            new ConditionalAccessorCacheKey(ownerType, memberName),
            static key => CreateAccessorBinding(key.OwnerType, key.MemberName));

        if (!accessor.IsValid)
        {
            WarnInvalidAccessorOnce(new ConditionalWarningCacheKey(ownerType, memberName, conditionName));
            return static () => false;
        }

        var getValue = accessor.GetValue;
        return !hasValue ? (() => getValue(owner) is true) : (() => Equals(getValue(owner), compareValue));
    }

    /// <summary>
    /// Creates and caches the compiled accessor binding for one owner-type/member-name pair.
    /// </summary>
    private static ConditionalAccessorBinding CreateAccessorBinding(Type ownerType, string memberName)
    {
        var targetProp = ownerType.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var targetField = ownerType.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (targetProp is null && targetField is null)
            return new ConditionalAccessorBinding(false, static _ => null);

        var rawType = (targetProp?.PropertyType ?? targetField!.FieldType)!;
        var getRaw = BuildRawAccessor(ownerType, targetProp, targetField);
        return rawType.IsGenericType && rawType.GetGenericTypeDefinition() == typeof(Parameter<>)
            ? new ConditionalAccessorBinding(true, owner => (getRaw(owner) as IParameter)?.GetValue())
            : new ConditionalAccessorBinding(true, getRaw);
    }

    /// <summary>
    /// Logs a warning once for an invalid conditional accessor binding.
    /// </summary>
    private static void WarnInvalidAccessorOnce(ConditionalWarningCacheKey key)
    {
        if (!_invalidAccessorWarnings.TryAdd(key, 0))
            return;

        Logger.Warning(
            $"ConfigDrawer: {key.ConditionName} member '{key.MemberName}' not found on {key.OwnerType.Name}; condition ignored.");
    }

    /// <summary>
    /// Builds the cached raw member accessor for a property or field referenced by a UI condition.
    /// </summary>
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
