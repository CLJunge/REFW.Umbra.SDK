namespace Umbra.Config.Validation.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterValidationPipeline"/>.
/// </summary>
[TestClass]
public sealed class ParameterValidationPipelineTests
{
    /// <summary>
    /// Verifies that required validation rejects a null candidate value.
    /// </summary>
    [TestMethod]
    public void Validate_WhenRequiredValueIsNull_ReturnsRequiredFailure()
    {
        // Arrange
        var context = CreateContext(typeof(string), new ParameterMetadata { Required = true }, null);
        var cache = new ParameterValidatorCache();

        // Act
        var result = ParameterValidationPipeline.Validate(context, cache);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Value is required.", result.ErrorMessage);
    }

    /// <summary>
    /// Verifies that minimum-length validation rejects strings shorter than the configured length.
    /// </summary>
    [TestMethod]
    public void Validate_WhenStringIsShorterThanMinLength_ReturnsMinLengthFailure()
    {
        // Arrange
        var context = CreateContext(typeof(string), new ParameterMetadata { MinLength = 5 }, "abc");
        var cache = new ParameterValidatorCache();

        // Act
        var result = ParameterValidationPipeline.Validate(context, cache);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Value must be at least 5 characters long.", result.ErrorMessage);
    }

    /// <summary>
    /// Verifies that regex validation returns the configured custom failure message.
    /// </summary>
    [TestMethod]
    public void Validate_WhenRegexDoesNotMatch_ReturnsRegexFailureMessage()
    {
        // Arrange
        var context = CreateContext(
            typeof(string),
            new ParameterMetadata
            {
                RegexPattern = "^[A-Z]{3}$",
                RegexMessage = "Use exactly three uppercase letters."
            },
            "ab1");
        var cache = new ParameterValidatorCache();

        // Act
        var result = ParameterValidationPipeline.Validate(context, cache);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Use exactly three uppercase letters.", result.ErrorMessage);
    }

    /// <summary>
    /// Verifies that numeric range validation rejects values below the configured minimum.
    /// </summary>
    [TestMethod]
    public void Validate_WhenValueIsBelowMinimum_ReturnsRangeFailure()
    {
        // Arrange
        var context = CreateContext(typeof(int), new ParameterMetadata { Min = 10, Max = 100 }, 5);
        var cache = new ParameterValidatorCache();

        // Act
        var result = ParameterValidationPipeline.Validate(context, cache);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Value must be greater than or equal to 10.", result.ErrorMessage);
    }

    /// <summary>
    /// Verifies that built-in validation failures short-circuit custom validator execution.
    /// </summary>
    [TestMethod]
    public void Validate_WhenBuiltInValidationFails_DoesNotRunCustomValidator()
    {
        // Arrange
        CountingCustomValidator.Reset();
        var context = CreateContext(
            typeof(string),
            new ParameterMetadata
            {
                Required = true,
                ValidatorType = typeof(CountingCustomValidator)
            },
            null);
        var cache = new ParameterValidatorCache();

        // Act
        var result = ParameterValidationPipeline.Validate(context, cache);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("Value is required.", result.ErrorMessage);
        Assert.AreEqual(0, CountingCustomValidator.ValidateCallCount);
    }

    /// <summary>
    /// Verifies that a custom validator can reject a value with its own failure message.
    /// </summary>
    [TestMethod]
    public void Validate_WhenCustomValidatorRejectsValue_ReturnsCustomFailureMessage()
    {
        // Arrange
        ReservedValueValidator.Reset();
        var context = CreateContext(
            typeof(string),
            new ParameterMetadata { ValidatorType = typeof(ReservedValueValidator) },
            "blocked");
        var cache = new ParameterValidatorCache();

        // Act
        var result = ParameterValidationPipeline.Validate(context, cache);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("The value 'blocked' is reserved.", result.ErrorMessage);
        Assert.AreEqual(1, ReservedValueValidator.ValidateCallCount);
    }

    /// <summary>
    /// Verifies that a blank custom validator error message falls back to Umbra's default failure message.
    /// </summary>
    [TestMethod]
    public void Validate_WhenCustomValidatorReturnsBlankMessage_UsesFallbackFailureMessage()
    {
        // Arrange
        var context = CreateContext(
            typeof(string),
            new ParameterMetadata { ValidatorType = typeof(BlankMessageValidator) },
            "value");
        var cache = new ParameterValidatorCache();

        // Act
        var result = ParameterValidationPipeline.Validate(context, cache);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(
            $"Validator '{typeof(BlankMessageValidator).FullName}' rejected the value.",
            result.ErrorMessage);
    }

    private static ParameterValidationContext CreateContext(Type valueType, ParameterMetadata metadata, object? candidateValue)
        => new("test.parameter", valueType, metadata, candidateValue);

    private sealed class CountingCustomValidator : IParameterValidator
    {
        internal static int ValidateCallCount;

        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
        {
            ValidateCallCount++;
            return ParameterValidationResult.Valid();
        }

        internal static void Reset() => ValidateCallCount = 0;
    }

    private sealed class ReservedValueValidator : IParameterValidator
    {
        internal static int ValidateCallCount;

        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
        {
            ValidateCallCount++;
            if (value is string text && text == "blocked")
                return ParameterValidationResult.Invalid("The value 'blocked' is reserved.");

            return ParameterValidationResult.Valid();
        }

        internal static void Reset() => ValidateCallCount = 0;
    }

    private sealed class BlankMessageValidator : IParameterValidator
    {
        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
            => new(false, "   ");
    }
}
