namespace Umbra.Config.Attributes.UnitTests;

/// <summary>
/// Contains focused unit tests for the <see cref="UmbraRegexAttribute"/> class.
/// </summary>
[TestClass]
public sealed class UmbraRegexAttributeTests
{
    /// <summary>
    /// Verifies that the constructor preserves a valid pattern string.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidPattern_SetsPattern()
    {
        // Arrange
        const string pattern = "^[A-Z]{3}$";

        // Act
        var attribute = new UmbraRegexAttribute(pattern);

        // Assert
        Assert.AreEqual(pattern, attribute.Pattern);
    }

    /// <summary>
    /// Verifies that the constructor rejects an empty pattern.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyPattern_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.ThrowsExactly<ArgumentException>(() => new UmbraRegexAttribute(string.Empty));

        // Assert
        Assert.AreEqual("pattern", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a whitespace-only pattern.
    /// </summary>
    [TestMethod]
    public void Constructor_WithWhitespacePattern_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.ThrowsExactly<ArgumentException>(() => new UmbraRegexAttribute("   "));

        // Assert
        Assert.AreEqual("pattern", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null pattern.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullPattern_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new UmbraRegexAttribute(null!));

        // Assert
        Assert.AreEqual("pattern", exception.ParamName);
    }
}
