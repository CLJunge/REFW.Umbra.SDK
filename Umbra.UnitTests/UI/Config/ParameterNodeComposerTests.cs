using Umbra.Config;
using Umbra.UI.Config.Nodes;


namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterNodeComposer"/>.
/// </summary>
/// <remarks>
/// These tests exercise the <see cref="ParameterNodeComposer.Create"/> method, which composes
/// draw nodes for configuration parameters. Note that this method calls static methods
/// (<see cref="ControlFactory.BuildDrawAction"/> and <see cref="VisibilityPredicateResolver.Build"/>)
/// that cannot be mocked with Moq. Therefore, these tests have an integration-style characteristic
/// where the actual implementations of those static methods are invoked. The tests focus on
/// verifying the logic within <see cref="ParameterNodeComposer.Create"/> itself: margin assignment,
/// indent resolution, and <see cref="ParameterNode"/> construction.
/// </remarks>
[TestClass]
public class ParameterNodeComposerTests
{
    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> sets the alignment group margin
    /// when <paramref name="classLabelMarginPixels"/> has a value.
    /// </summary>
    [TestMethod]
    public void Create_ClassLabelMarginPixelsHasValue_SetsAlignmentGroupMargin()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = 10,
            SpacingBefore = 1,
            SpacingAfter = 2
        };
        var parameter = new Parameter<int>(42) { Metadata = metadata };

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();
        const float expectedMargin = 15.5f;

        // Act
        var result = ParameterNodeComposer.Create(
            parameter,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: expectedMargin);

        // Assert
        Assert.AreEqual(expectedMargin, alignmentGroup.Margin);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> does not modify the alignment group margin
    /// when <paramref name="classLabelMarginPixels"/> is null.
    /// </summary>
    [TestMethod]
    public void Create_ClassLabelMarginPixelsIsNull_DoesNotModifyAlignmentGroupMargin()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = 10,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameter = new Parameter<int>(42) { Metadata = metadata };

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();
        const float initialMargin = 5.0f;
        alignmentGroup.Margin = initialMargin;

        // Act
        var result = ParameterNodeComposer.Create(
            parameter,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.AreEqual(initialMargin, alignmentGroup.Margin);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> constructs a <see cref="ParameterNode"/>
    /// with the correct order value from metadata when specified.
    /// </summary>
    [TestMethod]
    public void Create_MetadataOrderSpecified_ParameterNodeHasCorrectOrder()
    {
        // Arrange
        const int expectedOrder = 42;
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = expectedOrder,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameter = new Parameter<int>(0) { Metadata = metadata };

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameter,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.AreEqual(expectedOrder, node.Order);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> constructs a <see cref="ParameterNode"/>
    /// with <see cref="int.MaxValue"/> as the order when metadata order is null.
    /// </summary>
    [TestMethod]
    public void Create_MetadataOrderIsNull_ParameterNodeHasMaxValueOrder()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = null,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameter = new Parameter<int>(0) { Metadata = metadata };

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameter,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.AreEqual(int.MaxValue, node.Order);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles extreme boundary values
    /// for the order parameter without throwing exceptions.
    /// </summary>
    /// <param name="order">The order value.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(0)]
    [DataRow(int.MaxValue)]
    [DataRow(-1)]
    [DataRow(1)]
    public void Create_ExtremeBoundaryOrderValues_HandlesGracefully(int order)
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = order,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameter = new Parameter<int>(0) { Metadata = metadata };

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameter,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.AreEqual(order, node.Order);
    }

    /// <summary>
    /// Tests that Create returns a null resource for built-in parameter types with no custom drawer.
    /// </summary>
    [TestMethod]
    public void Create_BuiltInParameterType_ReturnsNullResource()
    {
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0,
            DrawerType = null,
            TwoColumnDrawerType = null
        };
        var parameter = new Parameter<int>(42) { Metadata = metadata };
        var alignmentGroup = new LabelAlignmentGroup();

        var (_, resource) = ParameterNodeComposer.Create(parameter, new object(), alignmentGroup, null, null);

        Assert.IsNull(resource);
    }

    /// <summary>
    /// Tests that a class label margin assignment overwrites an earlier alignment-group margin.
    /// </summary>
    [TestMethod]
    public void Create_ClassLabelMarginPixelsOverwritesExistingMargin()
    {
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = 0,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameter = new Parameter<int>(0) { Metadata = metadata };
        var alignmentGroup = new LabelAlignmentGroup { Margin = 3f };

        _ = ParameterNodeComposer.Create(parameter, new object(), alignmentGroup, null, 9f);

        Assert.AreEqual(9f, alignmentGroup.Margin);
    }

}
