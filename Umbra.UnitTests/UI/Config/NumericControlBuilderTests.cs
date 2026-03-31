using Umbra.Config;

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
    /// Tests that BuildDouble accepts a parameter value of <see cref="double.NaN"/>.
    /// </summary>
    [TestMethod]
    public void BuildDouble_ValueIsNaN_ReturnsAction()
    {
        var parameter = new Parameter<double>(double.NaN)
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

        var result = NumericControlBuilder.BuildDouble("Test Label", parameter, alignGroup);

        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when the value is NaN.");
    }

    /// <summary>
    /// Tests that BuildDouble accepts a parameter value of positive infinity.
    /// </summary>
    [TestMethod]
    public void BuildDouble_ValueIsPositiveInfinity_ReturnsAction()
    {
        var parameter = new Parameter<double>(double.PositiveInfinity)
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

        var result = NumericControlBuilder.BuildDouble("Test Label", parameter, alignGroup);

        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when the value is positive infinity.");
    }

    /// <summary>
    /// Tests that BuildDouble accepts a parameter value of negative infinity.
    /// </summary>
    [TestMethod]
    public void BuildDouble_ValueIsNegativeInfinity_ReturnsAction()
    {
        var parameter = new Parameter<double>(double.NegativeInfinity)
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

        var result = NumericControlBuilder.BuildDouble("Test Label", parameter, alignGroup);

        Assert.IsNotNull(result, "BuildDouble should return a non-null Action when the value is negative infinity.");
    }

    /// <summary>
    /// Verifies that BuildInt returns a non-null Action when given valid inputs with both Min and Max defined,
    /// indicating the slider path should be taken.
    /// </summary>
    [TestMethod]
    public void BuildInt_ValidParameterWithMinAndMax_ReturnsNonNullAction()
    {
        // Arrange
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
        var label = "Test Label";
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
        var result = NumericControlBuilder.BuildInt(label, parameter, alignGroup);

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
    /// Tests BuildFloat with a parameter value of positive infinity.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ValuePositiveInfinity_ReturnsNonNullAction()
    {
        var parameter = CreateFloatParameter(float.PositiveInfinity, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

        Assert.IsNotNull(action);
    }

    /// <summary>
    /// Tests BuildFloat with a parameter value of NaN.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ValueNaN_ReturnsNonNullAction()
    {
        var parameter = CreateFloatParameter(float.NaN, min: null, max: null, step: null);
        var alignGroup = new LabelAlignmentGroup();

        var action = NumericControlBuilder.BuildFloat("label", parameter, alignGroup);

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
    /// Tests BuildFloat with a NaN value in the drag path.
    /// </summary>
    [TestMethod]
    public void BuildFloat_ValueIsNaN_ReturnsNonNullAction()
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
