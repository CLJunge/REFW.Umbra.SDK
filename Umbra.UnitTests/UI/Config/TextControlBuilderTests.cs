using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config;
using Umbra.UI.Config;


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
        string label = "Test Label";
        var parameter = new Parameter<string>("default")
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

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
        string label = "Single Line";
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
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

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
        string label = "Multiline";
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
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

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
        string label = "Label";
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
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles MaxLength set to zero.
    /// </summary>
    [TestMethod]
    public void BuildString_MaxLengthZero_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MaxLength = 0
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles MaxLength set to uint.MaxValue.
    /// </summary>
    [TestMethod]
    public void BuildString_MaxLengthMaxValue_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MaxLength = uint.MaxValue
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles MultilineLines set to 0.
    /// </summary>
    [TestMethod]
    public void BuildString_MultilineLinesZero_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = 0
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles MultilineLines set to int.MaxValue.
    /// </summary>
    [TestMethod]
    public void BuildString_MultilineLinesMaxValue_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = int.MaxValue
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles MultilineLines set to a negative value.
    /// </summary>
    [TestMethod]
    public void BuildString_MultilineLinesNegative_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = -10
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

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
        string label = "Label";
        var parameter = new Parameter<string>(null)
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles Parameter.Value being empty string.
    /// </summary>
    [TestMethod]
    public void BuildString_ParameterValueEmpty_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>(string.Empty)
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles Parameter.Value being a very long string.
    /// </summary>
    [TestMethod]
    public void BuildString_ParameterValueVeryLong_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        string longValue = new string('x', 10000);
        var parameter = new Parameter<string>(longValue)
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles label being null.
    /// </summary>
    [TestMethod]
    public void BuildString_LabelNull_ReturnsNonNullAction()
    {
        // Arrange
        string? label = null;
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label!, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles label being empty string.
    /// </summary>
    [TestMethod]
    public void BuildString_LabelEmpty_ReturnsNonNullAction()
    {
        // Arrange
        string label = string.Empty;
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles label being whitespace only.
    /// </summary>
    [TestMethod]
    public void BuildString_LabelWhitespace_ReturnsNonNullAction()
    {
        // Arrange
        string label = "   \t\n  ";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles label being a very long string.
    /// </summary>
    [TestMethod]
    public void BuildString_LabelVeryLong_ReturnsNonNullAction()
    {
        // Arrange
        string label = new string('A', 5000);
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles label with special characters.
    /// </summary>
    [TestMethod]
    public void BuildString_LabelWithSpecialCharacters_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test##Label##With##Special<>Characters!@#$%^&*()";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles Parameter.Value
    /// containing Unicode characters.
    /// </summary>
    [TestMethod]
    public void BuildString_ParameterValueWithUnicode_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Unicode Label 日本語";
        var parameter = new Parameter<string>("Test 中文 العربية 🎉")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles Parameter.Value
    /// containing control characters.
    /// </summary>
    [TestMethod]
    public void BuildString_ParameterValueWithControlCharacters_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>("Test\0\r\n\t\b")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles combination of
    /// multiline mode with custom MaxLength.
    /// </summary>
    [TestMethod]
    public void BuildString_MultilineWithCustomMaxLength_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Multiline";
        var parameter = new Parameter<string>("test content")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = 3,
                MaxLength = 1024
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles single-line mode
    /// with custom MaxLength.
    /// </summary>
    [TestMethod]
    public void BuildString_SingleLineWithCustomMaxLength_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Single";
        var parameter = new Parameter<string>("test")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                MultilineLines = null,
                MaxLength = 64
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles Parameter.Value
    /// being whitespace only.
    /// </summary>
    [TestMethod]
    public void BuildString_ParameterValueWhitespace_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Label";
        var parameter = new Parameter<string>("   \t\n   ")
        {
            Key = "key",
            Metadata = new ParameterMetadata()
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that <see cref="TextControlBuilder.BuildString"/> handles Parameter with
    /// all metadata fields populated.
    /// </summary>
    [TestMethod]
    public void BuildString_AllMetadataFieldsPopulated_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Full Metadata";
        var parameter = new Parameter<string>("value")
        {
            Key = "key",
            Metadata = new ParameterMetadata
            {
                Category = "Test Category",
                DisplayName = "Display Name",
                Description = "Test description",
                MaxLength = 512,
                MultilineLines = 4,
                ResolvedLabel = "Resolved Label"
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        Action result = TextControlBuilder.BuildString(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }
}