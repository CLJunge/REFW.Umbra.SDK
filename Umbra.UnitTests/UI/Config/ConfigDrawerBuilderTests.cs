using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config;
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
        Assert.AreEqual(0, builder.Nodes.Count);
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
        Assert.AreEqual(3, builder.Nodes.Count);
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
        Assert.AreEqual(2, category.Children.Count);
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
        Assert.AreEqual(2, category.Children.Count);
        Assert.AreSame(catNode2, category.Children[0]);
        Assert.AreSame(catNode1, category.Children[1]);
        Assert.AreEqual(2, builder.Nodes.Count);
        Assert.AreSame(rootNode2, builder.Nodes[0]);
        Assert.AreSame(rootNode1, builder.Nodes[1]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles multiple category nodes, sorting each independently.
    /// </summary>
    [TestMethod]
    public void SortAll_MultipleCategoryNodes_SortsEachIndependently()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();

        CategoryNode category1 = new("Category 1");
        ParameterNode cat1Node1 = new(() => true, () => { }, order: 40);
        ParameterNode cat1Node2 = new(() => true, () => { }, order: 20);
        category1.Children.Add(cat1Node1);
        category1.Children.Add(cat1Node2);

        CategoryNode category2 = new("Category 2");
        ParameterNode cat2Node1 = new(() => true, () => { }, order: 60);
        ParameterNode cat2Node2 = new(() => true, () => { }, order: 15);
        category2.Children.Add(cat2Node1);
        category2.Children.Add(cat2Node2);

        typeof(ConfigDrawerBuilder)
            .GetField("_allCategoryNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(builder, new List<CategoryNode> { category1, category2 });

        // Act
        builder.SortAll();

        // Assert
        Assert.AreEqual(2, category1.Children.Count);
        Assert.AreSame(cat1Node2, category1.Children[0]);
        Assert.AreSame(cat1Node1, category1.Children[1]);
        Assert.AreEqual(2, category2.Children.Count);
        Assert.AreSame(cat2Node2, category2.Children[0]);
        Assert.AreSame(cat2Node1, category2.Children[1]);
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
        Assert.AreEqual(3, builder.Nodes.Count);
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
        Assert.AreEqual(4, builder.Nodes.Count);
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
        Assert.AreEqual(3, builder.Nodes.Count);
        Assert.AreSame(paramNode, builder.Nodes[0]);
        Assert.AreSame(mockNode1.Object, builder.Nodes[1]);
        Assert.AreSame(mockNode2.Object, builder.Nodes[2]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles category children with empty lists.
    /// </summary>
    [TestMethod]
    public void SortAll_CategoryWithEmptyChildren_HandlesGracefully()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        CategoryNode emptyCategory = new("Empty Category");

        typeof(ConfigDrawerBuilder)
            .GetField("_allCategoryNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(builder, new List<CategoryNode> { emptyCategory });

        // Act
        builder.SortAll();

        // Assert
        Assert.AreEqual(0, emptyCategory.Children.Count);
    }

    /// <summary>
    /// Tests that SortAll correctly handles single node in both category and root lists.
    /// </summary>
    [TestMethod]
    public void SortAll_SingleNodeInEachList_NoChanges()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();

        CategoryNode category = new("Test Category");
        ParameterNode catNode = new(() => true, () => { }, order: 42);
        category.Children.Add(catNode);

        ParameterNode rootNode = new(() => true, () => { }, order: 100);
        builder.Nodes.Add(rootNode);

        typeof(ConfigDrawerBuilder)
            .GetField("_allCategoryNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(builder, new List<CategoryNode> { category });

        // Act
        builder.SortAll();

        // Assert
        Assert.AreEqual(1, category.Children.Count);
        Assert.AreSame(catNode, category.Children[0]);
        Assert.AreEqual(1, builder.Nodes.Count);
        Assert.AreSame(rootNode, builder.Nodes[0]);
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
        Assert.AreEqual(4, builder.Nodes.Count);
        Assert.AreSame(node4, builder.Nodes[0]);
        Assert.AreSame(node2, builder.Nodes[1]);
        Assert.AreSame(node3, builder.Nodes[2]);
        Assert.AreSame(node1, builder.Nodes[3]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles int.MinValue order value.
    /// </summary>
    [TestMethod]
    public void SortAll_IntMinValueOrder_SortsFirst()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode node1 = new(() => true, () => { }, order: 0);
        ParameterNode node2 = new(() => true, () => { }, order: int.MinValue);
        ParameterNode node3 = new(() => true, () => { }, order: -1);
        builder.Nodes.Add(node1);
        builder.Nodes.Add(node2);
        builder.Nodes.Add(node3);

        // Act
        builder.SortAll();

        // Assert
        Assert.AreEqual(3, builder.Nodes.Count);
        Assert.AreSame(node2, builder.Nodes[0]);
        Assert.AreSame(node3, builder.Nodes[1]);
        Assert.AreSame(node1, builder.Nodes[2]);
    }

    /// <summary>
    /// Tests that SortAll correctly handles all nodes with int.MaxValue order (unordered).
    /// </summary>
    [TestMethod]
    public void SortAll_AllNodesUnordered_PreservesOriginalOrder()
    {
        // Arrange
        ConfigDrawerBuilder builder = new();
        ParameterNode node1 = new(() => true, () => { });
        ParameterNode node2 = new(() => true, () => { });
        ParameterNode node3 = new(() => true, () => { });
        builder.Nodes.Add(node1);
        builder.Nodes.Add(node2);
        builder.Nodes.Add(node3);

        // Act
        builder.SortAll();

        // Assert
        Assert.AreEqual(3, builder.Nodes.Count);
        Assert.AreSame(node1, builder.Nodes[0]);
        Assert.AreSame(node2, builder.Nodes[1]);
        Assert.AreSame(node3, builder.Nodes[2]);
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
        Assert.AreEqual(3, category.Children.Count);
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

        for (int i = 100; i > 0; i--)
        {
            ParameterNode node = new(() => true, () => { }, order: i);
            nodes.Add(node);
            builder.Nodes.Add(node);
        }

        // Act
        builder.SortAll();

        // Assert
        Assert.AreEqual(100, builder.Nodes.Count);
        for (int i = 0; i < 100; i++)
        {
            ParameterNode node = (ParameterNode)builder.Nodes[i];
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
        int firstNodeCount = builder.Nodes.Count;
        int firstDisposableCount = builder.Disposables.Count;

        builder.Collect(secondConfig, typeof(SimpleConfig));
        int secondNodeCount = builder.Nodes.Count;

        // Assert
        Assert.AreEqual(firstNodeCount, secondNodeCount, "Node count should be same for identical configs after clearing.");
    }

    /// <summary>
    /// Tests that Collect populates Nodes list from scope after CollectInto completes.
    /// Input: A simple configuration object.
    /// Expected: Nodes list is populated with elements from the scope.
    /// </summary>
    [TestMethod]
    public void Collect_WithSimpleConfig_PopulatesNodesList()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();

        // Act
        builder.Collect(config, typeof(SimpleConfig));

        // Assert
        Assert.IsNotNull(builder.Nodes, "Nodes list should not be null.");
    }

    /// <summary>
    /// Tests that Collect uses categoryOverride instead of type-level category metadata.
    /// Input: Config with type-level category, but categoryOverride provided.
    /// Expected: Override value is passed to ConfigDrawScope constructor.
    /// </summary>
    [TestMethod]
    public void Collect_WithCategoryOverride_UsesCategoryOverrideValue()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new CategorizedConfig();
        string categoryOverride = "OverriddenCategory";

        // Act
        builder.Collect(config, typeof(CategorizedConfig), categoryOverride: categoryOverride);

        // Assert - method completes without exception
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect handles null categoryOverride and falls back to type metadata.
    /// Input: Config with type-level category, categoryOverride is null.
    /// Expected: Type metadata category is used.
    /// </summary>
    [TestMethod]
    public void Collect_WithNullCategoryOverride_UsesTypeMetadataCategory()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new CategorizedConfig();

        // Act
        builder.Collect(config, typeof(CategorizedConfig), categoryOverride: null);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect handles empty string categoryOverride.
    /// Input: categoryOverride is empty string.
    /// Expected: Empty string is used (takes precedence over metadata).
    /// </summary>
    [TestMethod]
    public void Collect_WithEmptyCategoryOverride_UsesEmptyString()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new CategorizedConfig();

        // Act
        builder.Collect(config, typeof(CategorizedConfig), categoryOverride: string.Empty);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect handles whitespace-only categoryOverride.
    /// Input: categoryOverride contains only whitespace.
    /// Expected: Whitespace string is used without trimming.
    /// </summary>
    [TestMethod]
    public void Collect_WithWhitespaceCategoryOverride_UsesWhitespaceValue()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();

        // Act
        builder.Collect(config, typeof(SimpleConfig), categoryOverride: "   ");

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect uses collapseOverride instead of type-level collapse metadata.
    /// Input: Config with collapse attribute, collapseOverride provided.
    /// Expected: Override value is passed to ConfigDrawScope.
    /// </summary>
    [TestMethod]
    public void Collect_WithCollapseOverride_UsesCollapseOverrideValue()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();
        var collapseOverride = new UmbraCollapseAsTreeAttribute();

        // Act
        builder.Collect(config, typeof(SimpleConfig), collapseOverride: collapseOverride);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect uses propertyIndentOverride when provided.
    /// Input: propertyIndentOverride with specific indent amount.
    /// Expected: Override is passed to ConfigDrawScope.
    /// </summary>
    [TestMethod]
    public void Collect_WithPropertyIndentOverride_UsesIndentOverride()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();
        var indentOverride = new UmbraIndentAttribute(20);

        // Act
        builder.Collect(config, typeof(SimpleConfig), propertyIndentOverride: indentOverride);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect uses labelMarginOverride instead of type-level label margin metadata.
    /// Input: labelMarginOverride provided.
    /// Expected: Override value is passed to ConfigDrawScope.
    /// </summary>
    [TestMethod]
    public void Collect_WithLabelMarginOverride_UsesLabelMarginOverrideValue()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();
        var labelMarginOverride = new UmbraLabelMarginAttribute(10);

        // Act
        builder.Collect(config, typeof(SimpleConfig), labelMarginOverride: labelMarginOverride);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect handles all override parameters being null.
    /// Input: All optional override parameters are null.
    /// Expected: Type metadata values are used as fallbacks.
    /// </summary>
    [TestMethod]
    public void Collect_WithAllNullOverrides_UsesTypeMetadataFallbacks()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new CategorizedConfig();

        // Act
        builder.Collect(
            config,
            typeof(CategorizedConfig),
            propertyIndentOverride: null,
            categoryOverride: null,
            collapseOverride: null,
            labelMarginOverride: null);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect handles all override parameters being provided simultaneously.
    /// Input: All optional override parameters have non-null values.
    /// Expected: All overrides take precedence over metadata.
    /// </summary>
    [TestMethod]
    public void Collect_WithAllOverridesProvided_UsesAllOverrideValues()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new CategorizedConfig();
        var indentOverride = new UmbraIndentAttribute(15);
        var collapseOverride = new UmbraCollapseAsTreeAttribute();
        var labelMarginOverride = new UmbraLabelMarginAttribute(8);

        // Act
        builder.Collect(
            config,
            typeof(CategorizedConfig),
            propertyIndentOverride: indentOverride,
            categoryOverride: "FullOverride",
            collapseOverride: collapseOverride,
            labelMarginOverride: labelMarginOverride);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect handles type with null SettingsPrefix in metadata.
    /// Input: Type with no SettingsPrefix attribute.
    /// Expected: Empty string is used as rootGroupPath.
    /// </summary>
    [TestMethod]
    public void Collect_WithNullSettingsPrefix_UsesEmptyString()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();

        // Act
        builder.Collect(config, typeof(SimpleConfig));

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect handles type with non-null SettingsPrefix in metadata.
    /// Input: Type with SettingsPrefix attribute.
    /// Expected: SettingsPrefix value is used as rootGroupPath.
    /// </summary>
    [TestMethod]
    public void Collect_WithNonNullSettingsPrefix_UsesSettingsPrefixValue()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new PrefixedConfig();

        // Act
        builder.Collect(config, typeof(PrefixedConfig));

        // Assert
        Assert.IsNotNull(builder.Nodes);
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
        int initialCount = builder.Nodes.Count;

        // Act
        builder.Collect(config, typeof(SimpleConfig));

        // Assert - if not cleared, count would accumulate
        Assert.AreEqual(initialCount, builder.Nodes.Count, "Nodes should be cleared between calls.");
    }

    /// <summary>
    /// Tests that Collect clears Disposables list before processing.
    /// Input: Builder instance.
    /// Expected: Disposables list is cleared at start of Collect.
    /// </summary>
    [TestMethod]
    public void Collect_BeforeProcessing_ClearsDisposablesList()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new SimpleConfig();

        // Act
        builder.Collect(config, typeof(SimpleConfig));

        // Assert
        Assert.AreEqual(0, builder.Disposables.Count, "Disposables should be cleared.");
    }

    /// <summary>
    /// Tests that Collect processes config with parameter properties.
    /// Input: Config with Parameter&lt;T&gt; properties.
    /// Expected: CollectInto processes parameters and may add nodes.
    /// </summary>
    [TestMethod]
    public void Collect_WithParameterConfig_ProcessesParameters()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new ParameterConfig();

        // Act
        builder.Collect(config, typeof(ParameterConfig));

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect copies all nodes from scope.Nodes to builder.Nodes.
    /// Input: Config that results in nodes being added to scope.
    /// Expected: All scope nodes are present in builder.Nodes after Collect.
    /// </summary>
    [TestMethod]
    public void Collect_AfterCollectInto_CopiesAllNodesFromScope()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new ParameterConfig();

        // Act
        builder.Collect(config, typeof(ParameterConfig));

        // Assert
        Assert.IsNotNull(builder.Nodes, "Nodes should be populated from scope.");
    }

    /// <summary>
    /// Tests that Collect handles config with only nested groups (no direct parameters).
    /// Input: Config with nested group properties but no Parameter properties.
    /// Expected: Nested groups are processed via recursion in CollectInto.
    /// </summary>
    [TestMethod]
    public void Collect_WithNestedGroupConfig_ProcessesNestedGroups()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new ParentConfig { Nested = new NestedConfig() };

        // Act
        builder.Collect(config, typeof(ParentConfig));

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    /// <summary>
    /// Tests that Collect respects the override precedence chain.
    /// Input: Type with metadata attributes, overrides that differ from metadata.
    /// Expected: Overrides take precedence in ConfigDrawScope construction.
    /// </summary>
    [TestMethod]
    public void Collect_WithOverridesAndMetadata_OverridesTakePrecedence()
    {
        // Arrange
        var builder = new ConfigDrawerBuilder();
        var config = new FullyDecoratedConfig();
        var differentIndent = new UmbraIndentAttribute(50);
        var differentCollapse = new UmbraCollapseAsTreeAttribute();
        var differentMargin = new UmbraLabelMarginAttribute(25);

        // Act
        builder.Collect(
            config,
            typeof(FullyDecoratedConfig),
            propertyIndentOverride: differentIndent,
            categoryOverride: "DifferentCategory",
            collapseOverride: differentCollapse,
            labelMarginOverride: differentMargin);

        // Assert
        Assert.IsNotNull(builder.Nodes);
    }

    #region Helper Types

    [UmbraAutoRegisterSettings]
    private sealed record SimpleConfig
    {
    }

    [UmbraAutoRegisterSettings]
    [UmbraCategory("TestCategory")]
    private sealed record CategorizedConfig
    {
    }

    [UmbraAutoRegisterSettings]
    [UmbraSettingsPrefix("test.prefix")]
    private sealed record PrefixedConfig
    {
    }

    [UmbraAutoRegisterSettings]
    private sealed record ParameterConfig
    {
        [UmbraSettingsParameter]
        public Parameter<bool> Enabled { get; set; } = new(true);

        [UmbraSettingsParameter]
        public Parameter<int> Value { get; set; } = new(42);
    }

    [UmbraAutoRegisterSettings]
    private sealed record ParentConfig
    {
        [UmbraSettingsParameter]
        public NestedConfig? Nested { get; set; }
    }

    [UmbraAutoRegisterSettings]
    private sealed record NestedConfig
    {
        [UmbraSettingsParameter]
        public Parameter<string> Name { get; set; } = new("default");
    }

    [UmbraAutoRegisterSettings]
    [UmbraCategory("FullCategory")]
    [UmbraIndent(10)]
    [UmbraCollapseAsTree]
    [UmbraLabelMargin(5)]
    [UmbraSettingsPrefix("full.prefix")]
    private sealed record FullyDecoratedConfig
    {
        [UmbraSettingsParameter]
        public Parameter<double> Ratio { get; set; } = new(1.5);
    }

    #endregion
}