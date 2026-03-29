using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterNode"/> class.
/// </summary>
[TestClass]
public sealed class ParameterNodeTests
{
    /// <summary>
    /// Verifies that when <c>isVisible</c> returns <see langword="false"/>, the <c>Draw</c> method
    /// returns immediately without invoking the draw action or spacing calls.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleReturnsFalse_DoesNotCallDrawAction()
    {
        // Arrange
        bool drawCalled = false;
        Func<bool> isVisible = () => false;
        Action draw = () => drawCalled = true;
        var node = new ParameterNode(isVisible, draw);

        // Act
        node.Draw();

        // Assert
        Assert.IsFalse(drawCalled, "Draw action should not be called when isVisible returns false.");
    }

    /// <summary>
    /// Verifies that when <c>isVisible</c> returns <see langword="true"/> and spacing values are zero,
    /// the <c>Draw</c> method invokes the draw action exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleReturnsTrueWithZeroSpacing_CallsDrawActionOnce()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: 0, spacingAfter: 0);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called exactly once when visible.");
    }

    /// <summary>
    /// Verifies that when <c>isVisible</c> returns <see langword="true"/> with positive spacing values,
    /// the draw action is invoked.
    /// Note: ImGui.Spacing() calls cannot be verified as the method is static and non-mockable.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleReturnsTrueWithPositiveSpacing_CallsDrawAction()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: 2, spacingAfter: 3);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called exactly once when visible with spacing.");
    }

    /// <summary>
    /// Verifies that when <c>spacingBefore</c> is negative, the draw action is still invoked
    /// (negative loop bounds cause zero iterations).
    /// </summary>
    [TestMethod]
    public void Draw_SpacingBeforeIsNegative_StillCallsDrawAction()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: -5, spacingAfter: 0);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called even with negative spacingBefore.");
    }

    /// <summary>
    /// Verifies that when <c>spacingAfter</c> is negative, the draw action is still invoked
    /// (negative loop bounds cause zero iterations).
    /// </summary>
    [TestMethod]
    public void Draw_SpacingAfterIsNegative_StillCallsDrawAction()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: 0, spacingAfter: -10);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called even with negative spacingAfter.");
    }

    /// <summary>
    /// Verifies that when both <c>spacingBefore</c> and <c>spacingAfter</c> are negative,
    /// the draw action is still invoked.
    /// </summary>
    [TestMethod]
    public void Draw_BothSpacingValuesNegative_StillCallsDrawAction()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: -1, spacingAfter: -1);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called even with both spacing values negative.");
    }

    /// <summary>
    /// Verifies that the <c>Draw</c> method can be called multiple times in sequence.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_InvokesDrawActionEachTime()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw);

        // Act
        node.Draw();
        node.Draw();
        node.Draw();

        // Assert
        Assert.AreEqual(3, drawCallCount, "Draw action should be called once per Draw() invocation.");
    }

    /// <summary>
    /// Verifies that the draw action is not called when <c>isVisible</c> returns <see langword="false"/>,
    /// even with positive spacing values.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleReturnsFalseWithPositiveSpacing_DoesNotCallDrawAction()
    {
        // Arrange
        bool drawCalled = false;
        Func<bool> isVisible = () => false;
        Action draw = () => drawCalled = true;
        var node = new ParameterNode(isVisible, draw, spacingBefore: 5, spacingAfter: 5);

        // Act
        node.Draw();

        // Assert
        Assert.IsFalse(drawCalled, "Draw action should not be called when isVisible returns false, regardless of spacing values.");
    }

    /// <summary>
    /// Verifies behavior when <c>isVisible</c> alternates between <see langword="true"/> and <see langword="false"/>
    /// across multiple draw calls.
    /// </summary>
    [TestMethod]
    public void Draw_IsVisibleAlternates_CallsDrawActionOnlyWhenVisible()
    {
        // Arrange
        int drawCallCount = 0;
        bool visible = true;
        Func<bool> isVisible = () => visible;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw);

        // Act & Assert
        node.Draw();
        Assert.AreEqual(1, drawCallCount, "First call: draw should be invoked when visible.");

        visible = false;
        node.Draw();
        Assert.AreEqual(1, drawCallCount, "Second call: draw should not be invoked when not visible.");

        visible = true;
        node.Draw();
        Assert.AreEqual(2, drawCallCount, "Third call: draw should be invoked again when visible.");
    }

    /// <summary>
    /// Verifies that <c>spacingBefore</c> of zero results in the draw action being called
    /// without any spacing calls before it.
    /// </summary>
    [TestMethod]
    public void Draw_SpacingBeforeIsZero_CallsDrawActionWithoutPriorSpacing()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: 0, spacingAfter: 1);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called with spacingBefore of zero.");
    }

    /// <summary>
    /// Verifies that <c>spacingAfter</c> of zero results in the draw action being called
    /// without any spacing calls after it.
    /// </summary>
    [TestMethod]
    public void Draw_SpacingAfterIsZero_CallsDrawActionWithoutSubsequentSpacing()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: 1, spacingAfter: 0);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called with spacingAfter of zero.");
    }

    /// <summary>
    /// Verifies that extreme positive spacing values do not prevent the draw action from being invoked.
    /// Note: Using a moderate value to avoid excessively long test execution.
    /// </summary>
    [TestMethod]
    public void Draw_LargeSpacingValues_StillCallsDrawAction()
    {
        // Arrange
        int drawCallCount = 0;
        Func<bool> isVisible = () => true;
        Action draw = () => drawCallCount++;
        var node = new ParameterNode(isVisible, draw, spacingBefore: 100, spacingAfter: 100);

        // Act
        node.Draw();

        // Assert
        Assert.AreEqual(1, drawCallCount, "Draw action should be called even with large spacing values.");
    }

    /// <summary>
    /// Verifies that the <c>Order</c> property returns the value provided during construction.
    /// </summary>
    [TestMethod]
    public void Order_ReturnsConstructorValue()
    {
        // Arrange
        const int expectedOrder = 42;
        Func<bool> isVisible = () => true;
        Action draw = () => { };
        var node = new ParameterNode(isVisible, draw, order: expectedOrder);

        // Act
        int actualOrder = node.Order;

        // Assert
        Assert.AreEqual(expectedOrder, actualOrder, "Order property should return the value provided during construction.");
    }

    /// <summary>
    /// Verifies that the <c>Order</c> property defaults to <see cref="int.MaxValue"/> when not specified.
    /// </summary>
    [TestMethod]
    public void Order_DefaultsToIntMaxValue()
    {
        // Arrange
        Func<bool> isVisible = () => true;
        Action draw = () => { };
        var node = new ParameterNode(isVisible, draw);

        // Act
        int actualOrder = node.Order;

        // Assert
        Assert.AreEqual(int.MaxValue, actualOrder, "Order property should default to int.MaxValue.");
    }

    /// <summary>
    /// Verifies that the <c>Order</c> property can be set to <see cref="int.MinValue"/>.
    /// </summary>
    [TestMethod]
    public void Order_CanBeSetToIntMinValue()
    {
        // Arrange
        const int expectedOrder = int.MinValue;
        Func<bool> isVisible = () => true;
        Action draw = () => { };
        var node = new ParameterNode(isVisible, draw, order: expectedOrder);

        // Act
        int actualOrder = node.Order;

        // Assert
        Assert.AreEqual(expectedOrder, actualOrder, "Order property should accept int.MinValue.");
    }
}