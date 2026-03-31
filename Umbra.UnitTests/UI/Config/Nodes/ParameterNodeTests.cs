namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterNode"/>.
/// </summary>
[TestClass]
public sealed class ParameterNodeTests
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

        var exception = AssertThrows<ArgumentNullException>(() => _ = new ParameterNode(null!, static () => { }, int.MaxValue, 0, 0, renderer));

        Assert.AreEqual("isVisible", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null draw action.
    /// </summary>
    [TestMethod]
    public void Constructor_NullDrawAction_ThrowsArgumentNullException()
    {
        var renderer = new TestParameterNodeRenderer();

        var exception = AssertThrows<ArgumentNullException>(() => _ = new ParameterNode(static () => true, null!, int.MaxValue, 0, 0, renderer));

        Assert.AreEqual("draw", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = AssertThrows<ArgumentNullException>(() => _ = new ParameterNode(static () => true, static () => { }, int.MaxValue, 0, 0, null!));

        Assert.AreEqual("renderer", exception.ParamName);
    }
}
