namespace Umbra.Config.Validation.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterValidatorCache"/>.
/// </summary>
[TestClass]
public sealed class ParameterValidatorCacheTests
{
    /// <summary>
    /// Verifies that the first successful lookup creates and returns a validator instance.
    /// </summary>
    [TestMethod]
    public void TryGet_FirstLookup_CreatesValidatorInstance()
    {
        // Arrange
        CountingValidator.Reset();
        var cache = new ParameterValidatorCache();

        // Act
        var result = cache.TryGet(typeof(CountingValidator), out var validator, out var failureReason);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(validator);
        Assert.IsNull(failureReason);
        Assert.AreEqual(1, CountingValidator.InstanceCount);
    }

    /// <summary>
    /// Verifies that the cache reuses one validator instance while the validator type is unchanged.
    /// </summary>
    [TestMethod]
    public void TryGet_WithSameValidatorType_ReusesCachedInstance()
    {
        // Arrange
        CountingValidator.Reset();
        var cache = new ParameterValidatorCache();

        // Act
        var firstResult = cache.TryGet(typeof(CountingValidator), out var firstValidator, out var firstFailureReason);
        var secondResult = cache.TryGet(typeof(CountingValidator), out var secondValidator, out var secondFailureReason);

        // Assert
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.IsNull(firstFailureReason);
        Assert.IsNull(secondFailureReason);
        Assert.AreSame(firstValidator, secondValidator);
        Assert.AreEqual(1, CountingValidator.InstanceCount);
    }

    /// <summary>
    /// Verifies that the cache recreates the validator when the validator type changes.
    /// </summary>
    [TestMethod]
    public void TryGet_WhenValidatorTypeChanges_CreatesNewValidator()
    {
        // Arrange
        CountingValidator.Reset();
        AlternateCountingValidator.Reset();
        var cache = new ParameterValidatorCache();

        var firstResult = cache.TryGet(typeof(CountingValidator), out var firstValidator, out var firstFailureReason);

        // Act
        var secondResult = cache.TryGet(typeof(AlternateCountingValidator), out var secondValidator, out var secondFailureReason);

        // Assert
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.IsNull(firstFailureReason);
        Assert.IsNull(secondFailureReason);
        Assert.AreNotSame(firstValidator, secondValidator);
        Assert.AreEqual(1, CountingValidator.InstanceCount);
        Assert.AreEqual(1, AlternateCountingValidator.InstanceCount);
    }

    /// <summary>
    /// Verifies that the cache rejects types that do not implement <see cref="IParameterValidator"/>.
    /// </summary>
    [TestMethod]
    public void TryGet_WithInvalidValidatorType_ReturnsFalseWithFailureReason()
    {
        // Arrange
        var cache = new ParameterValidatorCache();

        // Act
        var result = cache.TryGet(typeof(string), out _, out var failureReason);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("Validator type 'System.String' must implement IParameterValidator.", failureReason);
    }

    /// <summary>
    /// Verifies that validator creation failures are reported as deterministic cache failures.
    /// </summary>
    [TestMethod]
    public void TryGet_WhenValidatorConstructionThrows_ReturnsFalseWithFailureReason()
    {
        // Arrange
        var cache = new ParameterValidatorCache();

        // Act
        var result = cache.TryGet(typeof(ThrowingConstructorValidator), out _, out var failureReason);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(failureReason);
        Assert.Contains(failureReason, $"Validator '{typeof(ThrowingConstructorValidator).FullName}' could not be created:");
        Assert.Contains(failureReason, "Constructor failure.");
    }

    private sealed class CountingValidator : IParameterValidator
    {
        internal static int InstanceCount;

        public CountingValidator()
        {
            InstanceCount++;
        }

        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
            => ParameterValidationResult.Valid();

        internal static void Reset() => InstanceCount = 0;
    }

    private sealed class AlternateCountingValidator : IParameterValidator
    {
        internal static int InstanceCount;

        public AlternateCountingValidator()
        {
            InstanceCount++;
        }

        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
            => ParameterValidationResult.Valid();

        internal static void Reset() => InstanceCount = 0;
    }

    private sealed class ThrowingConstructorValidator : IParameterValidator
    {
        public ThrowingConstructorValidator()
        {
            throw new InvalidOperationException("Constructor failure.");
        }

        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
            => ParameterValidationResult.Valid();
    }
}
