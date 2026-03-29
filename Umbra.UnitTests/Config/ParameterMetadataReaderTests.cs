using System;
using System.Numerics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra;
using Umbra.Config;
using Umbra.Config.Attributes;


namespace Umbra.Config.UnitTests;

/// <summary>
/// Tests for the <see cref="ParameterMetadataReader"/> class.
/// </summary>
[TestClass]
public class ParameterMetadataReaderTests
{
    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with default values when the member has no attributes.
    /// Input: MemberInfo with no custom attributes, null inheritedCategory, null parameterKey.
    /// Expected: ParameterMetadata with null/default values, ResolvedLabel from member name, SpacingBefore/After = 0, InferredFloatFormat = "%.2f".
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithNoAttributes_ReturnsDefaultMetadata()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.IsNull(result.DisplayName);
        Assert.AreEqual("No Attributes", result.ResolvedLabel);
        Assert.IsNull(result.Description);
        Assert.IsNull(result.MaxLength);
        Assert.IsNull(result.Min);
        Assert.IsNull(result.Max);
        Assert.IsNull(result.Step);
        Assert.IsNull(result.Category);
        Assert.IsNull(result.Format);
        Assert.IsNull(result.ButtonStyle);
        Assert.IsNull(result.CustomButtonColors);
        Assert.IsNull(result.ControlWidth);
        Assert.IsNull(result.MultilineLines);
        Assert.IsNull(result.Order);
        Assert.AreEqual(0, result.SpacingBefore);
        Assert.AreEqual(0, result.SpacingAfter);
        Assert.IsNull(result.Indent);
        Assert.IsNull(result.CustomDrawerType);
        Assert.IsNull(result.TwoColumnCustomDrawerType);
        Assert.IsNull(result.HideIf);
        Assert.AreEqual("%.2f", result.InferredFloatFormat);
        Assert.IsNull(result.HiddenLabel);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with DisplayName and ResolvedLabel set from UmbraDisplayNameAttribute.
    /// Input: MemberInfo with UmbraDisplayNameAttribute.
    /// Expected: DisplayName = "Custom Name", ResolvedLabel = "Custom Name".
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithDisplayName_ReturnsDisplayNameAndResolvedLabel()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithDisplayName))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("Custom Name", result.DisplayName);
        Assert.AreEqual("Custom Name", result.ResolvedLabel);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Description set from UmbraDescriptionAttribute.
    /// Input: MemberInfo with UmbraDescriptionAttribute.
    /// Expected: Description = "Test Description".
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithDescription_ReturnsDescription()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithDescription))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("Test Description", result.Description);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with MaxLength set from UmbraMaxLengthAttribute.
    /// Input: MemberInfo with UmbraMaxLengthAttribute(100).
    /// Expected: MaxLength = 100.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithMaxLength_ReturnsMaxLength()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithMaxLength))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(100u, result.MaxLength);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Min and Max set from UmbraRangeAttribute.
    /// Input: MemberInfo with UmbraRangeAttribute(0.0, 100.0).
    /// Expected: Min = 0.0, Max = 100.0.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithRange_ReturnsMinAndMax()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithRange))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(0.0, result.Min);
        Assert.AreEqual(100.0, result.Max);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Step set from UmbraStepAttribute.
    /// Input: MemberInfo with UmbraStepAttribute(0.5).
    /// Expected: Step = 0.5.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithStep_ReturnsStep()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithStep))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(0.5, result.Step);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Category set from UmbraCategoryAttribute.
    /// Input: MemberInfo with UmbraCategoryAttribute("Test Category").
    /// Expected: Category = "Test Category".
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithCategory_ReturnsCategory()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithCategory))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("Test Category", result.Category);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Category set from inheritedCategory when no UmbraCategoryAttribute is present.
    /// Input: MemberInfo with no UmbraCategoryAttribute, inheritedCategory = "Inherited".
    /// Expected: Category = "Inherited".
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithoutCategoryAndInheritedCategoryProvided_ReturnsInheritedCategory()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, inheritedCategory: "Inherited");

        // Assert
        Assert.AreEqual("Inherited", result.Category);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Format set from UmbraFormatAttribute.
    /// Input: MemberInfo with UmbraFormatAttribute("%.3f").
    /// Expected: Format = "%.3f", InferredFloatFormat = "%.3f".
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithFormat_ReturnsFormatAndInferredFormat()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithFormat))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("%.3f", result.Format);
        Assert.AreEqual("%.3f", result.InferredFloatFormat);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with InferredFloatFormat derived from UmbraStepAttribute when no UmbraFormatAttribute is present.
    /// Input: MemberInfo with UmbraStepAttribute(0.5), no UmbraFormatAttribute.
    /// Expected: InferredFloatFormat = "%.1f" (derived from step 0.5).
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithStepAndNoFormat_ReturnsInferredFormatFromStep()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithStep))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("%.1f", result.InferredFloatFormat);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with ButtonStyle set from UmbraButtonStyleAttribute.
    /// Input: MemberInfo with UmbraButtonStyleAttribute(ButtonStyle.Primary).
    /// Expected: ButtonStyle = ButtonStyle.Primary.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithButtonStyle_ReturnsButtonStyle()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithButtonStyle))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(ButtonStyle.Primary, result.ButtonStyle);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with CustomButtonColors set from UmbraCustomButtonColorsAttribute.
    /// Input: MemberInfo with UmbraCustomButtonColorsAttribute.
    /// Expected: CustomButtonColors is a tuple of three Vector4 values.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithCustomButtonColors_ReturnsCustomButtonColors()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithCustomButtonColors))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.IsNotNull(result.CustomButtonColors);
        var (normal, hovered, active) = result.CustomButtonColors.Value;
        Assert.AreEqual(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), normal);
        Assert.AreEqual(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), hovered);
        Assert.AreEqual(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), active);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with ControlWidth set from UmbraControlWidthAttribute.
    /// Input: MemberInfo with UmbraControlWidthAttribute(200.0f).
    /// Expected: ControlWidth = 200.0f.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithControlWidth_ReturnsControlWidth()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithControlWidth))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(200.0f, result.ControlWidth);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with MultilineLines set from UmbraMultilineAttribute.
    /// Input: MemberInfo with UmbraMultilineAttribute(5).
    /// Expected: MultilineLines = 5.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithMultiline_ReturnsMultilineLines()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithMultiline))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(5, result.MultilineLines);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Order set from UmbraParameterOrderAttribute.
    /// Input: MemberInfo with UmbraParameterOrderAttribute(10).
    /// Expected: Order = 10.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithOrder_ReturnsOrder()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithOrder))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(10, result.Order);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with SpacingBefore set from UmbraSpacingBeforeAttribute.
    /// Input: MemberInfo with UmbraSpacingBeforeAttribute(3).
    /// Expected: SpacingBefore = 3.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithSpacingBefore_ReturnsSpacingBefore()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithSpacingBefore))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(3, result.SpacingBefore);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with SpacingAfter set from UmbraSpacingAfterAttribute.
    /// Input: MemberInfo with UmbraSpacingAfterAttribute(2).
    /// Expected: SpacingAfter = 2.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithSpacingAfter_ReturnsSpacingAfter()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithSpacingAfter))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(2, result.SpacingAfter);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with Indent set from UmbraIndentAttribute.
    /// Input: MemberInfo with UmbraIndentAttribute(20.0f).
    /// Expected: Indent = 20.0f.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithIndent_ReturnsIndent()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithIndent))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(20.0f, result.Indent);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with HiddenLabel set when parameterKey is provided.
    /// Input: MemberInfo, parameterKey = "testKey".
    /// Expected: HiddenLabel = "##testKey".
    /// </summary>
    [TestMethod]
    public void ReadFrom_ParameterKeyProvided_ReturnsHiddenLabel()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, parameterKey: "testKey");

        // Assert
        Assert.AreEqual("##testKey", result.HiddenLabel);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with null HiddenLabel when parameterKey is null.
    /// Input: MemberInfo, parameterKey = null.
    /// Expected: HiddenLabel = null.
    /// </summary>
    [TestMethod]
    public void ReadFrom_ParameterKeyNull_ReturnsNullHiddenLabel()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, parameterKey: null);

        // Assert
        Assert.IsNull(result.HiddenLabel);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with CustomDrawerType set from ICustomDrawerAttribute.
    /// Input: MemberInfo with an attribute implementing ICustomDrawerAttribute.
    /// Expected: CustomDrawerType = typeof(TestDrawer).
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithCustomDrawer_ReturnsCustomDrawerType()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithCustomDrawer))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(typeof(TestDrawer), result.CustomDrawerType);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with TwoColumnCustomDrawerType set from ITwoColumnCustomDrawerAttribute.
    /// Input: MemberInfo with an attribute implementing ITwoColumnCustomDrawerAttribute.
    /// Expected: TwoColumnCustomDrawerType = typeof(TestTwoColumnDrawer).
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithTwoColumnCustomDrawer_ReturnsTwoColumnCustomDrawerType()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithTwoColumnCustomDrawer))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(typeof(TestTwoColumnDrawer), result.TwoColumnCustomDrawerType);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with all properties set when member has all attributes.
    /// Input: MemberInfo with all supported attributes.
    /// Expected: All properties are set correctly.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithAllAttributes_ReturnsCompleteMetadata()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithAllAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, inheritedCategory: "Inherited", parameterKey: "key");

        // Assert
        Assert.AreEqual("All Attributes", result.DisplayName);
        Assert.AreEqual("All Attributes", result.ResolvedLabel);
        Assert.AreEqual("Full Description", result.Description);
        Assert.AreEqual(50u, result.MaxLength);
        Assert.AreEqual(1.0, result.Min);
        Assert.AreEqual(99.0, result.Max);
        Assert.AreEqual(0.25, result.Step);
        Assert.AreEqual("Specific Category", result.Category);
        Assert.AreEqual("%.4f", result.Format);
        Assert.AreEqual(ButtonStyle.Danger, result.ButtonStyle);
        Assert.AreEqual(150.0f, result.ControlWidth);
        Assert.AreEqual(4, result.MultilineLines);
        Assert.AreEqual(5, result.Order);
        Assert.AreEqual(1, result.SpacingBefore);
        Assert.AreEqual(1, result.SpacingAfter);
        Assert.AreEqual(15.0f, result.Indent);
        Assert.AreEqual("%.4f", result.InferredFloatFormat);
        Assert.AreEqual("##key", result.HiddenLabel);
    }

    /// <summary>
    /// Tests that ReadFrom returns "%.2f" as InferredFloatFormat when step is null.
    /// Input: MemberInfo with no UmbraStepAttribute and no UmbraFormatAttribute.
    /// Expected: InferredFloatFormat = "%.2f".
    /// </summary>
    [TestMethod]
    public void ReadFrom_NoStepAndNoFormat_ReturnsDefaultInferredFormat()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("%.2f", result.InferredFloatFormat);
    }

    /// <summary>
    /// Tests that ReadFrom returns "%.2f" as InferredFloatFormat when step is zero.
    /// Input: MemberInfo with UmbraStepAttribute(0.0).
    /// Expected: InferredFloatFormat = "%.2f".
    /// </summary>
    [TestMethod]
    public void ReadFrom_StepIsZero_ReturnsDefaultInferredFormat()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithStepZero))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("%.2f", result.InferredFloatFormat);
    }

    /// <summary>
    /// Tests that ReadFrom returns "%.0f" as InferredFloatFormat when step is an integer.
    /// Input: MemberInfo with UmbraStepAttribute(1.0).
    /// Expected: InferredFloatFormat = "%.0f".
    /// </summary>
    [TestMethod]
    public void ReadFrom_StepIsInteger_ReturnsIntegerFormat()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithStepInteger))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("%.0f", result.InferredFloatFormat);
    }

    /// <summary>
    /// Tests that ReadFrom returns correct InferredFloatFormat when step has multiple decimal places.
    /// Input: MemberInfo with UmbraStepAttribute(0.001).
    /// Expected: InferredFloatFormat = "%.3f".
    /// </summary>
    [TestMethod]
    public void ReadFrom_StepWithMultipleDecimalPlaces_ReturnsCorrectFormat()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithStepThreeDecimals))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("%.3f", result.InferredFloatFormat);
    }

    /// <summary>
    /// Tests that ReadFrom correctly handles empty parameterKey string.
    /// Input: MemberInfo, parameterKey = "".
    /// Expected: HiddenLabel = "##".
    /// </summary>
    [TestMethod]
    public void ReadFrom_EmptyParameterKey_ReturnsHiddenLabelWithHashOnly()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, parameterKey: "");

        // Assert
        Assert.AreEqual("##", result.HiddenLabel);
    }

    /// <summary>
    /// Tests that ReadFrom correctly handles whitespace parameterKey.
    /// Input: MemberInfo, parameterKey = "  ".
    /// Expected: HiddenLabel = "##  ".
    /// </summary>
    [TestMethod]
    public void ReadFrom_WhitespaceParameterKey_ReturnsHiddenLabelWithWhitespace()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, parameterKey: "  ");

        // Assert
        Assert.AreEqual("##  ", result.HiddenLabel);
    }

    // Test helper classes with various attribute combinations
    private class TestClass
    {
        public int NoAttributes { get; set; }

        [UmbraDisplayName("Custom Name")]
        public int WithDisplayName { get; set; }

        [UmbraDescription("Test Description")]
        public int WithDescription { get; set; }

        [UmbraMaxLength(100)]
        public string? WithMaxLength { get; set; }

        [UmbraRange(0.0, 100.0)]
        public double WithRange { get; set; }

        [UmbraStep(0.5)]
        public double WithStep { get; set; }

        [UmbraCategory("Test Category")]
        public int WithCategory { get; set; }

        [UmbraFormat("%.3f")]
        public double WithFormat { get; set; }

        [UmbraButtonStyle(ButtonStyle.Primary)]
        public int WithButtonStyle { get; set; }

        [UmbraCustomButtonColors(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f)]
        public int WithCustomButtonColors { get; set; }

        [UmbraControlWidth(200.0f)]
        public int WithControlWidth { get; set; }

        [UmbraMultiline(5)]
        public string? WithMultiline { get; set; }

        [UmbraParameterOrder(10)]
        public int WithOrder { get; set; }

        [UmbraSpacingBefore(3)]
        public int WithSpacingBefore { get; set; }

        [UmbraSpacingAfter(2)]
        public int WithSpacingAfter { get; set; }

        [UmbraIndent(20.0f)]
        public int WithIndent { get; set; }

        [TestCustomDrawer]
        public int WithCustomDrawer { get; set; }

        [TestTwoColumnCustomDrawer]
        public int WithTwoColumnCustomDrawer { get; set; }

        public int WithHideIf { get; set; }

        [UmbraDisplayName("All Attributes")]
        [UmbraDescription("Full Description")]
        [UmbraMaxLength(50)]
        [UmbraRange(1.0, 99.0)]
        [UmbraStep(0.25)]
        [UmbraCategory("Specific Category")]
        [UmbraFormat("%.4f")]
        [UmbraButtonStyle(ButtonStyle.Danger)]
        [UmbraControlWidth(150.0f)]
        [UmbraMultiline(4)]
        [UmbraParameterOrder(5)]
        [UmbraSpacingBefore(1)]
        [UmbraSpacingAfter(1)]
        [UmbraIndent(15.0f)]
        public string? WithAllAttributes { get; set; }

        [UmbraStep(0.0)]
        public double WithStepZero { get; set; }

        [UmbraStep(1.0)]
        public double WithStepInteger { get; set; }

        [UmbraStep(0.001)]
        public double WithStepThreeDecimals { get; set; }
    }

    // Helper attribute classes for testing interface-based detection
    [AttributeUsage(AttributeTargets.Property)]
    private class TestCustomDrawerAttribute : Attribute, ICustomDrawerAttribute
    {
        public Type DrawerType => typeof(TestDrawer);
    }

    [AttributeUsage(AttributeTargets.Property)]
    private class TestTwoColumnCustomDrawerAttribute : Attribute, ITwoColumnCustomDrawerAttribute
    {
        public Type DrawerType => typeof(TestTwoColumnDrawer);
    }

    // Dummy drawer types for testing
    private class TestDrawer { }
    private class TestTwoColumnDrawer { }
}