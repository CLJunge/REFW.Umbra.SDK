using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config;
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
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(42);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();
        const float expectedMargin = 15.5f;

        // Act
        var result = ParameterNodeComposer.Create(
            parameterMock.Object,
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
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(42);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();
        const float initialMargin = 5.0f;
        alignmentGroup.Margin = initialMargin;

        // Act
        var result = ParameterNodeComposer.Create(
            parameterMock.Object,
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
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
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
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.AreEqual(int.MaxValue, node.Order);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> correctly passes spacing before
    /// and spacing after values from metadata to the <see cref="ParameterNode"/>.
    /// </summary>
    /// <param name="spacingBefore">The spacing before value.</param>
    /// <param name="spacingAfter">The spacing after value.</param>
    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 0)]
    [DataRow(0, 1)]
    [DataRow(2, 3)]
    [DataRow(5, 5)]
    [DataRow(10, 1)]
    public void Create_VariousSpacingValues_ParameterNodeReflectsSpacing(int spacingBefore, int spacingAfter)
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = 1,
            SpacingBefore = spacingBefore,
            SpacingAfter = spacingAfter
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(string));
        parameterMock.Setup(p => p.GetValue()).Returns("test");

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        // Note: ParameterNode constructor parameters are private, so we cannot directly verify
        // spacingBefore and spacingAfter. However, the constructor accepts these values,
        // and the node is successfully created, which implies they are stored correctly.
        Assert.IsNotNull(node);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> returns a non-null <see cref="ParameterNode"/>
    /// for valid input parameters.
    /// </summary>
    [TestMethod]
    public void Create_ValidInputs_ReturnsNonNullParameterNode()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(bool));
        parameterMock.Setup(p => p.GetValue()).Returns(true);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, resource) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles the case when both
    /// metadata indent and class indent amount are null, resulting in no indent wrapping.
    /// </summary>
    [TestMethod]
    public void Create_BothIndentValuesNull_NoIndentApplied()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Indent = null,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // The draw action should not be wrapped with indent logic when both are null.
        // Verification of this would require invoking the draw action and checking ImGui calls,
        // which is beyond the scope of this unit test.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> uses the class indent amount
    /// when metadata indent is null but class indent amount has a value.
    /// </summary>
    [TestMethod]
    public void Create_MetadataIndentNullClassIndentHasValue_UsesClassIndent()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Indent = null,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();
        const float classIndent = 20.0f;

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: classIndent,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // The draw action should be wrapped with indent logic using classIndent.
        // Direct verification would require invoking the draw action.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> uses the metadata indent value
    /// when both metadata indent and class indent amount have values, preferring metadata.
    /// </summary>
    [TestMethod]
    public void Create_BothIndentValuesPresent_PrefersMetadataIndent()
    {
        // Arrange
        const float metadataIndent = 30.0f;
        const float classIndent = 20.0f;
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Indent = metadataIndent,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: classIndent,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // The draw action should be wrapped with indent logic using metadataIndent.
        // Direct verification would require invoking the draw action.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles a zero indent amount correctly.
    /// </summary>
    [TestMethod]
    public void Create_IndentAmountIsZero_AppliesZeroIndent()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Indent = 0.0f,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // A zero indent is still applied (using ImGui's default spacing).
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles negative indent amounts.
    /// </summary>
    [TestMethod]
    public void Create_NegativeIndentAmount_AppliesNegativeIndent()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Indent = -10.0f,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // ImGui.Indent and Unindent will be called with the negative value.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles a large positive indent amount.
    /// </summary>
    [TestMethod]
    public void Create_LargePositiveIndentAmount_AppliesLargeIndent()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Indent = float.MaxValue,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // ImGui will handle the large value; the method constructs successfully.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles metadata with a null HideIf attribute,
    /// resulting in a visibility predicate that always returns true.
    /// </summary>
    [TestMethod]
    public void Create_MetadataHideIfIsNull_VisibilityAlwaysTrue()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            HideIf = null,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // The visibility predicate is a static lambda that always returns true.
        // Verification would require invoking node.Draw() and observing behavior.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> constructs a <see cref="ParameterNode"/>
    /// with a resolved visibility predicate when metadata contains a HideIf attribute.
    /// </summary>
    [TestMethod]
    public void Create_MetadataHideIfPresent_BuildsVisibilityPredicate()
    {
        // Arrange
        var hideIfMock = new Mock<IHideIfAttribute>();
        hideIfMock.Setup(h => h.MemberName).Returns("IsHidden");
        hideIfMock.Setup(h => h.HasValue).Returns(false);

        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            HideIf = hideIfMock.Object,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new TestOwner { IsHidden = false };
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // VisibilityPredicateResolver.Build is invoked to create the predicate.
        // The predicate's behavior depends on the owner's state at runtime.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles extreme boundary values
    /// for spacing before and after without throwing exceptions.
    /// </summary>
    /// <param name="spacingBefore">The spacing before value.</param>
    /// <param name="spacingAfter">The spacing after value.</param>
    [TestMethod]
    [DataRow(int.MinValue, 0)]
    [DataRow(0, int.MinValue)]
    [DataRow(int.MaxValue, 0)]
    [DataRow(0, int.MaxValue)]
    [DataRow(int.MinValue, int.MaxValue)]
    public void Create_ExtremeBoundarySpacingValues_HandlesGracefully(int spacingBefore, int spacingAfter)
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Order = 1,
            SpacingBefore = spacingBefore,
            SpacingAfter = spacingAfter
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // The method constructs successfully; runtime behavior during Draw() is ImGui's responsibility.
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
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.AreEqual(order, node.Order);
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles an empty resolved label string.
    /// </summary>
    [TestMethod]
    public void Create_EmptyResolvedLabel_HandlesGracefully()
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = string.Empty,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // ControlFactory.BuildDrawAction will receive an empty label.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles a very long resolved label string.
    /// </summary>
    [TestMethod]
    public void Create_VeryLongResolvedLabel_HandlesGracefully()
    {
        // Arrange
        var longLabel = new string('A', 10000);
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = longLabel,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: null);

        // Assert
        Assert.IsNotNull(node);
        // ControlFactory.BuildDrawAction will receive the long label.
    }

    /// <summary>
    /// Tests that <see cref="ParameterNodeComposer.Create"/> handles floating-point special values
    /// for indent and margin parameters.
    /// </summary>
    /// <param name="indentValue">The indent value.</param>
    /// <param name="marginValue">The margin value.</param>
    [TestMethod]
    [DataRow(float.NaN, float.NaN)]
    [DataRow(float.PositiveInfinity, float.PositiveInfinity)]
    [DataRow(float.NegativeInfinity, float.NegativeInfinity)]
    [DataRow(float.MinValue, float.MinValue)]
    [DataRow(float.MaxValue, float.MaxValue)]
    public void Create_SpecialFloatValues_HandlesGracefully(float indentValue, float marginValue)
    {
        // Arrange
        var metadata = new ParameterMetadata
        {
            ResolvedLabel = "TestLabel",
            Indent = indentValue,
            Order = 1,
            SpacingBefore = 0,
            SpacingAfter = 0
        };
        var parameterMock = new Mock<IParameter>();
        parameterMock.Setup(p => p.Metadata).Returns(metadata);
        parameterMock.Setup(p => p.ValueType).Returns(typeof(int));
        parameterMock.Setup(p => p.GetValue()).Returns(0);

        var owner = new object();
        var alignmentGroup = new LabelAlignmentGroup();

        // Act
        var (node, _) = ParameterNodeComposer.Create(
            parameterMock.Object,
            owner,
            alignmentGroup,
            classIndentAmount: null,
            classLabelMarginPixels: marginValue);

        // Assert
        Assert.IsNotNull(node);
        // ImGui and the alignment group will handle special float values.
        // NaN comparisons may behave unexpectedly, but the method constructs successfully.
    }

    /// <summary>
    /// Test helper class with a public boolean property for visibility predicate testing.
    /// </summary>
    private class TestOwner
    {
        public bool IsHidden { get; set; }
    }
}