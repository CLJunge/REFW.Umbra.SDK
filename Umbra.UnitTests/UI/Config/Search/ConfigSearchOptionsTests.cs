namespace Umbra.UI.Config.Search.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigSearchOptions"/>.
/// </summary>
[TestClass]
public sealed class ConfigSearchOptionsTests
{
    /// <summary>
    /// Verifies that default-constructed options use the documented default values.
    /// </summary>
    [TestMethod]
    public void DefaultConstructor_ReturnsDocumentedDefaults()
    {
        // Act
        var options = new ConfigSearchOptions();

        // Assert
        Assert.AreEqual(ConfigSearchOptions.DefaultMaxInputLength, options.MaxInputLength);
        Assert.AreEqual(ConfigSearchOptions.DefaultMinimumSearchInputWidth, options.MinimumSearchInputWidth);
    }

    /// <summary>
    /// Verifies that explicit init values are preserved.
    /// </summary>
    [TestMethod]
    public void InitProperties_WithExplicitValues_PreservesValues()
    {
        // Act
        var options = new ConfigSearchOptions
        {
            MaxInputLength = 512,
            MinimumSearchInputWidth = 128f
        };

        // Assert
        Assert.AreEqual(512u, options.MaxInputLength);
        Assert.AreEqual(128f, options.MinimumSearchInputWidth);
    }

    /// <summary>
    /// Verifies that setting <see cref="ConfigSearchOptions.MaxInputLength"/> to zero falls back to the default.
    /// </summary>
    [TestMethod]
    public void MaxInputLength_WhenZero_FallsBackToDefault()
    {
        // Act
        var options = new ConfigSearchOptions { MaxInputLength = 0 };

        // Assert
        Assert.AreEqual(ConfigSearchOptions.DefaultMaxInputLength, options.MaxInputLength);
    }

    /// <summary>
    /// Verifies that setting <see cref="ConfigSearchOptions.MinimumSearchInputWidth"/> to zero falls back to the default.
    /// </summary>
    [TestMethod]
    public void MinimumSearchInputWidth_WhenZero_FallsBackToDefault()
    {
        // Act
        var options = new ConfigSearchOptions { MinimumSearchInputWidth = 0f };

        // Assert
        Assert.AreEqual(ConfigSearchOptions.DefaultMinimumSearchInputWidth, options.MinimumSearchInputWidth);
    }

    /// <summary>
    /// Verifies that setting <see cref="ConfigSearchOptions.MinimumSearchInputWidth"/> to a negative value falls back to the default.
    /// </summary>
    [TestMethod]
    public void MinimumSearchInputWidth_WhenNegative_FallsBackToDefault()
    {
        // Act
        var options = new ConfigSearchOptions { MinimumSearchInputWidth = -10f };

        // Assert
        Assert.AreEqual(ConfigSearchOptions.DefaultMinimumSearchInputWidth, options.MinimumSearchInputWidth);
    }

    /// <summary>
    /// Verifies that a very small positive <see cref="ConfigSearchOptions.MinimumSearchInputWidth"/> is preserved.
    /// </summary>
    [TestMethod]
    public void MinimumSearchInputWidth_WhenSmallPositive_PreservesValue()
    {
        // Act
        var options = new ConfigSearchOptions { MinimumSearchInputWidth = 1f };

        // Assert
        Assert.AreEqual(1f, options.MinimumSearchInputWidth);
    }

    /// <summary>
    /// Verifies that a <see cref="ConfigSearchOptions.MaxInputLength"/> of 1 is preserved (minimum valid value).
    /// </summary>
    [TestMethod]
    public void MaxInputLength_WhenOne_PreservesValue()
    {
        // Act
        var options = new ConfigSearchOptions { MaxInputLength = 1 };

        // Assert
        Assert.AreEqual(1u, options.MaxInputLength);
    }
}
