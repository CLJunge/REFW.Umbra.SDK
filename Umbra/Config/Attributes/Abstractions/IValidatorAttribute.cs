namespace Umbra.Config.Attributes;

/// <summary>
/// Exposes the validator type declared by <see cref="UmbraValidateWithAttribute{TValidator}"/> without requiring generic attribute inspection.
/// </summary>
/// <remarks>
/// Umbra's metadata pipeline reads this interface during registration so custom validators can be resolved once from reflected members.
/// </remarks>
internal interface IValidatorAttribute
{
    /// <summary>
    /// Gets the concrete validator type declared for the annotated member.
    /// </summary>
    /// <value>The validator type used when candidate values are validated.</value>
    Type ValidatorType { get; }
}
