using Umbra.Config.Validation;

namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the custom validator used to validate an annotated parameter.
/// </summary>
/// <typeparam name="TValidator">The <see cref="IParameterValidator"/> implementation used for validation.</typeparam>
/// <remarks>
/// Umbra resolves the declared validator type during metadata discovery so parameter mutation paths can execute custom rules without requiring repeated generic-attribute inspection.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraValidateWithAttribute<TValidator> : Attribute, IValidatorAttribute
    where TValidator : IParameterValidator, new()
{
    /// <summary>
    /// Gets the concrete validator type used for the annotated parameter.
    /// </summary>
    /// <value>The declared validator type.</value>
    public Type ValidatorType => typeof(TValidator);
}
