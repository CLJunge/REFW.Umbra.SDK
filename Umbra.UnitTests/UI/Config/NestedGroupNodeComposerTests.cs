using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Umbra.Config.Attributes;
using Umbra.UI.Config;
using Umbra.UI.Config.Drawers;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "NestedGroupNodeComposer"/> class.
/// </summary>
[TestClass]
public class NestedGroupNodeComposerTests
{
    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a valid scope path and non-empty nodes list.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_ValidScopePathAndNodes_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "test.scope.path";
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a valid scope path and an empty nodes list.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_ValidScopePathAndEmptyList_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "test.scope.path";
        var nodes = new List<IDrawNode>();
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with an empty string scope path.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_EmptyStringScopePath_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = string.Empty;
        var nodes = new List<IDrawNode>();
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a whitespace-only scope path.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_WhitespaceScopePath_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "   ";
        var nodes = new List<IDrawNode>();
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a very long scope path.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_VeryLongScopePath_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = new string ('a', 10000);
        var nodes = new List<IDrawNode>();
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a scope path containing special characters.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_ScopePathWithSpecialCharacters_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "test.scope!@#$%^&*()_+-={}[]|:;<>?,./~`";
        var nodes = new List<IDrawNode>();
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a scope path containing Unicode characters.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_ScopePathWithUnicodeCharacters_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "test.scope.パス.路径.путь";
        var nodes = new List<IDrawNode>();
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with multiple nodes.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_MultipleNodes_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "test.scope.path";
        var mockNode1 = new Mock<IDrawNode>();
        var mockNode2 = new Mock<IDrawNode>();
        var mockNode3 = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode1.Object,
            mockNode2.Object,
            mockNode3.Object
        };
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree handles various combinations of scope paths and node list states.
    /// Validates that the method consistently returns a valid IdScopeNode instance.
    /// </summary>
    [TestMethod]
    [DataRow("simple", 0, DisplayName = "Simple scope path with empty list")]
    [DataRow("simple", 1, DisplayName = "Simple scope path with single node")]
    [DataRow("simple", 5, DisplayName = "Simple scope path with multiple nodes")]
    [DataRow("", 0, DisplayName = "Empty scope path with empty list")]
    [DataRow("", 3, DisplayName = "Empty scope path with multiple nodes")]
    [DataRow("very.long.nested.scope.path.with.many.segments", 10, DisplayName = "Complex nested path with many nodes")]
    public void CreateIdScopedSubtree_VariousInputs_ReturnsIdScopeNode(string scopePath, int nodeCount)
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        for (int i = 0; i < nodeCount; i++)
        {
            var mockNode = new Mock<IDrawNode>();
            nodes.Add(mockNode.Object);
        }

        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a scope path containing control characters.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_ScopePathWithControlCharacters_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "test\tscope\npath\r\n";
        var nodes = new List<IDrawNode>();
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateIdScopedSubtree creates a valid IdScopeNode when provided with a typical dot-separated scope path.
    /// This represents the expected common use case where the scope path follows a hierarchical naming convention.
    /// </summary>
    [TestMethod]
    public void CreateIdScopedSubtree_TypicalDotSeparatedPath_ReturnsIdScopeNode()
    {
        // Arrange
        var scopePath = "config.ui.panel.settings";
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        // Act
        var result = NestedGroupNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IdScopeNode));
    }

    /// <summary>
    /// Tests that CreateWrappedNode returns a ParameterNode with the correct order value.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithValidInputs_ReturnsParameterNodeWithCorrectOrder()
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object ();
        const int expectedOrder = 42;
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: expectedOrder, spacingBefore: 0, spacingAfter: 0);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedOrder, result.Order);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with null propHideIf creates an always-visible node.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithNullPropHideIf_CreatesAlwaysVisibleNode()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
        // Assert
        Assert.IsNotNull(result);
        result.Draw();
        mockNode.Verify(n => n.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with non-null propHideIf uses visibility predicate.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithNonNullPropHideIf_UsesVisibilityPredicate()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new TestOwner
        {
            HideFlag = false
        };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.HideFlag));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns(null);
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        // Assert
        Assert.IsNotNull(result);
        result.Draw();
        mockNode.Verify(n => n.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with empty nodes list executes without error.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithEmptyNodesList_ExecutesWithoutError()
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
        // Assert
        Assert.IsNotNull(result);
        result.Draw(); // Should not throw
    }

    /// <summary>
    /// Tests that CreateWrappedNode calls Draw on all nodes in the list.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithMultipleNodes_CallsDrawOnAllNodes()
    {
        // Arrange
        var mockNode1 = new Mock<IDrawNode>();
        var mockNode2 = new Mock<IDrawNode>();
        var mockNode3 = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode1.Object,
            mockNode2.Object,
            mockNode3.Object
        };
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert
        mockNode1.Verify(n => n.Draw(), Times.Once);
        mockNode2.Verify(n => n.Draw(), Times.Once);
        mockNode3.Verify(n => n.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that CreateWrappedNode draw action can be invoked multiple times.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_DrawActionInvokedMultipleTimes_CallsDrawOnNodesEachTime()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        result.Draw();
        result.Draw();
        // Assert
        mockNode.Verify(n => n.Draw(), Times.Exactly(3));
    }

    /// <summary>
    /// Tests that CreateWrappedNode with various order boundary values returns correct order.
    /// </summary>
    /// <param name = "order">The order value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1000)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    public void CreateWrappedNode_WithVariousOrderValues_ReturnsCorrectOrder(int order)
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: order, spacingBefore: 0, spacingAfter: 0);
        // Assert
        Assert.AreEqual(order, result.Order);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with various spacingBefore values executes without error.
    /// </summary>
    /// <param name = "spacingBefore">The spacingBefore value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1000)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    public void CreateWrappedNode_WithVariousSpacingBeforeValues_ExecutesWithoutError(int spacingBefore)
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: spacingBefore, spacingAfter: 0);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with various spacingAfter values executes without error.
    /// </summary>
    /// <param name = "spacingAfter">The spacingAfter value to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1000)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(1000)]
    [DataRow(int.MaxValue)]
    public void CreateWrappedNode_WithVariousSpacingAfterValues_ExecutesWithoutError(int spacingAfter)
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: spacingAfter);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with single node calls Draw once.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithSingleNode_CallsDrawOnce()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert
        mockNode.Verify(n => n.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that CreateWrappedNode respects HideIf condition when visibility is false.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WhenHideIfConditionHidesNode_DoesNotCallDraw()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new TestOwner
        {
            HideFlag = true
        };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.HideFlag));
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns(null);
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert
        mockNode.Verify(n => n.Draw(), Times.Never);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with extreme order and spacing values creates valid node.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithExtremeValues_CreatesValidNode()
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: int.MaxValue, spacingBefore: int.MaxValue, spacingAfter: int.MaxValue);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(int.MaxValue, result.Order);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with all parameters at minimum values creates valid node.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithMinimumValues_CreatesValidNode()
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: int.MinValue, spacingBefore: int.MinValue, spacingAfter: int.MinValue);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(int.MinValue, result.Order);
    }

    /// <summary>
    /// Tests that CreateWrappedNode preserves node execution order within the draw action.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithMultipleNodes_PreservesExecutionOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var mockNode1 = new Mock<IDrawNode>();
        mockNode1.Setup(n => n.Draw()).Callback(() => executionOrder.Add(1));
        var mockNode2 = new Mock<IDrawNode>();
        mockNode2.Setup(n => n.Draw()).Callback(() => executionOrder.Add(2));
        var mockNode3 = new Mock<IDrawNode>();
        mockNode3.Setup(n => n.Draw()).Callback(() => executionOrder.Add(3));
        var nodes = new List<IDrawNode>
        {
            mockNode1.Object,
            mockNode2.Object,
            mockNode3.Object
        };
        var owner = new object ();
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert
        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, executionOrder);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with HideIf using value comparison respects the condition.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithHideIfValueComparison_RespectsCondition()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new TestOwner
        {
            Status = 5
        };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.Status));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(5);
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert - When value equals, node should be hidden
        mockNode.Verify(n => n.Draw(), Times.Never);
    }

    /// <summary>
    /// Tests that CreateWrappedNode with HideIf value not matching shows the node.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithHideIfValueNotMatching_ShowsNode()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new TestOwner
        {
            Status = 10
        };
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns(nameof(TestOwner.Status));
        mockHideIf.Setup(h => h.HasValue).Returns(true);
        mockHideIf.Setup(h => h.BoxedValue).Returns(5);
        // Act
        var result = NestedGroupNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert - When value differs, node should be visible
        mockNode.Verify(n => n.Draw(), Times.Once);
    }

    /// <summary>
    /// Helper class for testing visibility predicates with properties.
    /// </summary>
    private class TestOwner
    {
        public bool HideFlag { get; set; }
        public int Status { get; set; }
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode returns null when BuildDrawAction returns null.
    /// This occurs when the drawer type does not support the provided group type.
    /// </summary>
    [TestMethod]
    public void CreateNestedDrawerNode_BuildDrawActionReturnsNull_ReturnsNull()
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        // Use a drawer type that doesn't support the group type to force BuildDrawAction to return null
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(IncompatibleDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNull(result, "Expected null when BuildDrawAction returns null");
        Assert.IsNull(disposable, "Disposable should be null when result is null");
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode creates an IdScopeNode without local category scope
    /// when localCategory is null and BuildDrawAction succeeds.
    /// </summary>
    [TestMethod]
    public void CreateNestedDrawerNode_WithoutLocalCategory_ReturnsIdScopeNodeWithParameterNode()
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: 5, spacingBefore: 2, spacingAfter: 3, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope.path", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result when BuildDrawAction succeeds");
        Assert.IsInstanceOfType<IdScopeNode>(result, "Expected result to be IdScopeNode");
        Assert.IsNull(disposable, "Disposable should be null for non-disposable drawer");
        registerCategoryNodeMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Never, "Should not register category when localCategory is null");
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode creates an IdScopeNode with local category scope
    /// when localCategory is provided and BuildDrawAction succeeds.
    /// </summary>
    [TestMethod]
    public void CreateNestedDrawerNode_WithLocalCategory_ReturnsIdScopeNodeWithCategoryScope()
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: 10, spacingBefore: 1, spacingAfter: 1, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope.with.category", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: "TestCategory", collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result when BuildDrawAction succeeds");
        Assert.IsInstanceOfType<IdScopeNode>(result, "Expected result to be IdScopeNode");
        Assert.IsNull(disposable, "Disposable should be null for non-disposable drawer");
        registerCategoryNodeMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Once, "Should register category when localCategory is provided");
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode correctly handles various nullable attribute parameters
    /// including inheritedLabelMargin, collapseAttr, and indentAttr being null.
    /// </summary>
    [TestMethod]
    [DataRow(null, null, null, DisplayName = "All nullable attributes null")]
    [DataRow(10, null, null, DisplayName = "Only labelMargin provided")]
    [DataRow(null, true, null, DisplayName = "Only collapseAttr provided")]
    [DataRow(null, null, 5f, DisplayName = "Only indentAttr provided")]
    [DataRow(15, true, 10f, DisplayName = "All attributes provided")]
    public void CreateNestedDrawerNode_VariousNullableAttributes_HandlesCorrectly(int? labelMarginPixels, bool? collapseAsTree, float? indentAmount)
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        var labelMargin = labelMarginPixels.HasValue ? new UmbraLabelMarginAttribute(labelMarginPixels.Value) : null;
        var collapseAttr = collapseAsTree.HasValue ? new UmbraCollapseAsTreeAttribute() : null;
        var indentAttr = indentAmount.HasValue ? new UmbraIndentAttribute(indentAmount.Value) : null;
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: labelMargin, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: "Category", collapseAttr: collapseAttr, indentAttr: indentAttr, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result regardless of nullable attribute values");
        Assert.IsNull(disposable, "Disposable should be null for non-disposable drawer");
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode correctly handles various spacing and order values
    /// from PropertyDrawMetadata, including boundary values.
    /// </summary>
    [TestMethod]
    [DataRow(int.MinValue, 0, 0, DisplayName = "Order = int.MinValue")]
    [DataRow(0, 0, 0, DisplayName = "Order = 0, no spacing")]
    [DataRow(int.MaxValue, 0, 0, DisplayName = "Order = int.MaxValue")]
    [DataRow(0, 10, 5, DisplayName = "Positive spacing values")]
    [DataRow(0, 0, 100, DisplayName = "Large spacingAfter")]
    [DataRow(0, 50, 0, DisplayName = "Large spacingBefore")]
    public void CreateNestedDrawerNode_VariousOrderAndSpacing_HandlesCorrectly(int order, int spacingBefore, int spacingAfter)
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: order, spacingBefore: spacingBefore, spacingAfter: spacingAfter, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result for all order and spacing combinations");
        Assert.IsNull(disposable, "Disposable should be null for non-disposable drawer");
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode handles empty and whitespace-only string parameters.
    /// Empty groupScopePath should still produce a valid result, though it may affect ImGui ID scoping.
    /// Empty localCategory should be treated as a non-null category (different from null).
    /// </summary>
    [TestMethod]
    [DataRow("", null, DisplayName = "Empty groupScopePath, null localCategory")]
    [DataRow("   ", null, DisplayName = "Whitespace groupScopePath, null localCategory")]
    [DataRow("valid.path", "", DisplayName = "Valid groupScopePath, empty localCategory")]
    [DataRow("valid.path", "   ", DisplayName = "Valid groupScopePath, whitespace localCategory")]
    public void CreateNestedDrawerNode_EmptyOrWhitespaceStrings_HandlesCorrectly(string groupScopePath, string? localCategory)
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: groupScopePath, propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: localCategory, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result even with empty/whitespace strings");
        Assert.IsNull(disposable, "Disposable should be null for non-disposable drawer");
        // Verify category registration behavior based on localCategory
        if (localCategory is null)
        {
            registerCategoryNodeMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Never, "Should not register category when localCategory is null");
        }
        else
        {
            registerCategoryNodeMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Once, "Should register category when localCategory is non-null (even if empty)");
        }
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode correctly sets the disposable out parameter
    /// when the drawer implements IDisposable.
    /// </summary>
    [TestMethod]
    public void CreateNestedDrawerNode_DisposableDrawer_SetsDisposableOutParameter()
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(DisposableTestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result when BuildDrawAction succeeds");
        Assert.IsNotNull(disposable, "Disposable should be set when drawer implements IDisposable");
        Assert.IsInstanceOfType<DisposableTestNestedGroupDrawer>(disposable, "Disposable should be the drawer instance");
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode catches exceptions from BuildDrawAction,
    /// logs them, and returns null with disposable set to null.
    /// This test verifies exception handling by using a drawer type that will cause
    /// an exception during instantiation.
    /// </summary>
    [TestMethod]
    public void CreateNestedDrawerNode_ExceptionDuringBuildDrawAction_ReturnsNullAndLogsException()
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        // Use a drawer type without parameterless constructor to trigger exception during Activator.CreateInstance
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(InvalidDrawerWithoutDefaultConstructor));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNull(result, "Expected null when exception occurs during BuildDrawAction");
        Assert.IsNull(disposable, "Disposable should be null when exception occurs");
    }

    /// <summary>
    /// Tests that CreateNestedDrawerNode correctly handles HideIf attribute in PropertyDrawMetadata.
    /// The visibility predicate is built from HideIf and owner, affecting the ParameterNode behavior.
    /// </summary>
    [TestMethod]
    public void CreateNestedDrawerNode_WithHideIfAttribute_CreatesNodeWithVisibilityPredicate()
    {
        // Arrange
        var registerCategoryNodeMock = new Mock<Action<CategoryNode>>();
        var nestedDrawerAttrMock = new Mock<INestedGroupDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var hideIfMock = new Mock<IHideIfAttribute>();
        hideIfMock.Setup(x => x.MemberName).Returns(nameof(TestConfig.IsHidden));
        hideIfMock.Setup(x => x.HasValue).Returns(false);
        hideIfMock.Setup(x => x.BoxedValue).Returns(null);
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedGroupDrawerAttr: null, hideIf: hideIfMock.Object, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig
        {
            IsHidden = false
        };
        var nested = new TestNestedGroup();
        // Act
        var result = NestedGroupNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result when HideIf is configured");
        Assert.IsNull(disposable, "Disposable should be null for non-disposable drawer");
    }

#region Helper Types
    /// <summary>
    /// Test configuration class for reflection-based property metadata creation.
    /// </summary>
    private sealed class TestConfig
    {
        public TestNestedGroup TestProperty { get; set; } = new();
        public bool IsHidden { get; set; }
    }

    /// <summary>
    /// Test nested group type for drawer compatibility testing.
    /// </summary>
    private sealed class TestNestedGroup
    {
    }

    /// <summary>
    /// Test drawer implementation compatible with TestNestedGroup.
    /// </summary>
    private sealed class TestNestedGroupDrawer : INestedGroupDrawer<TestNestedGroup>
    {
        public void Draw(TestNestedGroup groupInstance)
        {
        // No-op for testing
        }
    }

    /// <summary>
    /// Test drawer implementation that implements IDisposable.
    /// </summary>
    private sealed class DisposableTestNestedGroupDrawer : INestedGroupDrawer<TestNestedGroup>, IDisposable
    {
        private bool _disposed;
        public void Draw(TestNestedGroup groupInstance)
        {
        // No-op for testing
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Incompatible drawer for different group type to test BuildDrawAction returning null.
    /// </summary>
    private sealed class IncompatibleDrawer : INestedGroupDrawer<string>
    {
        public void Draw(string groupInstance)
        {
        // No-op for testing
        }
    }

    /// <summary>
    /// Invalid drawer without default constructor to test exception handling.
    /// </summary>
    private sealed class InvalidDrawerWithoutDefaultConstructor : INestedGroupDrawer<TestNestedGroup>
    {
        public InvalidDrawerWithoutDefaultConstructor(string required)
        {
        // Intentionally requires a parameter to prevent default construction
        }

        public void Draw(TestNestedGroup groupInstance)
        {
        // No-op for testing
        }
    }
#endregion
}