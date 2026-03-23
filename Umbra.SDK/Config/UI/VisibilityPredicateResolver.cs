using System.Linq.Expressions;
using System.Reflection;
using Umbra.SDK.Config.Attributes;
using Umbra.SDK.Logging;

namespace Umbra.SDK.Config.UI;

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
    /// that per-frame evaluation does not pay the cost of <see cref="PropertyInfo.GetValue"/>
    /// reflection. The compiled delegate reads directly from the closed-over
    /// <paramref name="owner"/> instance.
    /// <para>
    /// Callers that have already determined <paramref name="hideIf"/> is
    /// <see langword="null"/> should short-circuit with <c>static () =&gt; true</c> and skip
    /// this call entirely to avoid the function-call overhead for the common no-condition case.
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
        var targetProp = ownerType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
        var targetField = ownerType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);

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
        var getValue = rawType.IsGenericType && rawType.GetGenericTypeDefinition() == typeof(Parameter<>)
            ? () => (getRaw() as IParameter)?.GetValue()
            : getRaw;

        // No explicit value: treat member as bool — visible while NOT true.
        if (!hasValue) return () => getValue() is not true;

        // Explicit value: visible while member value does NOT equal compareValue.
        return () => !Equals(getValue(), compareValue);
    }
}
