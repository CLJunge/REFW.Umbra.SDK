using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Nodes.UnitTests;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
[TestClass]
public sealed class ConfigDrawerTests
{
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
    /// Tests that the built-in search UI is not rendered when the feature is disabled.
    /// </summary>
    [TestMethod]
    public void Draw_WhenSearchBarDisabled_DoesNotRenderSearchControls()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [],
            [],
            renderer,
            new ConfigDrawerOptions());

        // Act
        drawer.Draw();

        // Assert
        Assert.IsEmpty(renderer.InputTextLabels);
        Assert.IsEmpty(renderer.ButtonLabels);
        Assert.IsEmpty(renderer.RenderedTexts);
    }

    /// <summary>
    /// Tests that the built-in search UI is rendered with a visible label and a hidden input label when the feature is enabled.
    /// </summary>
    [TestMethod]
    public void Draw_WhenSearchBarEnabled_RendersVisibleLabelAndRemainingWidthInput()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        renderer.TextWidths["Search"] = 36f;
        renderer.ButtonWidths["<##ConfigDrawerSearchPrevious"] = 40f;
        renderer.ButtonWidths[">##ConfigDrawerSearchNext"] = 44f;
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [],
            [],
            renderer,
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() });

        // Act
        drawer.Draw();

        // Assert
        Assert.HasCount(1, renderer.RenderedTexts);
        Assert.AreEqual("Search", renderer.RenderedTexts[0]);
        Assert.HasCount(1, renderer.InputTextLabels);
        Assert.AreEqual("##ConfigDrawerSearch", renderer.InputTextLabels[0]);
        Assert.HasCount(1, renderer.NextItemWidths);
        Assert.AreEqual(300f - 36f - 40f - 44f - (8f * 3f), renderer.NextItemWidths[0]);
        Assert.HasCount(2, renderer.ButtonLabels);
        Assert.AreEqual("<##ConfigDrawerSearchPrevious", renderer.ButtonLabels[0]);
        Assert.AreEqual(">##ConfigDrawerSearchNext", renderer.ButtonLabels[1]);
        Assert.AreEqual(3, renderer.SameLineCount);
    }

    /// <summary>
    /// Tests that the drawer reports no active search query before the user enters search text.
    /// </summary>
    [TestMethod]
    public void HasActiveSearchQuery_BeforeQueryEntry_ReturnsFalse()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        renderer.TextWidths["Search"] = 36f;
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [],
            [],
            renderer,
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() });

        // Act
        var hasActiveSearchQuery = drawer.HasActiveSearchQuery;

        // Assert
        Assert.IsFalse(hasActiveSearchQuery);
    }

    /// <summary>
    /// Tests that the drawer reports an active search query after the user enters non-empty search text.
    /// </summary>
    [TestMethod]
    public void HasActiveSearchQuery_AfterQueryEntry_ReturnsTrue()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "audio"
        };
        renderer.TextWidths["Search"] = 36f;
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [],
            [],
            renderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() });

        // Act
        drawer.Draw();
        var hasActiveSearchQuery = drawer.HasActiveSearchQuery;

        // Assert
        Assert.IsTrue(hasActiveSearchQuery);
    }

    /// <summary>
    /// Tests that the search row reuses cached button and label measurements when the available width is unchanged.
    /// </summary>
    [TestMethod]
    public void Draw_WhenAvailableWidthIsUnchanged_ReusesCachedSearchLayout()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        renderer.TextWidths["Search"] = 36f;
        renderer.ButtonWidths["<##ConfigDrawerSearchPrevious"] = 40f;
        renderer.ButtonWidths[">##ConfigDrawerSearchNext"] = 44f;
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [],
            [],
            renderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() });

        // Act
        drawer.Draw();
        drawer.Draw();

        // Assert
        Assert.HasCount(1, renderer.TextWidthRequests);
        Assert.HasCount(2, renderer.ButtonWidthRequests);
        Assert.HasCount(2, renderer.NextItemWidths);
        Assert.AreEqual(renderer.NextItemWidths[0], renderer.NextItemWidths[1]);
    }

    /// <summary>
    /// Tests that the cached search-row width is recomputed when the available width changes.
    /// </summary>
    [TestMethod]
    public void Draw_WhenAvailableWidthChanges_RecomputesSearchLayout()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        renderer.TextWidths["Search"] = 36f;
        renderer.ButtonWidths["<##ConfigDrawerSearchPrevious"] = 40f;
        renderer.ButtonWidths[">##ConfigDrawerSearchNext"] = 44f;
        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [],
            [],
            renderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() });

        // Act
        drawer.Draw();
        renderer.AvailableWidth = 360f;
        drawer.Draw();

        // Assert
        Assert.HasCount(2, renderer.TextWidthRequests);
        Assert.HasCount(4, renderer.ButtonWidthRequests);
        Assert.HasCount(2, renderer.NextItemWidths);
        Assert.AreNotEqual(renderer.NextItemWidths[0], renderer.NextItemWidths[1]);
        Assert.AreEqual(360f - 36f - 40f - 44f - (8f * 3f), renderer.NextItemWidths[1]);
    }

    /// <summary>
    /// Tests that entering a query filters the drawer through the flat search index and only draws matching results without auto-focusing them.
    /// </summary>
    [TestMethod]
    public void Draw_WhenSearchQueryMatchesSingleResult_DrawsOnlyMatchingNodeWithoutAutoFocus()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "audio"
        };
        renderer.TextWidths["Search"] = 36f;
        var alphaNode = new SearchAwareTestNode("alpha");
        var betaNode = new SearchAwareTestNode("beta");
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Master Volume", "Adjusts output level.", "Audio", "config.audio");
        searchIndex.AddParameterResult("beta", "Gamma", "Adjusts display brightness.", "Graphics", "config.graphics");

        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [alphaNode, betaNode],
            [],
            renderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() },
            searchIndex);

        // Act
        drawer.Draw();

        // Assert
        Assert.AreEqual(1, alphaNode.DrawCount);
        Assert.AreEqual(0, betaNode.DrawCount);
        Assert.IsTrue(alphaNode.LastIsMatch);
        Assert.IsFalse(alphaNode.LastIsFocused);
        Assert.IsFalse(betaNode.LastWasVisible);
    }

    /// <summary>
    /// Tests that query changes alone do not request keyboard focus for any matched control.
    /// </summary>
    [TestMethod]
    public void Draw_WhenQueryProducesMatches_DoesNotRequestKeyboardFocusUntilNavigationOccurs()
    {
        // Arrange
        var drawerRenderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "gamma"
        };
        drawerRenderer.TextWidths["Search"] = 36f;
        var alphaRenderer = new TestParameterNodeRenderer();
        var betaRenderer = new TestParameterNodeRenderer();
        var alphaNode = new ParameterNode(static () => { }, order: 0, spacingBefore: 0, spacingAfter: 0, renderer: alphaRenderer, resultId: "alpha");
        var betaNode = new ParameterNode(static () => { }, order: 1, spacingBefore: 0, spacingAfter: 0, renderer: betaRenderer, resultId: "beta");
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Gamma", null, "Graphics", "config.graphics");
        searchIndex.AddParameterResult("beta", "Brightness", null, "Graphics", "config.graphics");

        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [alphaNode, betaNode],
            [],
            drawerRenderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() },
            searchIndex);

        // Act
        drawer.Draw();
        drawer.Draw();

        // Assert
        Assert.AreEqual(0, alphaRenderer.KeyboardFocusCount);
        Assert.AreEqual(0, betaRenderer.KeyboardFocusCount);
    }

    /// <summary>
    /// Tests that the next and previous navigation buttons move focus through the ordered match list after an initially unfocused query result set.
    /// </summary>
    [TestMethod]
    public void Draw_WhenNavigationButtonsAreClicked_MovesFocusedResult()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "ga"
        };
        renderer.TextWidths["Search"] = 36f;
        var alphaNode = new SearchAwareTestNode("alpha");
        var betaNode = new SearchAwareTestNode("beta");
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Gamma", null, "Graphics", "config.graphics");
        searchIndex.AddParameterResult("beta", "Game Speed", null, "Gameplay", "config.gameplay");

        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [alphaNode, betaNode],
            [],
            renderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() },
            searchIndex);

        // Act
        drawer.Draw();
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        drawer.Draw();
        var alphaFocusedAfterFirstNext = alphaNode.LastIsFocused;
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        drawer.Draw();
        var betaFocusedAfterSecondNext = betaNode.LastIsFocused;
        renderer.ButtonResults.Enqueue(true);
        renderer.ButtonResults.Enqueue(false);
        drawer.Draw();

        // Assert
        Assert.IsTrue(alphaFocusedAfterFirstNext);
        Assert.IsTrue(betaFocusedAfterSecondNext);
        Assert.IsTrue(alphaNode.LastIsFocused);
    }

    /// <summary>
    /// Tests that navigation skips matching results that are currently hidden by runtime visibility.
    /// </summary>
    [TestMethod]
    public void Draw_WhenMatchingResultIsHidden_NavigationFocusesVisibleMatch()
    {
        // Arrange
        var drawerRenderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "ga"
        };
        drawerRenderer.TextWidths["Search"] = 36f;
        var hiddenRenderer = new TestParameterNodeRenderer();
        var visibleRenderer = new TestParameterNodeRenderer();
        var hiddenNode = new ParameterNode(static () => false, static () => { }, order: 0, spacingBefore: 0, spacingAfter: 0, renderer: hiddenRenderer, resultId: "alpha");
        var visibleNode = new ParameterNode(static () => true, static () => { }, order: 1, spacingBefore: 0, spacingAfter: 0, renderer: visibleRenderer, resultId: "beta");
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Gamma", null, "Graphics", "config.graphics", static () => false);
        searchIndex.AddParameterResult("beta", "Game Speed", null, "Gameplay", "config.gameplay", static () => true);

        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [hiddenNode, visibleNode],
            [],
            drawerRenderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() },
            searchIndex);

        // Act
        drawer.Draw();
        drawerRenderer.ButtonResults.Enqueue(false);
        drawerRenderer.ButtonResults.Enqueue(true);
        drawer.Draw();

        // Assert
        Assert.AreEqual(0, hiddenRenderer.KeyboardFocusCount);
        Assert.AreEqual(1, visibleRenderer.KeyboardFocusCount);
    }

    /// <summary>
    /// Tests that next and previous navigation transfer keyboard focus to the newly focused control.
    /// </summary>
    [TestMethod]
    public void Draw_WhenNavigationButtonsMoveFocus_RequestsKeyboardFocusForEachNewFocusedControl()
    {
        // Arrange
        var drawerRenderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "ga"
        };
        drawerRenderer.TextWidths["Search"] = 36f;
        var alphaRenderer = new TestParameterNodeRenderer();
        var betaRenderer = new TestParameterNodeRenderer();
        var alphaNode = new ParameterNode(static () => { }, order: 0, spacingBefore: 0, spacingAfter: 0, renderer: alphaRenderer, resultId: "alpha");
        var betaNode = new ParameterNode(static () => { }, order: 1, spacingBefore: 0, spacingAfter: 0, renderer: betaRenderer, resultId: "beta");
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Gamma", null, "Graphics", "config.graphics");
        searchIndex.AddParameterResult("beta", "Game Speed", null, "Gameplay", "config.gameplay");

        using var drawer = new ConfigDrawer<TestConfig>(
            "test-scope",
            [alphaNode, betaNode],
            [],
            drawerRenderer,
            new ConfigDrawerOptions { Search = new Umbra.UI.Config.Search.ConfigSearchOptions() },
            searchIndex);

        // Act
        drawer.Draw();
        drawerRenderer.ButtonResults.Enqueue(false);
        drawerRenderer.ButtonResults.Enqueue(true);
        drawer.Draw();
        drawerRenderer.ButtonResults.Enqueue(false);
        drawerRenderer.ButtonResults.Enqueue(true);
        drawer.Draw();
        drawerRenderer.ButtonResults.Enqueue(true);
        drawerRenderer.ButtonResults.Enqueue(false);
        drawer.Draw();
        drawer.Draw();

        // Assert
        Assert.AreEqual(2, alphaRenderer.KeyboardFocusCount);
        Assert.AreEqual(1, betaRenderer.KeyboardFocusCount);
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

    private sealed class SearchAwareTestNode(string resultId) : IDrawNode, IConfigSearchNode
    {
        public int DrawCount { get; private set; }
        public bool LastIsMatch { get; private set; }
        public bool LastIsFocused { get; private set; }
        public bool LastWasVisible { get; private set; }
        public bool WasFocusedAtLeastOnce { get; private set; }

        public void Draw()
        {
            if (!LastWasVisible)
                return;

            DrawCount++;
        }

        public bool ApplySearch(ConfigSearchRenderState? searchState)
        {
            if (searchState is null || !searchState.HasActiveQuery)
            {
                LastIsMatch = false;
                LastIsFocused = false;
                LastWasVisible = true;
                return true;
            }

            LastIsMatch = searchState.IsMatch(resultId);
            LastIsFocused = searchState.IsFocused(resultId);
            LastWasVisible = LastIsMatch;
            if (LastIsFocused)
                WasFocusedAtLeastOnce = true;

            return LastWasVisible;
        }
    }

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
    /// Tests that the options-aware constructor succeeds when search-bar support is enabled.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_WithOptions_ConstructsSuccessfully()
    {
        // Arrange
        var config = new SimpleConfig();
        var options = new ConfigDrawerOptions { Search = new ConfigSearchOptions() };

        // Act
        using var drawer = new ConfigDrawer<SimpleConfig>(config, "TestPlugin", options);

        // Assert
        Assert.IsNotNull(drawer);
    }

    /// <summary>
    /// Tests that the options-aware constructor rejects a null options instance.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new SimpleConfig();

        // Act
        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = new ConfigDrawer<SimpleConfig>(config, "TestPlugin", null!));

        // Assert
        Assert.AreEqual("options", exception.ParamName);
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
    /// Tests that the options-aware constructor suppresses the root wrapper when requested.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_WithOptionsSuppressRootNodeTrue_DoesNotWrapNodesInRootTreeNode()
    {
        // Arrange
        var config = new RootWrappedConfig();

        // Act
        using var drawer = new ConfigDrawer<RootWrappedConfig>(
            config,
            "TestPlugin",
            new ConfigDrawerOptions { SuppressRootNode = true });
        var nodes = GetTopLevelNodes(drawer);

        // Assert
        Assert.HasCount(1, nodes);
        Assert.IsFalse(nodes[0] is RootTreeNode);
    }

    /// <summary>
    /// Tests that the options-aware constructor still emits the root wrapper when suppression is not requested.
    /// </summary>
    [TestMethod]
    public void ConfigDrawer_WithOptionsSuppressRootNodeFalse_WrapsNodesInRootTreeNode()
    {
        // Arrange
        var config = new RootWrappedConfig();

        // Act
        using var drawer = new ConfigDrawer<RootWrappedConfig>(
            config,
            "TestPlugin",
            new ConfigDrawerOptions { SuppressRootNode = false });
        var nodes = GetTopLevelNodes(drawer);

        // Assert
        Assert.HasCount(1, nodes);
        Assert.IsTrue(nodes[0] is RootTreeNode);
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
    /// Configuration class that declares a root wrapper attribute.
    /// </summary>
    [UmbraAutoRegister]
    [UmbraRootNode("Root Wrapped", true)]
    private sealed class RootWrappedConfig
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
    /// Configuration class with a nested group.
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

    private static class ConfigDrawerReflection
    {
        private const string _nodesFieldName = "_nodes";

        public static List<IDrawNode> GetTopLevelNodes<TConfig>(ConfigDrawer<TConfig> drawer) where TConfig : class
        {
            var nodesField = drawer.GetType().GetField(
                _nodesFieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.IsNotNull(nodesField);

            var nodes = nodesField.GetValue(drawer) as List<IDrawNode>;
            Assert.IsNotNull(nodes);
            return nodes;
        }
    }

    private static List<IDrawNode> GetTopLevelNodes<TConfig>(ConfigDrawer<TConfig> drawer) where TConfig : class => ConfigDrawerReflection.GetTopLevelNodes(drawer);

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
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConfigDrawer<SimpleConfig>(null!, "TestScope"));

        Assert.AreEqual("config", exception.ParamName);
    }

    /// <summary>
    /// Tests that the public constructor rejects a null id scope.
    /// </summary>
    [TestMethod]
    public void Constructor_NullIdScope_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConfigDrawer<SimpleConfig>(new SimpleConfig(), null!));

        Assert.AreEqual("idScope", exception.ParamName);
    }

    /// <summary>
    /// Tests that the public constructor rejects whitespace-only id scopes.
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceIdScope_ThrowsArgumentException()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() => _ = new ConfigDrawer<SimpleConfig>(new SimpleConfig(), "   "));

        Assert.AreEqual("idScope", exception.ParamName);
    }

    /// <summary>
    /// Tests that the internal constructor rejects a null node list.
    /// </summary>
    [TestMethod]
    public void Constructor_InternalNullNodes_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConfigDrawer<SimpleTestConfig>("TestScope", null!, [], new TestConfigDrawerScope()));

        Assert.AreEqual("nodes", exception.ParamName);
    }

    /// <summary>
    /// Tests that the internal constructor rejects a null disposable list.
    /// </summary>
    [TestMethod]
    public void Constructor_InternalNullDisposables_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ConfigDrawer<SimpleTestConfig>("TestScope", [], null!, new TestConfigDrawerScope()));

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
