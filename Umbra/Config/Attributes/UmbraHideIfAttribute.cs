namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the condition that hides an annotated parameter in the configuration UI.
/// </summary>
/// <typeparam name="T">The comparison value type used by the referenced configuration member.</typeparam>
/// <remarks>
/// Umbra evaluates the named member on the same configuration object as the annotated parameter. The one-argument constructor uses Boolean semantics, while the two-argument constructor hides the parameter when the member equals the declared comparison value.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraHideIfAttribute<T> : Attribute, IHideIfAttribute
{
    /// <summary>
    /// Gets the name of the property or field evaluated by the hide condition.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Gets the comparison value used by the value-based overload.
    /// </summary>
    /// <value>The comparison value, or <see langword="null"/> when the Boolean overload is used.</value>
    public T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether this attribute uses an explicit comparison value.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Value"/> participates in the comparison; otherwise, <see langword="false"/>.</value>
    public bool HasValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbraHideIfAttribute{T}"/> class using Boolean semantics.
    /// </summary>
    /// <param name="memberName">The name of the Boolean member that hides the annotated parameter when it is <see langword="true"/>.</param>
    public UmbraHideIfAttribute(string memberName)
    {
        MemberName = memberName;
        Value = default;
        HasValue = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbraHideIfAttribute{T}"/> class using explicit value comparison.
    /// </summary>
    /// <param name="memberName">The name of the member evaluated by the hide condition.</param>
    /// <param name="value">The value that hides the annotated parameter when the member equals it.</param>
    public UmbraHideIfAttribute(string memberName, T value)
    {
        MemberName = memberName;
        Value = value;
        HasValue = true;
    }

    /// <inheritdoc/>
    object? IHideIfAttribute.BoxedValue => Value;
}
