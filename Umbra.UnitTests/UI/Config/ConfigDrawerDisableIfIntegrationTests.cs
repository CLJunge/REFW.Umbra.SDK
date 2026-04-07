using System.Reflection;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Integration tests for conditional disable propagation through configuration draw-tree composition.
/// </summary>
[TestClass]
public sealed class ConfigDrawerDisableIfIntegrationTests
{
    /// <summary>
    /// Verifies that a parameter using a full custom drawer still receives a live disabled predicate.
    /// </summary>
    [TestMethod]
    public void Collect_CustomParameterDrawerWithDisableIf_ComposesDisabledPredicate()
    {
        // Arrange
        var config = new CustomParameterDrawerConfig();
        var builder = new ConfigDrawerBuilder();

        // Act
        builder.Collect(config, typeof(CustomParameterDrawerConfig));

        // Assert
        Assert.HasCount(1, builder.Nodes);
        var node = AssertNode<ParameterNode>(builder.Nodes[0]);
        var isDisabled = GetDisabledPredicate(node);
        Assert.IsNotNull(isDisabled);
        Assert.IsFalse(isDisabled!());

        config.DisableCustom = true;

        Assert.IsTrue(isDisabled());
    }

    /// <summary>
    /// Verifies that disabling a nested group propagates to descendant parameters while the tree-wrapper category node remains a separate non-parameter wrapper.
    /// </summary>
    [TestMethod]
    public void Collect_NestedTreeGroupWithDisableIf_PropagatesToDescendantParameter()
    {
        // Arrange
        var config = new NestedTreeDisableConfig();
        var builder = new ConfigDrawerBuilder();

        // Act
        builder.Collect(config, typeof(NestedTreeDisableConfig));

        // Assert
        Assert.HasCount(1, builder.Nodes);
        var scopedNode = AssertNode<IdScopeNode>(builder.Nodes[0]);
        var scopedChildren = GetPrivateField<List<IDrawNode>>(scopedNode, "_children");
        Assert.HasCount(1, scopedChildren);
        Assert.IsInstanceOfType<CategoryNode>(scopedChildren[0]);

        var categoryNode = (CategoryNode)scopedChildren[0];
        Assert.HasCount(1, categoryNode.Children);
        var parameterNode = AssertNode<ParameterNode>(categoryNode.Children[0]);
        var isDisabled = GetDisabledPredicate(parameterNode);
        Assert.IsNotNull(isDisabled);
        Assert.IsFalse(isDisabled!());

        config.DisableNested = true;

        Assert.IsTrue(isDisabled());
    }

    /// <summary>
    /// Verifies that a custom nested-group drawer receives a live disabled predicate on its composed drawer node.
    /// </summary>
    [TestMethod]
    public void Collect_CustomNestedDrawerWithDisableIf_ComposesDisabledPredicate()
    {
        // Arrange
        var config = new CustomNestedDrawerDisableConfig();
        var builder = new ConfigDrawerBuilder();

        // Act
        builder.Collect(config, typeof(CustomNestedDrawerDisableConfig));

        // Assert
        Assert.HasCount(1, builder.Nodes);
        var scopedNode = AssertNode<IdScopeNode>(builder.Nodes[0]);
        var scopedChildren = GetPrivateField<List<IDrawNode>>(scopedNode, "_children");
        Assert.HasCount(1, scopedChildren);

        var drawerNode = AssertNode<ParameterNode>(scopedChildren[0]);
        var isDisabled = GetDisabledPredicate(drawerNode);
        Assert.IsNotNull(isDisabled);
        Assert.IsFalse(isDisabled!());

        config.DisableDrawer = true;

        Assert.IsTrue(isDisabled());
    }

    private static Func<bool>? GetDisabledPredicate(ParameterNode node)
        => GetPrivateField<Func<bool>?>(node, "_isDisabled");

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field);
        return (T)field.GetValue(instance)!;
    }

    private static TNode AssertNode<TNode>(IDrawNode node) where TNode : class, IDrawNode
    {
        Assert.IsInstanceOfType<TNode>(node);
        return (TNode)node;
    }

    [UmbraAutoRegister]
    private sealed class CustomParameterDrawerConfig
    {
        public bool DisableCustom { get; set; }

        [UmbraParameter]
        [UmbraDisableIf<bool>(nameof(DisableCustom))]
        [UmbraDrawer<ConfigDrawerDisableIfIntegrationParameterDrawer>]
        public Parameter<int> Value { get; } = new(1)
        {
            Metadata = new ParameterMetadata { ResolvedLabel = "Value" }
        };
    }

    [UmbraAutoRegister]
    private sealed class NestedTreeDisableConfig
    {
        public bool DisableNested { get; set; }

        [UmbraDisableIf<bool>(nameof(DisableNested))]
        [UmbraCategory("Nested Group")]
        [UmbraCollapseAsTree]
        public NestedTreeDisableGroup Group { get; } = new();
    }

    [UmbraAutoRegister]
    private sealed class NestedTreeDisableGroup
    {
        [UmbraParameter]
        public Parameter<int> Value { get; } = new(1)
        {
            Metadata = new ParameterMetadata { ResolvedLabel = "Nested Value" }
        };
    }

    [UmbraAutoRegister]
    private sealed class CustomNestedDrawerDisableConfig
    {
        public bool DisableDrawer { get; set; }

        [UmbraDisableIf<bool>(nameof(DisableDrawer))]
        [UmbraNestedDrawer<ConfigDrawerDisableIfIntegrationNestedDrawer>]
        public CustomNestedDrawerDisableGroup Group { get; } = new();
    }

    [UmbraAutoRegister]
    private sealed class CustomNestedDrawerDisableGroup
    {
        [UmbraParameter]
        public Parameter<int> Value { get; } = new(1)
        {
            Metadata = new ParameterMetadata { ResolvedLabel = "Drawer Value" }
        };
    }

    private sealed class ConfigDrawerDisableIfIntegrationParameterDrawer : IParameterDrawer
    {
        public void Draw(string label, IParameter parameter)
        {
            _ = label;
            _ = parameter;
        }
    }

    private sealed class ConfigDrawerDisableIfIntegrationNestedDrawer : INestedDrawer<CustomNestedDrawerDisableGroup>
    {
        public void Draw(CustomNestedDrawerDisableGroup groupInstance) => _ = groupInstance;
    }
}
