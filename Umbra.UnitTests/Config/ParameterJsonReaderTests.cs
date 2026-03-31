using System.Text.Json;
using Moq;


namespace Umbra.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterJsonReader.Apply"/>.
/// </summary>
[TestClass]
public partial class ParameterJsonReaderTests
{
    /// <summary>
    /// Tests that Apply correctly applies a valid integer value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidIntegerNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("42");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(42), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid double value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidDoubleNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(double));
        using var doc = JsonDocument.Parse("3.14");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(3.14), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid float value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidFloatNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(float));
        using var doc = JsonDocument.Parse("2.5");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(2.5f), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid long value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidLongNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(long));
        using var doc = JsonDocument.Parse("9223372036854775807");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(9223372036854775807L), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid uint value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidUIntNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(uint));
        using var doc = JsonDocument.Parse("100");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify((uint)100), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid short value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidShortNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(short));
        using var doc = JsonDocument.Parse("32767");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify((short)32767), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid byte value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidByteNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(byte));
        using var doc = JsonDocument.Parse("255");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify((byte)255), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid nullable int value from a JSON number element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidNullableIntNumber_CallsSetValueWithoutNotifyWithConvertedValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int?));
        using var doc = JsonDocument.Parse("123");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(123), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a boolean true value from a JSON true element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidBooleanTrue_CallsSetValueWithoutNotifyWithTrue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(bool));
        using var doc = JsonDocument.Parse("true");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(true), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a boolean false value from a JSON false element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidBooleanFalse_CallsSetValueWithoutNotifyWithFalse()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(bool));
        using var doc = JsonDocument.Parse("false");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(false), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a nullable boolean value from a JSON true element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidNullableBooleanTrue_CallsSetValueWithoutNotifyWithTrue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(bool?));
        using var doc = JsonDocument.Parse("true");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(true), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a string value from a JSON string element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidString_CallsSetValueWithoutNotifyWithStringValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(string));
        using var doc = JsonDocument.Parse("\"test string\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify("test string"), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies an empty string value from a JSON string element.
    /// </summary>
    [TestMethod]
    public void Apply_EmptyString_CallsSetValueWithoutNotifyWithEmptyString()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(string));
        using var doc = JsonDocument.Parse("\"\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(string.Empty), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid enum value from a JSON string element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidEnumString_CallsSetValueWithoutNotifyWithEnumValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(StringComparison));
        using var doc = JsonDocument.Parse("\"Ordinal\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(StringComparison.Ordinal), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid enum value with case-insensitive matching.
    /// </summary>
    [TestMethod]
    public void Apply_ValidEnumStringCaseInsensitive_CallsSetValueWithoutNotifyWithEnumValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(StringComparison));
        using var doc = JsonDocument.Parse("\"ordinal\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(StringComparison.Ordinal), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly applies a valid nullable enum value from a JSON string element.
    /// </summary>
    [TestMethod]
    public void Apply_ValidNullableEnumString_CallsSetValueWithoutNotifyWithEnumValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(StringComparison?));
        using var doc = JsonDocument.Parse("\"OrdinalIgnoreCase\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(StringComparison.OrdinalIgnoreCase), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles a JSON null element by calling SetValueWithoutNotify with null.
    /// </summary>
    [TestMethod]
    public void Apply_NullJsonElement_CallsSetValueWithoutNotifyWithNull()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(string));
        using var doc = JsonDocument.Parse("null");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(null), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles a JSON null element for nullable int by calling SetValueWithoutNotify with null.
    /// </summary>
    [TestMethod]
    public void Apply_NullJsonElementForNullableInt_CallsSetValueWithoutNotifyWithNull()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int?));
        using var doc = JsonDocument.Parse("null");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(null), Times.Once);
    }

    /// <summary>
    /// Tests that Apply skips assignment when an invalid enum string is provided,
    /// preserving the parameter's default value.
    /// </summary>
    [TestMethod]
    public void Apply_InvalidEnumString_DoesNotCallSetValueWithoutNotify()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(StringComparison));
        using var doc = JsonDocument.Parse("\"InvalidEnumValue\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Apply skips assignment when a string element is provided for an integer target.
    /// </summary>
    [TestMethod]
    public void Apply_StringJsonElementForIntTarget_DoesNotCallSetValueWithoutNotify()
    {
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("\"42\"");
        var element = doc.RootElement;

        ParameterJsonReader.Apply(mockParam.Object, element);

        mockParam.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Apply skips assignment when a numeric element is provided for a boolean target.
    /// </summary>
    [TestMethod]
    public void Apply_NumberJsonElementForBoolTarget_DoesNotCallSetValueWithoutNotify()
    {
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(bool));
        using var doc = JsonDocument.Parse("1");
        var element = doc.RootElement;

        ParameterJsonReader.Apply(mockParam.Object, element);

        mockParam.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Apply skips assignment when an invalid nullable enum string is provided.
    /// </summary>
    [TestMethod]
    public void Apply_InvalidNullableEnumString_DoesNotCallSetValueWithoutNotify()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(StringComparison?));
        using var doc = JsonDocument.Parse("\"NonExistentValue\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Apply skips assignment when a JSON object element is provided
    /// (unsupported JsonValueKind).
    /// </summary>
    [TestMethod]
    public void Apply_ObjectJsonElement_DoesNotCallSetValueWithoutNotify()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(string));
        using var doc = JsonDocument.Parse("{\"key\": \"value\"}");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Apply skips assignment when a JSON array element is provided
    /// (unsupported JsonValueKind).
    /// </summary>
    [TestMethod]
    public void Apply_ArrayJsonElement_DoesNotCallSetValueWithoutNotify()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("[1, 2, 3]");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Apply correctly handles a JSON number with zero value.
    /// </summary>
    [TestMethod]
    public void Apply_ZeroIntegerNumber_CallsSetValueWithoutNotifyWithZero()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("0");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(0), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles a negative integer value.
    /// </summary>
    [TestMethod]
    public void Apply_NegativeIntegerNumber_CallsSetValueWithoutNotifyWithNegativeValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("-42");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(-42), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles int.MinValue.
    /// </summary>
    [TestMethod]
    public void Apply_IntMinValue_CallsSetValueWithoutNotifyWithMinValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("-2147483648");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(int.MinValue), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles int.MaxValue.
    /// </summary>
    [TestMethod]
    public void Apply_IntMaxValue_CallsSetValueWithoutNotifyWithMaxValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("2147483647");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(int.MaxValue), Times.Once);
    }

    /// <summary>
    /// Tests that Apply throws when a numeric JSON value overflows the target int type.
    /// </summary>
    [TestMethod]
    public void Apply_IntOverflow_ThrowsFormatException()
    {
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(int));
        using var doc = JsonDocument.Parse("2147483648");
        var element = doc.RootElement;

        AssertThrows<FormatException>(() => ParameterJsonReader.Apply(mockParam.Object, element));
    }

    /// <summary>
    /// Tests that Apply correctly handles long.MinValue.
    /// </summary>
    [TestMethod]
    public void Apply_LongMinValue_CallsSetValueWithoutNotifyWithMinValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(long));
        using var doc = JsonDocument.Parse("-9223372036854775808");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(long.MinValue), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles a very large double value.
    /// </summary>
    [TestMethod]
    public void Apply_LargeDoubleNumber_CallsSetValueWithoutNotifyWithLargeValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(double));
        using var doc = JsonDocument.Parse("1.7976931348623157E+308");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(It.Is<double>(d => d > 1.7e308)), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles byte.MinValue (0).
    /// </summary>
    [TestMethod]
    public void Apply_ByteMinValue_CallsSetValueWithoutNotifyWithMinValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(byte));
        using var doc = JsonDocument.Parse("0");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify((byte)0), Times.Once);
    }

    /// <summary>
    /// Tests that Apply throws when a numeric JSON value overflows the target byte type.
    /// </summary>
    [TestMethod]
    public void Apply_ByteOverflow_ThrowsFormatException()
    {
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(byte));
        using var doc = JsonDocument.Parse("256");
        var element = doc.RootElement;

        AssertThrows<FormatException>(() => ParameterJsonReader.Apply(mockParam.Object, element));

    }

    /// <summary>
    /// Verifies that an action throws the expected exception type and returns the captured exception.
    /// </summary>
    private static TException AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
        throw new InvalidOperationException("Unreachable");
    }

    /// <summary>
    /// Tests that Apply correctly handles short.MinValue.
    /// </summary>
    [TestMethod]
    public void Apply_ShortMinValue_CallsSetValueWithoutNotifyWithMinValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(short));
        using var doc = JsonDocument.Parse("-32768");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify((short)-32768), Times.Once);
    }

    /// <summary>
    /// Tests the current unchecked-cast behavior when a numeric JSON value exceeds the target short range.
    /// </summary>
    [TestMethod]
    public void Apply_ShortOverflow_UsesUncheckedCastBehavior()
    {
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(short));
        using var doc = JsonDocument.Parse("40000");
        var element = doc.RootElement;

        ParameterJsonReader.Apply(mockParam.Object, element);

        mockParam.Verify(p => p.SetValueWithoutNotify(unchecked((short)40000)), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles uint.MaxValue.
    /// </summary>
    [TestMethod]
    public void Apply_UIntMaxValue_CallsSetValueWithoutNotifyWithMaxValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(uint));
        using var doc = JsonDocument.Parse("4294967295");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(4294967295), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles a string with special characters.
    /// </summary>
    [TestMethod]
    public void Apply_StringWithSpecialCharacters_CallsSetValueWithoutNotifyWithStringValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(string));
        using var doc = JsonDocument.Parse("\"Test\\nString\\twith\\rSpecial\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify("Test\nString\twith\rSpecial"), Times.Once);
    }

    /// <summary>
    /// Tests that Apply correctly handles a string with Unicode characters.
    /// </summary>
    [TestMethod]
    public void Apply_StringWithUnicodeCharacters_CallsSetValueWithoutNotifyWithStringValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(string));
        using var doc = JsonDocument.Parse("\"Hello \\u4E16\\u754C\"");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify("Hello 世界"), Times.Once);
    }

    /// <summary>
    /// Tests that Apply skips assignment when a boolean element is provided for non-boolean target type.
    /// </summary>
    [TestMethod]
    public void Apply_BooleanForNonBooleanType_DoesNotCallSetValueWithoutNotify()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(string));
        using var doc = JsonDocument.Parse("true");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(It.IsAny<object?>()), Times.Never);
    }

    /// <summary>
    /// Tests that Apply correctly handles an unrecognized numeric type by falling back to double.
    /// </summary>
    [TestMethod]
    public void Apply_UnrecognizedNumericType_CallsSetValueWithoutNotifyWithDoubleValue()
    {
        // Arrange
        var mockParam = new Mock<IParameter>();
        mockParam.Setup(p => p.ValueType).Returns(typeof(decimal)); // Unsupported numeric type
        using var doc = JsonDocument.Parse("3.14");
        var element = doc.RootElement;

        // Act
        ParameterJsonReader.Apply(mockParam.Object, element);

        // Assert
        mockParam.Verify(p => p.SetValueWithoutNotify(3.14), Times.Once);
    }
}
