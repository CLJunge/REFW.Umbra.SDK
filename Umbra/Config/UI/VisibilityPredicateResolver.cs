using System.Linq.Expressions;
using System.Reflection;
using Umbra.Config.Attributes;
using Umbra.Logging;

namespace Umbra.Config.UI;

/// <summary>
/// Resolves <see cref="HideIfAttribute{T}"/> declarations into per-frame visibility
/// predicates. Keeps HideIf logic isolated from both tree traversal and control rendering.
/// </summary>
internal static class VisibilityPredicateResolver
{
    /// <summary>
    /// Resolves a cached <see cref="IHideIfAttribute"/> into a <see cref="Func{Boolean}"/> that
    /// returns <see langword="true"/> when the parameter should be visible (i.e. the hide
    /// condition is NOT met).
    /// </summary>
    /// <remarks>
    /// The raw property or field accessor is compiled once into a delegate at build time so
    /// that per-frame evaluation does not pay the cost of <see cref="PropertyInfo.GetValue(object)"/>
    /// reflection. The compiled delegate reads directly from the closed-over
    /// <paramref name="owner"/> instance.
    /// <para>
    /// Callers that have already determined <paramref name="hideIf"/> is
    /// <see langword="null"/> should short-circuit with <c>static () =&gt; true</c> and skip
    /// this call entirely to avoid the function-call overhead for the common no-condition case.
    /// </para>
    /// <para>
    /// When an explicit comparison value is present, a typed
    /// <see cref="EqualityComparer{T}.Default"/> delegate is compiled once at build time via
    /// <see cref="BuildTypedEquals"/> so that per-frame equality checks use
    /// <see cref="IEquatable{T}"/> rather than virtual <see cref="object.Equals(object)"/> dispatch.
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
        var targetProp = ownerType.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var targetField = ownerType.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (targetProp is null && targetField is null)
        {
            Logger.Warning($"ConfigDrawer: HideIf member '{memberName}' not found on {ownerType.Name}; condition ignored.");
            return static () => true;
        }

        // Compile a direct accessor once — avoids PropertyInfo.GetValue overhead each frame.
        var ownerExpr = Expression.Constant(owner);
        Expression rawAccess = targetProp is not null
            ? Expression.Property(ownerExpr, targetProp)
            : Expression.Field(ownerExpr, targetField!);

        var getRaw = Expression.Lambda<Func<object?>>(
            Expression.Convert(rawAccess, typeof(object))).Compile();

        // If the target is a Parameter<T>, unwrap its live Value via the IParameter interface.
        var rawType = (targetProp?.PropertyType ?? targetField!.FieldType)!;
        var isParameter = rawType.IsGenericType && rawType.GetGenericTypeDefinition() == typeof(Parameter<>);
        var getValue = isParameter
            ? () => (getRaw() as IParameter)?.GetValue()
            : getRaw;

        // No explicit value: treat member as bool — visible while NOT true.
        if (!hasValue) return () => getValue() is not true;

        // Explicit value: compile a typed EqualityComparer<T>.Default.Equals delegate once
        // so per-frame evaluation uses IEquatable<T> rather than object.Equals dispatch.
        var valueType = isParameter ? rawType.GetGenericArguments()[0] : rawType;
        var typedEquals = BuildTypedEquals(valueType, compareValue);

        // Guard against null: getValue() may return null when Parameter<T>.Value has been
        // cleared (e.g. via SetWithoutNotify). Unboxing null to a value type inside the
        // compiled comparer would throw InvalidCastException, breaking the per-frame UI loop.
        // Treat null as equal to compareValue only when compareValue is also null.
        return () =>
        {
            var val = getValue();
            if (val is null) return compareValue is not null;
            return !typedEquals(val);
        };
    }

    /// <summary>
    /// Builds a compiled <see cref="Func{Object, Boolean}"/> that compares a boxed runtime
    /// value against <paramref name="compareValue"/> using
    /// <see cref="EqualityComparer{T}.Default"/> for the concrete <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    /// The compiled delegate unboxes the incoming <c>object?</c> to <c>T</c> and calls
    /// <see cref="EqualityComparer{T}.Equals(T, T)"/>, which respects
    /// <see cref="IEquatable{T}"/> implementations and avoids virtual
    /// <see cref="object.Equals(object)"/> dispatch on each frame. <paramref name="compareValue"/>
    /// is captured as a typed constant so it is never re-boxed on subsequent calls.
    /// <para>
    /// Falls back to <c>x =&gt; Equals(x, compareValue)</c> if expression compilation fails —
    /// for example when there is a type mismatch between the attribute value and the member's
    /// declared type.
    /// </para>
    /// </remarks>
    /// <param name="valueType">
    /// The concrete <c>T</c> used to parameterise the comparer; derived from the member's
    /// declared type or from the inner generic argument of <c>Parameter&lt;T&gt;</c>.
    /// </param>
    /// <param name="compareValue">
    /// The boxed value to compare against. Must be assignable to <paramref name="valueType"/>;
    /// a type mismatch triggers the fallback path rather than an exception.
    /// </param>
    /// <returns>
    /// A compiled predicate that returns <see langword="true"/> when the runtime value equals
    /// <paramref name="compareValue"/> according to <see cref="EqualityComparer{T}.Default"/>.
    /// </returns>
    private static Func<object?, bool> BuildTypedEquals(Type valueType, object? compareValue)
    {
        try
        {
            var comparerType = typeof(EqualityComparer<>).MakeGenericType(valueType);
            var defaultComparer = comparerType.GetProperty("Default")!.GetValue(null)!;
            var equalsMethod = comparerType.GetMethod("Equals", [valueType, valueType])!;

            var xParam = Expression.Parameter(typeof(object), "x");
            var comparerExpr = Expression.Constant(defaultComparer, comparerType);
            var xConverted = Expression.Convert(xParam, valueType);
            var yConst = Expression.Constant(compareValue, valueType);
            var equalsCall = Expression.Call(comparerExpr, equalsMethod, xConverted, yConst);

            return Expression.Lambda<Func<object?, bool>>(equalsCall, xParam).Compile();
        }
        catch
        {
            return x => Equals(x, compareValue);
        }
    }
}
