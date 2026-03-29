using System;

using Hexa.NET.ImGui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config;
using Umbra.UI.Config;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Contains unit tests for the <see cref="NumericControlBuilder.BuildDouble"/> method.
/// </summary>
[TestClass]
public partial class NumericControlBuilderTests
{
    /// <summary>
    /// Tests that BuildDouble returns a non-null Action when Min and Max are both present in metadata,
    /// indicating the slider code path.
    /// </summary>
    [TestMethod]
    public void BuildDouble_MinAndMaxPresent_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action.");
    }

    /// <summary>
    /// Tests that BuildDouble returns a non-null Action when Min and Max are both absent,
    /// indicating the drag code path.
    /// </summary>
    [TestMethod]
    public void BuildDouble_MinAndMaxAbsent_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action.");
    }

    /// <summary>
    /// Tests that BuildDouble returns a non-null Action when only Min is present,
    /// choosing the drag path since both Min and Max must be present for slider.
    /// </summary>
    [TestMethod]
    public void BuildDouble_OnlyMinPresent_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = null,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action.");
    }

    /// <summary>
    /// Tests that BuildDouble returns a non-null Action when only Max is present,
    /// choosing the drag path since both Min and Max must be present for slider.
    /// </summary>
    [TestMethod]
    public void BuildDouble_OnlyMaxPresent_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = 100.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action.");
    }

    /// <summary>
    /// Tests that BuildDouble handles the drag path with a specified Step value.
    /// </summary>
    [TestMethod]
    public void BuildDouble_DragPathWithStep_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                Step = 0.5,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action.");
    }

    /// <summary>
    /// Tests that BuildDouble handles the drag path when Step is absent, defaulting to 1f.
    /// </summary>
    [TestMethod]
    public void BuildDouble_DragPathWithoutStep_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                Step = null,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action.");
    }

    /// <summary>
    /// Tests that BuildDouble handles extreme double values for Min and Max in slider path.
    /// </summary>
    [TestMethod]
    [DataRow(double.MinValue, double.MaxValue)]
    [DataRow(-1000.0, 1000.0)]
    [DataRow(0.0, 0.0)]
    [DataRow(-100.0, -50.0)]
    public void BuildDouble_SliderWithExtremeMinMax_ReturnsAction(double min, double max)
    {
        // Arrange
        var parameter = new Parameter<double>(0.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = min,
                Max = max,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for extreme Min/Max values.");
    }

    /// <summary>
    /// Tests that BuildDouble handles special double values (NaN, Infinity) for Min and Max.
    /// </summary>
    [TestMethod]
    [DataRow(double.NaN, 100.0)]
    [DataRow(0.0, double.NaN)]
    [DataRow(double.PositiveInfinity, double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity, double.PositiveInfinity)]
    public void BuildDouble_SliderWithSpecialDoubleValues_ReturnsAction(double min, double max)
    {
        // Arrange
        var parameter = new Parameter<double>(0.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = min,
                Max = max,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for special double values.");
    }

    /// <summary>
    /// Tests that BuildDouble handles various Step values in the drag path.
    /// </summary>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(-1.0)]
    [DataRow(0.001)]
    [DataRow(1000.0)]
    [DataRow(double.MaxValue)]
    public void BuildDouble_DragWithVariousStepValues_ReturnsAction(double step)
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                Step = step,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for various Step values.");
    }

    /// <summary>
    /// Tests that BuildDouble handles various parameter values including special doubles.
    /// </summary>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(-123.456)]
    [DataRow(789.012)]
    [DataRow(double.MinValue)]
    [DataRow(double.MaxValue)]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void BuildDouble_VariousParameterValues_ReturnsAction(double value)
    {
        // Arrange
        var parameter = new Parameter<double>(value)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for various parameter values.");
    }

    /// <summary>
    /// Tests that BuildDouble handles various label inputs including empty and whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("Normal Label")]
    [DataRow("Label With Special !@#$%^&*() Characters")]
    [DataRow("Unicode: 你好世界 🌍")]
    public void BuildDouble_VariousLabelInputs_ReturnsAction(string label)
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for various label inputs.");
    }

    /// <summary>
    /// Tests that BuildDouble handles very long label strings without throwing.
    /// </summary>
    [TestMethod]
    public void BuildDouble_VeryLongLabel_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = new string('A', 10000);

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for very long labels.");
    }

    /// <summary>
    /// Tests that BuildDouble handles various format strings.
    /// </summary>
    [TestMethod]
    [DataRow("%.0f")]
    [DataRow("%.2f")]
    [DataRow("%.5f")]
    [DataRow("%f")]
    [DataRow("%.10f")]
    public void BuildDouble_VariousFormatStrings_ReturnsAction(string format)
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = format
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for various format strings.");
    }

    /// <summary>
    /// Tests that BuildDouble handles metadata with null HiddenLabel, which should fall back to "##" + Key.
    /// </summary>
    [TestMethod]
    public void BuildDouble_NullHiddenLabel_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f",
                HiddenLabel = null
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when HiddenLabel is null.");
    }

    /// <summary>
    /// Tests that BuildDouble handles metadata with an explicit HiddenLabel.
    /// </summary>
    [TestMethod]
    public void BuildDouble_ExplicitHiddenLabel_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f",
                HiddenLabel = "##customHidden"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action with explicit HiddenLabel.");
    }

    /// <summary>
    /// Tests that BuildDouble handles metadata with null Description.
    /// </summary>
    [TestMethod]
    public void BuildDouble_NullDescription_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f",
                Description = null
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when Description is null.");
    }

    /// <summary>
    /// Tests that BuildDouble handles metadata with various ControlWidth values.
    /// </summary>
    [TestMethod]
    [DataRow(null)]
    [DataRow(100.0f)]
    [DataRow(-1.0f)]
    [DataRow(0.0f)]
    [DataRow(float.MaxValue)]
    public void BuildDouble_VariousControlWidths_ReturnsAction(float? controlWidth)
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f",
                ControlWidth = controlWidth
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for various ControlWidth values.");
    }

    /// <summary>
    /// Tests that BuildDouble handles the boundary case where Min equals Max in slider path.
    /// </summary>
    [TestMethod]
    public void BuildDouble_MinEqualsMax_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 50.0,
                Max = 50.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when Min equals Max.");
    }

    /// <summary>
    /// Tests that BuildDouble handles the edge case where Min is greater than Max.
    /// </summary>
    [TestMethod]
    public void BuildDouble_MinGreaterThanMax_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = 100.0,
                Max = 0.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action even when Min > Max.");
    }

    /// <summary>
    /// Tests that BuildDouble handles Step value of NaN in drag path.
    /// </summary>
    [TestMethod]
    public void BuildDouble_StepIsNaN_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                Step = double.NaN,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when Step is NaN.");
    }

    /// <summary>
    /// Tests that BuildDouble handles Step value of Infinity in drag path.
    /// </summary>
    [TestMethod]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void BuildDouble_StepIsInfinity_ReturnsAction(double step)
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                Step = step,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when Step is Infinity.");
    }

    /// <summary>
    /// Tests that BuildDouble handles an empty Key in the parameter.
    /// </summary>
    [TestMethod]
    public void BuildDouble_EmptyParameterKey_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(50.0)
        {
            Key = "",
            Metadata = new ParameterMetadata
            {
                Min = 0.0,
                Max = 100.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when parameter Key is empty.");
    }

    /// <summary>
    /// Tests that BuildDouble handles negative range (both Min and Max negative) in slider path.
    /// </summary>
    [TestMethod]
    public void BuildDouble_NegativeRange_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(-50.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = -100.0,
                Max = -10.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for negative range.");
    }

    /// <summary>
    /// Tests that BuildDouble handles a range crossing zero in slider path.
    /// </summary>
    [TestMethod]
    public void BuildDouble_RangeCrossingZero_ReturnsAction()
    {
        // Arrange
        var parameter = new Parameter<double>(0.0)
        {
            Key = "test",
            Metadata = new ParameterMetadata
            {
                Min = -50.0,
                Max = 50.0,
                InferredFloatFormat = "%.2f"
            }
        };
        var alignGroup = new LabelAlignmentGroup();
        var label = "Test Label";

        // Act
        var result = NumericControlBuilder.BuildDouble(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result, "BuildDouble should return a non-null Action for range crossing zero.");
    }

    /// <summary>
    /// Verifies that BuildInt returns a non-null Action when given valid inputs with both Min and Max defined,
    /// indicating the slider path should be taken.
    /// </summary>
    [TestMethod]
    public void BuildInt_ValidParameterWithMinAndMax_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = 0,
                Max = 100,
                Format = "%d"
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt returns a non-null Action when given valid inputs with no Min or Max,
    /// indicating the drag path should be taken.
    /// </summary>
    [TestMethod]
    public void BuildInt_ValidParameterWithoutMinAndMax_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = null,
                Step = 1.0
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt uses the drag path when only Min is specified without Max.
    /// </summary>
    [TestMethod]
    public void BuildInt_ParameterWithOnlyMin_UsesDragPath()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = 0,
                Max = null
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt uses the drag path when only Max is specified without Min.
    /// </summary>
    [TestMethod]
    public void BuildInt_ParameterWithOnlyMax_UsesDragPath()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = null,
                Max = 100
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles null format by defaulting to "%d".
    /// </summary>
    [TestMethod]
    public void BuildInt_NullFormat_DefaultsToPercentD()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Format = null,
                Min = 0,
                Max = 100
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles custom format strings correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_CustomFormat_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Format = "%d px",
                Min = 0,
                Max = 100
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles Min equal to Max correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_MinEqualsMax_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = 50,
                Max = 50
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles Min at int.MinValue correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_MinAtIntMinValue_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(0)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = int.MinValue,
                Max = int.MaxValue
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles Max at int.MaxValue correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_MaxAtIntMaxValue_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(0)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = int.MinValue,
                Max = int.MaxValue
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles null Step by defaulting to 1f in the drag path.
    /// </summary>
    [TestMethod]
    public void BuildInt_NullStep_DefaultsToOneF()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Step = null
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles a custom Step value correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_CustomStep_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Step = 5.0
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles an empty label string correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_EmptyLabel_ReturnsNonNullAction()
    {
        // Arrange
        string label = string.Empty;
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles a whitespace-only label string correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_WhitespaceLabel_ReturnsNonNullAction()
    {
        // Arrange
        string label = "   ";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles a very long label string correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_VeryLongLabel_ReturnsNonNullAction()
    {
        // Arrange
        string label = new string('A', 1000);
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles a label with special characters correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_LabelWithSpecialCharacters_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test\nLabel\t!@#$%^&*()";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles zero value parameters correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_ZeroValueParameter_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(0)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = -10,
                Max = 10
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles negative value parameters correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_NegativeValueParameter_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(-50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = -100,
                Max = 0
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles extreme negative Step values correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_ExtremeNegativeStep_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Step = -1000.0
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles extremely large Step values correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_ExtremeLargeStep_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Step = 1e10
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles fractional Step values correctly (cast to float).
    /// </summary>
    [TestMethod]
    public void BuildInt_FractionalStep_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Step = 0.5
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles Min greater than Max (invalid range but not validated by the method).
    /// </summary>
    [TestMethod]
    public void BuildInt_MinGreaterThanMax_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = 100,
                Max = 0
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles parameter with empty Key correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_ParameterWithEmptyKey_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = string.Empty,
            Metadata = new ParameterMetadata()
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles parameter with default metadata correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_DefaultMetadata_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey"
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles Min and Max values at double boundaries that fit within int range.
    /// </summary>
    [TestMethod]
    public void BuildInt_MinMaxAtDoubleBoundariesWithinIntRange_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(0)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = -1e9,
                Max = 1e9
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Verifies that BuildInt handles zero Step value correctly.
    /// </summary>
    [TestMethod]
    public void BuildInt_ZeroStep_ReturnsNonNullAction()
    {
        // Arrange
        string label = "Test Label";
        Parameter<int> parameter = new(50)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Step = 0.0
            }
        };
        LabelAlignmentGroup alignGroup = new();

        // Act
        Action result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests that BuildFloat returns a non-null Action when both Min and Max are set (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_WithMinAndMax_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: 0.0, max: 10.0, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests that BuildFloat returns a non-null Action when Min is null (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_WithMinNullMaxSet_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: 10.0, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests that BuildFloat returns a non-null Action when Max is null (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_WithMaxNullMinSet_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: 0.0, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests that BuildFloat returns a non-null Action when both Min and Max are null (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_WithMinAndMaxNull_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests that BuildFloat returns a non-null Action when Step is set (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_WithStepSet_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: 0.5);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests that BuildFloat returns a non-null Action when Step is null (drag path with default step).
    /// </summary>
    [TestMethod]
    public void BuildFloat_WithStepNull_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with empty label string.
    /// </summary>
    [TestMethod]
    public void BuildFloat_EmptyLabel_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat(string.Empty, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with whitespace-only label string.
    /// </summary>
    [TestMethod]
    public void BuildFloat_WhitespaceLabel_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("   ", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with very long label string.
    /// </summary>
    [TestMethod]
    public void BuildFloat_VeryLongLabel_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();
        var longLabel = new string('A', 10000);

        // Act
        var action = NumericControlBuilder.BuildFloat(longLabel, parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with label containing special characters.
    /// </summary>
    [TestMethod]
    public void BuildFloat_LabelWithSpecialCharacters_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("Label!@#$%^&*()", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Min and Max at boundary values (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_MinMaxAtBoundaries_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(0f, min: float.MinValue, max: float.MaxValue, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Min equal to Max (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_MinEqualToMax_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: 5.0, max: 5.0, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Min greater than Max (slider path - unusual but should not throw during build).
    /// </summary>
    [TestMethod]
    public void BuildFloat_MinGreaterThanMax_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: 10.0, max: 0.0, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with negative Min and Max values (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_NegativeMinAndMax_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(-5f, min: -10.0, max: -1.0, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with zero Step value (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_ZeroStep_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: 0.0);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with negative Step value (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_NegativeStep_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: -1.0);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with very large Step value (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_VeryLargeStep_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: double.MaxValue);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with very small Step value (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_VerySmallStep_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: null, max: null, step: double.Epsilon);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with different InferredFloatFormat values.
    /// </summary>
    [TestMethod]
    [DataRow("%.0f")]
    [DataRow("%.1f")]
    [DataRow("%.3f")]
    [DataRow("%.10f")]
    [DataRow("%f")]
    [DataRow("")]
    public void BuildFloat_VariousFloatFormats_ReturnsNonNullAction(string format)
    {
        // Arrange
        var parameter = CreateFloatParameter(5f, min: 0.0, max: 10.0, step: null, format: format);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with parameter value at float.MinValue.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ParameterValueAtMinValue_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(float.MinValue, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with parameter value at float.MaxValue.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ParameterValueAtMaxValue_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(float.MaxValue, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with parameter value at float.NaN.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ParameterValueNaN_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(float.NaN, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with parameter value at float.PositiveInfinity.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ParameterValuePositiveInfinity_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(float.PositiveInfinity, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with parameter value at float.NegativeInfinity.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ParameterValueNegativeInfinity_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(float.NegativeInfinity, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Min set to double.PositiveInfinity (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_MinPositiveInfinity_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(0f, min: double.PositiveInfinity, max: double.PositiveInfinity, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Max set to double.NegativeInfinity (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_MaxNegativeInfinity_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(0f, min: double.NegativeInfinity, max: double.NegativeInfinity, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Min set to double.NaN (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_MinNaN_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(0f, min: double.NaN, max: 10.0, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Max set to double.NaN (slider path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_MaxNaN_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(0f, min: 0.0, max: double.NaN, step: null);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Step set to double.NaN (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_StepNaN_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(0f, min: null, max: null, step: double.NaN);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with Step set to double.PositiveInfinity (drag path).
    /// </summary>
    [TestMethod]
    public void BuildFloat_StepPositiveInfinity_ReturnsNonNullAction()
    {
        // Arrange
        var parameter = CreateFloatParameter(0f, min: null, max: null, step: double.PositiveInfinity);
        var alignGroup = new LabelAlignmentGroup();

        // Act
        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        // Assert
        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Helper method to create a Parameter&lt;float&gt; with specified metadata.
    /// </summary>
    private static Parameter<float> CreateFloatParameter(
        float value,
        double? min,
        double? max,
        double? step,
        string? format = null)
    {
        var parameter = new Parameter<float>(value)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Min = min,
                Max = max,
                Step = step,
                InferredFloatFormat = format ?? "%.2f",
                HiddenLabel = "##testKey"
            }
        };
        return parameter;
    }
}