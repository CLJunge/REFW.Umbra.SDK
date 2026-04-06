using Umbra.UI.Config.Search;

namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterNode"/>.
/// </summary>
[TestClass]
public sealed class ParameterNodeTests
{
    /// <summary>
    /// Verifies that when the disabled predicate returns <see langword="true"/>, the draw action still runs inside one disabled region.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDisabledPredicateReturnsTrue_WrapsDrawInDisabledRegion()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        bool isVisible() => true;
        bool isDisabled() => true;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 0, 0, renderer, isDisabled: isDisabled);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(1, renderer.BeginDisabledCount);
        Assert.AreEqual(1, renderer.EndDisabledCount);
        Assert.AreEqual(true, renderer.LastBeginDisabledValue);
    }

    /// <summary>
    /// Verifies that when the disabled predicate returns <see langword="false"/>, no disabled region is emitted.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDisabledPredicateReturnsFalse_DoesNotEmitDisabledRegion()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        bool isVisible() => true;
        bool isDisabled() => false;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 0, 0, renderer, isDisabled: isDisabled);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(0, renderer.BeginDisabledCount);
        Assert.AreEqual(0, renderer.EndDisabledCount);
    }

    /// <summary>
    /// Verifies that a hidden node does not begin a disabled region even if its disabled predicate would return <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void Draw_WhenNotVisible_DoesNotEmitDisabledRegion()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCalled = false;
        bool isVisible() => false;
        bool isDisabled() => true;
        void draw() => drawCalled = true;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 0, 0, renderer, isDisabled: isDisabled);

        // Act
        node.Draw();

        // Assert
        Assert.IsFalse(drawCalled);
        Assert.AreEqual(0, renderer.BeginDisabledCount);
        Assert.AreEqual(0, renderer.EndDisabledCount);
    }

    /// <summary>
    /// Verifies that when <c>isVisible</c> returns <see langword="false"/>, the <see cref="ParameterNode.Draw"/>
    /// method returns immediately without invoking the draw action or spacing calls.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleReturnsFalse_DoesNotCallDrawActionOrSpacing()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCalled = false;
        bool isVisible() => false;
        void draw() => drawCalled = true;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 2, 3, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.IsFalse(drawCalled);
        Assert.AreEqual(0, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies that when <c>isVisible</c> returns <see langword="true"/> and spacing values are
    /// zero, the draw action is invoked exactly once with no spacing operations.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleReturnsTrueWithZeroSpacing_CallsDrawActionOnce()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        bool isVisible() => true;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 0, 0, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(0, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies that the always-visible constructor skips predicate evaluation and still invokes the draw action.
    /// </summary>
    [TestMethod]
    public void Draw_AlwaysVisibleConstructor_CallsDrawActionWithoutVisibilityPredicate()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        void draw() => drawCallCount++;
        var node = new ParameterNode(draw, int.MaxValue, 0, 0, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(0, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies that mixed spacing values emit only the positive side's spacing calls.
    /// </summary>
    [TestMethod]
    public void Draw_MixedSpacingValues_EmitsOnlyPositiveSpacing()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        bool isVisible() => true;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, -2, 3, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(3, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies that when <c>isVisible</c> returns <see langword="true"/> with positive spacing
    /// values, the configured number of spacing calls are emitted around the draw action.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleReturnsTrueWithPositiveSpacing_EmitsSpacingAroundDrawAction()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        bool isVisible() => true;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 2, 3, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(5, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies that a configured indent is applied around the draw action exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WithIndent_UsesRendererIndentationAroundDrawAction()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        const float indentAmount = 12.5f;
        bool isVisible() => true;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 0, 0, renderer, indentAmount);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(1, renderer.IndentCount);
        Assert.AreEqual(1, renderer.UnindentCount);
        Assert.AreEqual(indentAmount, renderer.LastIndentAmount);
        Assert.AreEqual(indentAmount, renderer.LastUnindentAmount);
    }

    /// <summary>
    /// Verifies that indentation is skipped when the parameter node is not visible.
    /// </summary>
    [TestMethod]
    public void Draw_NotVisibleWithIndent_DoesNotIndentOrDraw()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCalled = false;
        bool isVisible() => false;
        void draw() => drawCalled = true;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 0, 0, renderer, 8f);

        // Act
        node.Draw();

        // Assert
        Assert.IsFalse(drawCalled);
        Assert.AreEqual(0, renderer.IndentCount);
        Assert.AreEqual(0, renderer.UnindentCount);
    }

    /// <summary>
    /// Verifies that negative spacing values do not emit any spacing operations and still invoke the
    /// draw action.
    /// </summary>
    [TestMethod]
    public void Draw_BothSpacingValuesNegative_StillCallsDrawActionWithoutSpacing()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        bool isVisible() => true;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, -1, -1, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(0, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies that the <see cref="ParameterNode.Draw"/> method can be called multiple times in sequence.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_InvokesDrawActionEachTime()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        bool isVisible() => true;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 1, 1, renderer);

        // Act
        node.Draw();
        node.Draw();
        node.Draw();

        // Assert
        Assert.AreEqual(3, drawCallCount);
        Assert.AreEqual(6, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies behavior when <c>isVisible</c> alternates between <see langword="true"/> and
    /// <see langword="false"/> across multiple draw calls.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleAlternates_CallsDrawActionOnlyWhenVisible()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        var visible = true;
        bool isVisible() => visible;
        void draw() => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, int.MaxValue, 1, 0, renderer);

        // Act & Assert
        node.Draw();
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(1, renderer.SpacingCount);

        visible = false;
        node.Draw();
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(1, renderer.SpacingCount);

        visible = true;
        node.Draw();
        Assert.AreEqual(2, drawCallCount);
        Assert.AreEqual(2, renderer.SpacingCount);
    }

    /// <summary>
    /// Verifies that the <see cref="ParameterNode.Order"/> property returns the value provided during construction.
    /// </summary>
    [TestMethod]
    public void Order_ReturnsConstructorValue()
    {
        // Arrange
        const int expectedOrder = 42;
        static bool isVisible() => true;
        static void draw() { }
        var node = new ParameterNode(isVisible, draw, expectedOrder);

        // Act
        var actualOrder = node.Order;

        // Assert
        Assert.AreEqual(expectedOrder, actualOrder);
    }

    /// <summary>
    /// Verifies that the <see cref="ParameterNode.Order"/> property defaults to <see cref="int.MaxValue"/> when not specified.
    /// </summary>
    [TestMethod]
    public void Order_DefaultsToIntMaxValue()
    {
        // Arrange
        static bool isVisible() => true;
        static void draw() { }
        var node = new ParameterNode(isVisible, draw);

        // Act
        var actualOrder = node.Order;

        // Assert
        Assert.AreEqual(int.MaxValue, actualOrder);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null visibility predicate.
    /// </summary>
    [TestMethod]
    public void Constructor_NullVisibilityPredicate_ThrowsArgumentNullException()
    {
        var renderer = new TestParameterNodeRenderer();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ParameterNode(null!, static () => { }, int.MaxValue, 0, 0, renderer));

        Assert.AreEqual("isVisible", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null draw action.
    /// </summary>
    [TestMethod]
    public void Constructor_NullDrawAction_ThrowsArgumentNullException()
    {
        var renderer = new TestParameterNodeRenderer();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ParameterNode(static () => true, null!, int.MaxValue, 0, 0, renderer));

        Assert.AreEqual("draw", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the renderer-aware constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ParameterNode(static () => true, static () => { }, int.MaxValue, 0, 0, renderer: null!));

        Assert.AreEqual("renderer", exception.ParamName);
    }

    /// <summary>
    /// Verifies that applying a matching search state keeps the node visible and applies match highlighting.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenResultMatches_AppliesMatchHighlight()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        var node = new ParameterNode(static () => true, () => drawCallCount++, resultId: "alpha", renderer: renderer, order: int.MaxValue, spacingBefore: 0, spacingAfter: 0);
        var renderState = CreateRenderState(["alpha"], focusedResultId: null, pendingScrollResultId: null, pendingFocusResultId: null);

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();

        // Assert
        Assert.IsTrue(visible);
        Assert.AreEqual(1, drawCallCount);
        Assert.AreEqual(4, renderer.PushStyleColorCount);
        Assert.AreEqual(Hexa.NET.ImGui.ImGuiCol.Text, renderer.PushedStyleColors[0].Color);
        Assert.AreNotEqual(0, renderer.PopStyleColorCount);
        Assert.AreEqual(0, renderer.KeyboardFocusCount);
    }

    /// <summary>
    /// Verifies that applying a focused search state uses the focused highlight, scrolls the node into view once, and requests keyboard focus once.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenResultIsFocused_AppliesFocusedHighlightScrollAndKeyboardFocusOnce()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        var node = new ParameterNode(static () => true, () => drawCallCount++, resultId: "alpha", renderer: renderer, order: int.MaxValue, spacingBefore: 0, spacingAfter: 0);
        var renderState = CreateRenderState(["alpha"], focusedResultId: "alpha", pendingScrollResultId: "alpha", pendingFocusResultId: "alpha");

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();
        node.Draw();

        // Assert
        Assert.IsTrue(visible);
        Assert.AreEqual(2, drawCallCount);
        Assert.AreEqual(8, renderer.PushStyleColorCount);
        Assert.AreEqual(Hexa.NET.ImGui.ImGuiCol.Text, renderer.PushedStyleColors[0].Color);
        Assert.AreEqual(1, renderer.ScrollHereCount);
        Assert.AreEqual(1, renderer.KeyboardFocusCount);
    }

    /// <summary>
    /// Verifies that applying a non-matching search state hides the node and skips drawing.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenResultDoesNotMatch_HidesNode()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        var node = new ParameterNode(static () => true, () => drawCallCount++, resultId: "alpha", renderer: renderer, order: int.MaxValue, spacingBefore: 0, spacingAfter: 0);
        var renderState = CreateRenderState(["beta"], focusedResultId: null, pendingScrollResultId: null, pendingFocusResultId: null);

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();

        // Assert
        Assert.IsFalse(visible);
        Assert.AreEqual(0, drawCallCount);
        Assert.AreEqual(0, renderer.PushStyleColorCount);
        Assert.AreEqual(0, renderer.KeyboardFocusCount);
    }

    /// <summary>
    /// Verifies that a matching result stays hidden during search when the runtime visibility predicate is false.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenMatchingResultIsRuntimeHidden_HidesNode()
    {
        // Arrange
        var renderer = new TestParameterNodeRenderer();
        var drawCallCount = 0;
        var node = new ParameterNode(static () => false, () => drawCallCount++, resultId: "alpha", renderer: renderer, order: int.MaxValue, spacingBefore: 0, spacingAfter: 0);
        var renderState = CreateRenderState(["alpha"], focusedResultId: null, pendingScrollResultId: null, pendingFocusResultId: null);

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();

        // Assert
        Assert.IsFalse(visible);
        Assert.AreEqual(0, drawCallCount);
        Assert.AreEqual(0, renderer.PushStyleColorCount);
        Assert.AreEqual(0, renderer.KeyboardFocusCount);
    }

    /// <summary>
    /// Verifies that a hidden wrapper does not stay visible during search just because a child result matches.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenMatchingChildIsWrappedByHiddenNode_HidesWrapper()
    {
        // Arrange
        var wrapperRenderer = new TestParameterNodeRenderer();
        var childRenderer = new TestParameterNodeRenderer();
        var childDrawCallCount = 0;
        var childNode = new ParameterNode(static () => true, () => childDrawCallCount++, resultId: "alpha", renderer: childRenderer, order: 0, spacingBefore: 0, spacingAfter: 0);
        var wrapperNode = new ParameterNode(static () => false, static () => { }, order: 0, spacingBefore: 0, spacingAfter: 0, renderer: wrapperRenderer, children: [childNode]);
        var renderState = CreateRenderState(["alpha"], focusedResultId: null, pendingScrollResultId: null, pendingFocusResultId: null);

        // Act
        var visible = ((IConfigSearchNode)wrapperNode).ApplySearch(renderState);
        wrapperNode.Draw();

        // Assert
        Assert.IsFalse(visible);
        Assert.AreEqual(0, childDrawCallCount);
        Assert.AreEqual(0, childRenderer.PushStyleColorCount);
        Assert.AreEqual(0, wrapperRenderer.PushStyleColorCount);
    }

    private static ConfigSearchRenderState CreateRenderState(
        string[] matchIds,
        string? focusedResultId,
        string? pendingScrollResultId,
        string? pendingFocusResultId)
    {
        var searchState = new ConfigDrawerSearchState();
        searchState.SetQuery("alpha");
        searchState.SetMatches(matchIds);

        if (focusedResultId is not null && searchState.FocusedResultId != focusedResultId)
        {
            for (var i = 0; i < matchIds.Length; i++)
            {
                if (searchState.FocusedResultId == focusedResultId)
                    break;

                searchState.MoveNext();
            }
        }

        if (pendingScrollResultId is null && searchState.PendingScrollResultId is not null)
            searchState.ClearPendingScrollTarget(searchState.PendingScrollResultId);

        if (pendingFocusResultId is null && searchState.PendingFocusResultId is not null)
            searchState.ClearPendingFocusTarget(searchState.PendingFocusResultId);

        return new ConfigSearchRenderState(
            searchState,
            new HashSet<string>(matchIds, StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal));
    }
}
