using Umbra.Config.Validation;

namespace Umbra.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="Parameter{T}"/> class, specifically the <see cref="Parameter{T}.IsModified"/> property.
/// </summary>
[TestClass]
public class ParameterTests
{
    #region IsModified Tests - Initial State

    /// <summary>
    /// Tests that IsModified returns false immediately after default construction
    /// with various value types.
    /// </summary>
    [TestMethod]
    public void IsModified_AfterDefaultConstruction_ReturnsFalse()
    {
        // Arrange & Act - int
        var intParam = new Parameter<int>();

        // Assert
        Assert.IsFalse(intParam.IsModified, "IsModified should be false for default-constructed Parameter<int>");

        // Arrange & Act - bool
        var boolParam = new Parameter<bool>();

        // Assert
        Assert.IsFalse(boolParam.IsModified, "IsModified should be false for default-constructed Parameter<bool>");

        // Arrange & Act - double
        var doubleParam = new Parameter<double>();

        // Assert
        Assert.IsFalse(doubleParam.IsModified, "IsModified should be false for default-constructed Parameter<double>");

        // Arrange & Act - string
        var stringParam = new Parameter<string>();

        // Assert
        Assert.IsFalse(stringParam.IsModified, "IsModified should be false for default-constructed Parameter<string>");
    }

    /// <summary>
    /// Tests that IsModified returns false immediately after construction with an explicit default value.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(42)]
    [DataRow(-100)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void IsModified_AfterConstructionWithValue_ReturnsFalse(int defaultValue)
    {
        // Arrange & Act
        var parameter = new Parameter<int>(defaultValue);

        // Assert
        Assert.IsFalse(parameter.IsModified, $"IsModified should be false immediately after construction with default value {defaultValue}");
    }

    /// <summary>
    /// Tests that IsModified returns false when constructed with null for a nullable value type.
    /// </summary>
    [TestMethod]
    public void IsModified_NullableValueType_ConstructedWithNull_ReturnsFalse()
    {
        // Arrange & Act
        var parameter = new Parameter<int?>(null);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when constructed with null for nullable value type");
    }

    /// <summary>
    /// Tests that IsModified returns false when constructed with null for a reference type.
    /// </summary>
    [TestMethod]
    public void IsModified_ReferenceType_ConstructedWithNull_ReturnsFalse()
    {
        // Arrange & Act
        var parameter = new Parameter<string>(null);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when constructed with null for reference type");
    }

    #endregion

    #region IsModified Tests - After Modification

    /// <summary>
    /// Tests that IsModified returns true after the value is changed from the default.
    /// </summary>
    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(42, 43)]
    [DataRow(-100, 100)]
    [DataRow(0, int.MaxValue)]
    [DataRow(0, int.MinValue)]
    public void IsModified_AfterValueChanged_ReturnsTrue(int defaultValue, int newValue)
    {
        // Arrange
        var parameter = new Parameter<int>(defaultValue);

        // Act
        parameter.SetWithoutNotify(newValue);

        // Assert
        Assert.IsTrue(parameter.IsModified, $"IsModified should be true after changing value from {defaultValue} to {newValue}");
    }

    /// <summary>
    /// Tests that IsModified returns true when the value is changed to a different value using the Value property.
    /// </summary>
    [TestMethod]
    public void IsModified_AfterValuePropertySet_ReturnsTrue()
    {
        // Arrange
        var parameter = new Parameter<int>(10)
        {
            // Act
            Value = 20
        };

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true after setting Value property to a different value");
    }

    /// <summary>
    /// Tests that IsModified returns true when the value is changed using Set method.
    /// </summary>
    [TestMethod]
    public void IsModified_AfterSetMethod_ReturnsTrue()
    {
        // Arrange
        var parameter = new Parameter<int>(100);

        // Act
        parameter.Set(200);

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true after calling Set with a different value");
    }

    /// <summary>
    /// Tests that IsModified returns true when a nullable value type changes from null to a value.
    /// </summary>
    [TestMethod]
    public void IsModified_NullableValueType_FromNullToValue_ReturnsTrue()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);

        // Act
        parameter.SetWithoutNotify(42);

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true when nullable value type changes from null to a value");
    }

    /// <summary>
    /// Tests that IsModified returns true when a nullable value type changes from a value to null.
    /// </summary>
    [TestMethod]
    public void IsModified_NullableValueType_FromValueToNull_ReturnsTrue()
    {
        // Arrange
        var parameter = new Parameter<int?>(42);

        // Act
        parameter.SetWithoutNotify(null);

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true when nullable value type changes from a value to null");
    }

    /// <summary>
    /// Tests that IsModified returns true when a reference type changes from null to a value.
    /// </summary>
    [TestMethod]
    public void IsModified_ReferenceType_FromNullToValue_ReturnsTrue()
    {
        // Arrange
        var parameter = new Parameter<string>(null);

        // Act
        parameter.SetWithoutNotify("test");

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true when reference type changes from null to a value");
    }

    /// <summary>
    /// Tests that IsModified returns true when a reference type changes from a value to null.
    /// </summary>
    [TestMethod]
    public void IsModified_ReferenceType_FromValueToNull_ReturnsTrue()
    {
        // Arrange
        var parameter = new Parameter<string>("test");

        // Act
        parameter.SetWithoutNotify(null);

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true when reference type changes from a value to null");
    }

    /// <summary>
    /// Tests that IsModified returns true when a string is changed to a different string value.
    /// </summary>
    [TestMethod]
    [DataRow("", "test")]
    [DataRow("test", "")]
    [DataRow("hello", "world")]
    [DataRow("a", "A")]
    public void IsModified_String_ChangedToDifferentValue_ReturnsTrue(string defaultValue, string newValue)
    {
        // Arrange
        var parameter = new Parameter<string>(defaultValue);

        // Act
        parameter.SetWithoutNotify(newValue);

        // Assert
        Assert.IsTrue(parameter.IsModified, $"IsModified should be true when string changes from '{defaultValue}' to '{newValue}'");
    }

    #endregion

    #region IsModified Tests - After Reset

    /// <summary>
    /// Tests that IsModified returns false after Reset is called, even if the value was previously modified.
    /// </summary>
    [TestMethod]
    public void IsModified_AfterReset_ReturnsFalse()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        parameter.SetWithoutNotify(20);
        Assert.IsTrue(parameter.IsModified, "Precondition: IsModified should be true after modification");

        // Act
        parameter.Reset(raiseEvent: false);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false after Reset");
    }

    /// <summary>
    /// Tests that IsModified returns false after Reset with raiseEvent=true.
    /// </summary>
    [TestMethod]
    public void IsModified_AfterResetWithRaiseEvent_ReturnsFalse()
    {
        // Arrange
        var parameter = new Parameter<int>(50);
        parameter.SetWithoutNotify(100);

        // Act
        parameter.Reset(raiseEvent: true);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false after Reset with raiseEvent=true");
    }

    #endregion

    #region IsModified Tests - Same Value

    /// <summary>
    /// Tests that IsModified remains false when the value is set to the same value as the default.
    /// </summary>
    [TestMethod]
    public void IsModified_SetToSameValueAsDefault_ReturnsFalse()
    {
        // Arrange
        var parameter = new Parameter<int>(42);

        // Act
        parameter.SetWithoutNotify(42);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when value is set to the same value as default");
    }

    /// <summary>
    /// Tests that IsModified returns false when both default and current value are null for nullable value types.
    /// </summary>
    [TestMethod]
    public void IsModified_NullableValueType_BothNull_ReturnsFalse()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);

        // Act
        parameter.SetWithoutNotify(null);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when both default and current value are null");
    }

    /// <summary>
    /// Tests that IsModified returns false when string values are equal.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("test")]
    [DataRow("   ")]
    [DataRow("very long string with special characters: !@#$%^&*()")]
    public void IsModified_String_SameValueAsDefault_ReturnsFalse(string value)
    {
        // Arrange
        var parameter = new Parameter<string>(value);

        // Act
        parameter.SetWithoutNotify(value);

        // Assert
        Assert.IsFalse(parameter.IsModified, $"IsModified should be false when string value equals default: '{value}'");
    }

    #endregion

    #region IsModified Tests - Changed Back to Default

    /// <summary>
    /// Tests that IsModified returns false when the value is changed back to the default after being modified.
    /// </summary>
    [TestMethod]
    public void IsModified_ChangedBackToDefault_ReturnsFalse()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        parameter.SetWithoutNotify(20);
        Assert.IsTrue(parameter.IsModified, "Precondition: IsModified should be true after modification");

        // Act
        parameter.SetWithoutNotify(10);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when value is changed back to default");
    }

    /// <summary>
    /// Tests that IsModified returns false for nullable value types when changed back to default null.
    /// </summary>
    [TestMethod]
    public void IsModified_NullableValueType_ChangedBackToDefaultNull_ReturnsFalse()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);
        parameter.SetWithoutNotify(100);
        Assert.IsTrue(parameter.IsModified, "Precondition: IsModified should be true after changing from null to value");

        // Act
        parameter.SetWithoutNotify(null);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when nullable value type is changed back to default null");
    }

    /// <summary>
    /// Tests that IsModified returns false for reference types when changed back to default null.
    /// </summary>
    [TestMethod]
    public void IsModified_ReferenceType_ChangedBackToDefaultNull_ReturnsFalse()
    {
        // Arrange
        var parameter = new Parameter<string>(null);
        parameter.SetWithoutNotify("test");
        Assert.IsTrue(parameter.IsModified, "Precondition: IsModified should be true after changing from null to value");

        // Act
        parameter.SetWithoutNotify(null);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when reference type is changed back to default null");
    }

    #endregion

    #region IsModified Tests - Special Numeric Values

    /// <summary>
    /// Tests IsModified behavior with double.NaN values.
    /// </summary>
    [TestMethod]
    public void IsModified_Double_NaN_BehavesCorrectly()
    {
        // Arrange - default is NaN
        var parameter = new Parameter<double>(double.NaN);

        // Act & Assert - NaN != NaN by IEEE 754, so IsModified should be true even when "unchanged"
        // EqualityComparer<double>.Default treats NaN == NaN as true, so IsModified should be false
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when default and current are both NaN (EqualityComparer behavior)");

        // Act - change to a different value
        parameter.SetWithoutNotify(1.0);

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true when value changes from NaN to 1.0");

        // Act - change back to NaN
        parameter.SetWithoutNotify(double.NaN);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when value changes back to NaN");
    }

    /// <summary>
    /// Tests IsModified behavior with float.NaN values.
    /// </summary>
    [TestMethod]
    public void IsModified_Float_NaN_BehavesCorrectly()
    {
        // Arrange
        var parameter = new Parameter<float>(float.NaN);

        // Assert
        Assert.IsFalse(parameter.IsModified, "IsModified should be false when default and current are both NaN (EqualityComparer behavior)");

        // Act
        parameter.SetWithoutNotify(1.0f);

        // Assert
        Assert.IsTrue(parameter.IsModified, "IsModified should be true when value changes from NaN to 1.0");
    }

    #endregion

    #region IsModified Tests - Multiple Modifications

    /// <summary>
    /// Tests that IsModified correctly reflects state through multiple value changes.
    /// </summary>
    [TestMethod]
    public void IsModified_MultipleChanges_ReflectsCorrectState()
    {
        // Arrange
        var parameter = new Parameter<int>(10);

        // Assert initial state
        Assert.IsFalse(parameter.IsModified, "Initial: IsModified should be false");

        // Act & Assert - first change
        parameter.SetWithoutNotify(20);
        Assert.IsTrue(parameter.IsModified, "After first change: IsModified should be true");

        // Act & Assert - second change
        parameter.SetWithoutNotify(30);
        Assert.IsTrue(parameter.IsModified, "After second change: IsModified should still be true");

        // Act & Assert - change back to default
        parameter.SetWithoutNotify(10);
        Assert.IsFalse(parameter.IsModified, "After changing back to default: IsModified should be false");

        // Act & Assert - change again
        parameter.SetWithoutNotify(40);
        Assert.IsTrue(parameter.IsModified, "After changing again: IsModified should be true");
    }

    #endregion

    #region IsModified Tests - Different Type Parameters

    /// <summary>
    /// Tests IsModified with bool type parameter.
    /// </summary>
    [TestMethod]
    public void IsModified_Bool_BehavesCorrectly()
    {
        // Arrange
        var parameter = new Parameter<bool>(false);

        // Assert initial
        Assert.IsFalse(parameter.IsModified);

        // Act & Assert - change
        parameter.SetWithoutNotify(true);
        Assert.IsTrue(parameter.IsModified);

        // Act & Assert - change back
        parameter.SetWithoutNotify(false);
        Assert.IsFalse(parameter.IsModified);
    }

    #endregion

    /// <summary>
    /// Tests that the parameterless constructor initializes an int parameter with default value zero,
    /// and IsModified is false since the value equals the default.
    /// </summary>
    [TestMethod]
    public void Constructor_IntType_InitializesWithDefaultValueZero()
    {
        // Act
        var parameter = new Parameter<int>();

        // Assert
        Assert.AreEqual(0, parameter.DefaultValue);
        Assert.AreEqual(0, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(typeof(int), parameter.ValueType);
    }

    /// <summary>
    /// Tests that the parameterless constructor initializes a string parameter with default value null,
    /// and IsModified is false since the value equals the default.
    /// </summary>
    [TestMethod]
    public void Constructor_StringType_InitializesWithDefaultValueNull()
    {
        // Act
        var parameter = new Parameter<string>();

        // Assert
        Assert.IsNull(parameter.DefaultValue);
        Assert.IsNull(parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(typeof(string), parameter.ValueType);
    }

    /// <summary>
    /// Tests that the parameterless constructor initializes a nullable int parameter with default value null,
    /// and IsModified is false since the value equals the default.
    /// </summary>
    [TestMethod]
    public void Constructor_NullableIntType_InitializesWithDefaultValueNull()
    {
        // Act
        var parameter = new Parameter<int?>();

        // Assert
        Assert.IsNull(parameter.DefaultValue);
        Assert.IsNull(parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(typeof(int?), parameter.ValueType);
    }

    /// <summary>
    /// Tests that the parameterless constructor initializes an enum parameter with the zero value of the enum,
    /// and IsModified is false since the value equals the default.
    /// </summary>
    [TestMethod]
    public void Constructor_EnumType_InitializesWithDefaultZeroValue()
    {
        // Act
        var parameter = new Parameter<TestEnum>();

        // Assert
        Assert.AreEqual(TestEnum.None, parameter.DefaultValue);
        Assert.AreEqual(TestEnum.None, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(typeof(TestEnum), parameter.ValueType);
    }

    /// <summary>
    /// Tests that the parameterless constructor initializes a struct parameter with default zero-initialized struct,
    /// and IsModified is false since the value equals the default.
    /// </summary>
    [TestMethod]
    public void Constructor_StructType_InitializesWithDefaultZeroInitializedStruct()
    {
        // Act
        var parameter = new Parameter<TestStruct>();

        // Assert
        Assert.AreEqual(default, parameter.DefaultValue);
        Assert.AreEqual(default, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(typeof(TestStruct), parameter.ValueType);
    }

    /// <summary>
    /// Test enum used for enum parameter tests.
    /// </summary>
    private enum TestEnum
    {
        None = 0,
        First = 1,
        Second = 2
    }

    /// <summary>
    /// Test struct used for struct parameter tests.
    /// </summary>
    private struct TestStruct
    {
        public int Value { get; init; }
        public string? Name { get; init; }
    }

    /// <summary>
    /// Tests that the constructor correctly initializes both Value and DefaultValue
    /// with the provided default value for a non-nullable value type (int).
    /// </summary>
    [TestMethod]
    public void Constructor_WithIntValue_InitializesValueAndDefaultValue()
    {
        // Arrange
        const int expectedValue = 42;

        // Act
        var parameter = new Parameter<int>(expectedValue);

        // Assert
        Assert.AreEqual(expectedValue, parameter.Value);
        Assert.AreEqual(expectedValue, parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes both Value and DefaultValue
    /// with a double value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDoubleValue_InitializesCorrectly()
    {
        // Arrange
        const double expectedValue = 3.14159;

        // Act
        var parameter = new Parameter<double>(expectedValue);

        // Assert
        Assert.AreEqual(expectedValue, parameter.Value);
        Assert.AreEqual(expectedValue, parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly handles double.NaN as a default value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDoubleNaN_InitializesCorrectly()
    {
        // Arrange
        const double expectedValue = double.NaN;

        // Act
        var parameter = new Parameter<double>(expectedValue);

        // Assert
        Assert.IsTrue(double.IsNaN(parameter.Value));
        Assert.IsTrue(double.IsNaN(parameter.DefaultValue));
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly handles float.NaN as a default value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithFloatNaN_InitializesCorrectly()
    {
        // Arrange
        const float expectedValue = float.NaN;

        // Act
        var parameter = new Parameter<float>(expectedValue);

        // Assert
        Assert.IsTrue(float.IsNaN(parameter.Value));
        Assert.IsTrue(float.IsNaN(parameter.DefaultValue));
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a boolean true value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBoolTrue_InitializesCorrectly()
    {
        // Arrange
        const bool expectedValue = true;

        // Act
        var parameter = new Parameter<bool>(expectedValue);

        // Assert
        Assert.AreEqual(expectedValue, parameter.Value);
        Assert.AreEqual(expectedValue, parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a non-null string value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNonNullString_InitializesCorrectly()
    {
        // Arrange
        const string expectedValue = "test value";

        // Act
        var parameter = new Parameter<string>(expectedValue);

        // Assert
        Assert.AreEqual(expectedValue, parameter.Value);
        Assert.AreEqual(expectedValue, parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a null string value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullString_InitializesCorrectly()
    {
        // Arrange
        string? expectedValue = null;

        // Act
        var parameter = new Parameter<string>(expectedValue);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsNull(parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a nullable int that has a value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullableIntValue_InitializesCorrectly()
    {
        // Arrange
        int? expectedValue = 123;

        // Act
        var parameter = new Parameter<int?>(expectedValue);

        // Assert
        Assert.AreEqual(expectedValue, parameter.Value);
        Assert.AreEqual(expectedValue, parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that a newly constructed parameter initializes Key and Metadata to usable defaults.
    /// </summary>
    [TestMethod]
    public void Constructor_NewParameter_InitializesKeyAndMetadataDefaults()
    {
        // Act
        var parameter = new Parameter<int>(42);

        // Assert
        Assert.AreEqual(string.Empty, parameter.Key);
        Assert.IsNotNull(parameter.Metadata);
    }

    /// <summary>
    /// Tests that the public parameter surface exposes registration identity and metadata as
    /// read-only values while keeping non-public setters available for Umbra internals.
    /// </summary>
    [TestMethod]
    public void RegistrationState_PublicSurfaceIsReadOnly()
    {
        // Arrange
        var interfaceKey = typeof(IParameter).GetProperty(nameof(IParameter.Key))!;
        var interfaceMetadata = typeof(IParameter).GetProperty(nameof(IParameter.Metadata))!;
        var concreteKey = typeof(Parameter<int>).GetProperty(nameof(Parameter<int>.Key))!;
        var concreteMetadata = typeof(Parameter<int>).GetProperty(nameof(Parameter<int>.Metadata))!;

        // Assert
        Assert.IsFalse(interfaceKey.CanWrite);
        Assert.IsFalse(interfaceMetadata.CanWrite);
        Assert.IsNotNull(concreteKey.SetMethod);
        Assert.IsNotNull(concreteMetadata.SetMethod);
        Assert.IsFalse(concreteKey.SetMethod.IsPublic);
        Assert.IsFalse(concreteMetadata.SetMethod.IsPublic);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a nullable int that is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullableIntNull_InitializesCorrectly()
    {
        // Arrange
        int? expectedValue = null;

        // Act
        var parameter = new Parameter<int?>(expectedValue);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsNull(parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a nullable double that is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullableDoubleNull_InitializesCorrectly()
    {
        // Arrange
        double? expectedValue = null;

        // Act
        var parameter = new Parameter<double?>(expectedValue);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsNull(parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a nullable bool that is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullableBoolNull_InitializesCorrectly()
    {
        // Arrange
        bool? expectedValue = null;

        // Act
        var parameter = new Parameter<bool?>(expectedValue);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsNull(parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a custom reference type.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCustomReferenceType_InitializesCorrectly()
    {
        // Arrange
        var expectedValue = new TestReferenceType { Value = 42 };

        // Act
        var parameter = new Parameter<TestReferenceType>(expectedValue);

        // Assert
        Assert.AreSame(expectedValue, parameter.Value);
        Assert.AreSame(expectedValue, parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes with a null custom reference type.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullCustomReferenceType_InitializesCorrectly()
    {
        // Arrange
        TestReferenceType? expectedValue = null;

        // Act
        var parameter = new Parameter<TestReferenceType>(expectedValue);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsNull(parameter.DefaultValue);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Helper class for testing with custom reference types.
    /// </summary>
    private class TestReferenceType
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Test validator that rejects one reserved string value.
    /// </summary>
    private sealed class RejectBlockedValueValidator : IParameterValidator
    {
        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
        {
            _ = parameterKey;
            _ = valueType;
            _ = metadata;

            if (value is string text && text == "blocked")
                return ParameterValidationResult.Invalid("The value 'blocked' is reserved.");

            return ParameterValidationResult.Valid();
        }
    }

    /// <summary>
    /// Test validator that counts instance creation and validation calls.
    /// </summary>
    private sealed class CountingValidator : IParameterValidator
    {
        internal static int InstanceCount;
        internal static int ValidateCallCount;

        public CountingValidator()
        {
            InstanceCount++;
        }

        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
        {
            _ = parameterKey;
            _ = value;
            _ = valueType;
            _ = metadata;
            ValidateCallCount++;
            return ParameterValidationResult.Valid();
        }

        internal static void Reset()
        {
            InstanceCount = 0;
            ValidateCallCount = 0;
        }
    }

    /// <summary>
    /// Alternate test validator used to verify cache refresh when validator types change.
    /// </summary>
    private sealed class AlternateCountingValidator : IParameterValidator
    {
        internal static int InstanceCount;

        public AlternateCountingValidator()
        {
            InstanceCount++;
        }

        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
        {
            _ = parameterKey;
            _ = value;
            _ = valueType;
            _ = metadata;
            return ParameterValidationResult.Valid();
        }

        internal static void Reset() => InstanceCount = 0;
    }

    /// <summary>
    /// Tests that Reset with raiseEvent=true raises both ValueChanged and IParameter.ValueChanged events
    /// when the current value differs from the default value.
    /// </summary>
    [TestMethod]
    public void Reset_WithRaiseEventTrueAndValueChanged_RaisesTypedAndUntypedEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10) { Value = 20 };
        var typedOldValue = 0;
        var typedNewValue = 0;
        var typedEventCallCount = 0;
        var untypedEventCallCount = 0;

        parameter.ValueChanged += (oldVal, newVal) =>
        {
            typedOldValue = oldVal;
            typedNewValue = newVal;
            typedEventCallCount++;
        };

        ((IParameter)parameter).ValueChanged += () => untypedEventCallCount++;

        // Act
        parameter.Reset(raiseEvent: true);

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(1, typedEventCallCount);
        Assert.AreEqual(20, typedOldValue);
        Assert.AreEqual(10, typedNewValue);
        Assert.AreEqual(1, untypedEventCallCount);
    }

    /// <summary>
    /// Tests that Reset with raiseEvent=true does not raise events when the current value
    /// already equals the default value.
    /// </summary>
    [TestMethod]
    public void Reset_WithRaiseEventTrueAndValueUnchanged_DoesNotRaiseEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10) { Value = 10 };
        var typedEventCallCount = 0;
        var untypedEventCallCount = 0;

        parameter.ValueChanged += (_, _) => typedEventCallCount++;
        ((IParameter)parameter).ValueChanged += () => untypedEventCallCount++;

        // Act
        parameter.Reset(raiseEvent: true);

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(0, typedEventCallCount);
        Assert.AreEqual(0, untypedEventCallCount);
    }

    /// <summary>
    /// Tests that Reset with raiseEvent=false does not raise events even when the value changes.
    /// </summary>
    [TestMethod]
    public void Reset_WithRaiseEventFalse_DoesNotRaiseEventsEvenWhenValueChanged()
    {
        // Arrange
        var parameter = new Parameter<int>(10) { Value = 20 };
        var typedEventCallCount = 0;
        var untypedEventCallCount = 0;

        parameter.ValueChanged += (_, _) => typedEventCallCount++;
        ((IParameter)parameter).ValueChanged += () => untypedEventCallCount++;

        // Act
        parameter.Reset(raiseEvent: false);

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(0, typedEventCallCount);
        Assert.AreEqual(0, untypedEventCallCount);
    }

    /// <summary>
    /// Tests that Reset correctly resets the value to DefaultValue for various value types.
    /// </summary>
    [TestMethod]
    [DataRow(0, 100)]
    [DataRow(int.MinValue, 0)]
    [DataRow(int.MaxValue, 0)]
    [DataRow(-500, 500)]
    public void Reset_WithVariousIntegerValues_ResetsToDefaultValue(int defaultValue, int currentValue)
    {
        // Arrange
        var parameter = new Parameter<int>(defaultValue) { Value = currentValue };

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual(defaultValue, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset correctly handles special double values: NaN.
    /// </summary>
    [TestMethod]
    public void Reset_WithDoubleNaNDefaultValue_ResetsToNaN()
    {
        // Arrange
        var parameter = new Parameter<double>(double.NaN) { Value = 1.0 };

        // Act
        parameter.Reset();

        // Assert
        Assert.IsTrue(double.IsNaN(parameter.Value));
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset correctly handles special float values: NaN.
    /// </summary>
    [TestMethod]
    public void Reset_WithFloatNaNDefaultValue_ResetsToNaN()
    {
        // Arrange
        var parameter = new Parameter<float>(float.NaN) { Value = 1.0f };

        // Act
        parameter.Reset();

        // Assert
        Assert.IsTrue(float.IsNaN(parameter.Value));
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset correctly handles boolean values.
    /// </summary>
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void Reset_WithBooleanValues_ResetsToDefaultValue(bool defaultValue, bool currentValue)
    {
        // Arrange
        var parameter = new Parameter<bool>(defaultValue) { Value = currentValue };

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual(defaultValue, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset correctly handles string values including null, empty, and whitespace.
    /// </summary>
    [TestMethod]
    public void Reset_WithNullStringDefaultValue_ResetsToNull()
    {
        // Arrange
        var parameter = new Parameter<string?>(null) { Value = "test" };

        // Act
        parameter.Reset();

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset correctly handles nullable value types with null default value.
    /// </summary>
    [TestMethod]
    public void Reset_WithNullableIntNullDefaultValue_ResetsToNull()
    {
        // Arrange
        var parameter = new Parameter<int?>(null) { Value = 42 };

        // Act
        parameter.Reset();

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset correctly handles nullable value types with non-null default value.
    /// </summary>
    [TestMethod]
    public void Reset_WithNullableIntNonNullDefaultValue_ResetsToValue()
    {
        // Arrange
        var parameter = new Parameter<int?>(10) { Value = 20 };

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset with default parameter (raiseEvent not specified) raises events by default.
    /// </summary>
    [TestMethod]
    public void Reset_WithDefaultParameter_RaisesEventsByDefault()
    {
        // Arrange
        var parameter = new Parameter<int>(10) { Value = 20 };
        var eventCallCount = 0;

        parameter.ValueChanged += (_, _) => eventCallCount++;

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(1, eventCallCount);
    }

    /// <summary>
    /// Tests that Reset raises events with correct old and new values for reference types.
    /// </summary>
    [TestMethod]
    public void Reset_WithReferenceType_RaisesEventsWithCorrectValues()
    {
        // Arrange
        var parameter = new Parameter<string>("default") { Value = "modified" };
        string? capturedOldValue = null;
        string? capturedNewValue = null;

        parameter.ValueChanged += (oldVal, newVal) =>
        {
            capturedOldValue = oldVal;
            capturedNewValue = newVal;
        };

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual("default", parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual("modified", capturedOldValue);
        Assert.AreEqual("default", capturedNewValue);
    }

    /// <summary>
    /// Tests that Reset works correctly when called multiple times in succession.
    /// </summary>
    [TestMethod]
    public void Reset_CalledMultipleTimes_WorksCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(10) { Value = 20 };
        var eventCallCount = 0;

        parameter.ValueChanged += (_, _) => eventCallCount++;

        // Act
        parameter.Reset();
        parameter.Reset();
        parameter.Reset();

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(1, eventCallCount); // Only first reset should raise event
    }

    /// <summary>
    /// Tests that Reset correctly resets value when current value is default(T) but DefaultValue is not.
    /// </summary>
    [TestMethod]
    public void Reset_WithDefaultValueDifferentFromTypeDefault_ResetsCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(100);
        parameter.SetWithoutNotify(0); // Set to default(int) without raising event

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual(100, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset correctly handles parameterless constructor (default DefaultValue).
    /// </summary>
    [TestMethod]
    public void Reset_WithParameterlessConstructor_ResetsToTypeDefault()
    {
        // Arrange
        var parameter = new Parameter<int> { Value = 50 };

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual(0, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that Reset with raiseEvent=false and value unchanged does not raise events.
    /// </summary>
    [TestMethod]
    public void Reset_WithRaiseEventFalseAndValueUnchanged_DoesNotRaiseEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10) { Value = 10 };
        var eventCallCount = 0;

        parameter.ValueChanged += (_, _) => eventCallCount++;

        // Act
        parameter.Reset(raiseEvent: false);

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(parameter.IsModified);
        Assert.AreEqual(0, eventCallCount);
    }

    /// <summary>
    /// Tests that Reset correctly handles negative boundary values for various numeric types.
    /// </summary>
    [TestMethod]
    public void Reset_WithNegativeBoundaryValues_ResetsCorrectly()
    {
        // Arrange
        var intParameter = new Parameter<int>(int.MinValue) { Value = int.MaxValue };
        var doubleParameter = new Parameter<double>(double.MinValue) { Value = double.MaxValue };
        var floatParameter = new Parameter<float>(float.MinValue) { Value = float.MaxValue };

        // Act
        intParameter.Reset();
        doubleParameter.Reset();
        floatParameter.Reset();

        // Assert
        Assert.AreEqual(int.MinValue, intParameter.Value);
        Assert.AreEqual(double.MinValue, doubleParameter.Value);
        Assert.AreEqual(float.MinValue, floatParameter.Value);
        Assert.IsFalse(intParameter.IsModified);
        Assert.IsFalse(doubleParameter.IsModified);
        Assert.IsFalse(floatParameter.IsModified);
    }

    /// <summary>
    /// Tests that both typed and untyped events are invoked in the correct order.
    /// </summary>
    [TestMethod]
    public void Reset_WithBothEventsSubscribed_InvokesBothInCorrectOrder()
    {
        // Arrange
        var parameter = new Parameter<int>(10) { Value = 20 };
        var callOrder = 0;
        var typedCallOrder = 0;
        var untypedCallOrder = 0;

        parameter.ValueChanged += (_, _) => typedCallOrder = ++callOrder;
        ((IParameter)parameter).ValueChanged += () => untypedCallOrder = ++callOrder;

        // Act
        parameter.Reset();

        // Assert
        Assert.AreEqual(1, typedCallOrder);
        Assert.AreEqual(2, untypedCallOrder);
    }

    /// <summary>
    /// Tests that Set updates the value and raises both typed and untyped ValueChanged events when setting a different valid value.
    /// </summary>
    [TestMethod]
    public void Set_DifferentValidValue_UpdatesValueAndRaisesEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var typedEventRaised = false;
        var untypedEventRaised = false;
        var oldValueFromEvent = 0;
        var newValueFromEvent = 0;

        parameter.ValueChanged += (oldVal, newVal) =>
        {
            typedEventRaised = true;
            oldValueFromEvent = oldVal;
            newValueFromEvent = newVal;
        };

        ((IParameter)parameter).ValueChanged += () => untypedEventRaised = true;

        // Act
        parameter.Set(20);

        // Assert
        Assert.AreEqual(20, parameter.Value);
        Assert.IsTrue(typedEventRaised, "Typed ValueChanged event should be raised");
        Assert.IsTrue(untypedEventRaised, "Untyped ValueChanged event should be raised");
        Assert.AreEqual(10, oldValueFromEvent, "Old value in event should be 10");
        Assert.AreEqual(20, newValueFromEvent, "New value in event should be 20");
    }

    /// <summary>
    /// Tests that Set does not update the value or raise events when setting the same value.
    /// </summary>
    [TestMethod]
    public void Set_SameValue_DoesNotUpdateOrRaiseEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var typedEventRaised = false;
        var untypedEventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => typedEventRaised = true;
        ((IParameter)parameter).ValueChanged += () => untypedEventRaised = true;

        // Act
        parameter.Set(10);

        // Assert
        Assert.AreEqual(10, parameter.Value);
        Assert.IsFalse(typedEventRaised, "Typed ValueChanged event should not be raised");
        Assert.IsFalse(untypedEventRaised, "Untyped ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that Set accepts null for nullable reference types when no constraints are defined.
    /// </summary>
    [TestMethod]
    public void Set_NullValueForNullableReferenceType_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        var eventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => eventRaised = true;

        // Act
        parameter.Set(null);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that Set accepts null for nullable value types.
    /// </summary>
    [TestMethod]
    public void Set_NullValueForNullableValueType_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int?>(42);
        var eventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => eventRaised = true;

        // Act
        parameter.Set(null);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that Set works correctly when called multiple times with different values.
    /// </summary>
    [TestMethod]
    public void Set_MultipleSequentialCalls_UpdatesValueEachTime()
    {
        // Arrange
        var parameter = new Parameter<int>(0);
        var eventCount = 0;

        parameter.ValueChanged += (oldVal, newVal) => eventCount++;

        // Act & Assert
        parameter.Set(10);
        Assert.AreEqual(10, parameter.Value);
        Assert.AreEqual(1, eventCount);

        parameter.Set(20);
        Assert.AreEqual(20, parameter.Value);
        Assert.AreEqual(2, eventCount);

        parameter.Set(30);
        Assert.AreEqual(30, parameter.Value);
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that Set correctly updates value when setting to default value of the type.
    /// </summary>
    [TestMethod]
    public void Set_DefaultValue_UpdatesValueIfDifferentFromCurrent()
    {
        // Arrange
        var parameter = new Parameter<int>(42);
        var eventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => eventRaised = true;

        // Act
        parameter.Set(0);

        // Assert
        Assert.AreEqual(0, parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that Set works with double values including extreme values.
    /// </summary>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(-1.5)]
    [DataRow(1.5)]
    [DataRow(double.MaxValue)]
    [DataRow(double.MinValue)]
    public void Set_DoubleValues_UpdatesValue(double newValue)
    {
        // Arrange
        var parameter = new Parameter<double>(1.0);
        var eventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => eventRaised = true;

        // Act
        parameter.Set(newValue);

        // Assert
        Assert.AreEqual(newValue, parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that Set works with special double values (NaN, Infinity).
    /// </summary>
    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void Set_SpecialDoubleValues_UpdatesValue(double newValue)
    {
        // Arrange
        var parameter = new Parameter<double>(0.0);
        var eventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => eventRaised = true;

        // Act
        parameter.Set(newValue);

        // Assert
        Assert.AreEqual(newValue, parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that Set works with extreme integer values.
    /// </summary>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    public void Set_ExtremeIntegerValues_UpdatesValue(int newValue)
    {
        // Arrange
        var parameter = new Parameter<int>(50);
        var eventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => eventRaised = true;

        // Act
        parameter.Set(newValue);

        // Assert
        Assert.AreEqual(newValue, parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that Set works with bool values.
    /// </summary>
    [TestMethod]
    public void Set_BoolValue_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<bool>(false);
        var eventRaised = false;

        parameter.ValueChanged += (oldVal, newVal) => eventRaised = true;

        // Act
        parameter.Set(true);

        // Assert
        Assert.IsTrue(parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that Set allows any value when no Min/Max metadata is defined.
    /// </summary>
    [TestMethod]
    public void Set_NoMetadataConstraints_AcceptsAnyValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50);
        var eventCount = 0;

        parameter.ValueChanged += (oldVal, newVal) => eventCount++;

        // Act & Assert
        parameter.Set(-1000);
        Assert.AreEqual(-1000, parameter.Value);
        Assert.AreEqual(1, eventCount);

        parameter.Set(1000);
        Assert.AreEqual(1000, parameter.Value);
        Assert.AreEqual(2, eventCount);
    }

    /// <summary>
    /// Tests that TrySet returns false and leaves the current value unchanged when validation fails.
    /// </summary>
    [TestMethod]
    public void TrySet_ValueOutsideMax_ReturnsFalseAndLeavesValueUnchanged()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Max = 100 }
        };
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        var result = parameter.TrySet(150);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(50, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that TrySet returns true and updates the value when validation succeeds.
    /// </summary>
    [TestMethod]
    public void TrySet_ValueWithinRange_ReturnsTrueAndUpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        var result = parameter.TrySet(75);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(75, parameter.Value);
        Assert.IsTrue(eventRaised, "ValueChanged event should be raised");
    }

    /// <summary>
    /// Tests that SetOrThrow throws when validation fails and leaves the value unchanged.
    /// </summary>
    [TestMethod]
    public void SetOrThrow_ValueOutsideMin_ThrowsAndLeavesValueUnchanged()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10 }
        };

        // Act
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => parameter.SetOrThrow(5));

        // Assert
        Assert.AreEqual("value", exception.ParamName);
        Assert.AreEqual(50, parameter.Value);
    }

    /// <summary>
    /// Tests that SetOrThrow updates the value when validation succeeds.
    /// </summary>
    [TestMethod]
    public void SetOrThrow_ValueWithinRange_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };

        // Act
        parameter.SetOrThrow(100);

        // Assert
        Assert.AreEqual(100, parameter.Value);
    }

    /// <summary>
    /// Tests that the non-throwing Value setter rejects a missing required value and records the validation error.
    /// </summary>
    [TestMethod]
    public void Value_WhenRequiredValueIsNull_LeavesValueUnchangedAndStoresValidationError()
    {
        // Arrange
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { Required = true }
        };
        var validationState = (IParameterValidationState)parameter;

        // Act
        parameter.Value = null;

        // Assert
        Assert.AreEqual("valid", parameter.Value);
        Assert.IsTrue(validationState.HasValidationError);
        Assert.AreEqual("Value is required.", validationState.ValidationError);
    }

    /// <summary>
    /// Tests that TrySet rejects values shorter than the configured minimum length and records the failure reason.
    /// </summary>
    [TestMethod]
    public void TrySet_ValueShorterThanMinLength_ReturnsFalseAndStoresValidationError()
    {
        // Arrange
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { MinLength = 5 }
        };
        var validationState = (IParameterValidationState)parameter;

        // Act
        var result = parameter.TrySet("abc");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("valid", parameter.Value);
        Assert.IsTrue(validationState.HasValidationError);
        Assert.AreEqual("Value must be at least 5 characters long.", validationState.ValidationError);
    }

    /// <summary>
    /// Tests that SetOrThrow rejects regex mismatches and preserves the specific failure message.
    /// </summary>
    [TestMethod]
    public void SetOrThrow_ValueDoesNotMatchRegex_ThrowsAndStoresValidationError()
    {
        // Arrange
        var parameter = new Parameter<string>("ABC")
        {
            Metadata = new ParameterMetadata
            {
                RegexPattern = "^[A-Z]{3}$",
                RegexMessage = "Use exactly three uppercase letters."
            }
        };
        var validationState = (IParameterValidationState)parameter;

        // Act
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => parameter.SetOrThrow("ab1"));

        // Assert
        Assert.AreEqual("value", exception.ParamName);
        Assert.AreEqual("ABC", parameter.Value);
        Assert.IsTrue(validationState.HasValidationError);
        Assert.AreEqual("Use exactly three uppercase letters.", validationState.ValidationError);
    }

    /// <summary>
    /// Tests that a custom validator can reject a value and report its own failure reason.
    /// </summary>
    [TestMethod]
    public void TrySet_CustomValidatorRejectsValue_ReturnsFalseAndStoresValidationError()
    {
        // Arrange
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { ValidatorType = typeof(RejectBlockedValueValidator) }
        };
        var validationState = (IParameterValidationState)parameter;

        // Act
        var result = parameter.TrySet("blocked");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("valid", parameter.Value);
        Assert.IsTrue(validationState.HasValidationError);
        Assert.AreEqual("The value 'blocked' is reserved.", validationState.ValidationError);
    }

    /// <summary>
    /// Tests that a custom validator instance is reused across repeated validation attempts.
    /// </summary>
    [TestMethod]
    public void TrySet_CustomValidatorRepeatedValidation_CachesValidatorInstance()
    {
        // Arrange
        CountingValidator.Reset();
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { ValidatorType = typeof(CountingValidator) }
        };

        // Act
        var firstResult = parameter.TrySet("first");
        var secondResult = parameter.TrySet("second");

        // Assert
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.AreEqual(1, CountingValidator.InstanceCount);
        Assert.AreEqual(2, CountingValidator.ValidateCallCount);
    }

    /// <summary>
    /// Tests that changing the configured validator type causes the cached validator instance to be recreated.
    /// </summary>
    [TestMethod]
    public void TrySet_WhenValidatorTypeChanges_RecreatesCachedValidator()
    {
        // Arrange
        CountingValidator.Reset();
        AlternateCountingValidator.Reset();
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { ValidatorType = typeof(CountingValidator) }
        };

        var firstResult = parameter.TrySet("first");

        // Act
        parameter.Metadata = new ParameterMetadata { ValidatorType = typeof(AlternateCountingValidator) };
        var secondResult = parameter.TrySet("second");

        // Assert
        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.AreEqual(1, CountingValidator.InstanceCount);
        Assert.AreEqual(1, AlternateCountingValidator.InstanceCount);
    }

    /// <summary>
    /// Tests that a successful set clears a previously recorded validation error.
    /// </summary>
    [TestMethod]
    public void TrySet_WhenValidAfterFailure_ClearsValidationError()
    {
        // Arrange
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { MinLength = 5 }
        };
        var validationState = (IParameterValidationState)parameter;
        var firstResult = parameter.TrySet("abc");
        Assert.IsFalse(firstResult);
        Assert.IsTrue(validationState.HasValidationError);

        // Act
        var secondResult = parameter.TrySet("updated");

        // Assert
        Assert.IsTrue(secondResult);
        Assert.AreEqual("updated", parameter.Value);
        Assert.IsFalse(validationState.HasValidationError);
        Assert.IsNull(validationState.ValidationError);
    }

    /// <summary>
    /// Tests that Reset clears any previously recorded validation error.
    /// </summary>
    [TestMethod]
    public void Reset_AfterValidationFailure_ClearsValidationError()
    {
        // Arrange
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { Required = true }
        };
        var validationState = (IParameterValidationState)parameter;
        parameter.Value = null;
        Assert.IsTrue(validationState.HasValidationError);

        // Act
        parameter.Reset();

        // Assert
        Assert.IsFalse(validationState.HasValidationError);
        Assert.IsNull(validationState.ValidationError);
    }

    /// <summary>
    /// Tests that Set maintains IsModified state correctly when setting different values.
    /// </summary>
    [TestMethod]
    public void Set_DifferentValue_UpdatesIsModifiedState()
    {
        // Arrange
        var parameter = new Parameter<int>(10); // DefaultValue = 10

        // Act & Assert
        Assert.IsFalse(parameter.IsModified, "Should not be modified initially");

        parameter.Set(20);
        Assert.IsTrue(parameter.IsModified, "Should be modified after setting different value");

        parameter.Set(10);
        Assert.IsFalse(parameter.IsModified, "Should not be modified after setting back to default");
    }

    #region GetValue Tests - Int (Non-Nullable Value Type)

    /// <summary>
    /// Verifies that GetValue returns the default value (0) for an int parameter
    /// constructed with the parameterless constructor.
    /// </summary>
    [TestMethod]
    public void GetValue_IntParameterWithDefaultConstructor_ReturnsZero()
    {
        // Arrange
        var parameter = new Parameter<int>();
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Verifies that GetValue returns the correct value for an int parameter
    /// constructed with a specific initial value.
    /// </summary>
    /// <param name="initialValue">The initial value to test.</param>
    /// <param name="expectedValue">The expected return value from GetValue.</param>
    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 1)]
    [DataRow(42, 42)]
    [DataRow(-1, -1)]
    [DataRow(-100, -100)]
    [DataRow(2147483647, 2147483647)] // int.MaxValue
    [DataRow(-2147483648, -2147483648)] // int.MinValue
    public void GetValue_IntParameterWithSpecificValue_ReturnsCorrectValue(int initialValue, int expectedValue)
    {
        // Arrange
        var parameter = new Parameter<int>(initialValue);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedValue, result);
    }

    /// <summary>
    /// Verifies that GetValue returns the new value after the parameter's Value property
    /// has been modified.
    /// </summary>
    [TestMethod]
    [DataRow(0, 100)]
    [DataRow(50, -50)]
    [DataRow(2147483647, -2147483648)] // MaxValue to MinValue
    public void GetValue_IntParameterAfterValueModification_ReturnsNewValue(int initialValue, int newValue)
    {
        // Arrange
        var parameter = new Parameter<int>(initialValue);
        IParameter iParameter = parameter;
        parameter.Value = newValue;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(newValue, result);
    }

    /// <summary>
    /// Verifies that GetValue returns the new value after SetWithoutNotify has been called.
    /// </summary>
    [TestMethod]
    public void GetValue_IntParameterAfterSetWithoutNotify_ReturnsNewValue()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        parameter.SetWithoutNotify(999);

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(999, result);
    }

    #endregion

    #region GetValue Tests - Double (Floating-Point Value Type)

    /// <summary>
    /// Verifies that GetValue returns the correct value for a double parameter
    /// with regular numeric values.
    /// </summary>
    /// <param name="initialValue">The initial value to test.</param>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(1.5)]
    [DataRow(-2.7)]
    [DataRow(3.14159)]
    [DataRow(1.7976931348623157E+308)] // double.MaxValue
    [DataRow(-1.7976931348623157E+308)] // double.MinValue
    public void GetValue_DoubleParameterWithRegularValue_ReturnsCorrectValue(double initialValue)
    {
        // Arrange
        var parameter = new Parameter<double>(initialValue);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(initialValue, result);
    }

    /// <summary>
    /// Verifies that GetValue correctly returns double.NaN.
    /// </summary>
    [TestMethod]
    public void GetValue_DoubleParameterWithNaN_ReturnsNaN()
    {
        // Arrange
        var parameter = new Parameter<double>(double.NaN);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(double.IsNaN((double)result));
    }

    /// <summary>
    /// Verifies that GetValue correctly returns double.PositiveInfinity.
    /// </summary>
    [TestMethod]
    public void GetValue_DoubleParameterWithPositiveInfinity_ReturnsPositiveInfinity()
    {
        // Arrange
        var parameter = new Parameter<double>(double.PositiveInfinity);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(double.PositiveInfinity, result);
        Assert.IsTrue(double.IsPositiveInfinity((double)result));
    }

    /// <summary>
    /// Verifies that GetValue correctly returns double.NegativeInfinity.
    /// </summary>
    [TestMethod]
    public void GetValue_DoubleParameterWithNegativeInfinity_ReturnsNegativeInfinity()
    {
        // Arrange
        var parameter = new Parameter<double>(double.NegativeInfinity);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(double.NegativeInfinity, result);
        Assert.IsTrue(double.IsNegativeInfinity((double)result));
    }

    #endregion

    #region GetValue Tests - Bool (Value Type)

    /// <summary>
    /// Verifies that GetValue returns the correct boolean value.
    /// </summary>
    /// <param name="initialValue">The initial boolean value to test.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void GetValue_BoolParameter_ReturnsCorrectValue(bool initialValue)
    {
        // Arrange
        var parameter = new Parameter<bool>(initialValue);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(initialValue, result);
    }

    #endregion

    #region GetValue Tests - Nullable Int (Nullable Value Type)

    /// <summary>
    /// Verifies that GetValue returns null when a nullable int parameter is set to null.
    /// </summary>
    [TestMethod]
    public void GetValue_NullableIntParameterWithNull_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that GetValue returns the correct value for a nullable int parameter
    /// with a non-null value.
    /// </summary>
    /// <param name="initialValue">The initial nullable int value to test.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(42)]
    [DataRow(-100)]
    [DataRow(2147483647)] // int.MaxValue
    [DataRow(-2147483648)] // int.MinValue
    public void GetValue_NullableIntParameterWithValue_ReturnsCorrectValue(int initialValue)
    {
        // Arrange
        var parameter = new Parameter<int?>(initialValue);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(initialValue, result);
    }

    /// <summary>
    /// Verifies that GetValue returns null after a nullable int parameter is set to null
    /// via SetWithoutNotify.
    /// </summary>
    [TestMethod]
    public void GetValue_NullableIntParameterAfterSetWithoutNotifyToNull_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<int?>(100);
        IParameter iParameter = parameter;
        parameter.SetWithoutNotify(null);

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region GetValue Tests - String (Reference Type)

    /// <summary>
    /// Verifies that GetValue returns null when a string parameter is initialized with null.
    /// </summary>
    [TestMethod]
    public void GetValue_StringParameterWithNull_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<string>(null);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that GetValue returns an empty string when a string parameter is initialized
    /// with an empty string.
    /// </summary>
    [TestMethod]
    public void GetValue_StringParameterWithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var parameter = new Parameter<string>(string.Empty);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that GetValue returns the correct string value for various string inputs.
    /// </summary>
    /// <param name="initialValue">The initial string value to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("Hello")]
    [DataRow("Hello, World!")]
    [DataRow("123")]
    [DataRow("Special!@#$%^&*()_+{}|:\"<>?")]
    public void GetValue_StringParameterWithVariousValues_ReturnsCorrectValue(string initialValue)
    {
        // Arrange
        var parameter = new Parameter<string>(initialValue);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(initialValue, result);
    }

    /// <summary>
    /// Verifies that GetValue returns the correct value for a very long string.
    /// </summary>
    [TestMethod]
    public void GetValue_StringParameterWithVeryLongString_ReturnsCorrectValue()
    {
        // Arrange
        var longString = new string('A', 10000);
        var parameter = new Parameter<string>(longString);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(longString, result);
    }

    /// <summary>
    /// Verifies that GetValue returns null after a string parameter is set to null
    /// via the Value property.
    /// </summary>
    [TestMethod]
    public void GetValue_StringParameterAfterSetToNull_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<string>("InitialValue");
        IParameter iParameter = parameter;
        parameter.Value = null;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region GetValue Tests - Boxing Verification

    /// <summary>
    /// Verifies that GetValue properly boxes value types by returning a reference type
    /// (object) that contains the value.
    /// </summary>
    [TestMethod]
    public void GetValue_ValueTypeParameter_ReturnsBoxedValue()
    {
        // Arrange
        var parameter = new Parameter<int>(42);
        IParameter iParameter = parameter;

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<int>(result);
        Assert.AreEqual(42, (int)result);
    }

    /// <summary>
    /// Verifies that GetValue returns the same reference for reference types
    /// (no defensive copying).
    /// </summary>
    [TestMethod]
    public void GetValue_ReferenceTypeParameter_ReturnsSameReference()
    {
        // Arrange
        var testString = "TestString";
        var parameter = new Parameter<string>(testString);
        IParameter iParameter = parameter;

        // Act
        var result1 = iParameter.GetValue();
        var result2 = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreSame(testString, result1);
        Assert.AreSame(result1, result2);
    }

    #endregion

    #region GetValue Tests - After Reset

    /// <summary>
    /// Verifies that GetValue returns the default value after Reset is called.
    /// </summary>
    [TestMethod]
    public void GetValue_IntParameterAfterReset_ReturnsDefaultValue()
    {
        // Arrange
        var parameter = new Parameter<int>(42);
        IParameter iParameter = parameter;
        parameter.Value = 100;
        parameter.Reset(raiseEvent: false);

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result);
    }

    /// <summary>
    /// Verifies that GetValue returns null after Reset is called on a nullable parameter
    /// initialized with null.
    /// </summary>
    [TestMethod]
    public void GetValue_NullableIntParameterAfterReset_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);
        IParameter iParameter = parameter;
        parameter.Value = 100;
        parameter.Reset(raiseEvent: false);

        // Act
        var result = iParameter.GetValue();

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    /// <summary>
    /// Tests that SetValue with a valid int value successfully updates the parameter and raises events.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_ValidIntValue_SetsValueAndRaisesEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var typedEventRaised = false;
        var untypedEventRaised = false;
        parameter.ValueChanged += (oldVal, newVal) =>
        {
            typedEventRaised = true;
            Assert.AreEqual(10, oldVal);
            Assert.AreEqual(42, newVal);
        };
        iParameter.ValueChanged += () => untypedEventRaised = true;

        // Act
        iParameter.SetValue(42);

        // Assert
        Assert.AreEqual(42, parameter.Value);
        Assert.IsTrue(typedEventRaised);
        Assert.IsTrue(untypedEventRaised);
    }

    /// <summary>
    /// Tests that SetValue with null for a nullable int parameter sets the value to null.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_NullForNullableInt_SetsToNull()
    {
        // Arrange
        var parameter = new Parameter<int?>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        iParameter.SetValue(null);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue with null for a string parameter sets the value to null.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_NullForString_SetsToNull()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        iParameter.SetValue(null);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue with the same value does not raise events.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_SameValue_DoesNotRaiseEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(42);
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        iParameter.SetValue(42);

        // Assert
        Assert.AreEqual(42, parameter.Value);
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue with a value that fails validation does not update the value or raise events.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_ValueFailsValidation_DoesNotSetValueOrRaiseEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 0, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        iParameter.SetValue(150);

        // Assert
        Assert.AreEqual(50, parameter.Value);
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue with a value below the minimum constraint does not update the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_ValueBelowMin_DoesNotSetValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10 }
        };
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(5);

        // Assert
        Assert.AreEqual(50, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with a value above the maximum constraint does not update the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_ValueAboveMax_DoesNotSetValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Max = 100 }
        };
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(150);

        // Assert
        Assert.AreEqual(50, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with a value within the min/max bounds successfully sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_ValueWithinBounds_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 0, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        iParameter.SetValue(75);

        // Assert
        Assert.AreEqual(75, parameter.Value);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue with int.MinValue for an unconstrained parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_IntMinValue_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(0);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(int.MinValue);

        // Assert
        Assert.AreEqual(int.MinValue, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with int.MaxValue for an unconstrained parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_IntMaxValue_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(0);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(int.MaxValue);

        // Assert
        Assert.AreEqual(int.MaxValue, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with an empty string for a string parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_EmptyString_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue("");

        // Assert
        Assert.AreEqual("", parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with a whitespace string for a string parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_WhitespaceString_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue("   ");

        // Assert
        Assert.AreEqual("   ", parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with a very long string for a string parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_VeryLongString_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var longString = new string('x', 10000);

        // Act
        iParameter.SetValue(longString);

        // Assert
        Assert.AreEqual(longString, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with special characters in a string sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_StringWithSpecialCharacters_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var specialString = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~\n\r\t";

        // Act
        iParameter.SetValue(specialString);

        // Assert
        Assert.AreEqual(specialString, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with double.NaN for a double parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_DoubleNaN_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(double.NaN);

        // Assert
        Assert.IsTrue(double.IsNaN(parameter.Value));
    }

    /// <summary>
    /// Tests that SetValue with double.PositiveInfinity for a double parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_DoublePositiveInfinity_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(double.PositiveInfinity);

        // Assert
        Assert.AreEqual(double.PositiveInfinity, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with double.NegativeInfinity for a double parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_DoubleNegativeInfinity_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(double.NegativeInfinity);

        // Assert
        Assert.AreEqual(double.NegativeInfinity, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with zero for a numeric parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_Zero_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(0);

        // Assert
        Assert.AreEqual(0, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with a negative number for a numeric parameter sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_NegativeNumber_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(-42);

        // Assert
        Assert.AreEqual(-42, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with a boxed value type correctly unboxes and sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_BoxedValueType_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        object boxedValue = 99;

        // Act
        iParameter.SetValue(boxedValue);

        // Assert
        Assert.AreEqual(99, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue does not raise events when value equals current value (boundary case at min).
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_SameValueAtMinBoundary_DoesNotRaiseEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(0)
        {
            Metadata = new ParameterMetadata { Min = 0, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        iParameter.SetValue(0);

        // Assert
        Assert.AreEqual(0, parameter.Value);
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue does not raise events when value equals current value (boundary case at max).
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_SameValueAtMaxBoundary_DoesNotRaiseEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(100)
        {
            Metadata = new ParameterMetadata { Min = 0, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        iParameter.SetValue(100);

        // Assert
        Assert.AreEqual(100, parameter.Value);
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue with value at minimum boundary successfully sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_ValueAtMinBoundary_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 0, Max = 100 }
        };
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(0);

        // Assert
        Assert.AreEqual(0, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with value at maximum boundary successfully sets the value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_ValueAtMaxBoundary_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 0, Max = 100 }
        };
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(100);

        // Assert
        Assert.AreEqual(100, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue with null for a reference type parameter from a non-null value raises events.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_NullForReferenceTypeFromNonNull_SetsToNullAndRaisesEvents()
    {
        // Arrange
        var parameter = new Parameter<string>("test");
        IParameter iParameter = parameter;
        var eventRaised = false;
        parameter.ValueChanged += (oldVal, newVal) =>
        {
            eventRaised = true;
            Assert.AreEqual("test", oldVal);
            Assert.IsNull(newVal);
        };

        // Act
        iParameter.SetValue(null);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that SetValue with a bool value on a bool parameter sets the value correctly.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_BoolValue_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<bool>(false);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(true);

        // Assert
        Assert.IsTrue(parameter.Value);
    }

    /// <summary>
    /// Tests that SetValue correctly handles nullable value type with a non-null value.
    /// </summary>
    [TestMethod]
    public void IParameterSetValue_NullableValueTypeWithValue_SetsValue()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);
        IParameter iParameter = parameter;

        // Act
        iParameter.SetValue(42);

        // Assert
        Assert.AreEqual(42, parameter.Value);
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify with a valid value of the correct type assigns the value without raising events.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_ValidValue_AssignsValueWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(10);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(42);

        // Assert
        Assert.AreEqual(42, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify with null assigns null for a nullable reference type without raising events.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_NullValueForNullableReferenceType_AssignsNullWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<string?>("default");
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(null);

        // Assert
        Assert.IsNull(parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify with null assigns null for a nullable value type without raising events.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_NullValueForNullableValueType_AssignsNullWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int?>(10);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(null);

        // Assert
        Assert.IsNull(parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify bypasses min/max validation and assigns values outside the allowed range.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_ValueOutsideMinMaxRange_AssignsValueBypassingValidation()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 0, Max = 100 }
        };

        // Act
        parameter.SetValueWithoutNotify(150);

        // Assert
        Assert.AreEqual(150, parameter.GetValue(), "Value should be assigned even though it exceeds Max");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify with a value below the minimum bypasses validation.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_ValueBelowMinimum_AssignsValueBypassingValidation()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };

        // Act
        parameter.SetValueWithoutNotify(-10);

        // Assert
        Assert.AreEqual(-10, parameter.GetValue(), "Value should be assigned even though it is below Min");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles zero value for numeric types.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_ZeroValue_AssignsZeroWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(10);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(0);

        // Assert
        Assert.AreEqual(0, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles int.MinValue.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_IntMinValue_AssignsMinValueWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(0);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(int.MinValue);

        // Assert
        Assert.AreEqual(int.MinValue, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles int.MaxValue.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_IntMaxValue_AssignsMaxValueWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(0);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(int.MaxValue);

        // Assert
        Assert.AreEqual(int.MaxValue, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles double.NaN.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_DoubleNaN_AssignsNaNWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<double>(0.0);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(double.NaN);

        // Assert
        Assert.IsTrue(double.IsNaN((double)parameter.GetValue()!));
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles double.PositiveInfinity.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_DoublePositiveInfinity_AssignsInfinityWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<double>(0.0);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(double.PositiveInfinity);

        // Assert
        Assert.AreEqual(double.PositiveInfinity, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles double.NegativeInfinity.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_DoubleNegativeInfinity_AssignsInfinityWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<double>(0.0);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(double.NegativeInfinity);

        // Assert
        Assert.AreEqual(double.NegativeInfinity, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles empty string.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_EmptyString_AssignsEmptyStringWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<string>("default");
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles whitespace-only string.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_WhitespaceString_AssignsWhitespaceWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<string>("default");
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify("   ");

        // Assert
        Assert.AreEqual("   ", parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles very long string.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_VeryLongString_AssignsLongStringWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<string>("default");
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;
        var longString = new string('x', 10000);

        // Act
        parameter.SetValueWithoutNotify(longString);

        // Assert
        Assert.AreEqual(longString, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles string with special characters.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_StringWithSpecialCharacters_AssignsStringWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<string>("default");
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;
        var specialString = "Hello\nWorld\t\r\0\u0001";

        // Act
        parameter.SetValueWithoutNotify(specialString);

        // Assert
        Assert.AreEqual(specialString, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify with a bool value works correctly.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_BoolValue_AssignsBoolWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<bool>(false);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(true);

        // Assert
        Assert.IsTrue((bool?)parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify assigns the same value without raising events.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_SameValue_AssignsSameValueWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(42);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(42);

        // Assert
        Assert.AreEqual(42, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised even for same value");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles float values.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_FloatValue_AssignsFloatWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<float>(0.0f);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(3.14f);

        // Assert
        Assert.AreEqual(3.14f, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles negative float values.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_NegativeFloatValue_AssignsNegativeFloatWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<float>(0.0f);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetValueWithoutNotify(-123.456f);

        // Assert
        Assert.AreEqual(-123.456f, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that SetValueWithoutNotify correctly handles struct values.
    /// </summary>
    [TestMethod]
    public void SetValueWithoutNotify_StructValue_AssignsStructWithoutRaisingEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<DateTime>(DateTime.MinValue);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;
        var expectedDate = new DateTime(2024, 1, 1);

        // Act
        parameter.SetValueWithoutNotify(expectedDate);

        // Assert
        Assert.AreEqual(expectedDate, parameter.GetValue());
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that ToString returns null when Value is null for a nullable reference type.
    /// </summary>
    [TestMethod]
    public void ToString_NullableReferenceTypeValueIsNull_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<string?>(null);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that ToString returns null when Value is null for a nullable value type.
    /// </summary>
    [TestMethod]
    public void ToString_NullableValueTypeValueIsNull_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that ToString returns the string representation of an integer value.
    /// </summary>
    /// <param name="value">The integer value to test.</param>
    /// <param name="expected">The expected string representation.</param>
    [DataRow(0, "0")]
    [DataRow(42, "42")]
    [DataRow(-1, "-1")]
    [DataRow(int.MinValue, "-2147483648")]
    [DataRow(int.MaxValue, "2147483647")]
    [TestMethod]
    public void ToString_IntValue_ReturnsStringRepresentation(int value, string expected)
    {
        // Arrange
        var parameter = new Parameter<int>(value);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that ToString returns the string representation of a boolean value.
    /// </summary>
    /// <param name="value">The boolean value to test.</param>
    /// <param name="expected">The expected string representation.</param>
    [DataRow(true, "True")]
    [DataRow(false, "False")]
    [TestMethod]
    public void ToString_BoolValue_ReturnsStringRepresentation(bool value, string expected)
    {
        // Arrange
        var parameter = new Parameter<bool>(value);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that ToString returns the current culture string representation of a double value.
    /// </summary>
    /// <param name="value">The double value to test.</param>
    [DataRow(0.0)]
    [DataRow(3.14159)]
    [DataRow(-2.5)]
    [TestMethod]
    public void ToString_DoubleValue_ReturnsStringRepresentation(double value)
    {
        // Arrange
        var parameter = new Parameter<double>(value);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(value.ToString(), result);
    }

    /// <summary>
    /// Verifies that ToString returns the correct string for double.NaN.
    /// </summary>
    [TestMethod]
    public void ToString_DoubleNaN_ReturnsNaNString()
    {
        // Arrange
        var parameter = new Parameter<double>(double.NaN);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(double.NaN.ToString(), result);
    }

    /// <summary>
    /// Verifies that ToString returns the correct string for double.PositiveInfinity.
    /// </summary>
    [TestMethod]
    public void ToString_DoublePositiveInfinity_ReturnsInfinityString()
    {
        // Arrange
        var parameter = new Parameter<double>(double.PositiveInfinity);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(double.PositiveInfinity.ToString(), result);
    }

    /// <summary>
    /// Verifies that ToString returns the correct string for double.NegativeInfinity.
    /// </summary>
    [TestMethod]
    public void ToString_DoubleNegativeInfinity_ReturnsNegativeInfinityString()
    {
        // Arrange
        var parameter = new Parameter<double>(double.NegativeInfinity);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(double.NegativeInfinity.ToString(), result);
    }

    /// <summary>
    /// Verifies that ToString returns the string value itself.
    /// </summary>
    /// <param name="value">The string value to test.</param>
    [DataRow("")]
    [DataRow("test")]
    [DataRow("  ")]
    [DataRow("Hello, World!")]
    [DataRow("Line1\nLine2")]
    [DataRow("\t\r\n")]
    [TestMethod]
    public void ToString_StringValue_ReturnsStringValue(string value)
    {
        // Arrange
        var parameter = new Parameter<string>(value);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(value, result);
    }

    /// <summary>
    /// Verifies that ToString returns the string representation for a nullable int with a value.
    /// </summary>
    [TestMethod]
    public void ToString_NullableIntWithValue_ReturnsStringRepresentation()
    {
        // Arrange
        var parameter = new Parameter<int?>(123);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual("123", result);
    }

    /// <summary>
    /// Verifies that ToString returns the same result as Value?.ToString() when Value is null.
    /// </summary>
    [TestMethod]
    public void ToString_ConsistentWithValueToStringForNull_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<string?>(null);
        var expected = parameter.Value?.ToString();

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(expected, result);
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that ToString returns the correct representation after value changes.
    /// </summary>
    [TestMethod]
    public void ToString_AfterValueChange_ReturnsNewStringRepresentation()
    {
        // Arrange
        var parameter = new Parameter<int>(10)
        {
            Value = 20
        };

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual("20", result);
    }

    /// <summary>
    /// Verifies that ToString returns the correct representation for boundary values of long.
    /// </summary>
    /// <param name="value">The long value to test.</param>
    /// <param name="expected">The expected string representation.</param>
    [DataRow(long.MinValue, "-9223372036854775808")]
    [DataRow(long.MaxValue, "9223372036854775807")]
    [DataRow(0L, "0")]
    [TestMethod]
    public void ToString_LongBoundaryValues_ReturnsStringRepresentation(long value, string expected)
    {
        // Arrange
        var parameter = new Parameter<long>(value);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that ToString returns the correct representation for very large and very small double values.
    /// </summary>
    /// <param name="value">The double value to test.</param>
    [DataRow(double.MinValue)]
    [DataRow(double.MaxValue)]
    [DataRow(double.Epsilon)]
    [TestMethod]
    public void ToString_DoubleExtremeValues_ReturnsStringRepresentation(double value)
    {
        // Arrange
        var parameter = new Parameter<double>(value);

        // Act
        var result = parameter.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(value.ToString(), result);
    }

    /// <summary>
    /// Verifies that SetWithoutNotify sets the value of an integer parameter without raising events.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_IntValue_SetsValueWithoutRaisingEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var typedEventRaised = false;
        var untypedEventRaised = false;

        parameter.ValueChanged += (_, _) => typedEventRaised = true;
        ((IParameter)parameter).ValueChanged += () => untypedEventRaised = true;

        // Act
        parameter.SetWithoutNotify(42);

        // Assert
        Assert.AreEqual(42, parameter.Value);
        Assert.IsFalse(typedEventRaised, "Typed ValueChanged event should not be raised");
        Assert.IsFalse(untypedEventRaised, "Untyped ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify sets the value of a string parameter without raising events.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_StringValue_SetsValueWithoutRaisingEvents()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        var typedEventRaised = false;
        var untypedEventRaised = false;

        parameter.ValueChanged += (_, _) => typedEventRaised = true;
        ((IParameter)parameter).ValueChanged += () => untypedEventRaised = true;

        // Act
        parameter.SetWithoutNotify("updated");

        // Assert
        Assert.AreEqual("updated", parameter.Value);
        Assert.IsFalse(typedEventRaised, "Typed ValueChanged event should not be raised");
        Assert.IsFalse(untypedEventRaised, "Untyped ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify accepts null for nullable reference type parameter.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_NullStringValue_SetsNullWithoutRaisingEvents()
    {
        // Arrange
        var parameter = new Parameter<string?>("initial");
        var typedEventRaised = false;
        var untypedEventRaised = false;

        parameter.ValueChanged += (_, _) => typedEventRaised = true;
        ((IParameter)parameter).ValueChanged += () => untypedEventRaised = true;

        // Act
        parameter.SetWithoutNotify(null);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsFalse(typedEventRaised, "Typed ValueChanged event should not be raised");
        Assert.IsFalse(untypedEventRaised, "Untyped ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify accepts null for nullable value type parameter.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_NullNullableIntValue_SetsNullWithoutRaisingEvents()
    {
        // Arrange
        var parameter = new Parameter<int?>(10);
        var typedEventRaised = false;
        var untypedEventRaised = false;

        parameter.ValueChanged += (_, _) => typedEventRaised = true;
        ((IParameter)parameter).ValueChanged += () => untypedEventRaised = true;

        // Act
        parameter.SetWithoutNotify(null);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsFalse(typedEventRaised, "Typed ValueChanged event should not be raised");
        Assert.IsFalse(untypedEventRaised, "Untyped ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify bypasses min/max validation and sets a value below the minimum.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_IntValueBelowMin_BypassesValidationAndSetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(5);

        // Assert
        Assert.AreEqual(5, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify bypasses min/max validation and sets a value above the maximum.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_IntValueAboveMax_BypassesValidationAndSetsValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(200);

        // Assert
        Assert.AreEqual(200, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify updates IsModified property when value differs from default.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_ValueDifferentFromDefault_UpdatesIsModified()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        Assert.IsFalse(parameter.IsModified, "Parameter should not be modified initially");

        // Act
        parameter.SetWithoutNotify(20);

        // Assert
        Assert.IsTrue(parameter.IsModified, "Parameter should be marked as modified");
        Assert.AreEqual(20, parameter.Value);
    }

    /// <summary>
    /// Verifies that SetWithoutNotify updates IsModified to false when value is set back to default.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_ValueSameAsDefault_SetsIsModifiedToFalse()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        parameter.SetWithoutNotify(20);
        Assert.IsTrue(parameter.IsModified, "Parameter should be modified initially");

        // Act
        parameter.SetWithoutNotify(10);

        // Assert
        Assert.IsFalse(parameter.IsModified, "Parameter should not be marked as modified");
        Assert.AreEqual(10, parameter.Value);
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles int.MinValue correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_IntMinValue_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(0);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(int.MinValue);

        // Assert
        Assert.AreEqual(int.MinValue, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles int.MaxValue correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_IntMaxValue_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(0);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(int.MaxValue);

        // Assert
        Assert.AreEqual(int.MaxValue, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles empty string correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_EmptyString_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles whitespace-only string correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_WhitespaceString_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify("   ");

        // Assert
        Assert.AreEqual("   ", parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles very long string correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_VeryLongString_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        var longString = new string('x', 10000);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(longString);

        // Assert
        Assert.AreEqual(longString, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles string with special characters correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_StringWithSpecialCharacters_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        var specialString = "Line1\nLine2\tTabbed\r\nNewLine\0Null";
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(specialString);

        // Assert
        Assert.AreEqual(specialString, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles double.NaN correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_DoubleNaN_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(double.NaN);

        // Assert
        Assert.IsTrue(double.IsNaN(parameter.Value));
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles double.PositiveInfinity correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_DoublePositiveInfinity_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(double.PositiveInfinity);

        // Assert
        Assert.AreEqual(double.PositiveInfinity, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles double.NegativeInfinity correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_DoubleNegativeInfinity_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(double.NegativeInfinity);

        // Assert
        Assert.AreEqual(double.NegativeInfinity, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles zero correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_Zero_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(0);

        // Assert
        Assert.AreEqual(0, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify handles negative values correctly.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_NegativeValue_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(-100);

        // Assert
        Assert.AreEqual(-100, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify does not raise events even when setting same value multiple times.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_SameValueMultipleTimes_NeverRaisesEvents()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var eventRaisedCount = 0;
        parameter.ValueChanged += (_, _) => eventRaisedCount++;

        // Act
        parameter.SetWithoutNotify(20);
        parameter.SetWithoutNotify(20);
        parameter.SetWithoutNotify(20);

        // Assert
        Assert.AreEqual(20, parameter.Value);
        Assert.AreEqual(0, eventRaisedCount, "No events should be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify can be called after normal Set method without interference.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_AfterNormalSet_WorksCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var eventRaisedCount = 0;
        parameter.ValueChanged += (_, _) => eventRaisedCount++;

        parameter.Set(20);
        Assert.AreEqual(1, eventRaisedCount, "Event should be raised by Set");

        // Act
        parameter.SetWithoutNotify(30);

        // Assert
        Assert.AreEqual(30, parameter.Value);
        Assert.AreEqual(1, eventRaisedCount, "No additional event should be raised by SetWithoutNotify");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify works with default value.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_DefaultValueForInt_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(default);

        // Assert
        Assert.AreEqual(0, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify works with default value for reference type.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_DefaultValueForString_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<string?>("initial");
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(default);

        // Assert
        Assert.IsNull(parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify works with bool type.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_BoolValue_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<bool>(false);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(true);

        // Assert
        Assert.IsTrue(parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify works with float type including boundary values.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_FloatValue_SetsValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<float>(0.0f);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(3.14159f);

        // Assert
        Assert.AreEqual(3.14159f, parameter.Value, 0.00001f);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Verifies that SetWithoutNotify can set value to the same value as current without any side effects.
    /// </summary>
    [TestMethod]
    public void SetWithoutNotify_SameValueAsCurrent_NoSideEffects()
    {
        // Arrange
        var parameter = new Parameter<int>(42);
        var eventRaised = false;
        parameter.ValueChanged += (_, _) => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(42);

        // Assert
        Assert.AreEqual(42, parameter.Value);
        Assert.IsFalse(eventRaised, "ValueChanged event should not be raised");
    }

    /// <summary>
    /// Tests that getting the Value property returns the initial default value when constructed with no arguments.
    /// </summary>
    [TestMethod]
    public void Value_GetWithDefaultConstructor_ReturnsDefault()
    {
        // Arrange
        var parameter = new Parameter<int>();

        // Act
        var result = parameter.Value;

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Tests that getting the Value property returns the specified default value when constructed with a value.
    /// </summary>
    [TestMethod]
    public void Value_GetWithConstructorValue_ReturnsConstructorValue()
    {
        // Arrange
        var parameter = new Parameter<int>(42);

        // Act
        var result = parameter.Value;

        // Assert
        Assert.AreEqual(42, result);
    }

    /// <summary>
    /// Tests that getting the Value property returns null when initialized with null for nullable type.
    /// </summary>
    [TestMethod]
    public void Value_GetWithNullForNullableType_ReturnsNull()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);

        // Act
        var result = parameter.Value;

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that setting the Value property to a different value updates the internal field.
    /// </summary>
    [TestMethod]
    public void Value_SetDifferentValue_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(10)
        {
            // Act
            Value = 20
        };

        // Assert
        Assert.AreEqual(20, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to the same value does not change the stored value.
    /// </summary>
    [TestMethod]
    public void Value_SetSameValue_DoesNotChangeValue()
    {
        // Arrange
        var parameter = new Parameter<int>(42)
        {
            // Act
            Value = 42
        };

        // Assert
        Assert.AreEqual(42, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a different value raises the ValueChanged event with correct old and new values.
    /// </summary>
    [TestMethod]
    public void Value_SetDifferentValue_RaisesValueChangedEvent()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        int? capturedOld = null;
        int? capturedNew = null;
        parameter.ValueChanged += (old, newVal) =>
        {
            capturedOld = old;
            capturedNew = newVal;
        };

        // Act
        parameter.Value = 20;

        // Assert
        Assert.AreEqual(10, capturedOld);
        Assert.AreEqual(20, capturedNew);
    }

    /// <summary>
    /// Tests that setting the Value property to the same value does not raise the ValueChanged event.
    /// </summary>
    [TestMethod]
    public void Value_SetSameValue_DoesNotRaiseValueChangedEvent()
    {
        // Arrange
        var parameter = new Parameter<int>(42);
        var eventRaised = false;
        parameter.ValueChanged += (old, newVal) => eventRaised = true;

        // Act
        parameter.Value = 42;

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that setting the Value property to a different value raises the interface ValueChanged event.
    /// </summary>
    [TestMethod]
    public void Value_SetDifferentValue_RaisesInterfaceValueChangedEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(10);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        ((Parameter<int>)parameter).Value = 20;

        // Assert
        Assert.IsTrue(eventRaised);
    }

    /// <summary>
    /// Tests that setting the Value property to the same value does not raise the interface ValueChanged event.
    /// </summary>
    [TestMethod]
    public void Value_SetSameValue_DoesNotRaiseInterfaceValueChangedEvent()
    {
        // Arrange
        IParameter parameter = new Parameter<int>(42);
        var eventRaised = false;
        parameter.ValueChanged += () => eventRaised = true;

        // Act
        ((Parameter<int>)parameter).Value = 42;

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that setting the Value property to a value below the minimum constraint does not update the value.
    /// </summary>
    [TestMethod]
    public void Value_SetValueBelowMin_DoesNotUpdateValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10 },
            // Act
            Value = 5
        };

        // Assert
        Assert.AreEqual(50, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a value above the maximum constraint does not update the value.
    /// </summary>
    [TestMethod]
    public void Value_SetValueAboveMax_DoesNotUpdateValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Max = 100 },
            // Act
            Value = 150
        };

        // Assert
        Assert.AreEqual(50, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a value equal to the minimum constraint updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetValueEqualToMin_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10 },
            // Act
            Value = 10
        };

        // Assert
        Assert.AreEqual(10, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a value equal to the maximum constraint updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetValueEqualToMax_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Max = 100 },
            // Act
            Value = 100
        };

        // Assert
        Assert.AreEqual(100, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a value within the min/max range updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetValueWithinMinMaxRange_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 },
            // Act
            Value = 75
        };

        // Assert
        Assert.AreEqual(75, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a value that fails validation does not raise the ValueChanged event.
    /// </summary>
    [TestMethod]
    public void Value_SetInvalidValue_DoesNotRaiseValueChangedEvent()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        var eventRaised = false;
        parameter.ValueChanged += (old, newVal) => eventRaised = true;

        // Act
        parameter.Value = 150;

        // Assert
        Assert.IsFalse(eventRaised);
    }

    /// <summary>
    /// Tests that setting the Value property to int.MinValue updates the value when no constraints are set.
    /// </summary>
    [TestMethod]
    public void Value_SetIntMinValue_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(0)
        {
            // Act
            Value = int.MinValue
        };

        // Assert
        Assert.AreEqual(int.MinValue, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to int.MaxValue updates the value when no constraints are set.
    /// </summary>
    [TestMethod]
    public void Value_SetIntMaxValue_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(0)
        {
            // Act
            Value = int.MaxValue
        };

        // Assert
        Assert.AreEqual(int.MaxValue, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to zero updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetZero_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(10)
        {
            // Act
            Value = 0
        };

        // Assert
        Assert.AreEqual(0, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to double.NaN updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetDoubleNaN_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0)
        {
            // Act
            Value = double.NaN
        };

        // Assert
        Assert.IsTrue(double.IsNaN(parameter.Value));
    }

    /// <summary>
    /// Tests that setting the Value property to double.PositiveInfinity updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetDoublePositiveInfinity_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0)
        {
            // Act
            Value = double.PositiveInfinity
        };

        // Assert
        Assert.AreEqual(double.PositiveInfinity, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to double.NegativeInfinity updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetDoubleNegativeInfinity_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0)
        {
            // Act
            Value = double.NegativeInfinity
        };

        // Assert
        Assert.AreEqual(double.NegativeInfinity, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to null for a nullable int updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetNullForNullableInt_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int?>(42)
        {
            // Act
            Value = null
        };

        // Assert
        Assert.IsNull(parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to null for a nullable string updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetNullForNullableString_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<string?>("test")
        {
            // Act
            Value = null
        };

        // Assert
        Assert.IsNull(parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to an empty string updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetEmptyString_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<string>("test")
        {
            // Act
            Value = ""
        };

        // Assert
        Assert.AreEqual("", parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a whitespace string updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetWhitespaceString_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<string>("test")
        {
            // Act
            Value = "   "
        };

        // Assert
        Assert.AreEqual("   ", parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a very long string updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetVeryLongString_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<string>("test");
        var longString = new string('x', 10000);

        // Act
        parameter.Value = longString;

        // Assert
        Assert.AreEqual(longString, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a string with special characters updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetStringWithSpecialCharacters_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<string>("test");
        var specialString = "test\n\r\t\0\u0001";

        // Act
        parameter.Value = specialString;

        // Assert
        Assert.AreEqual(specialString, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to null when validation constraints are present still bypasses validation.
    /// </summary>
    [TestMethod]
    public void Value_SetNullWithValidationConstraints_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int?>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 },
            // Act
            Value = null
        };

        // Assert
        Assert.IsNull(parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a negative integer updates the value when no constraints are set.
    /// </summary>
    [TestMethod]
    public void Value_SetNegativeInteger_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<int>(10)
        {
            // Act
            Value = -100
        };

        // Assert
        Assert.AreEqual(-100, parameter.Value);
    }

    /// <summary>
    /// Tests that multiple subscribers to the ValueChanged event all receive notifications.
    /// </summary>
    [TestMethod]
    public void Value_SetDifferentValueWithMultipleSubscribers_RaisesEventForAll()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        var subscriber1Calls = 0;
        var subscriber2Calls = 0;
        parameter.ValueChanged += (old, newVal) => subscriber1Calls++;
        parameter.ValueChanged += (old, newVal) => subscriber2Calls++;

        // Act
        parameter.Value = 20;

        // Assert
        Assert.AreEqual(1, subscriber1Calls);
        Assert.AreEqual(1, subscriber2Calls);
    }

    /// <summary>
    /// Tests that setting the Value property to a different value updates IsModified correctly.
    /// </summary>
    [TestMethod]
    public void Value_SetDifferentValueFromDefault_UpdatesIsModified()
    {
        // Arrange
        var parameter = new Parameter<int>(10)
        {
            // Act
            Value = 20
        };

        // Assert
        Assert.IsTrue(parameter.IsModified);
    }

    /// <summary>
    /// Tests that setting the Value property back to the default value updates IsModified to false.
    /// </summary>
    [TestMethod]
    public void Value_SetBackToDefaultValue_UpdatesIsModifiedToFalse()
    {
        // Arrange
        var parameter = new Parameter<int>(10)
        {
            Value = 20
        };

        // Act
        parameter.Value = 10;

        // Assert
        Assert.IsFalse(parameter.IsModified);
    }

    /// <summary>
    /// Tests that setting the Value property with float.NaN updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetFloatNaN_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<float>(0.0f)
        {
            // Act
            Value = float.NaN
        };

        // Assert
        Assert.IsTrue(float.IsNaN(parameter.Value));
    }

    /// <summary>
    /// Tests that setting the Value property with float.PositiveInfinity updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetFloatPositiveInfinity_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<float>(0.0f)
        {
            // Act
            Value = float.PositiveInfinity
        };

        // Assert
        Assert.AreEqual(float.PositiveInfinity, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property with float.NegativeInfinity updates the value.
    /// </summary>
    [TestMethod]
    public void Value_SetFloatNegativeInfinity_UpdatesValue()
    {
        // Arrange
        var parameter = new Parameter<float>(0.0f)
        {
            // Act
            Value = float.NegativeInfinity
        };

        // Assert
        Assert.AreEqual(float.NegativeInfinity, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property to a boundary value at Min raises the event correctly.
    /// </summary>
    [TestMethod]
    public void Value_SetBoundaryValueAtMin_RaisesEventWithCorrectValues()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        int? capturedOld = null;
        int? capturedNew = null;
        parameter.ValueChanged += (old, newVal) =>
        {
            capturedOld = old;
            capturedNew = newVal;
        };

        // Act
        parameter.Value = 10;

        // Assert
        Assert.AreEqual(50, capturedOld);
        Assert.AreEqual(10, capturedNew);
    }

    /// <summary>
    /// Tests that setting the Value property to a boundary value at Max raises the event correctly.
    /// </summary>
    [TestMethod]
    public void Value_SetBoundaryValueAtMax_RaisesEventWithCorrectValues()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        int? capturedOld = null;
        int? capturedNew = null;
        parameter.ValueChanged += (old, newVal) =>
        {
            capturedOld = old;
            capturedNew = newVal;
        };

        // Act
        parameter.Value = 100;

        // Assert
        Assert.AreEqual(50, capturedOld);
        Assert.AreEqual(100, capturedNew);
    }

    /// <summary>
    /// Tests that setting the Value property multiple times in succession works correctly.
    /// </summary>
    [TestMethod]
    public void Value_SetMultipleTimes_UpdatesValueCorrectly()
    {
        // Arrange
        var parameter = new Parameter<int>(0)
        {
            // Act
            Value = 10
        };
        parameter.Value = 20;
        parameter.Value = 30;

        // Assert
        Assert.AreEqual(30, parameter.Value);
    }

    /// <summary>
    /// Tests that setting the Value property multiple times raises the event the correct number of times.
    /// </summary>
    [TestMethod]
    public void Value_SetMultipleTimes_RaisesEventCorrectNumberOfTimes()
    {
        // Arrange
        var parameter = new Parameter<int>(0);
        var eventCount = 0;
        parameter.ValueChanged += (old, newVal) => eventCount++;

        // Act
        parameter.Value = 10;
        parameter.Value = 20;
        parameter.Value = 30;

        // Assert
        Assert.AreEqual(3, eventCount);
    }

    /// <summary>
    /// Tests that the ValueType property returns the correct type for various generic type parameters,
    /// including value types, reference types, nullable types, arrays, enums, and custom types.
    /// </summary>
    /// <param name="expectedType">The expected type that should be returned by the ValueType property.</param>
    [TestMethod]
    [DataRow(typeof(int), DisplayName = "Int")]
    [DataRow(typeof(string), DisplayName = "String")]
    [DataRow(typeof(bool), DisplayName = "Bool")]
    [DataRow(typeof(double), DisplayName = "Double")]
    [DataRow(typeof(float), DisplayName = "Float")]
    [DataRow(typeof(long), DisplayName = "Long")]
    [DataRow(typeof(decimal), DisplayName = "Decimal")]
    [DataRow(typeof(byte), DisplayName = "Byte")]
    [DataRow(typeof(short), DisplayName = "Short")]
    [DataRow(typeof(object), DisplayName = "Object")]
    [DataRow(typeof(int?), DisplayName = "NullableInt")]
    [DataRow(typeof(double?), DisplayName = "NullableDouble")]
    [DataRow(typeof(bool?), DisplayName = "NullableBool")]
    [DataRow(typeof(decimal?), DisplayName = "NullableDecimal")]
    [DataRow(typeof(int[]), DisplayName = "IntArray")]
    [DataRow(typeof(string[]), DisplayName = "StringArray")]
    [DataRow(typeof(DayOfWeek), DisplayName = "Enum")]
    [DataRow(typeof(DayOfWeek?), DisplayName = "NullableEnum")]
    [DataRow(typeof(ParameterMetadata), DisplayName = "CustomClass")]
    public void ValueType_VariousTypeParameters_ReturnsCorrectType(Type expectedType)
    {
        // Arrange
        var parameterType = typeof(Parameter<>).MakeGenericType(expectedType);
        var parameter = Activator.CreateInstance(parameterType);
        var valueTypeProperty = parameterType.GetProperty("ValueType");

        // Act
        var actualType = valueTypeProperty?.GetValue(parameter) as Type;

        // Assert
        Assert.IsNotNull(actualType, "ValueType should never be null");
        Assert.AreEqual(expectedType, actualType, $"ValueType should return {expectedType.FullName}");
    }

    /// <summary>
    /// Tests that the ValueType property returns a consistent value across multiple accesses,
    /// verifying that the property getter is deterministic and returns the same Type instance.
    /// </summary>
    [TestMethod]
    public void ValueType_MultipleAccesses_ReturnsSameTypeInstance()
    {
        // Arrange
        var parameter = new Parameter<int>();

        // Act
        var type1 = parameter.ValueType;
        var type2 = parameter.ValueType;
        var type3 = parameter.ValueType;

        // Assert
        Assert.AreSame(type1, type2, "Multiple accesses should return the same Type instance");
        Assert.AreSame(type2, type3, "Multiple accesses should return the same Type instance");
        Assert.AreEqual(typeof(int), type1, "ValueType should return typeof(int)");
    }

    /// <summary>
    /// Tests that the ValueType property never returns null for any valid type parameter,
    /// including reference types that can themselves be null.
    /// </summary>
    [TestMethod]
    public void ValueType_ReferenceType_ReturnsNonNullType()
    {
        // Arrange
        var parameter = new Parameter<string?>();

        // Act
        var actualType = parameter.ValueType;

        // Assert
        Assert.IsNotNull(actualType, "ValueType should never return null even for nullable reference types");
        Assert.AreEqual(typeof(string), actualType, "ValueType should return typeof(string)");
    }

    /// <summary>
    /// Tests that the ValueType property correctly reflects the generic type argument
    /// for complex nested generic types.
    /// </summary>
    [TestMethod]
    public void ValueType_NestedGenericType_ReturnsCorrectType()
    {
        // Arrange
        var parameter = new Parameter<System.Collections.Generic.List<int>>();

        // Act
        var actualType = parameter.ValueType;

        // Assert
        Assert.IsNotNull(actualType, "ValueType should never be null");
        Assert.AreEqual(typeof(System.Collections.Generic.List<int>), actualType);
        Assert.IsTrue(actualType.IsGenericType, "Type should be recognized as a generic type");
    }

    /// <summary>
    /// Tests that the ValueType property correctly returns Type information
    /// for multidimensional array types.
    /// </summary>
    [TestMethod]
    public void ValueType_MultidimensionalArrayType_ReturnsCorrectType()
    {
        // Arrange
        var expectedType = typeof(int[,]);
        var parameterType = typeof(Parameter<>).MakeGenericType(expectedType);
        var parameter = Activator.CreateInstance(parameterType);
        var valueTypeProperty = parameterType.GetProperty("ValueType");

        // Act
        var actualType = valueTypeProperty?.GetValue(parameter) as Type;

        // Assert
        Assert.IsNotNull(actualType, "ValueType should never be null");
        Assert.AreEqual(expectedType, actualType);
        Assert.IsTrue(actualType.IsArray, "Type should be recognized as an array");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised when the value
    /// is changed via the strongly-typed <see cref="Parameter{T}.Value"/> property.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_ValuePropertyChanged_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = 20;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when Value property changes.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised when the value
    /// is changed via the <see cref="Parameter{T}.Set"/> method.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_SetMethodCalled_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Set("modified");

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when Set method is called.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised when the value
    /// is changed via the <see cref="IParameter.SetValue"/> method.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_IParameterSetValueCalled_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<double>(1.5);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        iParameter.SetValue(3.5);

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when IParameter.SetValue is called.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised when
    /// <see cref="IParameter.Reset"/> is called with <c>raiseEvent = true</c> and
    /// the value actually changes.
    /// </summary>
    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
    public void IParameterValueChanged_ResetWithRaiseEventTrue_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        parameter.Value = 20;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        iParameter.Reset(raiseEvent: true);

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when Reset(true) changes the value.");
        Assert.AreEqual(10, parameter.Value, "Value should be reset to default.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is NOT raised when
    /// <see cref="IParameter.Reset"/> is called with <c>raiseEvent = false</c>.
    /// </summary>
    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
    public void IParameterValueChanged_ResetWithRaiseEventFalse_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        parameter.Value = 20;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        iParameter.Reset(raiseEvent: false);

        // Assert
        Assert.IsFalse(eventRaised, "IParameter.ValueChanged should NOT be raised when Reset(false) is called.");
        Assert.AreEqual(10, parameter.Value, "Value should be reset to default.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is NOT raised when
    /// <see cref="IParameter.Reset"/> is called but the value does not actually change.
    /// </summary>
    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
    public void IParameterValueChanged_ResetWithNoChange_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        iParameter.Reset(raiseEvent: true);

        // Assert
        Assert.IsFalse(eventRaised, "IParameter.ValueChanged should NOT be raised when Reset does not change the value.");
        Assert.AreEqual(10, parameter.Value, "Value should remain at default.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is NOT raised when the
    /// value is set to the same value (no actual change).
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_SameValueSet_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = 10;

        // Assert
        Assert.IsFalse(eventRaised, "IParameter.ValueChanged should NOT be raised when the value is set to the same value.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is NOT raised when the
    /// value is changed via <see cref="Parameter{T}.SetWithoutNotify"/>.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_SetWithoutNotifyCalled_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.SetWithoutNotify(20);

        // Assert
        Assert.IsFalse(eventRaised, "IParameter.ValueChanged should NOT be raised when SetWithoutNotify is called.");
        Assert.AreEqual(20, parameter.Value, "Value should be changed silently.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is NOT raised when the
    /// value is changed via <see cref="IParameter.SetValueWithoutNotify"/>.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_IParameterSetValueWithoutNotifyCalled_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        iParameter.SetValueWithoutNotify("modified");

        // Assert
        Assert.IsFalse(eventRaised, "IParameter.ValueChanged should NOT be raised when IParameter.SetValueWithoutNotify is called.");
        Assert.AreEqual("modified", parameter.Value, "Value should be changed silently.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is NOT raised when
    /// validation fails due to value being below the minimum bound.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_ValidationFailsBelowMin_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = 5;

        // Assert
        Assert.IsFalse(eventRaised, "IParameter.ValueChanged should NOT be raised when validation fails (below min).");
        Assert.AreEqual(50, parameter.Value, "Value should remain unchanged when validation fails.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is NOT raised when
    /// validation fails due to value being above the maximum bound.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_ValidationFailsAboveMax_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = 150;

        // Assert
        Assert.IsFalse(eventRaised, "IParameter.ValueChanged should NOT be raised when validation fails (above max).");
        Assert.AreEqual(50, parameter.Value, "Value should remain unchanged when validation fails.");
    }

    /// <summary>
    /// Tests that multiple subscribers to <see cref="IParameter.ValueChanged"/> are all
    /// notified when the value changes.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_MultipleSubscribers_AllNotified()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var firstEventRaised = false;
        var secondEventRaised = false;
        var thirdEventRaised = false;

        iParameter.ValueChanged += () => firstEventRaised = true;
        iParameter.ValueChanged += () => secondEventRaised = true;
        iParameter.ValueChanged += () => thirdEventRaised = true;

        // Act
        parameter.Value = 20;

        // Assert
        Assert.IsTrue(firstEventRaised, "First subscriber should be notified.");
        Assert.IsTrue(secondEventRaised, "Second subscriber should be notified.");
        Assert.IsTrue(thirdEventRaised, "Third subscriber should be notified.");
    }

    /// <summary>
    /// Tests that a handler unsubscribed from <see cref="IParameter.ValueChanged"/>
    /// is no longer invoked when the value changes.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_HandlerUnsubscribed_EventNotRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;
        void handler() => eventRaised = true;

        iParameter.ValueChanged += handler;
        iParameter.ValueChanged -= handler;

        // Act
        parameter.Value = 20;

        // Assert
        Assert.IsFalse(eventRaised, "Unsubscribed handler should NOT be invoked.");
    }

    /// <summary>
    /// Tests that unsubscribing a handler that was subscribed multiple times removes
    /// only one subscription.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_HandlerSubscribedMultipleTimes_UnsubscribeRemovesOne()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var callCount = 0;
        void handler() => callCount++;

        iParameter.ValueChanged += handler;
        iParameter.ValueChanged += handler;
        iParameter.ValueChanged -= handler;

        // Act
        parameter.Value = 20;

        // Assert
        Assert.AreEqual(1, callCount, "Handler should be invoked once after unsubscribing one of two subscriptions.");
    }

    /// <summary>
    /// Tests that unsubscribing a handler that was never subscribed does not cause
    /// any errors or side effects.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_UnsubscribeNeverSubscribedHandler_NoError()
    {
        // Arrange
        var parameter = new Parameter<int>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;
        void handler() => eventRaised = true;
        void neverSubscribedHandler() { }

        iParameter.ValueChanged += handler;

        // Act
        iParameter.ValueChanged -= neverSubscribedHandler;
        parameter.Value = 20;

        // Assert
        Assert.IsTrue(eventRaised, "Subscribed handler should still be invoked.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works correctly
    /// with nullable reference types (e.g., <c>string?</c>).
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_NullableReferenceType_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<string?>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = null;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when nullable reference type is set to null.");
        Assert.IsNull(parameter.Value, "Value should be null.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works correctly
    /// with nullable value types (e.g., <c>int?</c>).
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_NullableValueType_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<int?>(10);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = null;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when nullable value type is set to null.");
        Assert.IsNull(parameter.Value, "Value should be null.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised when
    /// a nullable value type is changed from null to a value.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_NullableValueTypeFromNullToValue_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<int?>(null);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = 42;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when nullable value type changes from null to value.");
        Assert.AreEqual(42, parameter.Value, "Value should be 42.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works with
    /// <see cref="bool"/> parameters.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_BoolParameter_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<bool>(false);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = true;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when bool parameter changes.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works with
    /// <see cref="double"/> parameters, including special values.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, 1.0, DisplayName = "Zero to positive")]
    [DataRow(1.0, -1.0, DisplayName = "Positive to negative")]
    [DataRow(0.0, double.MaxValue, DisplayName = "Zero to MaxValue")]
    [DataRow(0.0, double.MinValue, DisplayName = "Zero to MinValue")]
    [DataRow(0.0, double.PositiveInfinity, DisplayName = "Zero to PositiveInfinity")]
    [DataRow(0.0, double.NegativeInfinity, DisplayName = "Zero to NegativeInfinity")]
    [DataRow(0.0, double.NaN, DisplayName = "Zero to NaN")]
    public void IParameterValueChanged_DoubleParameter_EventRaised(double initialValue, double newValue)
    {
        // Arrange
        var parameter = new Parameter<double>(initialValue);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = newValue;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when double parameter changes.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised when the value
    /// is changed via <see cref="IParameter.SetValue"/> with a valid boundary value at Min.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_SetValueAtMinBoundary_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        iParameter.SetValue(10);

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when value is set to Min boundary.");
        Assert.AreEqual(10, parameter.Value, "Value should be set to Min boundary.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised when the value
    /// is changed via <see cref="IParameter.SetValue"/> with a valid boundary value at Max.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_SetValueAtMaxBoundary_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<int>(50)
        {
            Metadata = new ParameterMetadata { Min = 10, Max = 100 }
        };
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        iParameter.SetValue(100);

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when value is set to Max boundary.");
        Assert.AreEqual(100, parameter.Value, "Value should be set to Max boundary.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works correctly with
    /// empty string values.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_EmptyString_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = "";

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when value is set to empty string.");
        Assert.AreEqual("", parameter.Value, "Value should be empty string.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works correctly with
    /// whitespace-only string values.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_WhitespaceString_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = "   ";

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when value is set to whitespace string.");
        Assert.AreEqual("   ", parameter.Value, "Value should be whitespace string.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works correctly with
    /// very long string values.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_VeryLongString_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;
        var longString = new string('x', 10000);

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = longString;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when value is set to very long string.");
        Assert.AreEqual(longString, parameter.Value, "Value should be the long string.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event works correctly with
    /// string values containing special characters.
    /// </summary>
    [TestMethod]
    public void IParameterValueChanged_StringWithSpecialCharacters_EventRaised()
    {
        // Arrange
        var parameter = new Parameter<string>("initial");
        IParameter iParameter = parameter;
        var eventRaised = false;
        var specialString = "Test\n\r\t\0\u0001Special©™";

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = specialString;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised when value is set to string with special characters.");
        Assert.AreEqual(specialString, parameter.Value, "Value should be the special string.");
    }

    /// <summary>
    /// Tests that the <see cref="IParameter.ValueChanged"/> event is raised with extreme
    /// integer values.
    /// </summary>
    [TestMethod]
    [DataRow(0, int.MinValue, DisplayName = "Zero to MinValue")]
    [DataRow(0, int.MaxValue, DisplayName = "Zero to MaxValue")]
    [DataRow(int.MinValue, int.MaxValue, DisplayName = "MinValue to MaxValue")]
    [DataRow(-1, 0, DisplayName = "Negative to zero")]
    public void IParameterValueChanged_ExtremeIntValues_EventRaised(int initialValue, int newValue)
    {
        // Arrange
        var parameter = new Parameter<int>(initialValue);
        IParameter iParameter = parameter;
        var eventRaised = false;

        iParameter.ValueChanged += () => eventRaised = true;

        // Act
        parameter.Value = newValue;

        // Assert
        Assert.IsTrue(eventRaised, "IParameter.ValueChanged should be raised for extreme int values.");
        Assert.AreEqual(newValue, parameter.Value, "Value should be the new extreme value.");
    }

}
