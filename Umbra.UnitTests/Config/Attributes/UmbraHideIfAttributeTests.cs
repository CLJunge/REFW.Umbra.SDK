namespace Umbra.Config.Attributes.UnitTests;


/// <summary>
/// Contains unit tests for the <see cref="UmbraHideIfAttribute{T}"/> class.
/// </summary>
[TestClass]
public sealed class UmbraHideIfAttributeTests
{
    /// <summary>
    /// Tests that the constructor with memberName and value correctly initializes all properties
    /// when given a valid string memberName and integer value.
    /// Expected: MemberName is set to provided value, Value is set to provided integer, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMemberNameAndIntValue_SetsAllPropertiesCorrectly()
    {
        // Arrange
        const string memberName = "TestMember";
        const int value = 42;

        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles various integer values including boundary values.
    /// Expected: All properties are set correctly for each test case.
    /// </summary>
    /// <param name="memberName">The member name to test.</param>
    /// <param name="value">The integer value to test.</param>
    [TestMethod]
    [DataRow("Member1", 0)]
    [DataRow("Member2", -1)]
    [DataRow("Member3", 1)]
    [DataRow("Member4", int.MaxValue)]
    [DataRow("Member5", int.MinValue)]
    public void Constructor_WithVariousIntValues_SetsPropertiesCorrectly(string memberName, int value)
    {
        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes all properties with a string value.
    /// Expected: MemberName is set to provided value, Value is set to provided string, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMemberNameAndStringValue_SetsAllPropertiesCorrectly()
    {
        // Arrange
        const string memberName = "StringMember";
        const string value = "TestValue";

        // Act
        var attribute = new UmbraHideIfAttribute<string>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles empty string as value.
    /// Expected: Empty string value is preserved, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyStringValue_SetsValueToEmptyString()
    {
        // Arrange
        const string memberName = "Member";
        const string value = "";

        // Act
        var attribute = new UmbraHideIfAttribute<string>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles whitespace-only string as value.
    /// Expected: Whitespace string value is preserved, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithWhitespaceStringValue_PreservesWhitespace()
    {
        // Arrange
        const string memberName = "Member";
        const string value = "   ";

        // Act
        var attribute = new UmbraHideIfAttribute<string>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles empty string as memberName.
    /// Expected: Empty memberName is accepted and stored, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyMemberName_AcceptsEmptyString()
    {
        // Arrange
        const string memberName = "";
        const int value = 10;

        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles whitespace-only memberName.
    /// Expected: Whitespace memberName is accepted and stored, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithWhitespaceMemberName_AcceptsWhitespace()
    {
        // Arrange
        const string memberName = "   ";
        const int value = 10;

        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles very long memberName string.
    /// Expected: Long memberName is accepted and stored, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithVeryLongMemberName_AcceptsLongString()
    {
        // Arrange
        var memberName = new string('a', 10000);
        const int value = 100;

        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles memberName with special characters.
    /// Expected: Special characters in memberName are preserved, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithSpecialCharactersInMemberName_PreservesSpecialCharacters()
    {
        // Arrange
        const string memberName = "Test@Member#123$%^&*()";
        const int value = 42;

        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes all properties with a boolean value.
    /// Expected: MemberName and Value are set correctly, HasValue is true.
    /// </summary>
    /// <param name="memberName">The member name to test.</param>
    /// <param name="value">The boolean value to test.</param>
    [TestMethod]
    [DataRow("BoolMember1", true)]
    [DataRow("BoolMember2", false)]
    public void Constructor_WithBoolValue_SetsPropertiesCorrectly(string memberName, bool value)
    {
        // Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles double values including special values.
    /// Expected: All properties are set correctly including NaN and Infinity values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDoubleNaN_SetsValueToNaN()
    {
        // Arrange
        const string memberName = "DoubleMember";
        const double value = double.NaN;

        // Act
        var attribute = new UmbraHideIfAttribute<double>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.IsTrue(double.IsNaN(attribute.Value));
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles positive infinity double value.
    /// Expected: Value is set to PositiveInfinity, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDoublePositiveInfinity_SetsValueToPositiveInfinity()
    {
        // Arrange
        const string memberName = "DoubleMember";
        const double value = double.PositiveInfinity;

        // Act
        var attribute = new UmbraHideIfAttribute<double>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(double.PositiveInfinity, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles negative infinity double value.
    /// Expected: Value is set to NegativeInfinity, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDoubleNegativeInfinity_SetsValueToNegativeInfinity()
    {
        // Arrange
        const string memberName = "DoubleMember";
        const double value = double.NegativeInfinity;

        // Act
        var attribute = new UmbraHideIfAttribute<double>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(double.NegativeInfinity, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles various normal double values.
    /// Expected: All properties are set correctly for each test case.
    /// </summary>
    /// <param name="memberName">The member name to test.</param>
    /// <param name="value">The double value to test.</param>
    [TestMethod]
    [DataRow("Double1", 0.0)]
    [DataRow("Double2", -1.5)]
    [DataRow("Double3", 1.5)]
    [DataRow("Double4", double.MaxValue)]
    [DataRow("Double5", double.MinValue)]
    public void Constructor_WithVariousDoubleValues_SetsPropertiesCorrectly(string memberName, double value)
    {
        // Act
        var attribute = new UmbraHideIfAttribute<double>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(value, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles nullable integer with null value.
    /// Expected: Value is set to null, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullableIntNull_SetsValueToNull()
    {
        // Arrange
        const string memberName = "NullableMember";
        int? value = null;

        // Act
        var attribute = new UmbraHideIfAttribute<int?>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.IsNull(attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the constructor correctly handles nullable integer with actual value.
    /// Expected: Value is set to the provided integer, HasValue is true.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullableIntValue_SetsValueCorrectly()
    {
        // Arrange
        const string memberName = "NullableMember";
        int? value = 42;

        // Act
        var attribute = new UmbraHideIfAttribute<int?>(memberName, value);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.AreEqual(42, attribute.Value);
        Assert.IsTrue(attribute.HasValue);
    }

    /// <summary>
    /// Tests that the BoxedValue property returns the correct boxed value.
    /// Expected: BoxedValue returns the same value as Value property.
    /// </summary>
    [TestMethod]
    public void Constructor_WithIntValue_BoxedValueMatchesValue()
    {
        // Arrange
        const string memberName = "Member";
        const int value = 123;

        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName, value);
        var boxedValue = ((IHideIfAttribute)attribute).BoxedValue;

        // Assert
        Assert.AreEqual(value, boxedValue);
    }

    /// <summary>
    /// Tests that the BoxedValue property returns null when string value is null.
    /// Expected: BoxedValue returns null matching the null Value property.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullStringValue_BoxedValueIsNull()
    {
        // Arrange
        const string memberName = "Member";
        string? value = null;

        // Act
        var attribute = new UmbraHideIfAttribute<string?>(memberName, value);
        var boxedValue = ((IHideIfAttribute)attribute).BoxedValue;

        // Assert
        Assert.IsNull(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns the correct boxed integer value when constructed with an explicit value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithIntValue_ReturnsBoxedValue()
    {
        // Arrange
        const int expectedValue = 42;
        var attribute = new UmbraHideIfAttribute<int>("memberName", expectedValue);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(expectedValue, boxedValue);
        Assert.IsInstanceOfType<int>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns the correct boxed string value when constructed with an explicit value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithStringValue_ReturnsBoxedValue()
    {
        // Arrange
        const string expectedValue = "test";
        var attribute = new UmbraHideIfAttribute<string>("memberName", expectedValue);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(expectedValue, boxedValue);
        Assert.IsInstanceOfType<string>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns null when constructed with a null string value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithNullStringValue_ReturnsNull()
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<string>("memberName", null!);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNull(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns null when constructed using the single-parameter constructor with a reference type.
    /// </summary>
    [TestMethod]
    public void BoxedValue_ReferenceTypeWithDefaultConstructor_ReturnsNull()
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<string>("memberName");
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNull(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns boxed default value when constructed using the single-parameter constructor with a value type.
    /// </summary>
    [TestMethod]
    public void BoxedValue_ValueTypeWithDefaultConstructor_ReturnsBoxedDefault()
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<int>("memberName");
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(0, boxedValue);
        Assert.IsInstanceOfType<int>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns the correct boxed boolean value.
    /// </summary>
    /// <param name="value">The boolean value to test.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void BoxedValue_WithBooleanValue_ReturnsBoxedValue(bool value)
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<bool>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
        Assert.IsInstanceOfType<bool>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly boxes integer boundary values.
    /// </summary>
    /// <param name="value">The integer boundary value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1)]
    public void BoxedValue_WithIntBoundaryValues_ReturnsBoxedValue(int value)
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<int>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
        Assert.IsInstanceOfType<int>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly boxes double values including special values.
    /// </summary>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(1.0)]
    [DataRow(-1.0)]
    [DataRow(double.MaxValue)]
    [DataRow(double.MinValue)]
    [DataRow(double.Epsilon)]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void BoxedValue_WithDoubleValues_ReturnsBoxedValue(double value)
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<double>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        if (double.IsNaN(value))
        {
            Assert.IsTrue(double.IsNaN((double)boxedValue));
        }
        else
        {
            Assert.AreEqual(value, boxedValue);
        }
        Assert.IsInstanceOfType<double>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles nullable int with null value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithNullableIntNull_ReturnsNull()
    {
        // Arrange
        int? value = null;
        var attribute = new UmbraHideIfAttribute<int?>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNull(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles nullable int with a non-null value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithNullableIntValue_ReturnsBoxedValue()
    {
        // Arrange
        int? value = 123;
        var attribute = new UmbraHideIfAttribute<int?>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles nullable bool with null value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithNullableBoolNull_ReturnsNull()
    {
        // Arrange
        bool? value = null;
        var attribute = new UmbraHideIfAttribute<bool?>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNull(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles empty string.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<string>("memberName", string.Empty);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(string.Empty, boxedValue);
        Assert.IsInstanceOfType<string>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles whitespace-only string.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithWhitespaceString_ReturnsWhitespaceString()
    {
        // Arrange
        const string value = "   ";
        var attribute = new UmbraHideIfAttribute<string>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
        Assert.IsInstanceOfType<string>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles strings with special characters.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithSpecialCharactersString_ReturnsString()
    {
        // Arrange
        const string value = "!@#$%^&*()_+-=[]{}|;':\",./<>?\n\r\t";
        var attribute = new UmbraHideIfAttribute<string>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
        Assert.IsInstanceOfType<string>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles very long strings.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithVeryLongString_ReturnsString()
    {
        // Arrange
        var value = new string('x', 10000);
        var attribute = new UmbraHideIfAttribute<string>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
        Assert.IsInstanceOfType<string>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles enum values.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithEnumValue_ReturnsBoxedValue()
    {
        // Arrange
        const DayOfWeek value = DayOfWeek.Friday;
        var attribute = new UmbraHideIfAttribute<DayOfWeek>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
        Assert.IsInstanceOfType<DayOfWeek>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles enum values outside defined range.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithEnumValueOutsideRange_ReturnsBoxedValue()
    {
        // Arrange
        const DayOfWeek value = (DayOfWeek)999;
        var attribute = new UmbraHideIfAttribute<DayOfWeek>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreEqual(value, boxedValue);
        Assert.IsInstanceOfType<DayOfWeek>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles object reference type with null value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithObjectNull_ReturnsNull()
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<object>("memberName", null!);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNull(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue correctly handles object reference type with non-null value.
    /// </summary>
    [TestMethod]
    public void BoxedValue_WithObjectValue_ReturnsValue()
    {
        // Arrange
        var value = new object();
        var attribute = new UmbraHideIfAttribute<object>("memberName", value);
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.AreSame(value, boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns boxed false when constructed with default boolean constructor.
    /// </summary>
    [TestMethod]
    public void BoxedValue_BooleanWithDefaultConstructor_ReturnsBoxedFalse()
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<bool>("memberName");
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNotNull(boxedValue);
        Assert.IsFalse((bool?)boxedValue);
        Assert.IsInstanceOfType<bool>(boxedValue);
    }

    /// <summary>
    /// Verifies that BoxedValue returns null when constructed with default nullable int constructor.
    /// </summary>
    [TestMethod]
    public void BoxedValue_NullableIntWithDefaultConstructor_ReturnsNull()
    {
        // Arrange
        var attribute = new UmbraHideIfAttribute<int?>("memberName");
        var hideIfAttribute = (IHideIfAttribute)attribute;

        // Act
        var boxedValue = hideIfAttribute.BoxedValue;

        // Assert
        Assert.IsNull(boxedValue);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes all properties
    /// when provided a valid member name with a value type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    [DataRow("IsEnabled")]
    [DataRow("ShowAdvancedOptions")]
    [DataRow("mode")]
    [DataRow("_privateField")]
    [DataRow("Property1")]
    public void Constructor_ValidMemberNameWithBoolType_InitializesPropertiesCorrectly(string memberName)
    {
        // Arrange & Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes all properties
    /// when provided a valid member name with an integer type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    [DataRow("Count")]
    [DataRow("MaxValue")]
    [DataRow("index")]
    public void Constructor_ValidMemberNameWithIntType_InitializesPropertiesCorrectly(string memberName)
    {
        // Arrange & Act
        var attribute = new UmbraHideIfAttribute<int>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes all properties
    /// when provided a valid member name with a string type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidMemberNameWithStringType_InitializesPropertiesCorrectly()
    {
        // Arrange
        var memberName = "UserName";

        // Act
        var attribute = new UmbraHideIfAttribute<string>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.IsNull(attribute.Value);
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes all properties
    /// when provided a valid member name with a double type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidMemberNameWithDoubleType_InitializesPropertiesCorrectly()
    {
        // Arrange
        var memberName = "Temperature";

        // Act
        var attribute = new UmbraHideIfAttribute<double>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes all properties
    /// when provided a valid member name with a nullable int type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidMemberNameWithNullableIntType_InitializesPropertiesCorrectly()
    {
        // Arrange
        var memberName = "OptionalCount";

        // Act
        var attribute = new UmbraHideIfAttribute<int?>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.IsNull(attribute.Value);
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor accepts an empty string as member name.
    /// Expected: MemberName is empty, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_EmptyStringMemberName_AcceptsAndInitializesProperties()
    {
        // Arrange
        var memberName = string.Empty;

        // Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName);

        // Assert
        Assert.AreEqual(string.Empty, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor accepts whitespace-only strings as member name.
    /// Expected: MemberName is set to the whitespace string, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\r\n")]
    [DataRow(" \t \n ")]
    public void Constructor_WhitespaceOnlyMemberName_AcceptsAndInitializesProperties(string memberName)
    {
        // Arrange & Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor accepts very long strings as member name.
    /// Expected: MemberName is set to the long string, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_VeryLongMemberName_AcceptsAndInitializesProperties()
    {
        // Arrange
        var memberName = new string('A', 10000);

        // Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor accepts member names with special characters.
    /// Expected: MemberName is set to input with special characters, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    [DataRow("Property-Name")]
    [DataRow("Property@Name")]
    [DataRow("Property#Name")]
    [DataRow("Property$Name")]
    [DataRow("Property Name")]
    [DataRow("Property.Name")]
    [DataRow("123InvalidIdentifier")]
    [DataRow("Property<T>")]
    [DataRow("Właściwość")]
    [DataRow("属性名")]
    public void Constructor_MemberNameWithSpecialCharacters_AcceptsAndInitializesProperties(string memberName)
    {
        // Arrange & Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor accepts null as member name without throwing.
    /// Expected: MemberName is null, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_NullMemberName_AcceptsAndInitializesProperties()
    {
        // Arrange
        string? memberName = null;

        // Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName!);

        // Assert
        Assert.IsNull(attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes properties with an enum type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidMemberNameWithEnumType_InitializesPropertiesCorrectly()
    {
        // Arrange
        var memberName = "Status";

        // Act
        var attribute = new UmbraHideIfAttribute<DayOfWeek>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes properties with an object type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidMemberNameWithObjectType_InitializesPropertiesCorrectly()
    {
        // Arrange
        var memberName = "Data";

        // Act
        var attribute = new UmbraHideIfAttribute<object>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.IsNull(attribute.Value);
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that Value property is default(T?) for float type parameter.
    /// Expected: MemberName is set to input, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidMemberNameWithFloatType_InitializesPropertiesCorrectly()
    {
        // Arrange
        var memberName = "Ratio";

        // Act
        var attribute = new UmbraHideIfAttribute<float>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }

    /// <summary>
    /// Verifies that the BoxedValue property returns null when Value is default.
    /// Expected: BoxedValue returns null for the single-parameter constructor.
    /// </summary>
    [TestMethod]
    public void Constructor_SingleParameter_BoxedValueReturnsNull()
    {
        // Arrange
        var memberName = "TestMember";

        // Act
        var attribute = new UmbraHideIfAttribute<int>(memberName);
        IHideIfAttribute hideIfAttribute = attribute;

        // Assert
        Assert.IsNull(hideIfAttribute.BoxedValue);
    }

    /// <summary>
    /// Verifies that member names with leading/trailing whitespace are preserved.
    /// Expected: MemberName exactly matches input including whitespace, Value is null, HasValue is false.
    /// </summary>
    [TestMethod]
    [DataRow(" PropertyName")]
    [DataRow("PropertyName ")]
    [DataRow(" PropertyName ")]
    public void Constructor_MemberNameWithLeadingOrTrailingWhitespace_PreservesWhitespace(string memberName)
    {
        // Arrange & Act
        var attribute = new UmbraHideIfAttribute<bool>(memberName);

        // Assert
        Assert.AreEqual(memberName, attribute.MemberName);
        Assert.Fail();
        Assert.IsFalse(attribute.HasValue);
    }
}
