namespace Umbra.Config.Attributes;

/// <summary>
/// Hides a settings parameter in the UI when a named member on the same configuration
/// class satisfies a condition.
/// </summary>
/// <typeparam name="T">The type of the member value to compare against.</typeparam>
/// <remarks>
/// <para>
/// When constructed with only a <c>memberName</c>, the referenced member must
/// be a <c>bool</c>; the parameter is hidden while that member is <c>true</c>.
/// </para>
/// <para>
/// When constructed with both a <c>memberName</c> and a <c>value</c>,
/// the parameter is hidden while the member's current value equals <c>value</c>.
/// Comparison is performed using <c>EqualityComparer&lt;T&gt;.Default</c> where possible,
/// so <see cref="System.IEquatable{T}"/> implementations are respected; if a typed
/// comparer cannot be used, the resolver falls back to <see cref="object.Equals(object, object)"/>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HideIfAttribute<T> : Attribute, IHideIfAttribute
{
    /// <summary>Gets the name of the property or field on the configuration class to evaluate.</summary>
    public string MemberName { get; }

    /// <summary>
    /// Gets the value to compare the member against. When <see cref="HasValue"/> is
    /// <see langword="false"/>, this is <see langword="default"/> and the member is
    /// treated as a plain <c>bool</c> (hidden while <c>true</c>).
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether an explicit comparison value was provided.
    /// When <see langword="false"/>, the member is treated as a <c>bool</c> and the
    /// parameter is hidden while it is <c>true</c>.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Hides this parameter while the named <c>bool</c> member on the configuration class is <c>true</c>.
    /// </summary>
    /// <param name="memberName">The name of a <c>bool</c> property or field on the configuration class.</param>
    public HideIfAttribute(string memberName)
    {
        MemberName = memberName;
        Value = default;
        HasValue = false;
    }

    /// <summary>
    /// Hides this parameter while the named member on the configuration class equals <paramref name="value"/>.
    /// </summary>
    /// <param name="memberName">The name of a property or field on the configuration class.</param>
    /// <param name="value">The value that, when matched, causes the parameter to be hidden.</param>
    public HideIfAttribute(string memberName, T value)
    {
        MemberName = memberName;
        Value = value;
        HasValue = true;
    }

    /// <inheritdoc/>
    object? IHideIfAttribute.BoxedValue => Value;
}
