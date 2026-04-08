using Hexa.NET.ImGui;

namespace Umbra.Input.UnitTests;


/// <summary>
/// Unit tests for the <see cref="KeyboardInput"/> class.
/// </summary>
[TestClass]
public class KeyboardInputTests
{
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
    /// when the key value is zero (representing <c>UmbraKey.None</c>).
    /// </summary>
    [TestMethod]
    public void IsValidKey_ZeroKeyValue_ReturnsFalse()
    {
        // Arrange
        var key = 0;

        // Act
        var result = KeyboardInput.IsValidKey(key);

        // Assert
        Assert.IsFalse(result, "Expected IsValidKey to return false for key value 0 (UmbraKey.None)");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsValidKey(int)"/> returns <c>false</c>
    /// for arbitrary positive integers that are not supported keyboard keys.
    /// </summary>
    /// <param name="key">The positive key value to test.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(50)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    public void IsValidKey_ArbitraryPositiveValue_ReturnsFalse(int key)
    {
        // Act
        var result = KeyboardInput.IsValidKey(key);

        // Assert
        Assert.IsFalse(result, $"Expected IsValidKey to return false for unsupported key value {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns a formatted fallback string
    /// for invalid enum values (values that do not correspond to any defined UmbraKey member).
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
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns the canonical <c>None</c>
    /// name for the zero value rather than an alternate zero-valued alias.
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
        Assert.AreEqual("None", result);
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns the enum member name for known Umbra keys.
    /// </summary>
    [TestMethod]
    [DataRow(UmbraKey.A, "A")]
    [DataRow(UmbraKey.Enter, "Enter")]
    [DataRow(UmbraKey.LeftCtrl, "LeftCtrl")]
    [DataRow(UmbraKey.F12, "F12")]
    public void GetKeyName_KnownUmbraKey_ReturnsEnumMemberName(UmbraKey key, string expectedName)
    {
        var result = KeyboardInput.GetKeyName((int)key);

        Assert.AreEqual(expectedName, result);
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsValidKey(int)"/> returns <c>true</c> for real known keyboard keys.
    /// </summary>
    [TestMethod]
    [DataRow(UmbraKey.A)]
    [DataRow(UmbraKey.Enter)]
    [DataRow(UmbraKey.LeftShift)]
    public void IsValidKey_KnownUmbraKey_ReturnsTrue(UmbraKey key)
    {
        var result = KeyboardInput.IsValidKey((int)key);

        Assert.IsTrue(result, $"Expected IsValidKey to return true for known UmbraKey {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsValidKey(int)"/> returns <c>false</c> for values
    /// that are not keyboard keys (out-of-range or undefined UmbraKey values).
    /// </summary>
    [TestMethod]
    [DataRow((UmbraKey)(-1))]
    [DataRow((UmbraKey)99999)]
    [DataRow((UmbraKey)1)]
    [DataRow((UmbraKey)100)]
    public void IsValidKey_NonKeyboardValue_ReturnsFalse(UmbraKey key)
    {
        var result = KeyboardInput.IsValidKey((int)key);

        Assert.IsFalse(result, $"Expected IsValidKey to return false for non-keyboard value {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsModifierKey(int)"/> returns <c>true</c> for all modifier keys.
    /// </summary>
    [TestMethod]
    [DataRow(UmbraKey.LeftCtrl)]
    [DataRow(UmbraKey.RightCtrl)]
    [DataRow(UmbraKey.LeftShift)]
    [DataRow(UmbraKey.RightShift)]
    [DataRow(UmbraKey.LeftAlt)]
    [DataRow(UmbraKey.RightAlt)]
    [DataRow(UmbraKey.LeftSuper)]
    [DataRow(UmbraKey.RightSuper)]
    public void IsModifierKey_ModifierKey_ReturnsTrue(UmbraKey key)
    {
        // Act
        var result = KeyboardInput.IsModifierKey((int)key);

        // Assert
        Assert.IsTrue(result, $"Expected IsModifierKey to return true for {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsModifierKey(int)"/> returns <c>false</c> for non-modifier keys.
    /// </summary>
    [TestMethod]
    [DataRow(UmbraKey.A)]
    [DataRow(UmbraKey.Enter)]
    [DataRow(UmbraKey.F12)]
    [DataRow(UmbraKey.Space)]
    [DataRow(UmbraKey.Tab)]
    [DataRow(UmbraKey.Escape)]
    [DataRow(UmbraKey.Key0)]
    [DataRow(UmbraKey.Menu)]
    public void IsModifierKey_NonModifierKey_ReturnsFalse(UmbraKey key)
    {
        // Act
        var result = KeyboardInput.IsModifierKey((int)key);

        // Assert
        Assert.IsFalse(result, $"Expected IsModifierKey to return false for {key}");
    }

    /// <summary>
    /// Tests that <see cref="KeyboardInput.IsModifierKey(int)"/> returns <c>false</c> for invalid key values.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(99999)]
    public void IsModifierKey_InvalidValue_ReturnsFalse(int key)
    {
        // Act
        var result = KeyboardInput.IsModifierKey(key);

        // Assert
        Assert.IsFalse(result, $"Expected IsModifierKey to return false for invalid key value {key}");
    }
}
