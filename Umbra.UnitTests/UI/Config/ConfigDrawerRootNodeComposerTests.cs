using System.Reflection;
using Umbra;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigDrawerRootNodeComposer"/>.
/// </summary>
[TestClass]
public sealed class ConfigDrawerRootNodeComposerTests
{
    /// <summary>
    /// Verifies that composition returns the original node list unchanged when root-node suppression is enabled.
    /// </summary>
    [TestMethod]
    public void Compose_WhenSuppressRootNodeIsTrue_ReturnsOriginalNodes()
    {
        // Arrange
        var nodes = new List<IDrawNode> { new TestDrawNode() };
        var searchIndex = CreateSearchIndex();

        // Act
        var composedNodes = ConfigDrawerRootNodeComposer.Compose<ConfigWithRootLabel>(
            "test-scope",
            nodes,
            searchIndex,
            suppressRootNode: true);

        // Assert
        Assert.AreSame(nodes, composedNodes);
        Assert.AreEqual("group:settings", searchIndex.Branches[0].BranchId);
        Assert.AreEqual("group:settings.audio", searchIndex.Branches[1].BranchId);
        Assert.AreEqual("category:settings.audio|Audio", searchIndex.Branches[2].BranchId);
        Assert.AreEqual("group:settings", searchIndex.Entries[0].AncestorBranchIds[0]);
    }

    /// <summary>
    /// Verifies that composition returns the original node list unchanged when the config type has no root-node attribute.
    /// </summary>
    [TestMethod]
    public void Compose_WhenRootNodeMetadataIsMissing_ReturnsOriginalNodes()
    {
        // Arrange
        var nodes = new List<IDrawNode> { new TestDrawNode() };
        var searchIndex = CreateSearchIndex();

        // Act
        var composedNodes = ConfigDrawerRootNodeComposer.Compose<ConfigWithoutRootNode>(
            "test-scope",
            nodes,
            searchIndex,
            suppressRootNode: false);

        // Assert
        Assert.AreSame(nodes, composedNodes);
        Assert.HasCount(3, searchIndex.Branches);
    }

    /// <summary>
    /// Verifies that composition falls back to the display name of the config type when no explicit label is declared.
    /// </summary>
    [TestMethod]
    public void Compose_WhenLabelIsNotDeclared_UsesDisplayNameFallback()
    {
        // Arrange
        var nodes = new List<IDrawNode> { new TestDrawNode() };
        var searchIndex = CreateSearchIndex();

        // Act
        var composedNodes = ConfigDrawerRootNodeComposer.Compose<ConfigWithDefaultLabel>(
            "test-scope",
            nodes,
            searchIndex,
            suppressRootNode: false);

        // Assert
        Assert.HasCount(1, composedNodes);
        Assert.IsInstanceOfType<RootTreeNode>(composedNodes[0]);
        Assert.AreEqual(typeof(ConfigWithDefaultLabel).Name.ToDisplayName(), GetPrivateField<string>(composedNodes[0], "_label"));
        Assert.IsTrue(GetPrivateField<bool>(composedNodes[0], "_defaultOpen"));
    }

    /// <summary>
    /// Verifies that composition prepends the synthetic root branch when wrapping occurs.
    /// </summary>
    [TestMethod]
    public void Compose_WhenRootNodeIsCreated_PrependsRootBranchToSearchIndex()
    {
        // Arrange
        var nodes = new List<IDrawNode> { new TestDrawNode() };
        var searchIndex = CreateSearchIndex();

        // Act
        var composedNodes = ConfigDrawerRootNodeComposer.Compose<ConfigWithRootLabel>(
            "test-scope",
            nodes,
            searchIndex,
            suppressRootNode: false);

        // Assert
        Assert.HasCount(1, composedNodes);
        Assert.IsInstanceOfType<RootTreeNode>(composedNodes[0]);
        Assert.AreEqual("Root Label", GetPrivateField<string>(composedNodes[0], "_label"));
        Assert.AreEqual("root:test-scope", searchIndex.Branches[^1].BranchId);
        Assert.AreEqual("root:test-scope", searchIndex.Entries[0].AncestorBranchIds[0]);
    }

    private static ConfigSearchIndex CreateSearchIndex()
    {
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Master Volume", null, "Audio", "settings.audio");
        return searchIndex;
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field);
        return (T)field.GetValue(instance)!;
    }

    private sealed class ConfigWithoutRootNode;

    [UmbraRootNode("Root Label")]
    private sealed class ConfigWithRootLabel;

    [UmbraRootNode(defaultOpen: true)]
    private sealed class ConfigWithDefaultLabel;
}
