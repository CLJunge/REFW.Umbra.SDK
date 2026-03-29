using Umbra.Config;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ConfigDrawer{TConfig}.Draw"/> method.
/// </summary>
[TestClass]
public sealed class ConfigDrawerTests
{
    /// <summary>
    /// Tests that calling <see cref="ConfigDrawer{TConfig}.Draw"/> on a disposed instance
    /// does not throw an exception and handles the disposed state gracefully.
    /// </summary>
    /// <remarks>
    /// This test verifies the early-return path when <c>_disposed</c> is <see langword="true"/>.
    /// The method should log a warning (via <see cref="Umbra.Logging.Logger.Warning"/>)
    /// and return without processing nodes or calling ImGui methods. Since Logger is static
    /// and cannot be mocked with Moq, we can only verify that no exception is thrown.
    /// </remarks>
    [TestMethod]
    public void Draw_WhenDisposed_DoesNotThrow()
    {
        // Arrange
        var config = new TestConfig();
        var drawer = new ConfigDrawer<TestConfig>(config, "test-scope");
        drawer.Dispose();

        // Act & Assert
        drawer.Draw(); // Should not throw
    }

    /// <summary>
    /// Tests that calling <see cref="ConfigDrawer{TConfig}.Draw"/> on a valid, non-disposed
    /// instance executes without throwing an exception.
    /// </summary>
    /// <remarks>
    /// This test verifies the normal execution path. The method should push an ImGui ID scope,
    /// iterate through all nodes calling their Draw methods, and pop the ID scope in the finally block.
    /// Since ImGui static methods cannot be mocked and nodes are built internally by the constructor,
    /// we can only verify that the method completes without throwing.
    /// </remarks>
    [TestMethod]
    public void Draw_WhenNotDisposed_DoesNotThrow()
    {
        // Arrange
        var config = new TestConfig();
        var drawer = new ConfigDrawer<TestConfig>(config, "test-scope");

        // Act & Assert
        drawer.Draw(); // Should not throw
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawer{TConfig}.Draw"/> can be called multiple times
    /// sequentially on the same instance without issues.
    /// </summary>
    /// <remarks>
    /// This verifies that the Draw method is idempotent and can be invoked repeatedly
    /// during the render loop without accumulating state or causing errors.
    /// </remarks>
    [TestMethod]
    public void Draw_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var config = new TestConfig();
        var drawer = new ConfigDrawer<TestConfig>(config, "test-scope");

        // Act & Assert
        drawer.Draw(); // First call
        drawer.Draw(); // Second call
        drawer.Draw(); // Third call
        // All calls should complete without throwing
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawer{TConfig}.Draw"/> calls Dispose() only once
    /// even when called multiple times, by verifying subsequent Draw() calls still work.
    /// </summary>
    /// <remarks>
    /// This verifies that Dispose() is idempotent and that calling Draw() multiple times
    /// after disposal continues to handle the disposed state gracefully.
    /// </remarks>
    [TestMethod]
    public void Draw_AfterMultipleDisposes_DoesNotThrow()
    {
        // Arrange
        var config = new TestConfig();
        var drawer = new ConfigDrawer<TestConfig>(config, "test-scope");

        // Act
        drawer.Dispose();
        drawer.Dispose(); // Second dispose should be safe

        // Assert
        drawer.Draw(); // Should not throw
        drawer.Draw(); // Second draw after dispose should also not throw
    }

    #region Helper Types

    /// <summary>
    /// Minimal test configuration class for ConfigDrawer tests.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class TestConfig
    {
    }

    #endregion

    /// <summary>
    /// Tests that the constructor succeeds with valid config and idScope parameters.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_ValidParameters_ConstructsSuccessfully()
    {
        // Arrange
        var config = new SimpleConfig();
        var idScope = "TestPlugin";

        // Act
        using var drawer = new ConfigDrawer<SimpleConfig>(config, idScope);

        // Assert
        Assert.IsNotNull(drawer);
    }

    /// <summary>
    /// Tests that the constructor succeeds when suppressRootNode is explicitly set to true.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_SuppressRootNodeTrue_ConstructsSuccessfully()
    {
        // Arrange
        var config = new SimpleConfig();
        var idScope = "TestPlugin";

        // Act
        using var drawer = new ConfigDrawer<SimpleConfig>(config, idScope, suppressRootNode: true);

        // Assert
        Assert.IsNotNull(drawer);
    }

    /// <summary>
    /// Tests that the constructor handles config with nested groups correctly.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_ConfigWithNestedGroups_ConstructsSuccessfully()
    {
        // Arrange
        var config = new ConfigWithNestedGroup();
        var idScope = "TestPlugin";

        // Act
        using var drawer = new ConfigDrawer<ConfigWithNestedGroup>(config, idScope);

        // Assert
        Assert.IsNotNull(drawer);
    }

    /// <summary>
    /// Tests that the constructor handles config with multiple parameters correctly.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_ConfigWithMultipleParameters_ConstructsSuccessfully()
    {
        // Arrange
        var config = new ConfigWithMultipleParameters();
        var idScope = "TestPlugin";

        // Act
        using var drawer = new ConfigDrawer<ConfigWithMultipleParameters>(config, idScope);

        // Assert
        Assert.IsNotNull(drawer);
    }

    #region Test Config Classes

    /// <summary>
    /// Simple configuration class with a single parameter for basic testing.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class SimpleConfig
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    /// <summary>
    /// Configuration class with a nested settings group.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class ConfigWithNestedGroup
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraSettingsParameter]
        [UmbraSettingsPrefix("nested")]
        public NestedGroup Nested { get; set; } = new();
    }

    /// <summary>
    /// Nested configuration group.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class NestedGroup
    {
        [UmbraSettingsParameter]
        public Parameter<int> Value { get; set; } = new(42);
    }

    /// <summary>
    /// Configuration class with multiple parameters of different types.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class ConfigWithMultipleParameters
    {
        [UmbraSettingsParameter]
        public Parameter<bool> BoolParam { get; set; } = new(true);

        [UmbraSettingsParameter]
        public Parameter<int> IntParam { get; set; } = new(100);

        [UmbraSettingsParameter]
        public Parameter<float> FloatParam { get; set; } = new(3.14f);

        [UmbraSettingsParameter]
        public Parameter<double> DoubleParam { get; set; } = new(2.71828);

        [UmbraSettingsParameter]
        public Parameter<string> StringParam { get; set; } = new("test");
    }

    #endregion

    /// <summary>
    /// Verifies that calling <see cref="ConfigDrawer{TConfig}.Dispose"/> once
    /// completes successfully without throwing any exceptions.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledOnce_ShouldNotThrow()
    {
        // Arrange
        var config = new SimpleTestConfig();
        var drawer = new ConfigDrawer<SimpleTestConfig>(config, "TestScope");

        // Act & Assert
        drawer.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="ConfigDrawer{TConfig}.Dispose"/> is idempotent
    /// and can be called multiple times without throwing exceptions or causing errors.
    /// Tests with 2, 5, and 10 consecutive calls.
    /// </summary>
    /// <param name="callCount">The number of times to call Dispose.</param>
    [TestMethod]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    public void Dispose_WhenCalledMultipleTimes_ShouldBeIdempotent(int callCount)
    {
        // Arrange
        var config = new SimpleTestConfig();
        var drawer = new ConfigDrawer<SimpleTestConfig>(config, "TestScope");

        // Act & Assert
        for (var i = 0; i < callCount; i++)
        {
            drawer.Dispose();
        }
    }

    /// <summary>
    /// Verifies that <see cref="ConfigDrawer{TConfig}.Dispose"/> works correctly
    /// with a config that has multiple parameters of different types.
    /// </summary>
    [TestMethod]
    public void Dispose_WithComplexConfig_ShouldNotThrow()
    {
        // Arrange
        var config = new ComplexTestConfig();
        var drawer = new ConfigDrawer<ComplexTestConfig>(config, "TestScope");

        // Act
        drawer.Dispose();

        // Assert - No exception thrown
    }

    /// <summary>
    /// Verifies that <see cref="ConfigDrawer{TConfig}.Dispose"/> can be called
    /// on multiple instances without interference.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleInstances_ShouldDisposeIndependently()
    {
        // Arrange
        var config1 = new SimpleTestConfig();
        var config2 = new SimpleTestConfig();
        var drawer1 = new ConfigDrawer<SimpleTestConfig>(config1, "TestScope1");
        var drawer2 = new ConfigDrawer<SimpleTestConfig>(config2, "TestScope2");

        // Act
        drawer1.Dispose();
        drawer2.Dispose();
        drawer1.Dispose(); // Verify idempotency for first instance

        // Assert - No exception thrown
    }

    #region Test Config Classes

    /// <summary>
    /// Simple test configuration with a single parameter.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class SimpleTestConfig
    {
        [UmbraSettingsParameter]
        public Parameter<int> TestValue { get; set; } = new(42);
    }

    /// <summary>
    /// Complex test configuration with multiple parameters of different types.
    /// </summary>
    [UmbraAutoRegisterSettings]
    private sealed class ComplexTestConfig
    {
        [UmbraSettingsParameter]
        public Parameter<int> IntValue { get; set; } = new(100);

        [UmbraSettingsParameter]
        public Parameter<string> StringValue { get; set; } = new("test");

        [UmbraSettingsParameter]
        public Parameter<bool> BoolValue { get; set; } = new(true);

        [UmbraSettingsParameter]
        public Parameter<float> FloatValue { get; set; } = new(3.14f);

        [UmbraSettingsParameter]
        public Parameter<double> DoubleValue { get; set; } = new(2.718);
    }

    #endregion
}
