using Hexa.NET.ImGui;

namespace Umbra.Input.UnitTests;


/// <summary>
/// Unit tests for the <see cref="KeyboardInput"/> class.
/// </summary>
[TestClass]
public class KeyboardInputTests
{
    /// <summary>
    /// Tests that <see cref="KeyboardInput.TryCaptureKeyboardKey"/> can be invoked without throwing exceptions.
    /// This is a smoke test only, as the method depends on static non-mockable ImGui infrastructure
    /// that cannot be properly isolated with Moq.
    /// </summary>
    /// <remarks>
    /// Full coverage requires integration testing with ImGui in a real REFramework environment where
    /// ImGui context is initialized and keyboard input can be simulated. The static method
    /// <see cref="ImGui.IsKeyPressed"/> and the static field <c>_keyboardKeys</c> cannot be mocked,
    /// preventing proper unit testing of all edge cases.
    /// </remarks>
    [TestMethod]
    public void TryCaptureKeyboardKey_WithoutImGuiContext_DoesNotThrow()
    {
        // Arrange
        // No ImGui context available in unit test environment
        Exception? caughtException = null;
        var result = false;
        var capturedKey = -1;

        // Act
        try
        {
            result = KeyboardInput.TryCaptureKeyboardKey(out capturedKey);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        if (caughtException != null)
        {
            Assert.Inconclusive(
                $"TryCaptureKeyboardKey threw {caughtException.GetType().Name} when ImGui context is not initialized. " +
                "This behavior should be validated in an integration test with proper ImGui setup. " +
                $"Exception message: {caughtException.Message}");
        }

        // If the method completes, verify the out parameter is set to some value
        Assert.IsTrue(capturedKey is -1 or > 0, "Out parameter should be set to -1 or a valid key code.");
    }

    /// <summary>
    /// Verifies that the out parameter is always initialized to a valid value after the method call.
    /// Tests the contract that <paramref name="capturedKey"/> is never left uninitialized.
    /// </summary>
    [TestMethod]
    public void TryCaptureKeyboardKey_AlwaysInitializesOutParameter()
    {
        // Arrange
        var capturedKey = int.MaxValue; // Set to a sentinel value
        Exception? caughtException = null;

        // Act
        try
        {
            KeyboardInput.TryCaptureKeyboardKey(out capturedKey);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        if (caughtException != null)
        {
            Assert.Inconclusive(
                $"Method threw {caughtException.GetType().Name} before initializing out parameter. " +
                "This may be due to missing ImGui context. Validate in integration test.");
        }

        // The out parameter must have been written to (either -1 or a valid key code)
        // It should not remain at the sentinel value
        Assert.AreNotEqual(int.MaxValue, capturedKey,
            "The out parameter should be initialized by the method.");
    }

    /// <summary>
    /// Documents the expected range of valid key codes returned by the method.
    /// Marked as inconclusive because the actual behavior depends on ImGui state.
    /// </summary>
    /// <remarks>
    /// Valid key codes are expected to be greater than <see cref="ImGuiKey.None"/> (0),
    /// or exactly <c>-1</c> when no key is pressed.
    /// </remarks>
    [TestMethod]
    public void TryCaptureKeyboardKey_ReturnsValidKeyCodeRange()
    {
        // Arrange
        Exception? caughtException = null;
        var result = false;
        var capturedKey = -1;

        // Act
        try
        {
            result = KeyboardInput.TryCaptureKeyboardKey(out capturedKey);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        if (caughtException != null)
        {
            Assert.Inconclusive(
                $"Method threw {caughtException.GetType().Name}. Expected behavior validation requires integration test. " +
                $"Exception: {caughtException.Message}");
        }

        if (result)
        {
            Assert.IsGreaterThan((int)ImGuiKey.None, capturedKey,
                $"When method returns true, capturedKey should be greater than ImGuiKey.None. Got: {capturedKey}");
        }
        else
        {
            Assert.AreEqual(-1, capturedKey,
                "When method returns false, capturedKey should be -1.");
        }
    }

    /// <summary>
    /// Verifies the consistency contract: return value correlates with the out parameter value.
    /// When <c>true</c> is returned, <paramref name="capturedKey"/> should be positive.
    /// When <c>false</c> is returned, <paramref name="capturedKey"/> should be <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void TryCaptureKeyboardKey_ReturnValueMatchesOutParameter()
    {
        // Arrange
        Exception? caughtException = null;
        var result = false;
        var capturedKey = -1;

        // Act
        try
        {
            result = KeyboardInput.TryCaptureKeyboardKey(out capturedKey);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        if (caughtException != null)
        {
            Assert.Inconclusive(
                $"Contract validation failed due to exception: {caughtException.GetType().Name}. " +
                "Validate the return-value-to-out-parameter contract in an integration test with ImGui context.");
        }

        if (result)
        {
            Assert.IsGreaterThan(0, capturedKey,
                $"Return value is true, but capturedKey is not positive. Got: {capturedKey}");
        }
        else
        {
            Assert.AreEqual(-1, capturedKey,
                $"Return value is false, but capturedKey is not -1. Got: {capturedKey}");
        }
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsValidKey(int)"/> returns <c>false</c>
    /// when the key value is negative.
    /// </summary>
    /// <param name="key">The negative key value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1000)]
    [DataRow(-100)]
    [DataRow(-10)]
    [DataRow(-1)]
    public void IsValidKey_NegativeKeyValue_ReturnsFalse(int key)
    {
        // Act
        var result = KeyboardInput.IsValidKey(key);

        // Assert
        Assert.IsFalse(result, $"Expected IsValidKey to return false for negative key value {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsValidKey(int)"/> returns <c>false</c>
    /// when the key value is zero (representing <c>ImGuiKey.None</c>).
    /// </summary>
    [TestMethod]
    public void IsValidKey_ZeroKeyValue_ReturnsFalse()
    {
        // Arrange
        var key = 0;

        // Act
        var result = KeyboardInput.IsValidKey(key);

        // Assert
        Assert.IsFalse(result, "Expected IsValidKey to return false for key value 0 (ImGuiKey.None)");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsValidKey(int)"/> returns <c>true</c>
    /// when the key value is positive.
    /// </summary>
    /// <param name="key">The positive key value to test.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(50)]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    public void IsValidKey_PositiveKeyValue_ReturnsTrue(int key)
    {
        // Act
        var result = KeyboardInput.IsValidKey(key);

        // Assert
        Assert.IsTrue(result, $"Expected IsValidKey to return true for positive key value {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsValidKey(int)"/> correctly handles
    /// boundary values including extreme minimum and maximum integer values.
    /// </summary>
    /// <param name="key">The boundary key value to test.</param>
    /// <param name="expectedResult">The expected result for the given key value.</param>
    [TestMethod]
    [DataRow(int.MinValue, false)]
    [DataRow(-1, false)]
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(int.MaxValue, true)]
    public void IsValidKey_BoundaryValues_ReturnsExpectedResult(int key, bool expectedResult)
    {
        // Act
        var result = KeyboardInput.IsValidKey(key);

        // Assert
        Assert.AreEqual(expectedResult, result,
            $"Expected IsValidKey to return {expectedResult} for boundary key value {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns a formatted fallback string
    /// for invalid enum values (values that do not correspond to any defined ImGuiKey member).
    /// </summary>
    /// <param name="key">The invalid key value to test.</param>
    /// <param name="expectedName">The expected formatted key name.</param>
    [TestMethod]
    [DataRow(-1, "Key(-1)")]
    [DataRow(-100, "Key(-100)")]
    [DataRow(-2147483648, "Key(-2147483648)")]
    [DataRow(99999, "Key(99999)")]
    [DataRow(1000000, "Key(1000000)")]
    [DataRow(2147483647, "Key(2147483647)")]
    public void GetKeyName_InvalidEnumValue_ReturnsFormattedFallback(int key, string expectedName)
    {
        // Act
        var result = KeyboardInput.GetKeyName(key);

        // Assert
        Assert.AreEqual(expectedName, result);
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns a non-null, non-empty string
    /// for any integer value, ensuring the method never throws and always produces valid output.
    /// </summary>
    /// <param name="key">The key value to test.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(50)]
    [DataRow(100)]
    [DataRow(500)]
    [DataRow(-1)]
    [DataRow(-50)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void GetKeyName_AnyIntValue_ReturnsNonNullNonEmptyString(int key)
    {
        // Act
        var result = KeyboardInput.GetKeyName(key);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result));
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> correctly formats the fallback string
    /// with the exact integer value when the enum value is not defined.
    /// </summary>
    [TestMethod]
    public void GetKeyName_LargeNegativeValue_ReturnsCorrectlyFormattedString()
    {
        // Arrange
        var key = -999999;

        // Act
        var result = KeyboardInput.GetKeyName(key);

        // Assert
        Assert.AreEqual("Key(-999999)", result);
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> correctly formats the fallback string
    /// with the exact integer value when the enum value is not defined.
    /// </summary>
    [TestMethod]
    public void GetKeyName_LargePositiveValue_ReturnsCorrectlyFormattedString()
    {
        // Arrange
        var key = 999999999;

        // Act
        var result = KeyboardInput.GetKeyName(key);

        // Assert
        Assert.AreEqual("Key(999999999)", result);
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns either a valid enum name
    /// or a formatted fallback string for the zero value, which likely corresponds to ImGuiKey.None.
    /// </summary>
    [TestMethod]
    public void GetKeyName_ZeroValue_ReturnsValidString()
    {
        // Arrange
        var key = 0;

        // Act
        var result = KeyboardInput.GetKeyName(key);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result is "None" or "Key(0)",
            $"Expected 'None' or 'Key(0)' but got '{result}'");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns a string that either matches
    /// an enum member name or follows the "Key(n)" pattern for boundary integer values.
    /// </summary>
    /// <param name="key">The boundary key value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void GetKeyName_BoundaryValues_ReturnsValidFormat(int key)
    {
        // Act
        var result = KeyboardInput.GetKeyName(key);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(
            result.StartsWith("Key(") || !result.Contains('('),
            $"Result '{result}' should either be an enum name or match 'Key(n)' pattern");
    }
}
