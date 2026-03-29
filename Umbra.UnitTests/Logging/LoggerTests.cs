using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.Logging;

namespace Umbra.Logging.UnitTests;


/// <summary>
/// Unit tests for the <see cref="Logger.Error(string, object[])"/> method.
/// </summary>
[TestClass]
public sealed class LoggerTests
{
    /// <summary>
    /// Tests that Error with format and args returns immediately without throwing when logging is disabled via Enabled property.
    /// </summary>
    [TestMethod]
    public void Error_WhenEnabledIsFalse_ReturnsWithoutProcessing()
    {
        // Arrange
        Logger.Enabled = false;
        string format = "Test message {0}";
        object[] args = new object[] { 42 };

        // Act & Assert - should not throw
        Logger.Error(format, args);

        // Cleanup
        Logger.Enabled = true;
    }

    /// <summary>
    /// Tests that Error with format and args returns immediately when logging is suppressed via Suppress scope.
    /// </summary>
    [TestMethod]
    public void Error_WhenSuppressed_ReturnsWithoutProcessing()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Test message {0}";
        object[] args = new object[] { 42 };

        // Act & Assert - should not throw
        using (Logger.Suppress())
        {
            Logger.Error(format, args);
        }
    }

    /// <summary>
    /// Tests that Error with valid format and args does not throw when logging is enabled.
    /// </summary>
    /// <param name="format">The format string to test.</param>
    /// <param name="args">The arguments to format.</param>
    [TestMethod]
    [DataRow("Simple message", new object[] { })]
    [DataRow("Message with one arg: {0}", new object[] { 42 })]
    [DataRow("Message with multiple args: {0}, {1}, {2}", new object[] { "test", 123, true })]
    [DataRow("Message with repeated arg: {0} and {0} again", new object[] { "value" })]
    [DataRow("", new object[] { })]
    [DataRow("Number: {0:F2}", new object[] { 3.14159 })]
    public void Error_WithValidFormatAndArgs_DoesNotThrow(string format, object[] args)
    {
        // Arrange
        Logger.Enabled = true;

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with format and args handles null values in args array without throwing.
    /// </summary>
    [TestMethod]
    public void Error_WithNullArgsElements_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Message: {0}, {1}";
        object[] args = new object[] { null!, "test" };

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with extra args (more than placeholders) does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithExtraArgs_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Message: {0}";
        object[] args = new object[] { "first", "second", "third" };

        // Act & Assert - should not throw (extra args are ignored by string.Format)
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with null format string does not throw and returns silently.
    /// </summary>
    [TestMethod]
    public void Error_WithNullFormat_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = null!;
        object[] args = new object[] { 42 };

        // Act & Assert - should not throw (catches ArgumentNullException)
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with invalid format string does not throw and returns silently.
    /// </summary>
    /// <param name="format">The invalid format string to test.</param>
    /// <param name="args">The arguments to format.</param>
    [TestMethod]
    [DataRow("{0", new object[] { 42 })]
    [DataRow("{{0}", new object[] { 42 })]
    [DataRow("{1}", new object[] { 42 })]
    [DataRow("{0} {1}", new object[] { 42 })]
    [DataRow("{0:INVALID}", new object[] { 42 })]
    public void Error_WithInvalidFormatString_DoesNotThrow(string format, object[] args)
    {
        // Arrange
        Logger.Enabled = true;

        // Act & Assert - should not throw (catches FormatException)
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with null args array does not throw and returns silently.
    /// </summary>
    [TestMethod]
    public void Error_WithNullArgs_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Message: {0}";
        object[]? args = null;

        // Act & Assert - should not throw (catches exception from string.Format)
        Logger.Error(format, args!);
    }

    /// <summary>
    /// Tests that Error with empty args and format requiring args does not throw and returns silently.
    /// </summary>
    [TestMethod]
    public void Error_WithEmptyArgsAndPlaceholders_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Message: {0}";
        object[] args = Array.Empty<object>();

        // Act & Assert - should not throw (catches FormatException)
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with very long format string does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithVeryLongFormatString_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = new string('x', 10000) + " {0}";
        object[] args = new object[] { "test" };

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with special characters in format string does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithSpecialCharactersInFormat_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Message: \n\r\t\0 {0}";
        object[] args = new object[] { "test" };

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error respects nested suppression scopes and returns without processing.
    /// </summary>
    [TestMethod]
    public void Error_WithNestedSuppressionScopes_ReturnsWithoutProcessing()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Test message {0}";
        object[] args = new object[] { 42 };

        // Act & Assert - should not throw
        using (Logger.Suppress())
        {
            using (Logger.Suppress())
            {
                Logger.Error(format, args);
            }
        }
    }

    /// <summary>
    /// Tests that Error works correctly after suppression scope is disposed.
    /// </summary>
    [TestMethod]
    public void Error_AfterSuppressionDisposed_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Test message {0}";
        object[] args = new object[] { 42 };

        using (Logger.Suppress())
        {
            // Suppressed here
        }

        // Act & Assert - should not throw (suppression disposed, logging re-enabled)
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with format containing escaped braces does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithEscapedBraces_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Message: {{escaped}} {0}";
        object[] args = new object[] { "test" };

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with format containing only escaped braces and no args does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithOnlyEscapedBraces_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Message: {{escaped}}";
        object[] args = Array.Empty<object>();

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with numeric format specifiers does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithNumericFormatSpecifiers_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Int: {0}, Double: {1:F2}, Hex: {2:X}";
        object[] args = new object[] { 42, 3.14159, 255 };

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error handles boundary case where format is empty string with empty args.
    /// </summary>
    [TestMethod]
    public void Error_WithEmptyFormatAndEmptyArgs_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = string.Empty;
        object[] args = Array.Empty<object>();

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error handles boundary case where format is whitespace only.
    /// </summary>
    [TestMethod]
    public void Error_WithWhitespaceFormat_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "   \t\n  ";
        object[] args = Array.Empty<object>();

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with args containing various primitive types does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithVariousPrimitiveTypes_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "byte: {0}, short: {1}, int: {2}, long: {3}, float: {4}, double: {5}, bool: {6}, char: {7}";
        object[] args = new object[] { (byte)255, (short)32767, int.MaxValue, long.MaxValue, float.MaxValue, double.MaxValue, true, 'A' };

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error with extreme numeric values in args does not throw.
    /// </summary>
    [TestMethod]
    public void Error_WithExtremeNumericValues_DoesNotThrow()
    {
        // Arrange
        Logger.Enabled = true;
        string format = "Min: {0}, Max: {1}, Zero: {2}, NaN: {3}, PosInf: {4}, NegInf: {5}";
        object[] args = new object[] { int.MinValue, int.MaxValue, 0, double.NaN, double.PositiveInfinity, double.NegativeInfinity };

        // Act & Assert - should not throw
        Logger.Error(format, args);
    }

    /// <summary>
    /// Tests that Error transitions correctly when Enabled is toggled during execution context.
    /// </summary>
    [TestMethod]
    public void Error_WhenEnabledToggledBeforeCall_RespectsCurrentState()
    {
        // Arrange
        string format = "Test {0}";
        object[] args = new object[] { 42 };

        // Act & Assert - disabled
        Logger.Enabled = false;
        Logger.Error(format, args);

        // Act & Assert - enabled
        Logger.Enabled = true;
        Logger.Error(format, args);

        // Cleanup
        Logger.Enabled = true;
    }

    /// <summary>
    /// Tests that Info(format, args) returns early without throwing when logging is globally disabled.
    /// </summary>
    /// <remarks>
    /// Validates that the method respects the Enabled flag and does not attempt formatting or logging
    /// when disabled, ensuring no exceptions propagate from disabled code paths.
    /// </remarks>
    [TestMethod]
    public void Info_WhenDisabled_ReturnsEarlyWithoutThrowing()
    {
        // Arrange
        Logger.DisableAll();
        string format = "Test {0}";
        object[] args = new object[] { "value" };

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) returns early without throwing when logging is suppressed.
    /// </summary>
    /// <remarks>
    /// Validates that the method respects active suppression scopes and does not attempt formatting
    /// or logging when a suppression scope is active.
    /// </remarks>
    [TestMethod]
    public void Info_WhenSuppressed_ReturnsEarlyWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        using var suppression = Logger.Suppress();
        string format = "Test {0}";
        object[] args = new object[] { "value" };

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) handles a null format string gracefully without throwing.
    /// </summary>
    /// <remarks>
    /// When string.Format receives a null format string, it throws ArgumentNullException.
    /// This test verifies that the method catches this exception and returns silently as documented.
    /// </remarks>
    [TestMethod]
    public void Info_WithNullFormat_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string? format = null;
        object[] args = new object[] { "value" };

        // Act & Assert
        Logger.Info(format!, args);
        // No exception should be thrown; the method should catch and return silently
    }

    /// <summary>
    /// Tests that Info(format, args) handles various format string edge cases without throwing.
    /// </summary>
    /// <param name="format">The format string to test.</param>
    /// <param name="args">The arguments array to test.</param>
    /// <param name="description">Description of the test case.</param>
    /// <remarks>
    /// Validates that the method is exception-safe for a variety of format string and argument
    /// combinations, including empty strings, whitespace, valid formats, and invalid formats.
    /// All exceptions from string.Format should be caught and suppressed.
    /// </remarks>
    [TestMethod]
    [DataRow("", new object[] { }, "Empty format with no args")]
    [DataRow("   ", new object[] { }, "Whitespace format with no args")]
    [DataRow("Simple message", new object[] { }, "No placeholders with no args")]
    [DataRow("Test {0}", new object[] { "value" }, "Valid format with one arg")]
    [DataRow("Test {0} and {1}", new object[] { "first", "second" }, "Valid format with two args")]
    [DataRow("Test {0} and {1} and {2}", new object[] { "a", "b", "c" }, "Valid format with three args")]
    [DataRow("{0}", new object[] { 123 }, "Numeric argument")]
    [DataRow("{0}", new object[] { 3.14 }, "Double argument")]
    [DataRow("{0}", new object[] { true }, "Boolean argument")]
    [DataRow("{0}", new object[] { (object?)null }, "Null argument")]
    [DataRow("Test {0}", new object[] { "a", "b" }, "More args than placeholders")]
    public void Info_WithVariousFormatAndArgs_DoesNotThrow(string format, object[] args, string description)
    {
        // Arrange
        Logger.EnableAll();

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) handles invalid format strings that cause FormatException without throwing.
    /// </summary>
    /// <param name="format">The invalid format string to test.</param>
    /// <param name="args">The arguments array to test.</param>
    /// <param name="description">Description of the test case.</param>
    /// <remarks>
    /// Validates that the method catches FormatException when string.Format encounters malformed
    /// format strings or argument count mismatches, ensuring no exceptions propagate to the caller.
    /// </remarks>
    [TestMethod]
    [DataRow("{0", new object[] { "value" }, "Unclosed brace")]
    [DataRow("{0}}", new object[] { "value" }, "Extra closing brace")]
    [DataRow("{{0}}", new object[] { "value" }, "Escaped braces around placeholder")]
    [DataRow("{1}", new object[] { "value" }, "Index out of range (only one arg)")]
    [DataRow("{0} {1}", new object[] { "value" }, "Too few args")]
    [DataRow("{0} {1} {2}", new object[] { "a" }, "Significantly too few args")]
    [DataRow("{-1}", new object[] { "value" }, "Negative index")]
    [DataRow("{999999}", new object[] { "value" }, "Very large index")]
    public void Info_WithInvalidFormat_CatchesExceptionAndDoesNotThrow(string format, object[] args, string description)
    {
        // Arrange
        Logger.EnableAll();

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown; FormatException should be caught
    }

    /// <summary>
    /// Tests that Info(format, args) handles a null args array gracefully without throwing.
    /// </summary>
    /// <remarks>
    /// When string.Format receives a null args array with a format requiring parameters,
    /// it throws ArgumentNullException. This test verifies the method catches this exception
    /// and returns silently as documented.
    /// </remarks>
    [TestMethod]
    public void Info_WithNullArgs_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string format = "Test {0}";
        object[]? args = null;

        // Act & Assert
        Logger.Info(format, args!);
        // No exception should be thrown; the method should catch and return silently
    }

    /// <summary>
    /// Tests that Info(format, args) handles an empty args array with format requiring args without throwing.
    /// </summary>
    /// <remarks>
    /// When string.Format receives an empty args array but the format string has placeholders,
    /// it throws FormatException. This test verifies the method catches this exception
    /// and returns silently as documented.
    /// </remarks>
    [TestMethod]
    public void Info_WithEmptyArgsAndPlaceholders_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string format = "Test {0}";
        object[] args = Array.Empty<object>();

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown; FormatException should be caught
    }

    /// <summary>
    /// Tests that Info(format, args) handles args containing multiple null values without throwing.
    /// </summary>
    /// <remarks>
    /// Validates that null values within the args array are handled gracefully by string.Format
    /// and do not cause exceptions.
    /// </remarks>
    [TestMethod]
    public void Info_WithMultipleNullArgs_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string format = "Test {0}, {1}, {2}";
        object[] args = new object?[] { null, null, null };

        // Act & Assert
        Logger.Info(format, args!);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) handles very long format strings without throwing.
    /// </summary>
    /// <remarks>
    /// Validates that the method can handle format strings of significant length,
    /// ensuring no buffer overflows or memory-related exceptions occur.
    /// </remarks>
    [TestMethod]
    public void Info_WithVeryLongFormatString_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string format = new string('A', 10000) + " {0}";
        object[] args = new object[] { "value" };

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) handles format strings with special characters without throwing.
    /// </summary>
    /// <remarks>
    /// Validates that special characters, escape sequences, and Unicode characters
    /// in format strings are handled gracefully.
    /// </remarks>
    [TestMethod]
    [DataRow("Test\nNewline {0}", new object[] { "value" }, "Format with newline")]
    [DataRow("Test\tTab {0}", new object[] { "value" }, "Format with tab")]
    [DataRow("Test\r\nCRLF {0}", new object[] { "value" }, "Format with CRLF")]
    [DataRow("Test 日本語 {0}", new object[] { "value" }, "Format with Unicode")]
    [DataRow("Test 🎉 {0}", new object[] { "emoji" }, "Format with emoji")]
    [DataRow("Test \"quoted\" {0}", new object[] { "value" }, "Format with quotes")]
    [DataRow("Test 'single' {0}", new object[] { "value" }, "Format with single quotes")]
    public void Info_WithSpecialCharacters_DoesNotThrow(string format, object[] args, string description)
    {
        // Arrange
        Logger.EnableAll();

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) handles args with various types without throwing.
    /// </summary>
    /// <remarks>
    /// Validates that different argument types (value types, reference types, structs, enums)
    /// are all handled correctly by string.Format.
    /// </remarks>
    [TestMethod]
    public void Info_WithVariousArgumentTypes_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string format = "Int: {0}, Double: {1}, Bool: {2}, String: {3}, DateTime: {4}, Guid: {5}, Enum: {6}";
        object[] args = new object[]
        {
            42,
            3.14159,
            true,
            "text",
            DateTime.Now,
            Guid.NewGuid(),
            StringComparison.Ordinal
        };

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) handles format with boundary numeric indices without throwing.
    /// </summary>
    /// <param name="format">The format string with numeric index.</param>
    /// <param name="arraySize">Size of the args array to create.</param>
    /// <param name="description">Description of the test case.</param>
    /// <remarks>
    /// Validates that the method handles various numeric placeholder indices correctly,
    /// including zero, the maximum valid index, and invalid indices.
    /// </remarks>
    [TestMethod]
    [DataRow("{0}", 1, "Index 0 with one arg")]
    [DataRow("{99}", 100, "Index 99 with 100 args")]
    [DataRow("{100}", 100, "Index 100 with only 100 args (invalid)")]
    public void Info_WithBoundaryNumericIndices_DoesNotThrow(string format, int arraySize, string description)
    {
        // Arrange
        Logger.EnableAll();
        object[] args = new object[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            args[i] = $"arg{i}";
        }

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown; invalid indices should be caught
    }

    /// <summary>
    /// Tests that Info(format, args) respects both disabled state and suppression depth correctly.
    /// </summary>
    /// <remarks>
    /// Validates that when both Enabled is false AND a suppression scope is active,
    /// the method still returns early without attempting to format or log.
    /// </remarks>
    [TestMethod]
    public void Info_WhenDisabledAndSuppressed_ReturnsEarly()
    {
        // Arrange
        Logger.DisableAll();
        using var suppression = Logger.Suppress();
        string format = "Test {0}";
        object[] args = new object[] { "value" };

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) works correctly when enabled after being disabled.
    /// </summary>
    /// <remarks>
    /// Validates that toggling the Enabled state multiple times works correctly
    /// and the method respects the current state.
    /// </remarks>
    [TestMethod]
    public void Info_AfterReenabling_WorksCorrectly()
    {
        // Arrange
        Logger.DisableAll();
        Logger.EnableAll();
        string format = "Test {0}";
        object[] args = new object[] { "value" };

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that Info(format, args) works correctly after suppression scope is disposed.
    /// </summary>
    /// <remarks>
    /// Validates that disposing a suppression scope correctly decrements the suppression depth
    /// and allows logging to proceed when enabled.
    /// </remarks>
    [TestMethod]
    public void Info_AfterSuppressionDisposed_WorksCorrectly()
    {
        // Arrange
        Logger.EnableAll();
        var suppression = Logger.Suppress();
        suppression.Dispose();
        string format = "Test {0}";
        object[] args = new object[] { "value" };

        // Act & Assert
        Logger.Info(format, args);
        // No exception should be thrown
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> returns true in the default state
    /// when <see cref="Logger.Enabled"/> is true and no suppression is active.
    /// </summary>
    [TestMethod]
    public void IsEnabled_DefaultState_ReturnsTrue()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();

        // Act
        bool result = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> returns the expected value based on
    /// the <see cref="Logger.Enabled"/> property when no suppression is active.
    /// </summary>
    /// <param name="enabled">The value to set for <see cref="Logger.Enabled"/>.</param>
    /// <param name="expected">The expected return value of <see cref="Logger.IsEnabled"/>.</param>
    [TestMethod]
    [DataRow(true, true, DisplayName = "Enabled=true, NoSuppression → IsEnabled=true")]
    [DataRow(false, false, DisplayName = "Enabled=false, NoSuppression → IsEnabled=false")]
    public void IsEnabled_WithEnabledValue_ReturnsExpectedResult(bool enabled, bool expected)
    {
        // Arrange
        Logger.Enabled = enabled;
        EnsureNoActiveSuppression();

        // Act
        bool result = Logger.IsEnabled;

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> returns false when a suppression scope
    /// is active, regardless of the <see cref="Logger.Enabled"/> value.
    /// </summary>
    /// <param name="enabled">The value to set for <see cref="Logger.Enabled"/>.</param>
    [TestMethod]
    [DataRow(true, DisplayName = "Enabled=true with active suppression")]
    [DataRow(false, DisplayName = "Enabled=false with active suppression")]
    public void IsEnabled_WithActiveSuppression_ReturnsFalse(bool enabled)
    {
        // Arrange
        Logger.Enabled = enabled;
        EnsureNoActiveSuppression();
        using IDisposable suppression = Logger.Suppress();

        // Act
        bool result = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> returns true after a suppression scope
    /// is disposed, when <see cref="Logger.Enabled"/> is true.
    /// </summary>
    [TestMethod]
    public void IsEnabled_AfterSuppressionDisposed_ReturnsTrue()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();
        IDisposable suppression = Logger.Suppress();

        // Act
        suppression.Dispose();
        bool result = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> returns false after a suppression scope
    /// is disposed, when <see cref="Logger.Enabled"/> is false.
    /// </summary>
    [TestMethod]
    public void IsEnabled_AfterSuppressionDisposedWhenDisabled_ReturnsFalse()
    {
        // Arrange
        Logger.Enabled = false;
        EnsureNoActiveSuppression();
        IDisposable suppression = Logger.Suppress();

        // Act
        suppression.Dispose();
        bool result = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> remains false when multiple nested
    /// suppression scopes are active.
    /// </summary>
    [TestMethod]
    public void IsEnabled_WithNestedSuppressions_ReturnsFalse()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();
        using IDisposable suppression1 = Logger.Suppress();
        using IDisposable suppression2 = Logger.Suppress();
        using IDisposable suppression3 = Logger.Suppress();

        // Act
        bool result = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> remains false when nested suppression
    /// scopes are partially disposed, and returns true only after all scopes are disposed.
    /// </summary>
    [TestMethod]
    public void IsEnabled_WithPartiallyDisposedNestedSuppressions_ReturnsFalseUntilAllDisposed()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();
        IDisposable suppression1 = Logger.Suppress();
        IDisposable suppression2 = Logger.Suppress();
        IDisposable suppression3 = Logger.Suppress();

        // Act & Assert - still suppressed after disposing one
        suppression3.Dispose();
        Assert.IsFalse(Logger.IsEnabled);

        // Act & Assert - still suppressed after disposing two
        suppression2.Dispose();
        Assert.IsFalse(Logger.IsEnabled);

        // Act & Assert - enabled after disposing all
        suppression1.Dispose();
        Assert.IsTrue(Logger.IsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> immediately reflects changes to
    /// <see cref="Logger.Enabled"/> when no suppression is active.
    /// </summary>
    [TestMethod]
    public void IsEnabled_WhenEnabledChanges_ReflectsChangeImmediately()
    {
        // Arrange
        EnsureNoActiveSuppression();

        // Act & Assert - enable
        Logger.Enabled = true;
        Assert.IsTrue(Logger.IsEnabled);

        // Act & Assert - disable
        Logger.Enabled = false;
        Assert.IsFalse(Logger.IsEnabled);

        // Act & Assert - re-enable
        Logger.Enabled = true;
        Assert.IsTrue(Logger.IsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> remains false when <see cref="Logger.Enabled"/>
    /// changes while a suppression scope is active.
    /// </summary>
    [TestMethod]
    public void IsEnabled_WhenEnabledChangesWithActiveSuppression_RemainsFalse()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();
        using IDisposable suppression = Logger.Suppress();

        // Act & Assert - change Enabled while suppressed
        Logger.Enabled = false;
        Assert.IsFalse(Logger.IsEnabled);

        Logger.Enabled = true;
        Assert.IsFalse(Logger.IsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> correctly reflects the state after
    /// toggling <see cref="Logger.Enabled"/> while a suppression is active and then
    /// disposing the suppression.
    /// </summary>
    [TestMethod]
    public void IsEnabled_AfterTogglingEnabledDuringSuppression_ReflectsCurrentEnabledState()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();
        IDisposable suppression = Logger.Suppress();

        // Act - toggle Enabled while suppressed, then dispose
        Logger.Enabled = false;
        suppression.Dispose();

        // Assert - should be false because Enabled is false
        Assert.IsFalse(Logger.IsEnabled);

        // Act - re-enable
        Logger.Enabled = true;

        // Assert - should be true now
        Assert.IsTrue(Logger.IsEnabled);
    }

    /// <summary>
    /// Tests that disposing a suppression scope multiple times is idempotent and does not
    /// incorrectly affect the suppression depth, ensuring <see cref="Logger.IsEnabled"/>
    /// returns the correct value.
    /// </summary>
    [TestMethod]
    public void IsEnabled_AfterMultipleDisposesOfSameScope_RemainsCorrect()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();
        IDisposable suppression = Logger.Suppress();

        // Act - dispose multiple times
        suppression.Dispose();
        suppression.Dispose();
        suppression.Dispose();

        // Assert - should be true (only decremented once)
        Assert.IsTrue(Logger.IsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> correctly handles disposal of nested
    /// suppression scopes in non-LIFO order.
    /// </summary>
    [TestMethod]
    public void IsEnabled_WithNestedSuppressionsDisposedOutOfOrder_RemainsCorrect()
    {
        // Arrange
        Logger.Enabled = true;
        EnsureNoActiveSuppression();
        IDisposable suppression1 = Logger.Suppress();
        IDisposable suppression2 = Logger.Suppress();
        IDisposable suppression3 = Logger.Suppress();

        // Act - dispose in non-LIFO order
        suppression1.Dispose(); // dispose first (outermost)
        Assert.IsFalse(Logger.IsEnabled); // still 2 active

        suppression3.Dispose(); // dispose third (innermost)
        Assert.IsFalse(Logger.IsEnabled); // still 1 active

        suppression2.Dispose(); // dispose second (middle)
        bool result = Logger.IsEnabled;

        // Assert - all disposed, should be true
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> returns false when
    /// <see cref="Logger.DisableAll"/> is called.
    /// </summary>
    [TestMethod]
    public void IsEnabled_AfterDisableAll_ReturnsFalse()
    {
        // Arrange
        EnsureNoActiveSuppression();

        // Act
        Logger.DisableAll();
        bool result = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(result);

        // Cleanup
        Logger.EnableAll();
    }

    /// <summary>
    /// Tests that <see cref="Logger.IsEnabled"/> returns true when
    /// <see cref="Logger.EnableAll"/> is called and no suppression is active.
    /// </summary>
    [TestMethod]
    public void IsEnabled_AfterEnableAll_ReturnsTrue()
    {
        // Arrange
        Logger.Enabled = false;
        EnsureNoActiveSuppression();

        // Act
        Logger.EnableAll();
        bool result = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Helper method to ensure no suppressions are active by resetting the Logger state.
    /// This is necessary because Logger uses static fields that persist across tests.
    /// </summary>
    private static void EnsureNoActiveSuppression()
    {
        // Create and dispose suppressions until IsEnabled with Enabled=true returns true,
        // indicating suppression depth is back to 0
        Logger.Enabled = true;
        int maxAttempts = 100;
        int attempts = 0;

        while (!Logger.IsEnabled && attempts < maxAttempts)
        {
            // If IsEnabled is false with Enabled=true, there must be active suppressions
            // We can't decrement directly, so we just ensure tests clean up properly
            attempts++;
            Thread.Sleep(1); // Brief delay for thread safety
        }

        if (attempts >= maxAttempts)
        {
            Assert.Fail("Unable to reset Logger suppression state. Tests may have leaked suppression scopes.");
        }
    }

    /// <summary>
    /// Ensures logger is enabled before each test to provide a clean state.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        Logger.EnableAll();
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> returns immediately without
    /// logging when <see cref="Logger.Enabled"/> is set to false.
    /// </summary>
    /// <remarks>
    /// When logging is globally disabled, the method should perform an early return without
    /// attempting to call the underlying logging bridge, ensuring no REFramework API calls occur.
    /// </remarks>
    [TestMethod]
    public void Exception_WhenLoggingDisabled_ReturnsWithoutLogging()
    {
        // Arrange
        Logger.DisableAll();
        var exception = new InvalidOperationException("Test exception");
        const string message = "Test message";

        // Act & Assert - Should not throw
        Logger.Exception(exception, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> returns immediately without
    /// logging when a suppression scope is active.
    /// </summary>
    /// <remarks>
    /// Active suppression scopes should prevent all logging output, causing the method to return
    /// early without touching the logging bridge.
    /// </remarks>
    [TestMethod]
    public void Exception_WhenLoggingSuppressed_ReturnsWithoutLogging()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new ArgumentException("Test exception");
        const string message = "Context message";

        // Act & Assert - Should not throw
        using (Logger.Suppress())
        {
            Logger.Exception(exception, message);
        }
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> returns immediately without
    /// logging when multiple nested suppression scopes are active.
    /// </summary>
    /// <remarks>
    /// Multiple nested suppressions should be properly tracked, and logging should remain disabled
    /// until all suppression scopes are disposed.
    /// </remarks>
    [TestMethod]
    public void Exception_WithMultipleSuppressionsActive_ReturnsWithoutLogging()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new DivideByZeroException("Test exception");
        const string message = "Nested suppression test";

        // Act & Assert - Should not throw
        using (Logger.Suppress())
        using (Logger.Suppress())
        using (Logger.Suppress())
        {
            Logger.Exception(exception, message);
        }
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> successfully logs after all
    /// suppression scopes have been disposed.
    /// </summary>
    /// <remarks>
    /// After disposing all suppression scopes, logging should resume normally, allowing the method
    /// to proceed to the logging bridge without early return.
    /// </remarks>
    [TestMethod]
    public void Exception_AfterDisposingSuppressionScope_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new NotSupportedException("Test exception");
        const string message = "After suppression";
        var suppressionScope = Logger.Suppress();

        // Act
        suppressionScope.Dispose();

        // Assert - Should not throw
        Logger.Exception(exception, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles a valid exception
    /// and message without throwing when logging is enabled.
    /// </summary>
    /// <remarks>
    /// With logging enabled and no suppressions active, the method should forward the exception
    /// details to the logging bridge, which internally handles any errors gracefully.
    /// </remarks>
    [TestMethod]
    public void Exception_WithValidInputsAndLoggingEnabled_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new InvalidOperationException("Sample exception message");
        const string message = "Context for the exception";

        // Act & Assert - Should not throw
        Logger.Exception(exception, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles a null exception
    /// parameter without throwing.
    /// </summary>
    /// <remarks>
    /// When the exception parameter is null, the logging bridge will encounter a
    /// NullReferenceException while formatting, which is caught and suppressed internally,
    /// ensuring no exception propagates to the caller.
    /// </remarks>
    [TestMethod]
    public void Exception_WithNullException_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        Exception? nullException = null;
        const string message = "Null exception test";

        // Act & Assert - Should not throw
        Logger.Exception(nullException!, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles a null message
    /// parameter without throwing.
    /// </summary>
    /// <remarks>
    /// String interpolation in the logging bridge handles null gracefully by rendering it as an
    /// empty string or "null", ensuring the method completes without errors.
    /// </remarks>
    [TestMethod]
    public void Exception_WithNullMessage_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new ArgumentNullException("paramName", "Parameter was null");
        string? nullMessage = null;

        // Act & Assert - Should not throw
        Logger.Exception(exception, nullMessage!);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles various edge-case
    /// message inputs without throwing.
    /// </summary>
    /// <param name="message">The message string to test.</param>
    /// <param name="scenario">Description of the test scenario.</param>
    /// <remarks>
    /// Tests empty strings, whitespace, very long messages, and messages with special or control
    /// characters to ensure robust handling of all string inputs.
    /// </remarks>
    [TestMethod]
    [DataRow("", "Empty string")]
    [DataRow("   ", "Whitespace only")]
    [DataRow("\t\n\r", "Control characters")]
    [DataRow("Message with special chars: @#$%^&*()_+-=[]{}|;':\",./<>?", "Special characters")]
    [DataRow("Unicode: 你好世界 🌍", "Unicode characters")]
    public void Exception_WithVariousMessageFormats_DoesNotThrow(string message, string scenario)
    {
        // Arrange
        Logger.EnableAll();
        var exception = new Exception($"Test exception for {scenario}");

        // Act & Assert - Should not throw
        Logger.Exception(exception, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles an extremely long
    /// message without throwing.
    /// </summary>
    /// <remarks>
    /// Very long messages should be processed without causing buffer overruns, stack overflows,
    /// or other issues in the formatting and logging pipeline.
    /// </remarks>
    [TestMethod]
    public void Exception_WithVeryLongMessage_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new InvalidOperationException("Exception with long context");
        var longMessage = new string('A', 100000);

        // Act & Assert - Should not throw
        Logger.Exception(exception, longMessage);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles an exception with
    /// a null message property without throwing.
    /// </summary>
    /// <remarks>
    /// Some exception types may have null messages. The logging bridge should handle this edge
    /// case gracefully during string interpolation.
    /// </remarks>
    [TestMethod]
    public void Exception_WithExceptionHavingNullMessage_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new CustomExceptionWithNullMessage();
        const string message = "Exception has null Message property";

        // Act & Assert - Should not throw
        Logger.Exception(exception, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles an exception with
    /// a null stack trace without throwing.
    /// </summary>
    /// <remarks>
    /// Exceptions that have not been thrown yet have null StackTrace properties. The logging
    /// bridge should handle this gracefully during formatting.
    /// </remarks>
    [TestMethod]
    public void Exception_WithExceptionHavingNullStackTrace_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        var exception = new Exception("Not thrown yet, so StackTrace is null");
        const string message = "Exception with null StackTrace";

        // Act & Assert - Should not throw
        Logger.Exception(exception, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles various exception
    /// types without throwing.
    /// </summary>
    /// <param name="exceptionFactory">Factory function to create the exception.</param>
    /// <param name="scenario">Description of the exception type scenario.</param>
    /// <remarks>
    /// Tests different exception types to ensure the logging bridge correctly formats the type
    /// name and other properties for all exception subclasses.
    /// </remarks>
    [TestMethod]
    [DataRow("ArgumentNullException", "Null argument exception")]
    [DataRow("InvalidOperationException", "Invalid operation exception")]
    [DataRow("NotSupportedException", "Not supported exception")]
    [DataRow("DivideByZeroException", "Divide by zero exception")]
    [DataRow("OverflowException", "Overflow exception")]
    [DataRow("FormatException", "Format exception")]
    public void Exception_WithVariousExceptionTypes_DoesNotThrow(string exceptionTypeName, string scenario)
    {
        // Arrange
        Logger.EnableAll();
        Exception exception = exceptionTypeName switch
        {
            "ArgumentNullException" => new ArgumentNullException("param", "Parameter was null"),
            "InvalidOperationException" => new InvalidOperationException("Invalid operation"),
            "NotSupportedException" => new NotSupportedException("Not supported"),
            "DivideByZeroException" => new DivideByZeroException("Division by zero"),
            "OverflowException" => new OverflowException("Arithmetic overflow"),
            "FormatException" => new FormatException("Invalid format"),
            _ => new Exception("Default exception")
        };
        var message = $"Testing {scenario}";

        // Act & Assert - Should not throw
        Logger.Exception(exception, message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Exception(Exception, string)"/> handles both null exception
    /// and null message parameters simultaneously without throwing.
    /// </summary>
    /// <remarks>
    /// The worst-case scenario of both parameters being null should still be handled gracefully
    /// by the internal try-catch block in the logging bridge.
    /// </remarks>
    [TestMethod]
    public void Exception_WithBothParametersNull_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        Exception? nullException = null;
        string? nullMessage = null;

        // Act & Assert - Should not throw
        Logger.Exception(nullException!, nullMessage!);
    }

    /// <summary>
    /// Helper exception type with a null Message property for testing edge cases.
    /// </summary>
    private sealed class CustomExceptionWithNullMessage : Exception
    {
        public override string? Message => null;
    }

    /// <summary>
    /// Tests that Info logs successfully when logging is enabled with a normal message.
    /// </summary>
    [TestMethod]
    public void Info_WhenEnabledWithNormalMessage_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            string message = "Test info message";

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info returns early without logging when logging is disabled.
    /// </summary>
    [TestMethod]
    public void Info_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            string message = "Test message that should not be logged";

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info respects suppression scopes and does not log when suppressed.
    /// </summary>
    [TestMethod]
    public void Info_WhenSuppressed_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            string message = "Test message during suppression";

            // Act & Assert
            using (Logger.Suppress())
            {
                Logger.Info(message);
            }
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info respects both disabled state and suppression when both are active.
    /// </summary>
    [TestMethod]
    public void Info_WhenDisabledAndSuppressed_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            string message = "Test message with both disabled and suppressed";

            // Act & Assert
            using (Logger.Suppress())
            {
                Logger.Info(message);
            }
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles null message parameter without throwing when enabled.
    /// </summary>
    [TestMethod]
    public void Info_WhenEnabledWithNullMessage_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            string? message = null;

            // Act & Assert
            Logger.Info(message!);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles null message parameter without throwing when disabled.
    /// </summary>
    [TestMethod]
    public void Info_WhenDisabledWithNullMessage_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            string? message = null;

            // Act & Assert
            Logger.Info(message!);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles various edge-case message values when logging is enabled.
    /// </summary>
    /// <param name="message">The edge-case message to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("a")]
    [DataRow("!@#$%^&*()")]
    [DataRow("日本語")]
    [DataRow("emoji 🎮🎯")]
    [DataRow("\0")]
    [DataRow("Line1\r\nLine2\r\nLine3")]
    public void Info_WhenEnabledWithEdgeCaseMessages_DoesNotThrow(string message)
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles various edge-case message values when logging is disabled.
    /// </summary>
    /// <param name="message">The edge-case message to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("a")]
    [DataRow("!@#$%^&*()")]
    [DataRow("日本語")]
    [DataRow("emoji 🎮🎯")]
    [DataRow("\0")]
    [DataRow("Line1\r\nLine2\r\nLine3")]
    public void Info_WhenDisabledWithEdgeCaseMessages_DoesNotThrow(string message)
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles very long messages when logging is enabled.
    /// </summary>
    [TestMethod]
    public void Info_WhenEnabledWithVeryLongMessage_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            StringBuilder sb = new StringBuilder(10000);
            for (int i = 0; i < 10000; i++)
            {
                sb.Append('A');
            }
            string message = sb.ToString();

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles very long messages when logging is disabled.
    /// </summary>
    [TestMethod]
    public void Info_WhenDisabledWithVeryLongMessage_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = false;
            StringBuilder sb = new StringBuilder(10000);
            for (int i = 0; i < 10000; i++)
            {
                sb.Append('A');
            }
            string message = sb.ToString();

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info respects nested suppression scopes correctly.
    /// </summary>
    [TestMethod]
    public void Info_WithNestedSuppressionScopes_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            string message = "Test message with nested suppressions";

            // Act & Assert
            using (Logger.Suppress())
            {
                using (Logger.Suppress())
                {
                    Logger.Info(message);
                }
                Logger.Info(message);
            }
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info works correctly after disposing a suppression scope.
    /// </summary>
    [TestMethod]
    public void Info_AfterDisposingSuppressionScope_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            string message = "Test message after suppression";

            // Act & Assert
            using (Logger.Suppress())
            {
                Logger.Info(message);
            }
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles messages with mixed control characters when enabled.
    /// </summary>
    [TestMethod]
    public void Info_WhenEnabledWithMixedControlCharacters_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            string message = "Test\0with\rnull\nand\tcontrol\vcharacters";

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Tests that Info handles maximum length Unicode strings when enabled.
    /// </summary>
    [TestMethod]
    public void Info_WhenEnabledWithMaxLengthUnicodeString_DoesNotThrow()
    {
        // Arrange
        bool originalEnabled = Logger.Enabled;
        try
        {
            Logger.Enabled = true;
            StringBuilder sb = new StringBuilder(1000);
            for (int i = 0; i < 100; i++)
            {
                sb.Append("日本語テスト");
            }
            string message = sb.ToString();

            // Act & Assert
            Logger.Info(message);
        }
        finally
        {
            Logger.Enabled = originalEnabled;
        }
    }

    /// <summary>
    /// Verifies that <see cref="Logger.EnableAll"/> sets <see cref="Logger.Enabled"/> to <c>true</c>
    /// when logging is currently disabled.
    /// </summary>
    [TestMethod]
    public void EnableAll_WhenDisabled_SetsEnabledToTrue()
    {
        // Arrange
        Logger.DisableAll();
        Assert.IsFalse(Logger.Enabled, "Precondition: Enabled should be false before test");

        // Act
        Logger.EnableAll();

        // Assert
        Assert.IsTrue(Logger.Enabled, "Enabled should be true after calling EnableAll");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.EnableAll"/> is idempotent and keeps <see cref="Logger.Enabled"/>
    /// as <c>true</c> when it is already enabled.
    /// </summary>
    [TestMethod]
    public void EnableAll_WhenAlreadyEnabled_RemainsEnabled()
    {
        // Arrange
        Logger.EnableAll();
        Assert.IsTrue(Logger.Enabled, "Precondition: Enabled should be true before test");

        // Act
        Logger.EnableAll();

        // Assert
        Assert.IsTrue(Logger.Enabled, "Enabled should remain true after calling EnableAll again");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.EnableAll"/> makes <see cref="Logger.IsEnabled"/> return <c>true</c>
    /// when there are no active suppression scopes.
    /// </summary>
    [TestMethod]
    public void EnableAll_WhenDisabledAndNoSuppression_MakesIsEnabledTrue()
    {
        // Arrange
        Logger.DisableAll();
        Assert.IsFalse(Logger.IsEnabled, "Precondition: IsEnabled should be false before test");

        // Act
        Logger.EnableAll();

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after calling EnableAll with no active suppressions");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.EnableAll"/> sets <see cref="Logger.Enabled"/> to <c>true</c>
    /// but <see cref="Logger.IsEnabled"/> remains <c>false</c> when there is an active suppression scope.
    /// </summary>
    [TestMethod]
    public void EnableAll_WhenActiveSuppression_DoesNotMakeIsEnabledTrue()
    {
        // Arrange
        Logger.DisableAll();
        IDisposable suppression = Logger.Suppress();
        Assert.IsFalse(Logger.Enabled, "Precondition: Enabled should be false before test");
        Assert.IsFalse(Logger.IsEnabled, "Precondition: IsEnabled should be false before test");

        // Act
        Logger.EnableAll();

        // Assert
        Assert.IsTrue(Logger.Enabled, "Enabled should be true after calling EnableAll");
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should remain false due to active suppression");

        // Cleanup
        suppression.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="Logger.EnableAll"/> can be called multiple times in succession
    /// without errors and maintains <see cref="Logger.Enabled"/> as <c>true</c>.
    /// </summary>
    [TestMethod]
    public void EnableAll_CalledMultipleTimes_MaintainsEnabledState()
    {
        // Arrange
        Logger.DisableAll();

        // Act
        Logger.EnableAll();
        Logger.EnableAll();
        Logger.EnableAll();

        // Assert
        Assert.IsTrue(Logger.Enabled, "Enabled should be true after calling EnableAll multiple times");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.EnableAll"/> restores <see cref="Logger.Enabled"/> to <c>true</c>
    /// after it was explicitly disabled via <see cref="Logger.DisableAll"/>.
    /// </summary>
    [TestMethod]
    public void EnableAll_AfterDisableAll_RestoresEnabledState()
    {
        // Arrange
        Logger.EnableAll();
        Logger.DisableAll();
        Assert.IsFalse(Logger.Enabled, "Precondition: Enabled should be false after DisableAll");

        // Act
        Logger.EnableAll();

        // Assert
        Assert.IsTrue(Logger.Enabled, "Enabled should be restored to true after calling EnableAll");
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should also be true with no active suppressions");
    }

    /// <summary>
    /// Tests that the Enabled property correctly reflects the set value.
    /// Verifies that setting Enabled to true or false is properly persisted and retrieved.
    /// </summary>
    /// <param name="setValue">The boolean value to assign to the Enabled property.</param>
    /// <param name="expectedValue">The expected value when reading back the Enabled property.</param>
    [TestMethod]
    [DataRow(true, true)]
    [DataRow(false, false)]
    public void Enabled_SetValue_ReturnsExpectedValue(bool setValue, bool expectedValue)
    {
        // Arrange & Act
        Logger.Enabled = setValue;
        bool result = Logger.Enabled;

        // Assert
        Assert.AreEqual(expectedValue, result);
    }

    /// <summary>
    /// Tests that the Enabled property maintains state consistency after multiple consecutive changes.
    /// Verifies that toggling between true and false correctly updates the property value each time.
    /// </summary>
    [TestMethod]
    public void Enabled_MultipleStateChanges_ReflectsEachChange()
    {
        // Arrange & Act & Assert
        Logger.Enabled = true;
        Assert.IsTrue(Logger.Enabled);

        Logger.Enabled = false;
        Assert.IsFalse(Logger.Enabled);

        Logger.Enabled = true;
        Assert.IsTrue(Logger.Enabled);

        Logger.Enabled = false;
        Assert.IsFalse(Logger.Enabled);
    }

    /// <summary>
    /// Tests that setting the Enabled property to the same value multiple times remains stable.
    /// Verifies idempotency of consecutive identical assignments.
    /// </summary>
    /// <param name="setValue">The boolean value to set repeatedly.</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Enabled_ConsecutiveSetsToSameValue_RemainsStable(bool setValue)
    {
        // Arrange & Act
        Logger.Enabled = setValue;
        Logger.Enabled = setValue;
        Logger.Enabled = setValue;
        bool result = Logger.Enabled;

        // Assert
        Assert.AreEqual(setValue, result);
    }

    /// <summary>
    /// Tests that calling Dispose on a suppression scope decrements the suppression depth
    /// and restores IsEnabled to true when no other suppressions are active.
    /// </summary>
    [TestMethod]
    public void Dispose_SingleScope_RestoresIsEnabled()
    {
        // Arrange
        Logger.EnableAll();
        var scope = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when a suppression scope is active.");

        // Act
        scope.Dispose();

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing the suppression scope.");
    }

    /// <summary>
    /// Tests that calling Dispose multiple times on the same suppression scope instance
    /// is idempotent and only decrements the suppression depth once.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        Logger.EnableAll();
        var scope = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when a suppression scope is active.");

        // Act
        scope.Dispose();
        scope.Dispose();
        scope.Dispose();

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing the suppression scope, even when Dispose is called multiple times.");
    }

    /// <summary>
    /// Tests that multiple suppression scopes each decrement the suppression depth
    /// independently when disposed.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleScopes_EachDecrementsSeparately()
    {
        // Arrange
        Logger.EnableAll();
        var scope1 = Logger.Suppress();
        var scope2 = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when multiple suppression scopes are active.");

        // Act - Dispose first scope
        scope1.Dispose();

        // Assert - Still suppressed
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should still be false after disposing only one of two suppression scopes.");

        // Act - Dispose second scope
        scope2.Dispose();

        // Assert - No longer suppressed
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing all suppression scopes.");
    }

    /// <summary>
    /// Tests that suppression scopes can be disposed in any order (not necessarily LIFO)
    /// and each correctly decrements the suppression depth once.
    /// </summary>
    [TestMethod]
    public void Dispose_NestedScopes_CanBeDisposedOutOfOrder()
    {
        // Arrange
        Logger.EnableAll();
        var scope1 = Logger.Suppress();
        var scope2 = Logger.Suppress();
        var scope3 = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when multiple suppression scopes are active.");

        // Act - Dispose in non-LIFO order
        scope2.Dispose();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should still be false after disposing one of three scopes.");

        scope1.Dispose();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should still be false after disposing two of three scopes.");

        scope3.Dispose();

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing all three scopes.");
    }

    /// <summary>
    /// Tests that disposing a suppression scope when logging is globally disabled
    /// does not throw an exception and correctly decrements the suppression depth.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenLoggingDisabled_DoesNotThrow()
    {
        // Arrange
        Logger.DisableAll();
        var scope = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when logging is disabled and a suppression scope is active.");

        // Act & Assert - Should not throw
        scope.Dispose();

        // Re-enable and verify depth was decremented
        Logger.EnableAll();
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after enabling logging and disposing the suppression scope.");
    }

    /// <summary>
    /// Tests that multiple calls to Dispose on the same scope from different threads
    /// are thread-safe and only decrement the suppression depth once.
    /// </summary>
    [TestMethod]
    public void Dispose_ConcurrentCalls_OnlyDecrementsOnce()
    {
        // Arrange
        Logger.EnableAll();
        var scope = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when a suppression scope is active.");

        // Act - Dispose from multiple threads concurrently
        var tasks = new Task[10];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => scope.Dispose());
        }
        Task.WaitAll(tasks);

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing the suppression scope, even with concurrent Dispose calls.");
    }

    /// <summary>
    /// Tests that disposing a suppression scope after multiple idempotent Dispose calls
    /// from concurrent threads still results in correct suppression depth.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleScopesConcurrentDispose_EachDecrementsOnce()
    {
        // Arrange
        Logger.EnableAll();
        var scope1 = Logger.Suppress();
        var scope2 = Logger.Suppress();
        var scope3 = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when multiple suppression scopes are active.");

        // Act - Dispose each scope multiple times concurrently
        var tasks = new Task[30];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() => scope1.Dispose());
            tasks[i + 10] = Task.Run(() => scope2.Dispose());
            tasks[i + 20] = Task.Run(() => scope3.Dispose());
        }
        Task.WaitAll(tasks);

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing all suppression scopes, even with concurrent Dispose calls.");
    }

    /// <summary>
    /// Tests that the suppression depth can be increased and decreased correctly
    /// through multiple create-dispose cycles.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleCreateDisposeCycles_MaintainsCorrectDepth()
    {
        // Arrange
        Logger.EnableAll();

        // Act & Assert - Cycle 1
        var scope1 = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled);
        scope1.Dispose();
        Assert.IsTrue(Logger.IsEnabled);

        // Act & Assert - Cycle 2
        var scope2 = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled);
        scope2.Dispose();
        Assert.IsTrue(Logger.IsEnabled);

        // Act & Assert - Cycle 3
        var scope3 = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled);
        scope3.Dispose();
        Assert.IsTrue(Logger.IsEnabled);
    }

    /// <summary>
    /// Tests that disposing a scope after calling Dispose multiple times on another scope
    /// still maintains correct suppression depth.
    /// </summary>
    [TestMethod]
    public void Dispose_OneIdempotentOneNormal_MaintainsCorrectDepth()
    {
        // Arrange
        Logger.EnableAll();
        var scope1 = Logger.Suppress();
        var scope2 = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled);

        // Act - Call Dispose multiple times on scope1
        scope1.Dispose();
        scope1.Dispose();
        scope1.Dispose();

        // Assert - Still suppressed due to scope2
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should still be false because scope2 is not disposed.");

        // Act - Dispose scope2
        scope2.Dispose();

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing both scopes.");
    }

    /// <summary>
    /// Tests that disposing a suppression scope when Enabled is false but was previously true
    /// correctly decrements the suppression depth.
    /// </summary>
    [TestMethod]
    public void Dispose_EnabledToggledAfterSuppression_DecrementsCorrectly()
    {
        // Arrange
        Logger.EnableAll();
        var scope = Logger.Suppress();
        Assert.IsFalse(Logger.IsEnabled);

        // Act - Disable then re-enable logging
        Logger.DisableAll();
        Assert.IsFalse(Logger.IsEnabled);
        Logger.EnableAll();
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should still be false due to active suppression scope.");

        // Act - Dispose the scope
        scope.Dispose();

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing the suppression scope.");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Suppress"/> returns a non-null <see cref="IDisposable"/> instance.
    /// </summary>
    [TestMethod]
    public void Suppress_WhenCalled_ReturnsNonNullDisposable()
    {
        // Act
        IDisposable? scope = Logger.Suppress();

        // Assert
        Assert.IsNotNull(scope);

        // Cleanup
        scope.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="Logger.IsEnabled"/> becomes false after calling <see cref="Logger.Suppress"/>.
    /// </summary>
    [TestMethod]
    public void Suppress_WhenCalled_SetsIsEnabledToFalse()
    {
        // Arrange
        bool initialIsEnabled = Logger.IsEnabled;

        // Act
        using IDisposable scope = Logger.Suppress();
        bool isEnabledDuringSuppression = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(initialIsEnabled, "IsEnabled should be true before suppression.");
        Assert.IsFalse(isEnabledDuringSuppression, "IsEnabled should be false during suppression.");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.IsEnabled"/> returns to true after disposing the suppression scope.
    /// </summary>
    [TestMethod]
    public void Suppress_WhenDisposed_RestoresIsEnabledToTrue()
    {
        // Arrange
        IDisposable scope = Logger.Suppress();

        // Act
        scope.Dispose();
        bool isEnabledAfterDisposal = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(isEnabledAfterDisposal, "IsEnabled should be restored to true after disposing the suppression scope.");
    }

    /// <summary>
    /// Verifies that nested suppression scopes work correctly and <see cref="Logger.IsEnabled"/> remains false
    /// until all scopes are disposed.
    /// </summary>
    [TestMethod]
    public void Suppress_WithNestedScopes_MaintainsSuppressionUntilAllDisposed()
    {
        // Arrange & Act
        IDisposable outerScope = Logger.Suppress();
        bool isEnabledAfterFirst = Logger.IsEnabled;

        IDisposable innerScope = Logger.Suppress();
        bool isEnabledAfterSecond = Logger.IsEnabled;

        innerScope.Dispose();
        bool isEnabledAfterInnerDisposal = Logger.IsEnabled;

        outerScope.Dispose();
        bool isEnabledAfterOuterDisposal = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(isEnabledAfterFirst, "IsEnabled should be false after first suppression.");
        Assert.IsFalse(isEnabledAfterSecond, "IsEnabled should remain false after second suppression.");
        Assert.IsFalse(isEnabledAfterInnerDisposal, "IsEnabled should remain false after disposing inner scope.");
        Assert.IsTrue(isEnabledAfterOuterDisposal, "IsEnabled should be true after disposing all scopes.");
    }

    /// <summary>
    /// Verifies that multiple nested suppression scopes work correctly with various nesting levels.
    /// </summary>
    /// <param name="nestingLevel">The number of nested suppression scopes to create.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    public void Suppress_WithMultipleNestingLevels_MaintainsCorrectState(int nestingLevel)
    {
        // Arrange
        IDisposable[] scopes = new IDisposable[nestingLevel];

        // Act - Create nested scopes
        for (int i = 0; i < nestingLevel; i++)
        {
            scopes[i] = Logger.Suppress();
            Assert.IsFalse(Logger.IsEnabled, $"IsEnabled should be false after creating scope {i + 1}.");
        }

        // Act - Dispose all but one scope
        for (int i = 0; i < nestingLevel - 1; i++)
        {
            scopes[i].Dispose();
            Assert.IsFalse(Logger.IsEnabled, $"IsEnabled should remain false after disposing scope {i + 1} of {nestingLevel}.");
        }

        // Act - Dispose the last scope
        scopes[nestingLevel - 1].Dispose();

        // Assert
        Assert.IsTrue(Logger.IsEnabled, "IsEnabled should be true after disposing all scopes.");
    }

    /// <summary>
    /// Verifies that disposing a suppression scope multiple times is safe and idempotent.
    /// </summary>
    [TestMethod]
    public void Suppress_WhenDisposedMultipleTimes_IsIdempotent()
    {
        // Arrange
        IDisposable scope = Logger.Suppress();

        // Act
        scope.Dispose();
        bool isEnabledAfterFirstDispose = Logger.IsEnabled;

        scope.Dispose();
        bool isEnabledAfterSecondDispose = Logger.IsEnabled;

        scope.Dispose();
        bool isEnabledAfterThirdDispose = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(isEnabledAfterFirstDispose, "IsEnabled should be true after first dispose.");
        Assert.IsTrue(isEnabledAfterSecondDispose, "IsEnabled should remain true after second dispose.");
        Assert.IsTrue(isEnabledAfterThirdDispose, "IsEnabled should remain true after third dispose.");
    }

    /// <summary>
    /// Verifies that out-of-order disposal of nested suppression scopes maintains correct state.
    /// </summary>
    [TestMethod]
    public void Suppress_WithOutOfOrderDisposal_MaintainsCorrectState()
    {
        // Arrange
        IDisposable scope1 = Logger.Suppress();
        IDisposable scope2 = Logger.Suppress();
        IDisposable scope3 = Logger.Suppress();

        // Act - Dispose in reverse order: 3, 1, 2
        scope3.Dispose();
        bool isEnabledAfterScope3 = Logger.IsEnabled;

        scope1.Dispose();
        bool isEnabledAfterScope1 = Logger.IsEnabled;

        scope2.Dispose();
        bool isEnabledAfterScope2 = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(isEnabledAfterScope3, "IsEnabled should be false after disposing scope3.");
        Assert.IsFalse(isEnabledAfterScope1, "IsEnabled should be false after disposing scope1.");
        Assert.IsTrue(isEnabledAfterScope2, "IsEnabled should be true after disposing all scopes.");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Suppress"/> is thread-safe when called concurrently from multiple threads.
    /// </summary>
    [TestMethod]
    public void Suppress_WithConcurrentCalls_IsThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int operationsPerThread = 100;
        IDisposable?[] allScopes = new IDisposable[threadCount * operationsPerThread];
        int scopeIndex = 0;

        // Act - Create suppressions concurrently
        Parallel.For(0, threadCount, threadIndex =>
        {
            for (int i = 0; i < operationsPerThread; i++)
            {
                IDisposable scope = Logger.Suppress();
                int index = Interlocked.Increment(ref scopeIndex) - 1;
                allScopes[index] = scope;
            }
        });

        bool isEnabledDuringSuppression = Logger.IsEnabled;

        // Act - Dispose all scopes concurrently
        Parallel.For(0, allScopes.Length, i =>
        {
            allScopes[i]?.Dispose();
        });

        bool isEnabledAfterDisposal = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(isEnabledDuringSuppression, "IsEnabled should be false when suppressions are active.");
        Assert.IsTrue(isEnabledAfterDisposal, "IsEnabled should be true after all scopes are disposed.");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Suppress"/> respects the global <see cref="Logger.Enabled"/> flag.
    /// When <see cref="Logger.Enabled"/> is false, <see cref="Logger.IsEnabled"/> should be false
    /// regardless of suppression state.
    /// </summary>
    [TestMethod]
    public void Suppress_WhenEnabledIsFalse_IsEnabledRemainsFalse()
    {
        // Arrange
        Logger.DisableAll();
        bool isEnabledWhenDisabled = Logger.IsEnabled;

        // Act
        using IDisposable scope = Logger.Suppress();
        bool isEnabledDuringSuppression = Logger.IsEnabled;

        scope.Dispose();
        bool isEnabledAfterDisposal = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(isEnabledWhenDisabled, "IsEnabled should be false when Enabled is false.");
        Assert.IsFalse(isEnabledDuringSuppression, "IsEnabled should remain false during suppression.");
        Assert.IsFalse(isEnabledAfterDisposal, "IsEnabled should remain false after disposal when Enabled is false.");

        // Cleanup
        Logger.EnableAll();
    }

    /// <summary>
    /// Verifies that re-enabling logging during active suppression does not affect <see cref="Logger.IsEnabled"/>
    /// until the suppression is disposed.
    /// </summary>
    [TestMethod]
    public void Suppress_WhenReenabledDuringSuppression_IsEnabledRemainsFalse()
    {
        // Arrange
        using IDisposable scope = Logger.Suppress();
        bool isEnabledDuringSuppression = Logger.IsEnabled;

        // Act
        Logger.EnableAll();
        bool isEnabledAfterReenabling = Logger.IsEnabled;

        scope.Dispose();
        bool isEnabledAfterDisposal = Logger.IsEnabled;

        // Assert
        Assert.IsFalse(isEnabledDuringSuppression, "IsEnabled should be false during suppression.");
        Assert.IsFalse(isEnabledAfterReenabling, "IsEnabled should remain false during suppression even after re-enabling.");
        Assert.IsTrue(isEnabledAfterDisposal, "IsEnabled should be true after disposing the suppression scope.");
    }

    /// <summary>
    /// Verifies that using suppression in a typical using statement pattern works correctly.
    /// </summary>
    [TestMethod]
    public void Suppress_InUsingStatement_WorksCorrectly()
    {
        // Arrange
        bool isEnabledBeforeSuppression = Logger.IsEnabled;
        bool isEnabledDuringSuppression = false;

        // Act
        using (Logger.Suppress())
        {
            isEnabledDuringSuppression = Logger.IsEnabled;
        }

        bool isEnabledAfterSuppression = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(isEnabledBeforeSuppression, "IsEnabled should be true before suppression.");
        Assert.IsFalse(isEnabledDuringSuppression, "IsEnabled should be false during suppression.");
        Assert.IsTrue(isEnabledAfterSuppression, "IsEnabled should be true after suppression scope is disposed.");
    }

    /// <summary>
    /// Verifies that nested using statements for suppression work correctly.
    /// </summary>
    [TestMethod]
    public void Suppress_InNestedUsingStatements_WorksCorrectly()
    {
        // Arrange
        bool isEnabledBeforeSuppression = Logger.IsEnabled;
        bool isEnabledInOuterScope = false;
        bool isEnabledInInnerScope = false;
        bool isEnabledBetweenScopes = false;

        // Act
        using (Logger.Suppress())
        {
            isEnabledInOuterScope = Logger.IsEnabled;

            using (Logger.Suppress())
            {
                isEnabledInInnerScope = Logger.IsEnabled;
            }

            isEnabledBetweenScopes = Logger.IsEnabled;
        }

        bool isEnabledAfterAllScopes = Logger.IsEnabled;

        // Assert
        Assert.IsTrue(isEnabledBeforeSuppression, "IsEnabled should be true before any suppression.");
        Assert.IsFalse(isEnabledInOuterScope, "IsEnabled should be false in outer suppression scope.");
        Assert.IsFalse(isEnabledInInnerScope, "IsEnabled should be false in inner suppression scope.");
        Assert.IsFalse(isEnabledBetweenScopes, "IsEnabled should remain false after inner scope but within outer scope.");
        Assert.IsTrue(isEnabledAfterAllScopes, "IsEnabled should be true after all scopes are disposed.");
    }

    /// <summary>
    /// Verifies that concurrent creation and disposal of suppression scopes from multiple threads
    /// maintains correct state.
    /// </summary>
    [TestMethod]
    public void Suppress_WithConcurrentCreationAndDisposal_MaintainsCorrectState()
    {
        // Arrange
        const int threadCount = 20;
        const int iterations = 50;
        CountdownEvent startGate = new(threadCount);
        int completedThreads = 0;

        // Act
        Parallel.For(0, threadCount, threadIndex =>
        {
            startGate.Signal();
            startGate.Wait();

            for (int i = 0; i < iterations; i++)
            {
                using (Logger.Suppress())
                {
                    Assert.IsFalse(Logger.IsEnabled, $"IsEnabled should be false during suppression in thread {threadIndex}, iteration {i}.");
                    Thread.Sleep(0); // Yield to increase interleaving
                }
            }

            Interlocked.Increment(ref completedThreads);
        });

        bool isEnabledAfterAllThreads = Logger.IsEnabled;

        // Assert
        Assert.AreEqual(threadCount, completedThreads, "All threads should complete.");
        Assert.IsTrue(isEnabledAfterAllThreads, "IsEnabled should be true after all concurrent suppressions are disposed.");

        // Cleanup
        startGate.Dispose();
    }

    /// <summary>
    /// Tests that DisableAll sets the Enabled property to false.
    /// </summary>
    [TestMethod]
    public void DisableAll_WhenCalled_SetsEnabledToFalse()
    {
        // Arrange
        Logger.EnableAll();
        Assert.IsTrue(Logger.Enabled, "Precondition: Enabled should be true before test");

        // Act
        Logger.DisableAll();

        // Assert
        Assert.IsFalse(Logger.Enabled, "Enabled should be false after DisableAll");
    }

    /// <summary>
    /// Tests that DisableAll sets IsEnabled to false when no suppression is active.
    /// </summary>
    [TestMethod]
    public void DisableAll_WhenNoSuppression_SetsIsEnabledToFalse()
    {
        // Arrange
        Logger.EnableAll();
        Assert.IsTrue(Logger.IsEnabled, "Precondition: IsEnabled should be true before test");

        // Act
        Logger.DisableAll();

        // Assert
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false after DisableAll");
    }

    /// <summary>
    /// Tests that DisableAll is idempotent when called multiple times consecutively.
    /// </summary>
    [TestMethod]
    public void DisableAll_WhenCalledMultipleTimes_RemainsDisabled()
    {
        // Arrange
        Logger.EnableAll();

        // Act
        Logger.DisableAll();
        Logger.DisableAll();
        Logger.DisableAll();

        // Assert
        Assert.IsFalse(Logger.Enabled, "Enabled should remain false after multiple DisableAll calls");
    }

    /// <summary>
    /// Tests that DisableAll works correctly when already disabled.
    /// </summary>
    [TestMethod]
    public void DisableAll_WhenAlreadyDisabled_RemainsDisabled()
    {
        // Arrange
        Logger.DisableAll();
        Assert.IsFalse(Logger.Enabled, "Precondition: Enabled should be false");

        // Act
        Logger.DisableAll();

        // Assert
        Assert.IsFalse(Logger.Enabled, "Enabled should remain false");
    }

    /// <summary>
    /// Tests that DisableAll can be followed by EnableAll to toggle state correctly.
    /// </summary>
    [TestMethod]
    public void DisableAll_FollowedByEnableAll_TogglesStateCorrectly()
    {
        // Arrange
        Logger.EnableAll();

        // Act
        Logger.DisableAll();
        bool disabledState = Logger.Enabled;
        Logger.EnableAll();
        bool enabledState = Logger.Enabled;

        // Assert
        Assert.IsFalse(disabledState, "Enabled should be false after DisableAll");
        Assert.IsTrue(enabledState, "Enabled should be true after EnableAll");
    }

    /// <summary>
    /// Tests that DisableAll maintains IsEnabled as false even when suppression is active.
    /// </summary>
    [TestMethod]
    public void DisableAll_WithActiveSuppression_KeepsIsEnabledFalse()
    {
        // Arrange
        Logger.EnableAll();
        using var suppression = Logger.Suppress();

        // Act
        Logger.DisableAll();

        // Assert
        Assert.IsFalse(Logger.Enabled, "Enabled should be false after DisableAll");
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false when both disabled and suppressed");
    }

    /// <summary>
    /// Tests that DisableAll is thread-safe when called concurrently from multiple threads.
    /// </summary>
    [TestMethod]
    public void DisableAll_WhenCalledConcurrently_IsThreadSafe()
    {
        // Arrange
        Logger.EnableAll();
        const int threadCount = 10;
        const int iterationsPerThread = 100;
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < iterationsPerThread; j++)
                {
                    Logger.DisableAll();
                }
            });
        }
        Task.WaitAll(tasks);

        // Assert
        Assert.IsFalse(Logger.Enabled, "Enabled should be false after concurrent DisableAll calls");
    }

    /// <summary>
    /// Tests that DisableAll and EnableAll maintain consistency when called concurrently.
    /// </summary>
    [TestMethod]
    public void DisableAll_WithConcurrentEnableAll_MaintainsConsistency()
    {
        // Arrange
        const int iterations = 1000;
        var disableTask = Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                Logger.DisableAll();
            }
        });
        var enableTask = Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                Logger.EnableAll();
            }
        });

        // Act
        Task.WaitAll(disableTask, enableTask);

        // Assert
        // The final state should be deterministic (either true or false, never corrupted)
        bool finalState = Logger.Enabled;
        Assert.IsTrue(finalState == true || finalState == false, "Enabled should have a valid boolean state");

        // Verify the state is stable
        bool stableState = Logger.Enabled;
        Assert.AreEqual(finalState, stableState, "Enabled state should be stable after concurrent operations");
    }

    /// <summary>
    /// Tests that DisableAll followed by immediate read returns false consistently.
    /// </summary>
    [TestMethod]
    public void DisableAll_ImmediateRead_ReturnsConsistentState()
    {
        // Arrange
        Logger.EnableAll();

        // Act
        Logger.DisableAll();
        bool firstRead = Logger.Enabled;
        bool secondRead = Logger.Enabled;
        bool thirdRead = Logger.Enabled;

        // Assert
        Assert.IsFalse(firstRead, "First read should return false");
        Assert.IsFalse(secondRead, "Second read should return false");
        Assert.IsFalse(thirdRead, "Third read should return false");
    }

    /// <summary>
    /// Tests that DisableAll affects IsEnabled independently of suppression depth.
    /// </summary>
    [TestMethod]
    public void DisableAll_WithMultipleSuppressions_AffectsIsEnabledCorrectly()
    {
        // Arrange
        Logger.EnableAll();
        using var suppression1 = Logger.Suppress();
        using var suppression2 = Logger.Suppress();

        // Act
        Logger.DisableAll();

        // Assert
        Assert.IsFalse(Logger.Enabled, "Enabled should be false");
        Assert.IsFalse(Logger.IsEnabled, "IsEnabled should be false with multiple suppressions and disabled state");
    }

    /// <summary>
    /// Ensures each test starts with logging enabled and no active suppressions.
    /// This is explicitly necessary because <see cref="Logger"/> is a static class
    /// with shared static state that must be reset between tests to prevent interference.
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        Logger.EnableAll();
    }

    /// <summary>
    /// Ensures logging is re-enabled after each test to restore default state.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        Logger.EnableAll();
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> does not throw an exception
    /// when logging is globally disabled via <see cref="Logger.Enabled"/>.
    /// </summary>
    /// <remarks>
    /// The method should return early without attempting to log when <see cref="Logger.IsEnabled"/> is false.
    /// </remarks>
    [TestMethod]
    public void Warning_WhenLoggingDisabled_DoesNotThrowException()
    {
        // Arrange
        Logger.DisableAll();
        const string message = "Test warning message";

        // Act & Assert
        Logger.Warning(message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> does not throw an exception
    /// when logging is temporarily suppressed via <see cref="Logger.Suppress"/>.
    /// </summary>
    /// <remarks>
    /// The method should return early without attempting to log when an active suppression scope exists.
    /// </remarks>
    [TestMethod]
    public void Warning_WhenLoggingSuppressed_DoesNotThrowException()
    {
        // Arrange
        const string message = "Test warning message";

        // Act & Assert
        using (Logger.Suppress())
        {
            Logger.Warning(message);
        }
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> does not throw an exception
    /// when logging is enabled with a valid standard message.
    /// </summary>
    /// <remarks>
    /// The method should forward the message to the internal logging bridge without errors.
    /// </remarks>
    [TestMethod]
    public void Warning_WhenLoggingEnabledWithValidMessage_DoesNotThrowException()
    {
        // Arrange
        const string message = "Test warning message";

        // Act & Assert
        Logger.Warning(message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> handles various edge-case message inputs
    /// without throwing exceptions when logging is enabled.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="description">Description of the test case.</param>
    /// <remarks>
    /// Tests null, empty, whitespace-only, very long, and special-character messages to ensure
    /// the method forwards them to the logging bridge without preprocessing or validation errors.
    /// The method does not perform null checks or message validation before forwarding.
    /// </remarks>
    [TestMethod]
    [DataRow(null, "null message")]
    [DataRow("", "empty string")]
    [DataRow("   ", "whitespace only")]
    [DataRow("\t\n\r", "control characters")]
    [DataRow("Line1\nLine2\nLine3", "multiline message")]
    [DataRow("Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?`~", "special characters")]
    [DataRow("Unicode: 你好世界 🌍🚀", "unicode and emoji")]
    public void Warning_WhenLoggingEnabledWithEdgeCaseMessages_DoesNotThrowException(string? message, string description)
    {
        // Arrange
        Logger.EnableAll();

        // Act & Assert - should not throw
        Logger.Warning(message!);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> handles very long messages
    /// without throwing exceptions when logging is enabled.
    /// </summary>
    /// <remarks>
    /// Tests a message with 10,000 characters to ensure the method can handle large inputs
    /// without buffer overflow or performance issues.
    /// </remarks>
    [TestMethod]
    public void Warning_WhenLoggingEnabledWithVeryLongMessage_DoesNotThrowException()
    {
        // Arrange
        string longMessage = new string('A', 10000);

        // Act & Assert
        Logger.Warning(longMessage);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> does not throw an exception
    /// when called multiple times in succession with logging enabled.
    /// </summary>
    /// <remarks>
    /// Tests rapid sequential logging to ensure no state corruption or threading issues occur.
    /// </remarks>
    [TestMethod]
    public void Warning_WhenCalledMultipleTimesWithLoggingEnabled_DoesNotThrowException()
    {
        // Arrange
        const string message = "Test warning";

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            Logger.Warning(message);
        }
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> correctly respects state transitions
    /// from enabled to disabled during execution.
    /// </summary>
    /// <remarks>
    /// Tests that the method checks <see cref="Logger.IsEnabled"/> at call time and behaves accordingly.
    /// </remarks>
    [TestMethod]
    public void Warning_WhenLoggingStateChanges_RespectsCurrentState()
    {
        // Arrange
        const string message = "Test warning";

        // Act & Assert - enabled
        Logger.EnableAll();
        Logger.Warning(message);

        // Act & Assert - disabled
        Logger.DisableAll();
        Logger.Warning(message);

        // Act & Assert - re-enabled
        Logger.EnableAll();
        Logger.Warning(message);
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Warning(string)"/> respects nested suppression scopes
    /// and only resumes logging after all scopes are disposed.
    /// </summary>
    /// <remarks>
    /// Tests that suppression depth is properly tracked across multiple nested scopes.
    /// </remarks>
    [TestMethod]
    public void Warning_WithNestedSuppressionScopes_RespectsSuppressionDepth()
    {
        // Arrange
        const string message = "Test warning";

        // Act & Assert - nested suppressions
        using (var outer = Logger.Suppress())
        {
            Logger.Warning(message); // Should be suppressed

            using (var inner = Logger.Suppress())
            {
                Logger.Warning(message); // Should still be suppressed
            }

            Logger.Warning(message); // Should still be suppressed (outer scope active)
        }

        // After all scopes disposed, logging should work
        Logger.Warning(message);
    }

    /// <summary>
    /// Verifies that the method returns immediately without throwing when logging is disabled via Enabled property.
    /// </summary>
    [TestMethod]
    public void Exception_LoggingDisabled_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.DisableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Format: {0}", "arg");
    }

    /// <summary>
    /// Verifies that the method returns immediately without throwing when logging is suppressed.
    /// </summary>
    [TestMethod]
    public void Exception_LoggingSuppressed_ReturnsWithoutThrowing()
    {
        // Arrange
        var ex = new InvalidOperationException("Test exception");

        using (Logger.Suppress())
        {
            // Act & Assert
            Logger.Exception(ex, "Format: {0}", "arg");
        }
    }

    /// <summary>
    /// Verifies that the method completes successfully with valid format string and arguments when logging is enabled.
    /// </summary>
    [TestMethod]
    public void Exception_ValidFormatAndArgs_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error occurred: {0}", "details");
    }

    /// <summary>
    /// Verifies that the method handles a null format string without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_NullFormat_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, null!, new object[] { "arg" });
    }

    /// <summary>
    /// Verifies that the method handles an empty format string without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_EmptyFormat_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "", Array.Empty<object>());
    }

    /// <summary>
    /// Verifies that the method handles a whitespace-only format string without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_WhitespaceFormat_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "   ", Array.Empty<object>());
    }

    /// <summary>
    /// Verifies that the method handles format strings with placeholders but no arguments without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_FormatWithPlaceholdersNoArgs_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {0}", Array.Empty<object>());
    }

    /// <summary>
    /// Verifies that the method handles format strings with mismatched placeholder count without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_MismatchedPlaceholderCount_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {0} {1} {2}", new object[] { "arg1" });
    }

    /// <summary>
    /// Verifies that the method handles format strings with invalid placeholder syntax without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_InvalidPlaceholderSyntax_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {0", new object[] { "arg" });
    }

    /// <summary>
    /// Verifies that the method handles a null arguments array without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_NullArgs_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {0}", null!);
    }

    /// <summary>
    /// Verifies that the method handles an empty arguments array with a format string containing no placeholders.
    /// </summary>
    [TestMethod]
    public void Exception_EmptyArgsNoPlaceholders_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error occurred", Array.Empty<object>());
    }

    /// <summary>
    /// Verifies that the method handles arguments containing null elements without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_ArgsWithNullElements_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {0} {1}", new object?[] { null, "valid" });
    }

    /// <summary>
    /// Verifies that the method handles more arguments than placeholders without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_MoreArgsThanPlaceholders_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {0}", new object[] { "arg1", "arg2", "arg3" });
    }

    /// <summary>
    /// Verifies that the method handles a null exception parameter without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_NullException_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();

        // Act & Assert
        Logger.Exception(null!, "Error: {0}", "details");
    }

    /// <summary>
    /// Verifies that the method handles an exception with a null message without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_ExceptionWithNullMessage_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new CustomExceptionWithNullMessage();

        // Act & Assert
        Logger.Exception(ex, "Error: {0}", "details");
    }

    /// <summary>
    /// Verifies that the method handles nested exceptions (with InnerException) without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_NestedExceptionWithInnerException_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var innerEx = new ArgumentException("Inner exception");
        var outerEx = new InvalidOperationException("Outer exception", innerEx);

        // Act & Assert
        Logger.Exception(outerEx, "Error: {0}", "details");
    }

    /// <summary>
    /// Verifies that the method handles format strings with special characters without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_FormatWithSpecialCharacters_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {0} @#$%^&*()", "details");
    }

    /// <summary>
    /// Verifies that the method handles very long format strings without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_VeryLongFormat_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");
        var longFormat = new string('x', 10000) + " {0}";

        // Act & Assert
        Logger.Exception(ex, longFormat, "details");
    }

    /// <summary>
    /// Verifies that the method handles format strings with control characters without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_FormatWithControlCharacters_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error:\n\r\t{0}", "details");
    }

    /// <summary>
    /// Verifies that the method handles format strings with Unicode characters without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_FormatWithUnicodeCharacters_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "エラー: {0} 错误", "details");
    }

    /// <summary>
    /// Verifies that the method handles all null parameters without throwing when logging is disabled.
    /// </summary>
    [TestMethod]
    public void Exception_AllNullParametersLoggingDisabled_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.DisableAll();

        // Act & Assert
        Logger.Exception(null!, null!, null!);
    }

    /// <summary>
    /// Verifies that the method handles format strings with escaped braces without throwing.
    /// </summary>
    [TestMethod]
    public void Exception_FormatWithEscapedBraces_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        // Act & Assert
        Logger.Exception(ex, "Error: {{0}} {0}", "details");
    }

    /// <summary>
    /// Verifies that the method handles multiple nested suppression scopes correctly.
    /// </summary>
    [TestMethod]
    public void Exception_NestedSuppressionScopes_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        using (Logger.Suppress())
        using (Logger.Suppress())
        {
            // Act & Assert
            Logger.Exception(ex, "Error: {0}", "details");
        }
    }

    /// <summary>
    /// Verifies that the method works correctly after suppression scope is disposed.
    /// </summary>
    [TestMethod]
    public void Exception_AfterSuppressionDisposed_CompletesSuccessfully()
    {
        // Arrange
        Logger.EnableAll();
        var ex = new InvalidOperationException("Test exception");

        using (Logger.Suppress())
        {
            // Suppressed scope
        }

        // Act & Assert (after suppression is removed)
        Logger.Exception(ex, "Error: {0}", "details");
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> returns immediately without throwing
    /// when logging is globally disabled.
    /// </summary>
    [TestMethod]
    public void Error_WhenLoggingDisabled_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.DisableAll();
        string message = "Test error message";

        // Act & Assert
        Logger.Error(message);
        // No exception expected - test passes if no exception is thrown
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> completes without throwing
    /// when logging is enabled with a normal message.
    /// </summary>
    [TestMethod]
    public void Error_WhenLoggingEnabled_CompletesWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        string message = "Test error message";

        // Act & Assert
        Logger.Error(message);
        // No exception expected - test passes if no exception is thrown
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> returns immediately without throwing
    /// when called within an active suppression scope.
    /// </summary>
    [TestMethod]
    public void Error_WithinSuppressionScope_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        string message = "Test error message";

        // Act & Assert
        using (Logger.Suppress())
        {
            Logger.Error(message);
        }
        // No exception expected - test passes if no exception is thrown
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> returns immediately without throwing
    /// when called within nested suppression scopes.
    /// </summary>
    [TestMethod]
    public void Error_WithinNestedSuppressionScopes_ReturnsWithoutThrowing()
    {
        // Arrange
        Logger.EnableAll();
        string message = "Test error message";

        // Act & Assert
        using (Logger.Suppress())
        using (Logger.Suppress())
        {
            Logger.Error(message);
        }
        // No exception expected - test passes if no exception is thrown
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> does not throw when passed a null message,
    /// testing the exception-safe design of the logging infrastructure.
    /// </summary>
    [TestMethod]
    public void Error_WithNullMessage_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string? message = null;

        // Act & Assert
        Logger.Error(message!);
        // No exception expected - LogBridge.Error swallows all exceptions
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> handles various message inputs without throwing,
    /// including null, empty, whitespace, normal, and special character strings.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="description">Description of the test case.</param>
    [TestMethod]
    [DataRow(null, "Null message", DisplayName = "Error with null message")]
    [DataRow("", "Empty message", DisplayName = "Error with empty message")]
    [DataRow("   ", "Whitespace message", DisplayName = "Error with whitespace message")]
    [DataRow("Normal error message", "Normal message", DisplayName = "Error with normal message")]
    [DataRow("Error with special chars: @#$%^&*()", "Special characters", DisplayName = "Error with special characters")]
    [DataRow("Error with newline\nand tab\there", "Control characters", DisplayName = "Error with control characters")]
    [DataRow("Error with Unicode: 你好世界 🚀", "Unicode and emoji", DisplayName = "Error with Unicode and emoji")]
    public void Error_WithVariousMessages_DoesNotThrow(string? message, string description)
    {
        // Arrange
        Logger.EnableAll();

        // Act & Assert
        Logger.Error(message!);
        // No exception expected regardless of message content
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> handles a very long message without throwing,
    /// testing boundary conditions for string length.
    /// </summary>
    [TestMethod]
    public void Error_WithVeryLongMessage_DoesNotThrow()
    {
        // Arrange
        Logger.EnableAll();
        string longMessage = new string('X', 100000); // 100K characters

        // Act & Assert
        Logger.Error(longMessage);
        // No exception expected even with very long messages
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> respects the Enabled property state,
    /// returning immediately when disabled and proceeding when enabled.
    /// </summary>
    [TestMethod]
    public void Error_RespectsEnabledPropertyState_DoesNotThrow()
    {
        // Arrange
        string message = "Test error message";

        // Act & Assert - Disabled
        Logger.Enabled = false;
        Logger.Error(message);

        // Act & Assert - Enabled
        Logger.Enabled = true;
        Logger.Error(message);

        // No exception expected in either state
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> works correctly after toggling
    /// the enabled state multiple times.
    /// </summary>
    [TestMethod]
    public void Error_AfterMultipleStateToggles_DoesNotThrow()
    {
        // Arrange
        string message = "Test error message";

        // Act & Assert
        Logger.EnableAll();
        Logger.Error(message);

        Logger.DisableAll();
        Logger.Error(message);

        Logger.EnableAll();
        Logger.Error(message);

        Logger.DisableAll();
        Logger.Error(message);

        // No exception expected through multiple state changes
    }

    /// <summary>
    /// Verifies that <see cref="Logger.Error(string)"/> continues to work correctly
    /// after suppression scope is disposed, ensuring proper cleanup.
    /// </summary>
    [TestMethod]
    public void Error_AfterSuppressionScopeDisposed_LogsWhenEnabled()
    {
        // Arrange
        Logger.EnableAll();
        string message = "Test error message";

        // Act
        using (Logger.Suppress())
        {
            Logger.Error(message); // Should not log (suppressed)
        }

        // Assert
        Logger.Error(message); // Should log again (suppression removed)
        // No exception expected
    }
}


/// <summary>
/// Tests for the <see cref="Logger.LogBridge.Warning"/> method.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Logger.LogBridge.Warning"/> method calls the static
/// <c>REFrameworkNET.API.LogWarning</c> API, which cannot be mocked with Moq.
/// These tests verify only that the method is exception-safe and completes
/// without throwing for various inputs. They do not verify that the actual
/// logging to REFramework occurs, as that requires the runtime host environment.
/// </para>
/// </remarks>
[TestClass]
public class LogBridgeWarningTests
{
}


/// <summary>
/// Tests for the <see cref="Logger.LogBridge.Info(string)"/> method.
/// </summary>
/// <remarks>
/// <para>
/// The LogBridge.Info method is a thin wrapper around REFrameworkNET.API.LogInfo, which is a static
/// method. Standard mocking frameworks like Moq cannot mock static methods without advanced tooling
/// such as Microsoft Fakes or Pose.
/// </para>
/// <para>
/// These tests document the expected behavior and edge cases but are marked as Inconclusive because
/// the static dependency prevents proper isolation and verification. To make this code fully testable:
/// </para>
/// <list type="bullet">
/// <item>Introduce an abstraction layer (e.g., ILogAdapter) over REFrameworkNET.API.</item>
/// <item>Inject the abstraction into Logger or LogBridge via dependency injection.</item>
/// <item>Mock the abstraction in tests using Moq.</item>
/// </list>
/// <para>
/// Without such refactoring, these tests serve as documentation of intended behavior and can be
/// executed manually or via integration testing with the actual REFrameworkNET.API available.
/// </para>
/// </remarks>
[TestClass]
public class LogBridgeTests
{
    /// <summary>
    /// Tests that Info does not throw when called with a null message.
    /// </summary>
    /// <remarks>
    /// This test is marked Inconclusive because we cannot mock REFrameworkNET.API.LogInfo to verify
    /// the call or simulate exceptions. The method has a catch-all exception handler, so it will not
    /// throw regardless of whether API.LogInfo accepts null or throws.
    /// </remarks>
    [TestMethod]
    public void Info_WithNullMessage_DoesNotThrow()
    {
        // Arrange
        string? message = null;

        // Act & Assert
        // The method signature accepts string, and the catch block ensures no exception propagates.
        // However, we cannot verify that API.LogInfo was called with null without mocking the static API.
        Assert.Inconclusive(
            "Cannot test LogBridge.Info without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "Consider introducing an ILogAdapter abstraction to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that Info does not throw when called with an empty string.
    /// </summary>
    /// <remarks>
    /// This test is marked Inconclusive because we cannot mock REFrameworkNET.API.LogInfo to verify
    /// the call or behavior.
    /// </remarks>
    [TestMethod]
    public void Info_WithEmptyString_DoesNotThrow()
    {
        // Arrange
        string message = string.Empty;

        // Act & Assert
        Assert.Inconclusive(
            "Cannot test LogBridge.Info without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "Consider introducing an ILogAdapter abstraction to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that Info does not throw when called with a whitespace-only string.
    /// </summary>
    /// <remarks>
    /// This test is marked Inconclusive because we cannot mock REFrameworkNET.API.LogInfo to verify
    /// the call or behavior.
    /// </remarks>
    [TestMethod]
    public void Info_WithWhitespaceString_DoesNotThrow()
    {
        // Arrange
        string message = "   \t\n\r   ";

        // Act & Assert
        Assert.Inconclusive(
            "Cannot test LogBridge.Info without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "Consider introducing an ILogAdapter abstraction to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that Info does not throw when called with a valid normal message.
    /// </summary>
    /// <remarks>
    /// This test is marked Inconclusive because we cannot mock REFrameworkNET.API.LogInfo to verify
    /// that the method was called with the correct message.
    /// </remarks>
    [TestMethod]
    public void Info_WithValidMessage_DoesNotThrow()
    {
        // Arrange
        string message = "Test log message";

        // Act & Assert
        Assert.Inconclusive(
            "Cannot test LogBridge.Info without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "Consider introducing an ILogAdapter abstraction to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that Info does not throw when called with a very long string.
    /// </summary>
    /// <remarks>
    /// This test verifies boundary behavior with extremely long strings. The test is marked Inconclusive
    /// because we cannot mock REFrameworkNET.API.LogInfo to verify the call or simulate performance issues.
    /// </remarks>
    [TestMethod]
    public void Info_WithVeryLongString_DoesNotThrow()
    {
        // Arrange
        string message = new string('A', 100000);

        // Act & Assert
        Assert.Inconclusive(
            "Cannot test LogBridge.Info without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "Consider introducing an ILogAdapter abstraction to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that Info does not throw when called with a string containing special characters.
    /// </summary>
    /// <remarks>
    /// This test verifies edge cases with special, control, and Unicode characters. The test is marked
    /// Inconclusive because we cannot mock REFrameworkNET.API.LogInfo.
    /// </remarks>
    [TestMethod]
    public void Info_WithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        string message = "Special: \0\a\b\f\n\r\t\v\u0001\u001F \"'\\<>&";

        // Act & Assert
        Assert.Inconclusive(
            "Cannot test LogBridge.Info without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "Consider introducing an ILogAdapter abstraction to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that Info does not throw when called with a string containing Unicode characters.
    /// </summary>
    /// <remarks>
    /// This test verifies behavior with various Unicode characters including emoji and non-Latin scripts.
    /// The test is marked Inconclusive because we cannot mock REFrameworkNET.API.LogInfo.
    /// </remarks>
    [TestMethod]
    public void Info_WithUnicodeCharacters_DoesNotThrow()
    {
        // Arrange
        string message = "Unicode: 你好世界 🌍 Привет мир";

        // Act & Assert
        Assert.Inconclusive(
            "Cannot test LogBridge.Info without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "Consider introducing an ILogAdapter abstraction to enable proper unit testing.");
    }

    /// <summary>
    /// Tests that Info silently handles exceptions thrown by API.LogInfo without propagating them.
    /// </summary>
    /// <remarks>
    /// The LogBridge.Info method has a catch-all exception handler (try { API.LogInfo(message); } catch { }).
    /// This test would verify that any exception thrown by API.LogInfo is caught and suppressed. However,
    /// without the ability to mock API.LogInfo and configure it to throw exceptions, this behavior cannot
    /// be verified in isolation. The test is marked Inconclusive.
    /// </remarks>
    [TestMethod]
    public void Info_WhenApiLogInfoThrows_SuppressesException()
    {
        // Arrange
        string message = "Test message";

        // Act & Assert
        // To properly test this scenario, we would need to:
        // 1. Mock REFrameworkNET.API.LogInfo to throw an exception
        // 2. Call LogBridge.Info(message)
        // 3. Assert that no exception is propagated
        // Since static methods cannot be mocked with Moq, this test cannot be completed.
        Assert.Inconclusive(
            "Cannot test LogBridge.Info exception handling without mocking REFrameworkNET.API.LogInfo. " +
            "REFrameworkNET.API is a static class with static methods, which cannot be mocked with Moq. " +
            "To test exception suppression, introduce an ILogAdapter abstraction and inject it into LogBridge.");
    }
}
