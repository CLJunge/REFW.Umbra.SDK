using Umbra.Config;

namespace Umbra.UI.ChangeMonitor.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigChangeMonitorOptions"/>.
/// </summary>
[TestClass]
public sealed class ConfigChangeMonitorOptionsTests
{
    /// <summary>
    /// Verifies that default-constructed options use the documented default values.
    /// </summary>
    [TestMethod]
    public void DefaultConstructor_ReturnsDocumentedDefaults()
    {
        // Act
        var options = new ConfigChangeMonitorOptions();

        // Assert
        Assert.AreEqual(ConfigChangeLog.DefaultCapacity, options.LogCapacity);
        Assert.AreEqual(ConfigChangeMonitorOptions.DefaultDisplayHeight, options.DisplayHeight);
    }

    /// <summary>
    /// Verifies that the <see cref="ConfigChangeMonitorOptions.DefaultLogCapacity"/> constant
    /// matches <see cref="ConfigChangeLog.DefaultCapacity"/>.
    /// </summary>
    [TestMethod]
    public void DefaultLogCapacity_MatchesConfigChangeLogDefaultCapacity()
    {
        var expected = ConfigChangeLog.DefaultCapacity;
        var actual = ConfigChangeMonitorOptions.DefaultLogCapacity;
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that explicit init values are preserved.
    /// </summary>
    [TestMethod]
    public void InitProperties_WithExplicitValues_PreservesValues()
    {
        // Act
        var options = new ConfigChangeMonitorOptions
        {
            LogCapacity = 128,
            DisplayHeight = 350f
        };

        // Assert
        Assert.AreEqual(128, options.LogCapacity);
        Assert.AreEqual(350f, options.DisplayHeight);
    }

    /// <summary>
    /// Verifies that setting <see cref="ConfigChangeMonitorOptions.LogCapacity"/> to zero
    /// falls back to the default.
    /// </summary>
    [TestMethod]
    public void LogCapacity_WhenZero_FallsBackToDefault()
    {
        // Act
        var options = new ConfigChangeMonitorOptions { LogCapacity = 0 };

        // Assert
        Assert.AreEqual(ConfigChangeMonitorOptions.DefaultLogCapacity, options.LogCapacity);
    }

    /// <summary>
    /// Verifies that setting <see cref="ConfigChangeMonitorOptions.LogCapacity"/> to a negative value
    /// falls back to the default.
    /// </summary>
    [TestMethod]
    public void LogCapacity_WhenNegative_FallsBackToDefault()
    {
        // Act
        var options = new ConfigChangeMonitorOptions { LogCapacity = -5 };

        // Assert
        Assert.AreEqual(ConfigChangeMonitorOptions.DefaultLogCapacity, options.LogCapacity);
    }

    /// <summary>
    /// Verifies that a <see cref="ConfigChangeMonitorOptions.LogCapacity"/> of 1 is preserved
    /// (minimum valid value).
    /// </summary>
    [TestMethod]
    public void LogCapacity_WhenOne_PreservesValue()
    {
        // Act
        var options = new ConfigChangeMonitorOptions { LogCapacity = 1 };

        // Assert
        Assert.AreEqual(1, options.LogCapacity);
    }

    /// <summary>
    /// Verifies that setting <see cref="ConfigChangeMonitorOptions.DisplayHeight"/> to zero
    /// falls back to the default.
    /// </summary>
    [TestMethod]
    public void DisplayHeight_WhenZero_FallsBackToDefault()
    {
        // Act
        var options = new ConfigChangeMonitorOptions { DisplayHeight = 0f };

        // Assert
        Assert.AreEqual(ConfigChangeMonitorOptions.DefaultDisplayHeight, options.DisplayHeight);
    }

    /// <summary>
    /// Verifies that setting <see cref="ConfigChangeMonitorOptions.DisplayHeight"/> to a negative value
    /// falls back to the default.
    /// </summary>
    [TestMethod]
    public void DisplayHeight_WhenNegative_FallsBackToDefault()
    {
        // Act
        var options = new ConfigChangeMonitorOptions { DisplayHeight = -10f };

        // Assert
        Assert.AreEqual(ConfigChangeMonitorOptions.DefaultDisplayHeight, options.DisplayHeight);
    }

    /// <summary>
    /// Verifies that a very small positive <see cref="ConfigChangeMonitorOptions.DisplayHeight"/>
    /// is preserved.
    /// </summary>
    [TestMethod]
    public void DisplayHeight_WhenSmallPositive_PreservesValue()
    {
        // Act
        var options = new ConfigChangeMonitorOptions { DisplayHeight = 1f };

        // Assert
        Assert.AreEqual(1f, options.DisplayHeight);
    }
}
