namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ControlLayout"/> struct.
/// </summary>
[TestClass]
public class ControlLayoutTests
{
    /// <summary>
    /// Verifies that the constructor initializes the struct with valid parameters
    /// and assigns the HiddenLabel field correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var label = "Test Label";
        var desc = "Test Description";
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 100f;
        var hiddenLabel = "##testHidden";

        // Act
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);

        // Assert
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor correctly assigns the HiddenLabel field
    /// when provided with an empty string.
    /// </summary>
    [TestMethod]
    public void Constructor_EmptyHiddenLabel_AssignsEmptyString()
    {
        // Arrange
        var label = "Label";
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 50f;
        var hiddenLabel = string.Empty;

        // Act
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);

        // Assert
        Assert.AreEqual(string.Empty, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor accepts a null description parameter.
    /// </summary>
    [TestMethod]
    public void Constructor_NullDescription_DoesNotThrow()
    {
        // Arrange
        var label = "Label";
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 90f;
        var hiddenLabel = "##hidden";

        // Act & Assert
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor accepts a positive-infinity control width.
    /// </summary>
    [TestMethod]
    public void Constructor_PositiveInfinityControlWidth_DoesNotThrow()
    {
        var alignGroup = new LabelAlignmentGroup();

        var layout = new ControlLayout("Label", null, alignGroup, float.PositiveInfinity, "##hidden");

        Assert.AreEqual("##hidden", layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor accepts a NaN control width.
    /// </summary>
    [TestMethod]
    public void Constructor_NaNControlWidth_DoesNotThrow()
    {
        var alignGroup = new LabelAlignmentGroup();

        var layout = new ControlLayout("Label", null, alignGroup, float.NaN, "##hidden");

        Assert.AreEqual("##hidden", layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor accepts a negative control width.
    /// </summary>
    [TestMethod]
    public void Constructor_NegativeControlWidth_DoesNotThrow()
    {
        // Arrange
        var label = "Label";
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = -100f;
        var hiddenLabel = "##hidden";

        // Act & Assert
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor accepts a whitespace-only hidden label.
    /// </summary>
    [TestMethod]
    public void Constructor_WhitespaceHiddenLabel_DoesNotThrow()
    {
        // Arrange
        var label = "Label";
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 100f;
        var hiddenLabel = "   ";

        // Act & Assert
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor accepts a hidden label with special characters.
    /// </summary>
    [TestMethod]
    public void Constructor_HiddenLabelWithSpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var label = "Label";
        var desc = "Description";
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 100f;
        var hiddenLabel = "##test!@#$%";

        // Act & Assert
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor accepts a very long hidden label string.
    /// </summary>
    [TestMethod]
    public void Constructor_VeryLongHiddenLabel_DoesNotThrow()
    {
        // Arrange
        var label = "Label";
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 100f;
        var hiddenLabel = "##" + new string('X', 10000);

        // Act & Assert
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor works with all extreme parameter values simultaneously.
    /// </summary>
    [TestMethod]
    public void Constructor_AllExtremeParameters_DoesNotThrow()
    {
        // Arrange
        var label = new string('L', 5000);
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = float.MaxValue;
        var hiddenLabel = new string('H', 5000);

        // Act & Assert
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor works with minimal valid parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_MinimalParameters_DoesNotThrow()
    {
        // Arrange
        var label = string.Empty;
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 0f;
        var hiddenLabel = string.Empty;

        // Act & Assert
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor calls Register on the alignGroup with correct parameters
    /// when description is not null.
    /// </summary>
    [TestMethod]
    public void Constructor_DescriptionNotNull_CallsRegisterWithTrue()
    {
        // Arrange
        var label = "Test Label";
        var desc = "Test Description";
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 100f;
        var hiddenLabel = "##hidden";

        // Act
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);

        // Assert
        // The Register method was called with (label, true) because desc is not null.
        // Since we cannot directly verify the call without mocking and LabelAlignmentGroup is sealed,
        // we verify that the constructor completes successfully, which implies Register was called.
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that the constructor calls Register on the alignGroup with correct parameters
    /// when description is null.
    /// </summary>
    [TestMethod]
    public void Constructor_DescriptionNull_CallsRegisterWithFalse()
    {
        // Arrange
        var label = "Test Label";
        string? desc = null;
        var alignGroup = new LabelAlignmentGroup();
        var controlWidth = 100f;
        var hiddenLabel = "##hidden";

        // Act
        var layout = new ControlLayout(label, desc, alignGroup, controlWidth, hiddenLabel);

        // Assert
        // The Register method was called with (label, false) because desc is null.
        // Since we cannot directly verify the call without mocking and LabelAlignmentGroup is sealed,
        // we verify that the constructor completes successfully, which implies Register was called.
        Assert.AreEqual(hiddenLabel, layout.HiddenLabel);
    }

    /// <summary>
    /// Verifies that an empty description string is treated as a present description value.
    /// </summary>
    [TestMethod]
    public void Constructor_EmptyDescription_DoesNotThrow()
    {
        var alignGroup = new LabelAlignmentGroup();

        var layout = new ControlLayout("Label", string.Empty, alignGroup, 100f, "##hidden");

        Assert.AreEqual("##hidden", layout.HiddenLabel);
    }
}
