using Umbra.Config;
using Umbra.Config.Attributes;
namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
[TestClass]
public sealed class ConfigDrawerTests
{
    /// <summary>
    /// Verifies that an action throws the expected exception type and returns the captured exception.
    /// </summary>
    private static TException AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
        throw new InvalidOperationException("Unreachable");
    }

    /// <summary>
    /// Tests that calling <see cref="ConfigDrawer{TConfig}.Draw"/> on a disposed instance silently
    /// skips rendering work.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDisposed_SkipsRendering()
    {
        // Arrange
        var scope = new TestConfigDrawerScope();
        var node = new TestDrawNode();
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [node],
            [],
            scope);
        drawer.Dispose();

        // Act
        drawer.Draw();

        // Assert
        Assert.IsEmpty(scope.PushedIds);
        Assert.AreEqual(0, scope.PopCount);
        Assert.AreEqual(0, node.DrawCount);
    }

    /// <summary>
    /// Tests that calling <see cref="ConfigDrawer{TConfig}.Draw"/> on a valid instance pushes the
    /// configured scope, draws each node once, and pops the scope afterward.
    /// </summary>
    [TestMethod]
    public void Draw_WhenNotDisposed_PushesScopeAndDrawsNodes()
    {
        // Arrange
        var scope = new TestConfigDrawerScope();
        var firstNode = new TestDrawNode();
        var secondNode = new TestDrawNode();
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [firstNode, secondNode],
            [],
            scope);

        // Act
        drawer.Draw();

        // Assert
        Assert.HasCount(1, scope.PushedIds);
        Assert.AreEqual("test-scope", scope.PushedIds[0]);
        Assert.AreEqual(1, scope.PopCount);
        Assert.AreEqual(1, firstNode.DrawCount);
        Assert.AreEqual(1, secondNode.DrawCount);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawer{TConfig}.Draw"/> can be called multiple times sequentially
    /// on the same instance.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_DrawsNodesOnEachCall()
    {
        // Arrange
        var scope = new TestConfigDrawerScope();
        var node = new TestDrawNode();
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [node],
            [],
            scope);

        // Act
        drawer.Draw();
        drawer.Draw();
        drawer.Draw();

        // Assert
        Assert.HasCount(3, scope.PushedIds);
        Assert.AreEqual(3, scope.PopCount);
        Assert.AreEqual(3, node.DrawCount);
    }

    /// <summary>
    /// Tests that the draw scope is popped even when a node throws during rendering.
    /// </summary>
    [TestMethod]
    public void Draw_WhenNodeThrows_PopsScopeBeforeRethrowing()
    {
        // Arrange
        var scope = new TestConfigDrawerScope();
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [new TestDrawNode(() => throw new InvalidOperationException("boom"))],
            [],
            scope);

        // Act
        InvalidOperationException? exception = null;
        try
        {
            drawer.Draw();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("boom", exception.Message);
        Assert.HasCount(1, scope.PushedIds);
        Assert.AreEqual(1, scope.PopCount);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawer{TConfig}.Draw"/> remains safe after multiple dispose calls.
    /// </summary>
    [TestMethod]
    public void Draw_AfterMultipleDisposes_StillSkipsRendering()
    {
        // Arrange
        var scope = new TestConfigDrawerScope();
        var node = new TestDrawNode();
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [node],
            [],
            scope);
        drawer.Dispose();
        drawer.Dispose();

        // Act
        drawer.Draw();
        drawer.Draw();

        // Assert
        Assert.AreEqual(0, node.DrawCount);
        Assert.IsEmpty(scope.PushedIds);
        Assert.AreEqual(0, scope.PopCount);
    }

    #region Helper Types

    /// <summary>
    /// Minimal test configuration class for ConfigDrawer draw tests.
    /// </summary>
    [UmbraAutoRegister]
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
    /// Tests that <see cref="ConfigDrawer{TConfig}"/> does not require the config type to expose a
    /// public parameterless constructor when the caller already supplies the config instance.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_ConfigWithoutParameterlessConstructor_ConstructsSuccessfully()
    {
        // Arrange
        var config = new ConfigWithoutParameterlessConstructor(new Parameter<bool>(true));

        // Act
        using var drawer = new ConfigDrawer<ConfigWithoutParameterlessConstructor>(config, "TestPlugin");

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
    [UmbraAutoRegister]
    private sealed class SimpleConfig
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);
    }

    /// <summary>
    /// Configuration class without a public parameterless constructor.
    /// Used to verify that <see cref="ConfigDrawer{TConfig}"/> can still be constructed when the
    /// caller supplies the config instance explicitly.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class ConfigWithoutParameterlessConstructor(Parameter<bool> enabled)
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; } = enabled;
    }

    /// <summary>
    /// Configuration class with a nested settings group.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class ConfigWithNestedGroup
    {
        [UmbraParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraParameter]
        [UmbraPrefix("nested")]
        public NestedGroup Nested { get; set; } = new();
    }

    /// <summary>
    /// Nested configuration group.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class NestedGroup
    {
        [UmbraParameter]
        public Parameter<int> Value { get; set; } = new(42);
    }

    /// <summary>
    /// Configuration class with multiple parameters of different types.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class ConfigWithMultipleParameters
    {
        [UmbraParameter]
        public Parameter<bool> BoolParam { get; set; } = new(true);

        [UmbraParameter]
        public Parameter<int> IntParam { get; set; } = new(100);

        [UmbraParameter]
        public Parameter<float> FloatParam { get; set; } = new(3.14f);

        [UmbraParameter]
        public Parameter<double> DoubleParam { get; set; } = new(2.71828);

        [UmbraParameter]
        public Parameter<string> StringParam { get; set; } = new("test");
    }

    #endregion

    /// <summary>
    /// Verifies that calling <see cref="ConfigDrawer{TConfig}.Dispose"/> once disposes owned
    /// resources successfully.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledOnce_DisposesOwnedResources()
    {
        // Arrange
        var disposable = new TestDisposable();
        using var drawer = new ConfigDrawer<SimpleTestConfig>(
            "TestScope",
            [],
            [disposable],
            new TestConfigDrawerScope());

        // Act
        drawer.Dispose();

        // Assert
        Assert.AreEqual(1, disposable.DisposeCount);
    }

    /// <summary>
    /// Verifies that <see cref="ConfigDrawer{TConfig}.Dispose"/> is idempotent and can be called
    /// multiple times without disposing owned resources more than once.
    /// </summary>
    /// <param name="callCount">The number of times to call <see cref="ConfigDrawer{TConfig}.Dispose"/>.</param>
    [TestMethod]
    [DataRow(2)]
    [DataRow(5)]
    [DataRow(10)]
    public void Dispose_WhenCalledMultipleTimes_DisposesOwnedResourcesOnce(int callCount)
    {
        // Arrange
        var disposable = new TestDisposable();
        using var drawer = new ConfigDrawer<SimpleTestConfig>(
            "TestScope",
            [],
            [disposable],
            new TestConfigDrawerScope());

        // Act
        for (var i = 0; i < callCount; i++)
            drawer.Dispose();

        // Assert
        Assert.AreEqual(1, disposable.DisposeCount);
    }

    /// <summary>
    /// Verifies that <see cref="ConfigDrawer{TConfig}.Dispose"/> works correctly with a config that
    /// has multiple parameters of different types.
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
    /// Verifies that <see cref="ConfigDrawer{TConfig}.Dispose"/> can be called on multiple instances
    /// without interference.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleInstances_DisposeIndependently()
    {
        // Arrange
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();
        using var drawer1 = new ConfigDrawer<SimpleTestConfig>(
            "TestScope1",
            [],
            [disposable1],
            new TestConfigDrawerScope());
        using var drawer2 = new ConfigDrawer<SimpleTestConfig>(
            "TestScope2",
            [],
            [disposable2],
            new TestConfigDrawerScope());

        // Act
        drawer1.Dispose();
        drawer2.Dispose();
        drawer1.Dispose();

        // Assert
        Assert.AreEqual(1, disposable1.DisposeCount);
        Assert.AreEqual(1, disposable2.DisposeCount);
    }

    /// <summary>
    /// Tests that the public constructor rejects a null config instance.
    /// </summary>
    [TestMethod]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var exception = AssertThrows<ArgumentNullException>(() => _ = new ConfigDrawer<SimpleConfig>(null!, "TestScope"));

        Assert.AreEqual("config", exception.ParamName);
    }

    /// <summary>
    /// Tests that the public constructor rejects whitespace-only id scopes.
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceIdScope_ThrowsArgumentException()
    {
        var exception = AssertThrows<ArgumentException>(() => _ = new ConfigDrawer<SimpleConfig>(new SimpleConfig(), "   "));

        Assert.AreEqual("idScope", exception.ParamName);
    }

    /// <summary>
    /// Tests that the internal constructor rejects a null node list.
    /// </summary>
    [TestMethod]
    public void Constructor_InternalNullNodes_ThrowsArgumentNullException()
    {
        var exception = AssertThrows<ArgumentNullException>(() => _ = new ConfigDrawer<SimpleTestConfig>("TestScope", null!, [], new TestConfigDrawerScope()));

        Assert.AreEqual("nodes", exception.ParamName);
    }

    /// <summary>
    /// Tests that the internal constructor rejects a null disposable list.
    /// </summary>
    [TestMethod]
    public void Constructor_InternalNullDisposables_ThrowsArgumentNullException()
    {
        var exception = AssertThrows<ArgumentNullException>(() => _ = new ConfigDrawer<SimpleTestConfig>("TestScope", [], null!, new TestConfigDrawerScope()));

        Assert.AreEqual("disposables", exception.ParamName);
    }

    #region Test Config Classes

    /// <summary>
    /// Simple test configuration with a single parameter.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class SimpleTestConfig
    {
        [UmbraParameter]
        public Parameter<int> TestValue { get; set; } = new(42);
    }

    /// <summary>
    /// Complex test configuration with multiple parameters of different types.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class ComplexTestConfig
    {
        [UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(100);

        [UmbraParameter]
        public Parameter<string> StringValue { get; set; } = new("test");

        [UmbraParameter]
        public Parameter<bool> BoolValue { get; set; } = new(true);

        [UmbraParameter]
        public Parameter<float> FloatValue { get; set; } = new(3.14f);

        [UmbraParameter]
        public Parameter<double> DoubleValue { get; set; } = new(2.718);
    }

    #endregion
}
