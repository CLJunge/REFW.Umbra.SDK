using Moq;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "NestedNodeComposer"/> class.
/// </summary>
[TestClass]
public class NestedNodeComposerTests
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
        var result = NestedNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<IdScopeNode>(result);
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
        var result = NestedNodeComposer.CreateIdScopedSubtree(scopePath, nodes);
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<IdScopeNode>(result);
    }

    /// <summary>
    /// Tests that CreateWrappedNode returns a ParameterNode with the correct order value.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithValidInputs_ReturnsParameterNodeWithCorrectOrder()
    {
        // Arrange
        var nodes = new List<IDrawNode>();
        var owner = new object();
        const int expectedOrder = 42;
        // Act
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: expectedOrder, spacingBefore: 0, spacingAfter: 0);
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
        var owner = new object();
        // Act
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
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
        mockHideIf.Setup(h => h.BoxedValue).Returns(null!);
        // Act
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        // Assert
        Assert.IsNotNull(result);
        result.Draw();
        mockNode.Verify(n => n.Draw(), Times.Once);
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
        var owner = new object();
        // Act
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert
        mockNode1.Verify(n => n.Draw(), Times.Once);
        mockNode2.Verify(n => n.Draw(), Times.Once);
        mockNode3.Verify(n => n.Draw(), Times.Once);
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
        mockHideIf.Setup(h => h.BoxedValue).Returns(null!);
        // Act
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert
        mockNode.Verify(n => n.Draw(), Times.Never);
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
        var owner = new object();
        // Act
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0);
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
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
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
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();
        // Assert - When value differs, node should be visible
        mockNode.Verify(n => n.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that CreateWrappedNode ignores an invalid HideIf member name and keeps the node visible.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithInvalidHideIfMember_ShowsNode()
    {
        // Arrange
        var mockNode = new Mock<IDrawNode>();
        var nodes = new List<IDrawNode>
        {
            mockNode.Object
        };
        var owner = new TestOwner();
        var mockHideIf = new Mock<IHideIfAttribute>();
        mockHideIf.Setup(h => h.MemberName).Returns("MissingMember");
        mockHideIf.Setup(h => h.HasValue).Returns(false);
        mockHideIf.Setup(h => h.BoxedValue).Returns(null!);

        // Act
        var result = NestedNodeComposer.CreateWrappedNode(nodes, owner, propHideIf: mockHideIf.Object, order: 0, spacingBefore: 0, spacingAfter: 0);
        result.Draw();

        // Assert
        mockNode.Verify(n => n.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that CreateWrappedNode safely handles an empty child-node list.
    /// </summary>
    [TestMethod]
    public void CreateWrappedNode_WithEmptyNodeList_DrawsWithoutThrowing()
    {
        // Arrange
        var owner = new object();

        // Act
        var result = NestedNodeComposer.CreateWrappedNode([], owner, propHideIf: null, order: 3, spacingBefore: 0, spacingAfter: 0);
        result.Draw();

        // Assert
        Assert.AreEqual(3, result.Order);
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
        var nestedDrawerAttrMock = new Mock<INestedDrawerAttribute>();
        // Use a drawer type that doesn't support the group type to force BuildDrawAction to return null
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(IncompatibleDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), getValue: owner => propInfo.GetValue(owner), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
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
        var nestedDrawerAttrMock = new Mock<INestedDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), getValue: owner => propInfo.GetValue(owner), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedDrawerAttr: null, hideIf: null, order: 5, spacingBefore: 2, spacingAfter: 3, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope.path", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result when BuildDrawAction succeeds");
        Assert.IsInstanceOfType<IdScopeNode>(result, "Expected result to be IdScopeNode");
        Assert.IsNotNull(disposable, "Disposable should be set since INestedGroupDrawer<T> extends IDisposable");
        Assert.IsInstanceOfType<TestNestedGroupDrawer>(disposable, "Disposable should be the drawer instance");
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
        var nestedDrawerAttrMock = new Mock<INestedDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(TestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), getValue: owner => propInfo.GetValue(owner), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedDrawerAttr: null, hideIf: null, order: 10, spacingBefore: 1, spacingAfter: 1, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope.with.category", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: "TestCategory", collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNotNull(result, "Expected non-null result when BuildDrawAction succeeds");
        Assert.IsInstanceOfType<IdScopeNode>(result, "Expected result to be IdScopeNode");
        Assert.IsNotNull(disposable, "Disposable should be set since INestedGroupDrawer<T> extends IDisposable");
        Assert.IsInstanceOfType<TestNestedGroupDrawer>(disposable, "Disposable should be the drawer instance");
        registerCategoryNodeMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Once, "Should register category when localCategory is provided");
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
        var nestedDrawerAttrMock = new Mock<INestedDrawerAttribute>();
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(DisposableTestNestedGroupDrawer));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), getValue: owner => propInfo.GetValue(owner), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
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
        var nestedDrawerAttrMock = new Mock<INestedDrawerAttribute>();
        // Use a drawer type without parameterless constructor to trigger exception during Activator.CreateInstance
        nestedDrawerAttrMock.Setup(x => x.DrawerType).Returns(typeof(InvalidDrawerWithoutDefaultConstructor));
        var propInfo = typeof(TestConfig).GetProperty(nameof(TestConfig.TestProperty))!;
        var propMeta = new TypeDrawMetadata.PropertyDrawMetadata(property: propInfo, propertyType: typeof(TestNestedGroup), getValue: owner => propInfo.GetValue(owner), isParameter: false, category: null, indentAttr: null, collapseAttr: null, labelMarginAttr: null, nestedDrawerAttr: null, hideIf: null, order: 0, spacingBefore: 0, spacingAfter: 0, settingsPrefix: null, settingsParameterKeyOverride: null);
        var owner = new TestConfig();
        var nested = new TestNestedGroup();
        // Act
        var result = NestedNodeComposer.CreateNestedDrawerNode(registerCategoryNode: registerCategoryNodeMock.Object, inheritedLabelMargin: null, groupScopePath: "test.scope", propMeta: propMeta, propType: typeof(TestNestedGroup), nestedDrawerAttr: nestedDrawerAttrMock.Object, nested: nested, owner: owner, localCategory: null, collapseAttr: null, indentAttr: null, out var disposable);
        // Assert
        Assert.IsNull(result, "Expected null when exception occurs during BuildDrawAction");
        Assert.IsNull(disposable, "Disposable should be null when exception occurs");
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
    private sealed class TestNestedGroupDrawer : INestedDrawer<TestNestedGroup>
    {
        public void Draw(TestNestedGroup groupInstance)
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Test drawer implementation that implements IDisposable.
    /// </summary>
    private sealed class DisposableTestNestedGroupDrawer : INestedDrawer<TestNestedGroup>, IDisposable
    {
        public void Draw(TestNestedGroup groupInstance)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Incompatible drawer for different group type to test BuildDrawAction returning null.
    /// </summary>
    private sealed class IncompatibleDrawer : INestedDrawer<string>
    {
        public void Draw(string groupInstance)
        {
            // No-op for testing
        }
    }

    /// <summary>
    /// Invalid drawer without default constructor to test exception handling.
    /// </summary>
    private sealed class InvalidDrawerWithoutDefaultConstructor : INestedDrawer<TestNestedGroup>
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
