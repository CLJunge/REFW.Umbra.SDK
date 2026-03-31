using Umbra.Config;


namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Tests for <see cref="TextControlBuilder"/> methods.
/// </summary>
[TestClass]
public class TextControlBuilderTests
{
    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> returns a non-null Action
    /// when all valid parameters are provided with default metadata.
    /// </summary>
    [TestMethod]
    public void BuildString_ValidParametersWithDefaultMetadata_ReturnsNonNullAction()
    {
        // Arrange
        var label = "Test Label";
        var parameter = new Parameter<string>("default")
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> returns a non-null Action
    /// when MultilineLines is not set (single-line mode).
    /// </summary>
    [TestMethod]
    public void BuildString_SingleLineMode_ReturnsNonNullAction()
    {
        // Arrange
        var label = "Single Line";
        var parameter = new Parameter<string>("test")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = null,
                MaxLength = 128
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> returns a non-null Action
    /// when MultilineLines is set (multiline mode).
    /// </summary>
    [TestMethod]
    public void BuildString_MultilineMode_ReturnsNonNullAction()
    {
        // Arrange
        var label = "Multiline";
        var parameter = new Parameter<string>("test")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = 5,
                MaxLength = 512
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles MaxLength set to null
    /// by defaulting to 256.
    /// </summary>
    [TestMethod]
    public void BuildString_MaxLengthNull_ReturnsNonNullAction()
    {
        // Arrange
        var label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MaxLength = null
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles Parameter.Value being null.
    /// </summary>
    [TestMethod]
    public void BuildString_ParameterValueNull_ReturnsNonNullAction()
    {
        // Arrange
        var label = "Label";
        var parameter = new Parameter<string>(null)
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> accepts a zero max length.
    /// </summary>
    [TestMethod]
    public void BuildString_MaxLengthZero_ReturnsNonNullAction()
    {
        var label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MaxLength = 0
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> accepts zero multiline lines.
    /// </summary>
    [TestMethod]
    public void BuildString_MultilineLinesZero_ReturnsNonNullAction()
    {
        var label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = 0,
                MaxLength = 64
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> accepts negative multiline lines without failing during build.
    /// </summary>
    [TestMethod]
    public void BuildString_NegativeMultilineLines_ReturnsNonNullAction()
    {
        var label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = -2,
                MaxLength = 64
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> accepts very large maximum lengths.
    /// </summary>
    [TestMethod]
    public void BuildString_VeryLargeMaxLength_ReturnsNonNullAction()
    {
        var label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MaxLength = uint.MaxValue
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        var result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        Assert.IsNotNull(result);
    }

}
