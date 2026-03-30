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
    /// Tests that <see cref="KeyboardInput.GetKeyName"/> returns the canonical <c>None</c>
    /// name for the zero value rather than the alternate zero-valued ImGui modifier alias.
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

}
