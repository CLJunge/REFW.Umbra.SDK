using Moq;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ConfigDrawScope.CreateContainerNode"/> method.
/// </summary>
[TestClass]
public sealed partial class ConfigDrawScopeTests
{
    /// <summary>
    /// Tests that CreateContainerNode creates a CategoryNode with the correct category label,
    /// calls the registerCategory delegate, and returns a valid node when given a normal category string.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_ValidCategoryString_ReturnsNodeWithCorrectCategory()
    {
        // Arrange
        const string category = "TestCategory";
        CategoryNode? registeredNode = null;
        var scope = CreateScope(registerCategory: node => registeredNode = node);

        // Act
        var result = scope.CreateContainerNode(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(registeredNode);
        Assert.AreSame(result, registeredNode);
    }

    /// <summary>
    /// Tests that CreateContainerNode creates a node with empty Children when the scope has no nodes.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_EmptyNodesList_ReturnsNodeWithNoChildren()
    {
        // Arrange
        const string category = "TestCategory";
        var scope = CreateScope();

        // Act
        var result = scope.CreateContainerNode(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result.Children);
    }

    /// <summary>
    /// Tests that CreateContainerNode adds a single node from the scope's Nodes list
    /// to the returned CategoryNode's Children.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_SingleNodeInList_AddsChildToResult()
    {
        // Arrange
        const string category = "TestCategory";
        var mockNode = new Mock<IDrawNode>();
        var scope = CreateScope();
        scope.Nodes.Add(mockNode.Object);

        // Act
        var result = scope.CreateContainerNode(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result.Children);
        Assert.AreSame(mockNode.Object, result.Children[0]);
    }

    /// <summary>
    /// Tests that CreateContainerNode adds all nodes from the scope's Nodes list
    /// to the returned CategoryNode's Children in the correct order.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_MultipleNodesInList_AddsAllChildrenInOrder()
    {
        // Arrange
        const string category = "TestCategory";
        var mockNode1 = new Mock<IDrawNode>();
        var mockNode2 = new Mock<IDrawNode>();
        var mockNode3 = new Mock<IDrawNode>();
        var scope = CreateScope();
        scope.Nodes.Add(mockNode1.Object);
        scope.Nodes.Add(mockNode2.Object);
        scope.Nodes.Add(mockNode3.Object);

        // Act
        var result = scope.CreateContainerNode(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(3, result.Children);
        Assert.AreSame(mockNode1.Object, result.Children[0]);
        Assert.AreSame(mockNode2.Object, result.Children[1]);
        Assert.AreSame(mockNode3.Object, result.Children[2]);
    }

    /// <summary>
    /// Tests that CreateContainerNode calls the registerCategory delegate exactly once
    /// with the created node.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_ValidCategory_CallsRegisterCategoryOnce()
    {
        // Arrange
        const string category = "TestCategory";
        var callCount = 0;
        CategoryNode? registeredNode = null;
        var scope = CreateScope(registerCategory: node =>
        {
            callCount++;
            registeredNode = node;
        });

        // Act
        var result = scope.CreateContainerNode(category);

        // Assert
        Assert.AreEqual(1, callCount);
        Assert.IsNotNull(registeredNode);
        Assert.AreSame(result, registeredNode);
    }

    /// <summary>
    /// Tests that CreateContainerNode returns a new CategoryNode instance each time it is called.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_CalledMultipleTimes_ReturnsDistinctInstances()
    {
        // Arrange
        const string category1 = "Category1";
        const string category2 = "Category2";
        var scope = CreateScope();

        // Act
        var result1 = scope.CreateContainerNode(category1);
        var result2 = scope.CreateContainerNode(category2);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreNotSame(result1, result2);
    }

    /// <summary>
    /// Tests that CreateContainerNode correctly handles a large number of child nodes.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_LargeNumberOfNodes_AddsAllChildren()
    {
        // Arrange
        const string category = "TestCategory";
        const int nodeCount = 1000;
        var scope = CreateScope();
        var mockNodes = new List<Mock<IDrawNode>>(nodeCount);

        for (var i = 0; i < nodeCount; i++)
        {
            var mockNode = new Mock<IDrawNode>();
            mockNodes.Add(mockNode);
            scope.Nodes.Add(mockNode.Object);
        }

        // Act
        var result = scope.CreateContainerNode(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(nodeCount, result.Children);
        for (var i = 0; i < nodeCount; i++)
        {
            Assert.AreSame(mockNodes[i].Object, result.Children[i]);
        }
    }

    /// <summary>
    /// Tests that CreateContainerNode does not modify the scope's Nodes list.
    /// </summary>
    [TestMethod]
    public void CreateContainerNode_WithNodes_DoesNotModifyScopeNodesList()
    {
        // Arrange
        const string category = "TestCategory";
        var mockNode1 = new Mock<IDrawNode>();
        var mockNode2 = new Mock<IDrawNode>();
        var scope = CreateScope();
        scope.Nodes.Add(mockNode1.Object);
        scope.Nodes.Add(mockNode2.Object);
        var originalCount = scope.Nodes.Count;

        // Act
        var result = scope.CreateContainerNode(category);

        // Assert
        Assert.HasCount(originalCount, scope.Nodes);
        Assert.AreSame(mockNode1.Object, scope.Nodes[0]);
        Assert.AreSame(mockNode2.Object, scope.Nodes[1]);
    }

    /// <summary>
    /// Tests that AddNode with a null category appends directly to the root node list.
    /// </summary>
    [TestMethod]
    public void AddNode_NullCategory_AppendsToRootNodes()
    {
        var scope = CreateScope();
        var mockNode = new Mock<IDrawNode>();

        scope.AddNode(null, mockNode.Object);

        Assert.HasCount(1, scope.Nodes);
        Assert.AreSame(mockNode.Object, scope.Nodes[0]);
    }

    /// <summary>
    /// Tests that AddNode reuses the same category container for repeated category names.
    /// </summary>
    [TestMethod]
    public void AddNode_SameCategory_ReusesSingleCategoryNode()
    {
        var scope = CreateScope();
        var firstNode = new Mock<IDrawNode>();
        var secondNode = new Mock<IDrawNode>();

        scope.AddNode("Category", firstNode.Object);
        scope.AddNode("Category", secondNode.Object);

        Assert.HasCount(1, scope.Nodes);
        var categoryNode = (CategoryNode)scope.Nodes[0];
        Assert.HasCount(2, categoryNode.Children);
        Assert.AreSame(firstNode.Object, categoryNode.Children[0]);
        Assert.AreSame(secondNode.Object, categoryNode.Children[1]);
    }

    /// <summary>
    /// Helper method to create a ConfigDrawScope instance for testing.
    /// </summary>
    private static ConfigDrawScope CreateScope(
        string groupPath = "test.group",
        string? defaultCategory = null,
        UmbraCollapseAsTreeAttribute? collapseAttr = null,
        UmbraIndentAttribute? categoryIndentAttr = null,
        UmbraLabelMarginAttribute? labelMarginAttr = null,
        Action<CategoryNode>? registerCategory = null,
        LabelAlignmentGroup? alignmentGroup = null)
    {
        return new ConfigDrawScope(
            groupPath,
            defaultCategory,
            collapseAttr,
            categoryIndentAttr,
            labelMarginAttr,
            registerCategory ?? (_ => { }),
            alignmentGroup);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawScope.GetAlignmentGroup"/> returns the scope's alignment group when category is null.
    /// </summary>
    [TestMethod]
    public void GetAlignmentGroup_NullCategory_ReturnsScopeAlignmentGroup()
    {
        // Arrange
        var scopeAlignmentGroup = new LabelAlignmentGroup();
        var registerCategoryMock = new Mock<Action<CategoryNode>>();
        var scope = new ConfigDrawScope(
            groupPath: "test.group",
            defaultCategory: null,
            collapseAttr: null,
            categoryIndentAttr: null,
            labelMarginAttr: null,
            registerCategory: registerCategoryMock.Object,
            alignmentGroup: scopeAlignmentGroup
        );

        // Act
        var result = scope.GetAlignmentGroup(null);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(scopeAlignmentGroup, result);
        registerCategoryMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawScope.GetAlignmentGroup"/> returns the scope's alignment group when no alignment group is provided to the constructor.
    /// </summary>
    [TestMethod]
    public void GetAlignmentGroup_NullCategory_WithDefaultAlignmentGroup_ReturnsNonNull()
    {
        // Arrange
        var registerCategoryMock = new Mock<Action<CategoryNode>>();
        var scope = new ConfigDrawScope(
            groupPath: "test.group",
            defaultCategory: null,
            collapseAttr: null,
            categoryIndentAttr: null,
            labelMarginAttr: null,
            registerCategory: registerCategoryMock.Object,
            alignmentGroup: null
        );

        // Act
        var result = scope.GetAlignmentGroup(null);

        // Assert
        Assert.IsNotNull(result);
        registerCategoryMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawScope.GetAlignmentGroup"/> creates a category and returns its alignment group when category is a valid string.
    /// </summary>
    [TestMethod]
    [DataRow("TestCategory")]
    [DataRow("Category")]
    [DataRow("My.Category.Name")]
    [DataRow("Category_With_Underscores")]
    [DataRow("Category-With-Dashes")]
    [DataRow("123")]
    [DataRow("Category123")]
    public void GetAlignmentGroup_ValidCategory_ReturnsNewAlignmentGroup(string category)
    {
        // Arrange
        var scopeAlignmentGroup = new LabelAlignmentGroup();
        var registerCategoryMock = new Mock<Action<CategoryNode>>();
        var scope = new ConfigDrawScope(
            groupPath: "test.group",
            defaultCategory: null,
            collapseAttr: null,
            categoryIndentAttr: null,
            labelMarginAttr: null,
            registerCategory: registerCategoryMock.Object,
            alignmentGroup: scopeAlignmentGroup
        );

        // Act
        var result = scope.GetAlignmentGroup(category);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotSame(scopeAlignmentGroup, result);
        registerCategoryMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawScope.GetAlignmentGroup"/> returns the same alignment group instance when called multiple times with the same category.
    /// </summary>
    [TestMethod]
    public void GetAlignmentGroup_SameCategory_ReturnsSameAlignmentGroupInstance()
    {
        // Arrange
        var registerCategoryMock = new Mock<Action<CategoryNode>>();
        var scope = new ConfigDrawScope(
            groupPath: "test.group",
            defaultCategory: null,
            collapseAttr: null,
            categoryIndentAttr: null,
            labelMarginAttr: null,
            registerCategory: registerCategoryMock.Object,
            alignmentGroup: null
        );
        const string category = "TestCategory";

        // Act
        var result1 = scope.GetAlignmentGroup(category);
        var result2 = scope.GetAlignmentGroup(category);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreSame(result1, result2);
        registerCategoryMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawScope.GetAlignmentGroup"/> returns different alignment group instances for different categories.
    /// </summary>
    [TestMethod]
    public void GetAlignmentGroup_DifferentCategories_ReturnsDifferentAlignmentGroups()
    {
        // Arrange
        var registerCategoryMock = new Mock<Action<CategoryNode>>();
        var scope = new ConfigDrawScope(
            groupPath: "test.group",
            defaultCategory: null,
            collapseAttr: null,
            categoryIndentAttr: null,
            labelMarginAttr: null,
            registerCategory: registerCategoryMock.Object,
            alignmentGroup: null
        );

        // Act
        var result1 = scope.GetAlignmentGroup("Category1");
        var result2 = scope.GetAlignmentGroup("Category2");

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreNotSame(result1, result2);
        registerCategoryMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawScope.GetAlignmentGroup"/> invokes the registerCategory callback with the correct category node when a new category is created.
    /// </summary>
    [TestMethod]
    public void GetAlignmentGroup_NewCategory_InvokesRegisterCategoryCallback()
    {
        // Arrange
        CategoryNode? capturedNode = null;
        var registerCategoryMock = new Mock<Action<CategoryNode>>();
        registerCategoryMock.Setup(x => x(It.IsAny<CategoryNode>()))
            .Callback<CategoryNode>(node => capturedNode = node);
        var scope = new ConfigDrawScope(
            groupPath: "test.group",
            defaultCategory: null,
            collapseAttr: null,
            categoryIndentAttr: null,
            labelMarginAttr: null,
            registerCategory: registerCategoryMock.Object,
            alignmentGroup: null
        );
        const string category = "TestCategory";

        // Act
        var result = scope.GetAlignmentGroup(category);

        // Assert
        Assert.IsNotNull(capturedNode);
        registerCategoryMock.Verify(x => x(It.IsAny<CategoryNode>()), Times.Once);
        Assert.AreSame(capturedNode.AlignmentGroup, result);
    }

    /// <summary>
    /// Tests that GetAlignmentGroup returns the same scope alignment group after categories have been created when category is null.
    /// </summary>
    [TestMethod]
    public void GetAlignmentGroup_NullCategoryAfterCategoryCreation_ReturnsScopeAlignmentGroup()
    {
        var scopeAlignmentGroup = new LabelAlignmentGroup();
        var scope = CreateScope(alignmentGroup: scopeAlignmentGroup);

        _ = scope.GetAlignmentGroup("Category");
        var result = scope.GetAlignmentGroup(null);

        Assert.AreSame(scopeAlignmentGroup, result);
    }

}
