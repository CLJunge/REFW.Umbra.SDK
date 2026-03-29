using Moq;
using Umbra.Config;


namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for the <see cref="EnumControlBuilder"/> class.
/// </summary>
[TestClass]
public class EnumControlBuilderTests
{
    /// <summary>
    /// Test enum for non-nullable scenarios.
    /// </summary>
    private enum TestEnum
    {
        First,
        Second,
        Third
    }

    /// <summary>
    /// Test enum with single value.
    /// </summary>
    private enum SingleValueEnum
    {
        OnlyValue
    }

    /// <summary>
    /// Tests that Build returns a non-null Action when given a non-nullable enum parameter.
    /// </summary>
    [TestMethod]
    public void Build_WithNonNullableEnum_ReturnsNonNullAction()
    {
        // Arrange
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.ValueType).Returns(typeof(TestEnum));
        mockParameter.Setup(p => p.GetValue()).Returns(TestEnum.First);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var action = EnumControlBuilder.Build(label, mockParameter.Object, alignGroup);

        // Assert
        Assert.IsNotNull(action, "Build should return a non-null Action.");
    }

    /// <summary>
    /// Tests that Build returns a non-null Action when given a nullable enum parameter.
    /// </summary>
    [TestMethod]
    public void Build_WithNullableEnum_ReturnsNonNullAction()
    {
        // Arrange
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.ValueType).Returns(typeof(TestEnum?));
        mockParameter.Setup(p => p.GetValue()).Returns(TestEnum.First);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var action = EnumControlBuilder.Build(label, mockParameter.Object, alignGroup);

        // Assert
        Assert.IsNotNull(action, "Build should return a non-null Action for nullable enum.");
    }

    /// <summary>
    /// Tests that Build handles a single-value enum correctly.
    /// </summary>
    [TestMethod]
    public void Build_WithSingleValueEnum_ReturnsAction()
    {
        // Arrange
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.ValueType).Returns(typeof(SingleValueEnum));
        mockParameter.Setup(p => p.GetValue()).Returns(SingleValueEnum.OnlyValue);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Single Value Label";

        // Act
        var action = EnumControlBuilder.Build(label, mockParameter.Object, alignGroup);

        // Assert
        Assert.IsNotNull(action, "Build should return a non-null Action for single-value enum.");
    }

    /// <summary>
    /// Tests that the returned Action does not throw when the parameter returns a value
    /// not present in the enum values array (edge case: parameter value out of sync).
    /// Note: Full test execution requires ImGui context; this test verifies construction only.
    /// </summary>
    [TestMethod]
    public void Build_WithInvalidEnumValue_ReturnsAction()
    {
        // Arrange
        var mockParameter = new Mock<IParameter>();
        mockParameter.Setup(p => p.ValueType).Returns(typeof(TestEnum));
        // Return an invalid cast enum value that's not in the defined enum
        mockParameter.Setup(p => p.GetValue()).Returns((TestEnum)999);
        mockParameter.Setup(p => p.Key).Returns("testKey");
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var action = EnumControlBuilder.Build(label, mockParameter.Object, alignGroup);

        // Assert
        Assert.IsNotNull(action, "Build should return a non-null Action even with invalid enum value.");

        // Note: When the action executes, idx would be -1 and gets set to 0.
        // Full execution test requires ImGui context and is not feasible here.
    }

}
