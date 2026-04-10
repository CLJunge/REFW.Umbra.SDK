using System.Numerics;
using Umbra.Config.Attributes;
using Umbra.Config.Validation;


namespace Umbra.Config.UnitTests;

/// <summary>
/// Tests for the <see cref="ParameterMetadataReader"/> class.
/// </summary>
[TestClass]
public class ParameterMetadataReaderTests
{
    /// <summary>
    /// Tests that derived render-cache members on <see cref="ParameterMetadata"/> are not part of
    /// the public package surface.
    /// </summary>
    [TestMethod]
    public void ParameterMetadata_RenderCacheMembers_AreNonPublic()
    {
        Assert.IsNull(typeof(ParameterMetadata).GetProperty(nameof(ParameterMetadata.ResolvedLabel)));
        Assert.IsNull(typeof(ParameterMetadata).GetProperty(nameof(ParameterMetadata.DrawerType)));
        Assert.IsNull(typeof(ParameterMetadata).GetProperty(nameof(ParameterMetadata.TwoColumnDrawerType)));
        Assert.IsNull(typeof(ParameterMetadata).GetProperty(nameof(ParameterMetadata.ValidatorType)));
        Assert.IsNull(typeof(ParameterMetadata).GetProperty(nameof(ParameterMetadata.HideIf)));
        Assert.IsNull(typeof(ParameterMetadata).GetProperty(nameof(ParameterMetadata.InferredFloatFormat)));
        Assert.IsNull(typeof(ParameterMetadata).GetProperty(nameof(ParameterMetadata.HiddenLabel)));
    }

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
        Assert.IsFalse(result.Required);
        Assert.IsFalse(result.AllowWhitespace);
        Assert.IsNull(result.MinLength);
        Assert.IsNull(result.MaxLength);
        Assert.IsNull(result.Min);
        Assert.IsNull(result.Max);
        Assert.IsNull(result.Step);
        Assert.IsNull(result.Category);
        Assert.IsNull(result.Format);
        Assert.IsNull(result.RegexPattern);
        Assert.IsNull(result.RegexMessage);
        Assert.IsNull(result.ButtonStyle);
        Assert.IsNull(result.CustomButtonColors);
        Assert.IsNull(result.ControlWidth);
        Assert.IsNull(result.MultilineLines);
        Assert.IsNull(result.Order);
        Assert.AreEqual(0, result.SpacingBefore);
        Assert.AreEqual(0, result.SpacingAfter);
        Assert.IsNull(result.Indent);
        Assert.IsNull(result.DrawerType);
        Assert.IsNull(result.TwoColumnDrawerType);
        Assert.IsNull(result.HideIf);
        Assert.AreEqual("%.2f", result.InferredFloatFormat);
        Assert.IsNull(result.HiddenLabel);
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
    /// Tests that an explicit category attribute overrides an inherited category.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithCategory_OverridesInheritedCategory()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithCategory))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, inheritedCategory: "Inherited");

        // Assert
        Assert.AreEqual("Test Category", result.Category);
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
    /// Tests that ReadFrom returns a ParameterMetadata with ButtonStyle set from UmbraButtonStyleAttribute.
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
    /// Tests that ReadFrom still prefixes an empty parameter key with the hidden-label marker.
    /// </summary>
    [TestMethod]
    public void ReadFrom_EmptyParameterKey_ReturnsDoubleHashHiddenLabel()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.NoAttributes))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member, parameterKey: string.Empty);

        // Assert
        Assert.AreEqual("##", result.HiddenLabel);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with DrawerType set from ICustomDrawerAttribute.
    /// Input: MemberInfo with an attribute implementing ICustomDrawerAttribute.
    /// Expected: DrawerType = typeof(TestDrawer).
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithCustomDrawer_ReturnsDrawerType()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithCustomDrawer))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(typeof(TestDrawer), result.DrawerType);
    }

    /// <summary>
    /// Tests that ReadFrom returns a ParameterMetadata with TwoColumnDrawerType set from ITwoColumnCustomDrawerAttribute.
    /// Input: MemberInfo with an attribute implementing ITwoColumnCustomDrawerAttribute.
    /// Expected: TwoColumnDrawerType = typeof(TestTwoColumnDrawer).
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithTwoColumnCustomDrawer_ReturnsTwoColumnDrawerType()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithTwoColumnCustomDrawer))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(typeof(TestTwoColumnDrawer), result.TwoColumnDrawerType);
    }

    /// <summary>
    /// Tests that ReadFrom returns required-validation metadata from <see cref="UmbraRequiredAttribute"/>.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithRequired_ReturnsRequiredMetadata()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithRequired))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.IsTrue(result.Required);
        Assert.IsFalse(result.AllowWhitespace);
    }

    /// <summary>
    /// Tests that ReadFrom returns whitespace configuration from <see cref="UmbraRequiredAttribute"/>.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithRequiredAllowWhitespace_ReturnsWhitespaceMetadata()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithRequiredAllowWhitespace))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.IsTrue(result.Required);
        Assert.IsTrue(result.AllowWhitespace);
    }

    /// <summary>
    /// Tests that ReadFrom returns minimum-length metadata from <see cref="UmbraMinLengthAttribute"/>.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithMinLength_ReturnsMinLengthMetadata()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithMinLength))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(3u, result.MinLength);
    }

    /// <summary>
    /// Tests that ReadFrom returns regex metadata from <see cref="UmbraRegexAttribute"/>.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithRegex_ReturnsRegexMetadata()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithRegex))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("^[A-Z]{3}$", result.RegexPattern);
        Assert.AreEqual("Use exactly three uppercase letters.", result.RegexMessage);
    }

    /// <summary>
    /// Tests that ReadFrom returns the custom validator type declared through <see cref="UmbraValidateWithAttribute{TValidator}"/>.
    /// </summary>
    [TestMethod]
    public void ReadFrom_MemberWithCustomValidator_ReturnsValidatorType()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithCustomValidator))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual(typeof(TestValidator), result.ValidatorType);
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
        Assert.IsTrue(result.Required);
        Assert.IsTrue(result.AllowWhitespace);
        Assert.AreEqual(2u, result.MinLength);
        Assert.AreEqual(50u, result.MaxLength);
        Assert.AreEqual(1.0, result.Min);
        Assert.AreEqual(99.0, result.Max);
        Assert.AreEqual(0.25, result.Step);
        Assert.AreEqual("Specific Category", result.Category);
        Assert.AreEqual("%.4f", result.Format);
        Assert.AreEqual("^[a-z]+$", result.RegexPattern);
        Assert.AreEqual("Lowercase letters only.", result.RegexMessage);
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
    /// Tests that negative step values still infer precision from their decimal places.
    /// </summary>
    [TestMethod]
    public void ReadFrom_NegativeStep_ReturnsPrecisionFromDecimalPlaces()
    {
        // Arrange
        var member = typeof(TestClass).GetProperty(nameof(TestClass.WithNegativeStepThreeDecimals))!;

        // Act
        var result = ParameterMetadataReader.ReadFrom(member);

        // Assert
        Assert.AreEqual("%.3f", result.InferredFloatFormat);
    }

    private class TestClass
    {
        public int NoAttributes { get; set; }

        [UmbraDisplayName("Custom Name")]
        public int WithDisplayName { get; set; }

        [UmbraDescription("Test Description")]
        public int WithDescription { get; set; }

        [UmbraMaxLength(100)]
        public string? WithMaxLength { get; set; }

        [UmbraRequired]
        public string? WithRequired { get; set; }

        [UmbraRequired(AllowWhitespace = true)]
        public string? WithRequiredAllowWhitespace { get; set; }

        [UmbraMinLength(3)]
        public string? WithMinLength { get; set; }

        [UmbraRegex("^[A-Z]{3}$", Message = "Use exactly three uppercase letters.")]
        public string? WithRegex { get; set; }

        [UmbraValidateWith<TestValidator>]
        public string? WithCustomValidator { get; set; }

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
        [UmbraRequired(AllowWhitespace = true)]
        [UmbraMinLength(2)]
        [UmbraMaxLength(50)]
        [UmbraRange(1.0, 99.0)]
        [UmbraStep(0.25)]
        [UmbraCategory("Specific Category")]
        [UmbraFormat("%.4f")]
        [UmbraRegex("^[a-z]+$", Message = "Lowercase letters only.")]
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

        [UmbraStep(-0.125)]
        public double WithNegativeStepThreeDecimals { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    private class TestCustomDrawerAttribute : Attribute, IDrawerAttribute
    {
        public Type DrawerType => typeof(TestDrawer);
    }

    [AttributeUsage(AttributeTargets.Property)]
    private class TestTwoColumnCustomDrawerAttribute : Attribute, ITwoColumnDrawerAttribute
    {
        public Type DrawerType => typeof(TestTwoColumnDrawer);
    }

    private sealed class TestValidator : IParameterValidator
    {
        public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
        {
            _ = parameterKey;
            _ = value;
            _ = valueType;
            _ = metadata;
            return ParameterValidationResult.Valid();
        }
    }

    private class TestDrawer { }
    private class TestTwoColumnDrawer { }
}
