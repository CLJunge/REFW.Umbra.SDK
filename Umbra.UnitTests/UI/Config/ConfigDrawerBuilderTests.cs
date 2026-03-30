using Moq;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for <see cref="ConfigDrawerBuilder"/> class.
/// </summary>
[TestClass]
public partial class ConfigDrawerBuilderTests
{
    /// <summary>
    /// Tests that SortAll correctly handles an empty builder with no category nodes and no root nodes.
    /// </summary>
    [TestMethod]
    public void SortAll_EmptyBuilder_CompletesSuccessfully()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();

        // Act
        builder.SortAll();

        // Assert
        Assert.IsEmpty(builder.Nodes);
    }

    /// <summary>
    /// Tests that SortAll correctly handles a builder with empty category nodes list but populated root nodes.
    /// </summary>
    [TestMethod]
    public void SortAll_NoCategoriesWithRootNodes_SortsRootNodesOnly()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode node1 = new(() => true, () => { }, order: 10);
        ParameterNode node2 = new(() => true, () => { }, order: 5);
        ParameterNode node3 = new(() => true, () => { }, order: 15);
        builder.Nodes.Add(node1);
        builder.Nodes.Add(node2);
        builder.Nodes.Add(node3);

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(3, builder.Nodes);
        Assert.AreSame(node2, builder.Nodes[0]);
        Assert.AreSame(node1, builder.Nodes[1]);
        Assert.AreSame(node3, builder.Nodes[2]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles a builder with category nodes but no root nodes.
    /// </summary>
    [TestMethod]
    public void SortAll_CategoriesWithNoRootNodes_SortsCategoryChildrenOnly()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        CategoryNode category = new("Test Category");
        ParameterNode node1 = new(() => true, () => { }, order: 20);
        ParameterNode node2 = new(() => true, () => { }, order: 10);
        category.Children.Add(node1);
        category.Children.Add(node2);

        typeof(ConfigDrawerBuilder)
            .GetField("_allCategoryNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(builder, new List<CategoryNode> { category });

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(2, category.Children);
        Assert.AreSame(node2, category.Children[0]);
        Assert.AreSame(node1, category.Children[1]);
    }

    /// <summary>
    /// Tests that SortAll correctly sorts both category children and root nodes when both are present.
    /// </summary>
    [TestMethod]
    public void SortAll_CategoriesAndRootNodes_SortsBothLists()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();

        CategoryNode category = new("Test Category");
        ParameterNode catNode1 = new(() => true, () => { }, order: 30);
        ParameterNode catNode2 = new(() => true, () => { }, order: 10);
        category.Children.Add(catNode1);
        category.Children.Add(catNode2);

        ParameterNode rootNode1 = new(() => true, () => { }, order: 50);
        ParameterNode rootNode2 = new(() => true, () => { }, order: 25);
        builder.Nodes.Add(rootNode1);
        builder.Nodes.Add(rootNode2);

        typeof(ConfigDrawerBuilder)
            .GetField("_allCategoryNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(builder, new List<CategoryNode> { category });

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(2, category.Children);
        Assert.AreSame(catNode2, category.Children[0]);
        Assert.AreSame(catNode1, category.Children[1]);
        Assert.HasCount(2, builder.Nodes);
        Assert.AreSame(rootNode2, builder.Nodes[0]);
        Assert.AreSame(rootNode1, builder.Nodes[1]);
    }

    /// <summary>
    /// Tests that SortAll preserves stable sort order for nodes with equal order values.
    /// </summary>
    [TestMethod]
    public void SortAll_NodesWithEqualOrder_PreservesOriginalOrder()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode node1 = new(() => true, () => { }, order: 10);
        ParameterNode node2 = new(() => true, () => { }, order: 10);
        ParameterNode node3 = new(() => true, () => { }, order: 10);
        builder.Nodes.Add(node1);
        builder.Nodes.Add(node2);
        builder.Nodes.Add(node3);

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(3, builder.Nodes);
        Assert.AreSame(node1, builder.Nodes[0]);
        Assert.AreSame(node2, builder.Nodes[1]);
        Assert.AreSame(node3, builder.Nodes[2]);
    }

    /// <summary>
    /// Tests that SortAll correctly sorts nodes with mixed order values including int.MaxValue (default).
    /// </summary>
    [TestMethod]
    public void SortAll_MixedOrdersIncludingDefault_SortsCorrectly()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode node1 = new(() => true, () => { }, order: int.MaxValue);
        ParameterNode node2 = new(() => true, () => { }, order: 5);
        ParameterNode node3 = new(() => true, () => { }, order: int.MaxValue);
        ParameterNode node4 = new(() => true, () => { }, order: 10);
        builder.Nodes.Add(node1);
        builder.Nodes.Add(node2);
        builder.Nodes.Add(node3);
        builder.Nodes.Add(node4);

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(4, builder.Nodes);
        Assert.AreSame(node2, builder.Nodes[0]);
        Assert.AreSame(node4, builder.Nodes[1]);
        Assert.AreSame(node1, builder.Nodes[2]);
        Assert.AreSame(node3, builder.Nodes[3]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles non-ParameterNode types, treating them as having order int.MaxValue.
    /// </summary>
    [TestMethod]
    public void SortAll_NonParameterNodes_TreatedAsMaxValue()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode paramNode = new(() => true, () => { }, order: 50);
        Mock<IDrawNode> mockNode1 = new();
        Mock<IDrawNode> mockNode2 = new();
        builder.Nodes.Add(mockNode1.Object);
        builder.Nodes.Add(paramNode);
        builder.Nodes.Add(mockNode2.Object);

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(3, builder.Nodes);
        Assert.AreSame(paramNode, builder.Nodes[0]);
        Assert.AreSame(mockNode1.Object, builder.Nodes[1]);
        Assert.AreSame(mockNode2.Object, builder.Nodes[2]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles negative order values.
    /// </summary>
    [TestMethod]
    public void SortAll_NegativeOrderValues_SortsCorrectly()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode node1 = new(() => true, () => { }, order: 10);
        ParameterNode node2 = new(() => true, () => { }, order: -5);
        ParameterNode node3 = new(() => true, () => { }, order: 0);
        ParameterNode node4 = new(() => true, () => { }, order: -10);
        builder.Nodes.Add(node1);
        builder.Nodes.Add(node2);
        builder.Nodes.Add(node3);
        builder.Nodes.Add(node4);

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(4, builder.Nodes);
        Assert.AreSame(node4, builder.Nodes[0]);
        Assert.AreSame(node2, builder.Nodes[1]);
        Assert.AreSame(node3, builder.Nodes[2]);
        Assert.AreSame(node1, builder.Nodes[3]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles categories with mixed node types and orders.
    /// </summary>
    [TestMethod]
    public void SortAll_CategoryWithMixedNodeTypes_SortsCorrectly()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();

        CategoryNode category = new("Test Category");
        ParameterNode paramNode1 = new(() => true, () => { }, order: 20);
        Mock<IDrawNode> mockNode = new();
        ParameterNode paramNode2 = new(() => true, () => { }, order: 10);
        category.Children.Add(paramNode1);
        category.Children.Add(mockNode.Object);
        category.Children.Add(paramNode2);

        typeof(ConfigDrawerBuilder)
            .GetField("_allCategoryNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(builder, new List<CategoryNode> { category });

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(3, category.Children);
        Assert.AreSame(paramNode2, category.Children[0]);
        Assert.AreSame(paramNode1, category.Children[1]);
        Assert.AreSame(mockNode.Object, category.Children[2]);
    }

    /// <summary>
    /// Tests that SortAll can be called multiple times without side effects.
    /// </summary>
    [TestMethod]
    public void SortAll_CalledMultipleTimes_ProducesSameResult()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode node1 = new(() => true, () => { }, order: 30);
        ParameterNode node2 = new(() => true, () => { }, order: 10);
        ParameterNode node3 = new(() => true, () => { }, order: 20);
        builder.Nodes.Add(node1);
        builder.Nodes.Add(node2);
        builder.Nodes.Add(node3);

        // Act
        builder.SortAll();
        IDrawNode[] firstResult = [builder.Nodes[0], builder.Nodes[1], builder.Nodes[2]];
        builder.SortAll();

        // Assert
        Assert.AreSame(firstResult[0], builder.Nodes[0]);
        Assert.AreSame(firstResult[1], builder.Nodes[1]);
        Assert.AreSame(firstResult[2], builder.Nodes[2]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles a large number of nodes to verify performance characteristics.
    /// </summary>
    [TestMethod]
    public void SortAll_LargeNumberOfNodes_SortsCorrectly()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        List<ParameterNode> nodes = [];

        for (var i = 100; i > 0; i--)
        {
            ParameterNode node = new(() => true, () => { }, order: i);
            nodes.Add(node);
            builder.Nodes.Add(node);
        }

        // Act
        builder.SortAll();

        // Assert
        Assert.HasCount(100, builder.Nodes);
        for (var i = 0; i < 100; i++)
        {
            var node = (ParameterNode)builder.Nodes[i];
            Assert.AreEqual(i + 1, node.Order);
        }
    }

    /// <summary>
    /// Tests that Collect clears all internal collections before processing.
    /// Input: Call Collect twice with different configurations.
    /// Expected: Lists are cleared between calls and only second config's nodes remain.
    /// </summary>
    [TestMethod]
    public void Collect_CalledTwice_ClearsPreviousState()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var firstConfig = new SimpleConfig();
        var secondConfig = new SimpleConfig();

        // Act
        builder.Collect(firstConfig, typeof(SimpleConfig));
        var firstNodeCount = builder.Nodes.Count;
        var firstDisposableCount = builder.Disposables.Count;

        builder.Collect(secondConfig, typeof(SimpleConfig));
        var secondNodeCount = builder.Nodes.Count;

        // Assert
        Assert.AreEqual(firstNodeCount, secondNodeCount, "Node count should be same for identical configs after clearing.");
    }

    /// <summary>
    /// Tests that Collect clears Nodes list before processing.
    /// Input: Builder with pre-existing nodes from previous collection.
    /// Expected: Nodes list is cleared at start of Collect.
    /// </summary>
    [TestMethod]
    public void Collect_WithPreexistingNodes_ClearsNodesList()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();
        builder.Collect(config, typeof(SimpleConfig));
        var initialCount = builder.Nodes.Count;

        // Act
        builder.Collect(config, typeof(SimpleConfig));

        // Assert - if not cleared, count would accumulate
        Assert.HasCount(initialCount, builder.Nodes, "Nodes should be cleared between calls.");
    }

    #region Helper Types

    [UmbraAutoRegisterSettings]
    private sealed record SimpleConfig
    {
    }

    #endregion
}
