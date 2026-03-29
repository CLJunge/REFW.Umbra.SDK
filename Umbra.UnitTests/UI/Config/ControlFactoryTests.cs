using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Umbra.Config;
using Umbra.UI.Config;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.UnitTests;
/// <summary>
/// Unit tests for <see cref = "ControlFactory.CreateControlLayout"/>.
/// </summary>
[TestClass]
public partial class ControlFactoryTests
{
    /// <summary>
    /// Tests that CreateControlLayout returns a ControlLayout with correct values when all properties are provided.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithAllPropertiesProvided_ReturnsCorrectLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string description = "Test Description";
        const string hiddenLabel = "##CustomHidden";
        const float controlWidth = 200f;
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            Description = description,
            HiddenLabel = hiddenLabel,
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(hiddenLabel, result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout uses parameter.Key to build hidden label when HiddenLabel is null.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithNullHiddenLabel_UsesParameterKeyForHiddenLabel()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        const string expectedHiddenLabel = "##testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.AreEqual(expectedHiddenLabel, result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout defaults to -1f control width when ControlWidth is null.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithNullControlWidth_DefaultsToNegativeOne()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = null
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout correctly handles null description.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithNullDescription_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = null,
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles empty string label correctly.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithEmptyLabel_ReturnsValidLayout()
    {
        // Arrange
        const string label = "";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles whitespace-only label correctly.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithWhitespaceLabel_ReturnsValidLayout()
    {
        // Arrange
        const string label = "   ";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles very long label correctly.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithVeryLongLabel_ReturnsValidLayout()
    {
        // Arrange
        var label = new string ('A', 10000);
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles empty parameter key when HiddenLabel is null.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithEmptyParameterKeyAndNullHiddenLabel_ProducesCorrectHiddenLabel()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "";
        const string expectedHiddenLabel = "##";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.AreEqual(expectedHiddenLabel, result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles special characters in parameter key.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithSpecialCharactersInParameterKey_BuildsCorrectHiddenLabel()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "test.key/with\\special@chars";
        var expectedHiddenLabel = "##" + parameterKey;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.AreEqual(expectedHiddenLabel, result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles zero control width.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithZeroControlWidth_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        const float controlWidth = 0f;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles maximum float control width.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithMaxFloatControlWidth_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var controlWidth = float.MaxValue;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles minimum float control width.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithMinFloatControlWidth_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var controlWidth = float.MinValue;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles NaN control width.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithNaNControlWidth_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var controlWidth = float.NaN;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles positive infinity control width.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithPositiveInfinityControlWidth_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var controlWidth = float.PositiveInfinity;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles negative infinity control width.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithNegativeInfinityControlWidth_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var controlWidth = float.NegativeInfinity;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles negative control width.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithNegativeControlWidth_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        const float controlWidth = -100f;
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = controlWidth
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles empty HiddenLabel value.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithEmptyHiddenLabel_UsesEmptyHiddenLabel()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        const string hiddenLabel = "";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = hiddenLabel,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.AreEqual(hiddenLabel, result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout correctly passes label to ControlLayout constructor.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_PassesLabelToConstructor_ConstructorReceivesCorrectLabel()
    {
        // Arrange
        const string label = "My Custom Label";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout correctly passes description to ControlLayout constructor.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_PassesDescriptionToConstructor_ConstructorReceivesCorrectDescription()
    {
        // Arrange
        const string label = "Test Label";
        const string description = "This is a detailed description";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = description,
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout correctly passes alignGroup to ControlLayout constructor.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_PassesAlignGroupToConstructor_ConstructorReceivesCorrectAlignGroup()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout uses HiddenLabel from metadata when it is provided.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithProvidedHiddenLabel_PrefersMetadataHiddenLabel()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "testKey";
        const string providedHiddenLabel = "##CustomLabel";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = providedHiddenLabel,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.AreEqual(providedHiddenLabel, result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles very long description correctly.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithVeryLongDescription_ReturnsValidLayout()
    {
        // Arrange
        const string label = "Test Label";
        var description = new string ('D', 50000);
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = description,
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles Unicode characters in label.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithUnicodeCharactersInLabel_ReturnsValidLayout()
    {
        // Arrange
        const string label = "测试标签 🎮 Тест";
        const string parameterKey = "testKey";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = "##test",
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that CreateControlLayout handles null-coalescing for both HiddenLabel and ControlWidth in same call.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithBothNullValues_UsesDefaultsForBoth()
    {
        // Arrange
        const string label = "Test Label";
        const string parameterKey = "myParam";
        const string expectedHiddenLabel = "##myParam";
        var mockParameter = new Mock<IParameter>();
        var mockAlignGroup = new Mock<LabelAlignmentGroup>();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = null,
            ControlWidth = null
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, mockAlignGroup.Object);
        // Assert
        Assert.AreEqual(expectedHiddenLabel, result.HiddenLabel);
        Assert.IsNotNull(result);
    }

#region Helper Types
    internal class TestCustomDrawer : IParameterDrawer
    {
        public void Draw(string label, IParameter parameter)
        {
        }

        public void Dispose()
        {
        }
    }

    internal class TestTwoColumnDrawer : ITwoColumnParameterDrawer
    {
        public void Draw(IParameter parameter)
        {
        }

        public void Dispose()
        {
        }
    }

    internal enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    internal class TestCustomClass
    {
        public string? Value { get; set; }
    }
#endregion
}