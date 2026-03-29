using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra;

namespace Umbra.UnitTests;


/// <summary>
/// Contains unit tests for the <see cref="StringExtensions"/> class.
/// </summary>
[TestClass]
public class StringExtensionsTests
{
    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> returns an empty string
    /// when the input string is empty.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        string input = string.Empty;

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> returns the same character
    /// when the input is a single uppercase character.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_SingleUppercaseCharacter_ReturnsSameCharacter()
    {
        // Arrange
        string input = "A";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("A", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> returns the same character
    /// when the input is a single lowercase character.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_SingleLowercaseCharacter_ReturnsSameCharacter()
    {
        // Arrange
        string input = "a";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("a", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> returns the string unchanged
    /// when all characters are lowercase.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_AllLowercase_ReturnsUnchanged()
    {
        // Arrange
        string input = "fieldofview";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("fieldofview", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> returns the string unchanged
    /// when all characters are uppercase.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_AllUppercase_ReturnsUnchanged()
    {
        // Arrange
        string input = "FIELDOFVIEW";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("FIELDOFVIEW", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> correctly inserts spaces
    /// before uppercase letters that follow lowercase letters in PascalCase strings.
    /// </summary>
    /// <param name="input">The PascalCase input string.</param>
    /// <param name="expected">The expected display name with spaces.</param>
    [TestMethod]
    [DataRow("FieldOfView", "Field Of View")]
    [DataRow("UserName", "User Name")]
    [DataRow("FirstName", "First Name")]
    [DataRow("IsEnabled", "Is Enabled")]
    [DataRow("A", "A")]
    [DataRow("AB", "AB")]
    [DataRow("ABC", "ABC")]
    public void ToDisplayName_PascalCase_InsertsSpaces(string input, string expected)
    {
        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> correctly inserts spaces
    /// before uppercase letters that follow lowercase letters in camelCase strings.
    /// </summary>
    /// <param name="input">The camelCase input string.</param>
    /// <param name="expected">The expected display name with spaces.</param>
    [TestMethod]
    [DataRow("fieldOfView", "field Of View")]
    [DataRow("userName", "user Name")]
    [DataRow("firstName", "first Name")]
    [DataRow("isEnabled", "is Enabled")]
    public void ToDisplayName_CamelCase_InsertsSpaces(string input, string expected)
    {
        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> does not insert spaces
    /// within consecutive uppercase letters (acronyms).
    /// </summary>
    [TestMethod]
    public void ToDisplayName_ConsecutiveUppercase_NoSpacesBetweenUppercase()
    {
        // Arrange
        string input = "XMLParser";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("XMLParser", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles strings
    /// with numbers correctly, treating them as non-uppercase characters.
    /// </summary>
    /// <param name="input">The input string containing numbers.</param>
    /// <param name="expected">The expected display name.</param>
    [TestMethod]
    [DataRow("Field123", "Field123")]
    [DataRow("Field123View", "Field123 View")]
    [DataRow("Field1View2", "Field1 View2")]
    [DataRow("123Field", "123 Field")]
    [DataRow("123", "123")]
    public void ToDisplayName_StringsWithNumbers_HandlesCorrectly(string input, string expected)
    {
        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles strings
    /// with special characters correctly, treating them as non-uppercase characters.
    /// </summary>
    /// <param name="input">The input string containing special characters.</param>
    /// <param name="expected">The expected display name.</param>
    [TestMethod]
    [DataRow("Field_View", "Field_View")]
    [DataRow("Field-View", "Field-View")]
    [DataRow("Field.View", "Field.View")]
    [DataRow("Field@View", "Field@View")]
    [DataRow("_FieldView", "_Field View")]
    [DataRow("Field_", "Field_")]
    public void ToDisplayName_StringsWithSpecialCharacters_HandlesCorrectly(string input, string expected)
    {
        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> inserts additional spaces
    /// when the input string already contains spaces, because a space is not an uppercase character.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_StringWithSpaces_InsertsAdditionalSpaces()
    {
        // Arrange
        string input = "Field Of View";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("Field  Of  View", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles strings
    /// with mixed case transitions correctly.
    /// </summary>
    /// <param name="input">The input string with mixed case transitions.</param>
    /// <param name="expected">The expected display name.</param>
    [TestMethod]
    [DataRow("aB", "a B")]
    [DataRow("aBcD", "a Bc D")]
    [DataRow("iPhone", "i Phone")]
    [DataRow("aBCD", "a BCD")]
    [DataRow("ABc", "ABc")]
    public void ToDisplayName_MixedCaseTransitions_HandlesCorrectly(string input, string expected)
    {
        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles whitespace-only strings
    /// correctly, returning them unchanged.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_WhitespaceOnly_ReturnsUnchanged()
    {
        // Arrange
        string input = "   ";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("   ", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles very long strings
    /// correctly without performance issues or errors.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_VeryLongString_HandlesCorrectly()
    {
        // Arrange
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < 1000; i++)
        {
            builder.Append("FieldOfView");
        }
        string input = builder.ToString();

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length >= input.Length);
        Assert.IsTrue(result.StartsWith("Field Of View"));
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles strings
    /// with Unicode uppercase characters correctly.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_UnicodeUppercase_InsertsSpaces()
    {
        // Arrange
        string input = "fieldÖfView";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("field Öf View", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles strings
    /// starting with multiple consecutive uppercase letters followed by lowercase correctly.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="expected">The expected display name.</param>
    [TestMethod]
    [DataRow("HTTPServer", "HTTPServer")]
    [DataRow("URLParser", "URLParser")]
    [DataRow("IOError", "IOError")]
    [DataRow("HTTPSConnection", "HTTPSConnection")]
    public void ToDisplayName_AcronymAtStart_NoSpacesInAcronym(string input, string expected)
    {
        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles strings
    /// with uppercase letter transitions after non-letter characters.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_UppercaseAfterNonLetter_InsertsSpace()
    {
        // Arrange
        string input = "field123View";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("field123 View", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToDisplayName"/> handles strings
    /// with tab and newline characters correctly.
    /// </summary>
    [TestMethod]
    public void ToDisplayName_ControlCharacters_HandlesCorrectly()
    {
        // Arrange
        string input = "Field\tView\nName";

        // Act
        string result = input.ToDisplayName();

        // Assert
        Assert.AreEqual("Field\t View\n Name", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> returns null when the input is null.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_NullInput_ReturnsNull()
    {
        // Arrange
        string? value = null;

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> returns the same empty string when input is empty.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_EmptyString_ReturnsEmpty()
    {
        // Arrange
        string value = string.Empty;

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> converts a single uppercase character to lowercase.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_SingleUppercaseCharacter_ConvertsToLowercase()
    {
        // Arrange
        string value = "A";

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.AreEqual("a", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> returns the same string when it starts with a lowercase character.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_SingleLowercaseCharacter_ReturnsUnchanged()
    {
        // Arrange
        string value = "a";

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.AreSame(value, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> correctly converts various PascalCase strings to camelCase.
    /// Validates conversion of the first uppercase character to lowercase while preserving the rest of the string.
    /// </summary>
    /// <param name="input">The input string in PascalCase format.</param>
    /// <param name="expected">The expected camelCase output.</param>
    [TestMethod]
    [DataRow("PascalCase", "pascalCase")]
    [DataRow("UpperCase", "upperCase")]
    [DataRow("ABC", "aBC")]
    [DataRow("A", "a")]
    [DataRow("AB", "aB")]
    [DataRow("Test", "test")]
    [DataRow("MyProperty", "myProperty")]
    [DataRow("HTTPResponse", "hTTPResponse")]
    public void ToCamelCase_PascalCaseString_ConvertsFirstCharacterToLowercase(string input, string expected)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> returns the same string reference when input already starts with lowercase.
    /// Verifies reference equality to confirm no new string is created.
    /// </summary>
    /// <param name="input">The input string that already starts with lowercase.</param>
    [TestMethod]
    [DataRow("camelCase")]
    [DataRow("lowercase")]
    [DataRow("alreadyCamelCase")]
    [DataRow("test")]
    [DataRow("myProperty")]
    public void ToCamelCase_AlreadyCamelCase_ReturnsSameReference(string input)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreSame(input, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> handles strings starting with numbers.
    /// Numbers are not considered lowercase by <see cref="char.IsLower"/>, so the method processes them,
    /// but <see cref="char.ToLowerInvariant"/> returns the number unchanged.
    /// </summary>
    /// <param name="input">The input string starting with a number.</param>
    /// <param name="expected">The expected output (same as input for numbers).</param>
    [TestMethod]
    [DataRow("123", "123")]
    [DataRow("1test", "1test")]
    [DataRow("0Value", "0Value")]
    [DataRow("9ABC", "9ABC")]
    public void ToCamelCase_StartsWithNumber_ReturnsWithNumberUnchanged(string input, string expected)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> handles strings starting with special characters.
    /// Special characters are not considered lowercase, so the method processes them,
    /// but <see cref="char.ToLowerInvariant"/> returns the character unchanged.
    /// </summary>
    /// <param name="input">The input string starting with a special character.</param>
    /// <param name="expected">The expected output (same as input for special characters).</param>
    [TestMethod]
    [DataRow("_test", "_test")]
    [DataRow("$value", "$value")]
    [DataRow("@Property", "@Property")]
    [DataRow("#define", "#define")]
    [DataRow("!Important", "!Important")]
    public void ToCamelCase_StartsWithSpecialCharacter_ReturnsWithCharacterUnchanged(string input, string expected)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> handles strings starting with whitespace.
    /// Whitespace is not considered lowercase, so the method processes it,
    /// but <see cref="char.ToLowerInvariant"/> returns the whitespace unchanged.
    /// </summary>
    /// <param name="input">The input string starting with whitespace.</param>
    /// <param name="expected">The expected output (same as input for whitespace).</param>
    [TestMethod]
    [DataRow(" test", " test")]
    [DataRow("  Test", "  Test")]
    [DataRow("\tTab", "\tTab")]
    [DataRow("\nNewline", "\nNewline")]
    public void ToCamelCase_StartsWithWhitespace_ReturnsWithWhitespaceUnchanged(string input, string expected)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> correctly handles Unicode uppercase characters
    /// and converts them to lowercase using culture-invariant rules.
    /// </summary>
    /// <param name="input">The input string with Unicode uppercase character.</param>
    /// <param name="expected">The expected output with Unicode lowercase character.</param>
    [TestMethod]
    [DataRow("Übung", "übung")]
    [DataRow("École", "école")]
    [DataRow("Ñoño", "ñoño")]
    [DataRow("Żółw", "żółw")]
    [DataRow("Αλφα", "αλφα")]
    public void ToCamelCase_UnicodeUppercase_ConvertsToLowercase(string input, string expected)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> correctly handles a very long string,
    /// converting only the first character to lowercase while preserving the rest.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_VeryLongString_ConvertsFirstCharacterOnly()
    {
        // Arrange
        string value = "A" + new string('B', 10000);
        string expected = "a" + new string('B', 10000);

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
        Assert.AreEqual(10001, result?.Length);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> is idempotent,
    /// meaning calling it multiple times produces the same result.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_CalledMultipleTimes_ProducesSameResult()
    {
        // Arrange
        string value = "TestValue";

        // Act
        string? result1 = value.ToCamelCase();
        string? result2 = result1.ToCamelCase();
        string? result3 = result2.ToCamelCase();

        // Assert
        Assert.AreEqual("testValue", result1);
        Assert.AreSame(result1, result2);
        Assert.AreSame(result2, result3);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> handles strings with mixed case after the first character,
    /// preserving the original casing of all characters except the first.
    /// </summary>
    /// <param name="input">The input string with mixed case.</param>
    /// <param name="expected">The expected output with only the first character converted.</param>
    [TestMethod]
    [DataRow("AbCdEf", "abCdEf")]
    [DataRow("ALLCAPS", "aLLCAPS")]
    [DataRow("MixedCASEString", "mixedCASEString")]
    [DataRow("OneTwoThree", "oneTwoThree")]
    public void ToCamelCase_MixedCaseString_PreservesAllButFirstCharacter(string input, string expected)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> handles strings containing only whitespace.
    /// Since whitespace is not lowercase, the method processes it but returns it unchanged.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_OnlyWhitespace_ReturnsUnchanged()
    {
        // Arrange
        string value = "   ";

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.AreEqual("   ", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> handles strings with control characters.
    /// Control characters are not considered lowercase, so they are processed but returned unchanged.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_StartsWithControlCharacter_ReturnsUnchanged()
    {
        // Arrange
        string value = "\0test";

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.AreEqual("\0test", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> correctly processes strings with uppercase letters
    /// at various Unicode code points, ensuring culture-invariant conversion.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_VariousUnicodeUppercaseLetters_ConvertsCorrectly()
    {
        // Arrange & Act & Assert
        Assert.AreEqual("ā", "Ā".ToCamelCase()); // Latin Extended-A
        Assert.AreEqual("đ", "Đ".ToCamelCase()); // Latin Extended-A
        Assert.AreEqual("ș", "Ș".ToCamelCase()); // Latin Extended-B
        Assert.AreEqual("ω", "Ω".ToCamelCase()); // Greek
    }

    /// <summary>
    /// Tests edge case where the string contains only an uppercase letter followed by lowercase letters.
    /// </summary>
    [TestMethod]
    public void ToCamelCase_UppercaseFollowedByLowercase_ConvertsFirstOnly()
    {
        // Arrange
        string value = "Test";

        // Act
        string? result = value.ToCamelCase();

        // Assert
        Assert.AreEqual("test", result);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.ToCamelCase"/> handles two-character strings correctly.
    /// </summary>
    /// <param name="input">The two-character input string.</param>
    /// <param name="expected">The expected output.</param>
    [TestMethod]
    [DataRow("AB", "aB")]
    [DataRow("Ab", "ab")]
    [DataRow("aB", "aB")]
    [DataRow("ab", "ab")]
    public void ToCamelCase_TwoCharacterString_ConvertsCorrectly(string input, string expected)
    {
        // Act
        string? result = input.ToCamelCase();

        // Assert
        Assert.AreEqual(expected, result);
    }
}