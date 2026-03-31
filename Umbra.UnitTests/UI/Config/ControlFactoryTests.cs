using Moq;
using Umbra.Config;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.UnitTests;
/// <summary>
/// Unit tests for <see cref = "ControlFactory.CreateControlLayout"/>.
/// </summary>
[TestClass]
public partial class ControlFactoryTests
{
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
        var alignGroup = new LabelAlignmentGroup();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, alignGroup);
        // Assert
        Assert.AreEqual(expectedHiddenLabel, result.HiddenLabel);
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
        var alignGroup = new LabelAlignmentGroup();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, alignGroup);
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
        var alignGroup = new LabelAlignmentGroup();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, alignGroup);
        // Assert
        Assert.AreEqual(expectedHiddenLabel, result.HiddenLabel);
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
        var alignGroup = new LabelAlignmentGroup();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = hiddenLabel,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, alignGroup);
        // Assert
        Assert.AreEqual(hiddenLabel, result.HiddenLabel);
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
        var alignGroup = new LabelAlignmentGroup();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = providedHiddenLabel,
            Description = "Test",
            ControlWidth = 100f
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns(parameterKey);
        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, alignGroup);
        // Assert
        Assert.AreEqual(providedHiddenLabel, result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout falls back to <c>##</c> when both HiddenLabel is null and parameter.Key is null.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithNullParameterKeyAndNullHiddenLabel_UsesDoubleHashFallback()
    {
        // Arrange
        const string label = "Test Label";
        var mockParameter = new Mock<IParameter>();
        var alignGroup = new LabelAlignmentGroup();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = null,
            Description = null,
            ControlWidth = null
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns((string)null!);

        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, alignGroup);

        // Assert
        Assert.AreEqual("##", result.HiddenLabel);
    }

    /// <summary>
    /// Tests that CreateControlLayout preserves metadata HiddenLabel even when parameter.Key is null.
    /// </summary>
    [TestMethod]
    public void CreateControlLayout_WithProvidedHiddenLabelAndNullParameterKey_PreservesMetadataValue()
    {
        // Arrange
        const string label = "Test Label";
        const string hiddenLabel = "##Explicit";
        var mockParameter = new Mock<IParameter>();
        var alignGroup = new LabelAlignmentGroup();
        var metadata = new ParameterMetadata
        {
            HiddenLabel = hiddenLabel,
            Description = null,
            ControlWidth = null
        };
        mockParameter.SetupGet(p => p.Metadata).Returns(metadata);
        mockParameter.SetupGet(p => p.Key).Returns((string)null!);

        // Act
        var result = ControlFactory.CreateControlLayout(label, mockParameter.Object, alignGroup);

        // Assert
        Assert.AreEqual(hiddenLabel, result.HiddenLabel);
    }

    #region Helper Types
    internal class TestCustomDrawer : IParameterDrawer
    {
        public void Draw(string label, IParameter parameter)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
        }
    }

    internal class TestTwoColumnDrawer : ITwoColumnParameterDrawer
    {
        public void Draw(IParameter parameter)
        {
            // No-op for testing
        }

        public void Dispose()
        {
            // No-op for testing
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
