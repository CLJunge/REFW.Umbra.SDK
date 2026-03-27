namespace Umbra.Config.Attributes;

/// <summary>
/// Hides a settings parameter in the UI when a named member on the same configuration
/// class satisfies a condition.
/// </summary>
/// <typeparam name="T">The type of the member value to compare against.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraHideIfAttribute<T> : Attribute, IHideIfAttribute
{
    /// <summary>Gets the name of the property or field on the configuration class to evaluate.</summary>
    public string MemberName { get; }

    /// <summary>Gets the value to compare the member against.</summary>
    public T? Value { get; }

    /// <summary>Gets a value indicating whether an explicit comparison value was provided.</summary>
    public bool HasValue { get; }

    /// <summary>
    /// Hides this parameter while the named <c>bool</c> member on the configuration class is <c>true</c>.
    /// </summary>
    public UmbraHideIfAttribute(string memberName)
    {
        MemberName = memberName;
        Value = default;
        HasValue = false;
    }

    /// <summary>
    /// Hides this parameter while the named member on the configuration class equals <paramref name="value"/>.
    /// </summary>
    public UmbraHideIfAttribute(string memberName, T value)
    {
        MemberName = memberName;
        Value = value;
        HasValue = true;
    }

    /// <inheritdoc/>
    object? IHideIfAttribute.BoxedValue => Value;
}
