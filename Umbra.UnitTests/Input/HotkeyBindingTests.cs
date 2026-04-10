namespace Umbra.Input.UnitTests;

/// <summary>
/// Unit tests for the <see cref="HotkeyBinding"/> record struct.
/// </summary>
[TestClass]
public class HotkeyBindingTests
{
    /// <summary>
    /// Tests that <see cref="HotkeyBinding.None"/> has no key and no modifiers.
    /// </summary>
    [TestMethod]
    public void None_HasUmbraKeyNoneAndNoModifiers()
    {
        // Arrange / Act
        var binding = HotkeyBinding.None;

        // Assert
        Assert.AreEqual((int)UmbraKey.None, binding.Key);
        Assert.IsFalse(binding.Ctrl);
        Assert.IsFalse(binding.Shift);
        Assert.IsFalse(binding.Alt);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.IsEmpty"/> returns <c>true</c> for a binding with <c>UmbraKey.None</c>.
    /// </summary>
    [TestMethod]
    public void IsEmpty_WhenKeyIsNone_ReturnsTrue()
    {
        // Arrange
        var binding = HotkeyBinding.None;

        // Act / Assert
        Assert.IsTrue(binding.IsEmpty);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.IsEmpty"/> returns <c>false</c> when a non-None key is set.
    /// </summary>
    [TestMethod]
    public void IsEmpty_WhenKeyIsNotNone_ReturnsFalse()
    {
        // Arrange
        var binding = new HotkeyBinding((int)UmbraKey.F3, false, false, false);

        // Act / Assert
        Assert.IsFalse(binding.IsEmpty);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.IsEmpty"/> returns <c>false</c> even when all modifiers are set,
    /// as long as the primary key is not None.
    /// </summary>
    [TestMethod]
    public void IsEmpty_WhenKeyIsNotNoneWithModifiers_ReturnsFalse()
    {
        // Arrange
        var binding = new HotkeyBinding((int)UmbraKey.F3, true, true, true);

        // Act / Assert
        Assert.IsFalse(binding.IsEmpty);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.GetDisplayName"/> returns "None" for an empty binding.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_WhenEmpty_ReturnsNone()
    {
        // Arrange
        var binding = HotkeyBinding.None;

        // Act
        var result = binding.GetDisplayName();

        // Assert
        Assert.AreEqual("None", result);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.GetDisplayName"/> returns just the key name when no modifiers are set.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_KeyOnly_ReturnsKeyName()
    {
        // Arrange
        var key = (int)UmbraKey.F5;
        var binding = new HotkeyBinding(key, false, false, false);

        // Act
        var result = binding.GetDisplayName();

        // Assert
        Assert.AreEqual(KeyboardInput.GetKeyName(key), result);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.GetDisplayName"/> includes "Ctrl+" prefix when Ctrl is held.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_CtrlModifier_IncludesCtrlPrefix()
    {
        // Arrange
        var key = (int)UmbraKey.F5;
        var binding = new HotkeyBinding(key, Ctrl: true, Shift: false, Alt: false);

        // Act
        var result = binding.GetDisplayName();

        // Assert
        var expected = "Ctrl+" + KeyboardInput.GetKeyName(key);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.GetDisplayName"/> includes "Shift+" prefix when Shift is held.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_ShiftModifier_IncludesShiftPrefix()
    {
        // Arrange
        var key = (int)UmbraKey.F5;
        var binding = new HotkeyBinding(key, Ctrl: false, Shift: true, Alt: false);

        // Act
        var result = binding.GetDisplayName();

        // Assert
        var expected = "Shift+" + KeyboardInput.GetKeyName(key);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.GetDisplayName"/> includes "Alt+" prefix when Alt is held.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_AltModifier_IncludesAltPrefix()
    {
        // Arrange
        var key = (int)UmbraKey.F5;
        var binding = new HotkeyBinding(key, Ctrl: false, Shift: false, Alt: true);

        // Act
        var result = binding.GetDisplayName();

        // Assert
        var expected = "Alt+" + KeyboardInput.GetKeyName(key);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.GetDisplayName"/> concatenates all three modifiers in Ctrl+Shift+Alt order.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_AllModifiers_ConcatenatesCtrlShiftAltKey()
    {
        // Arrange
        var key = (int)UmbraKey.F5;
        var binding = new HotkeyBinding(key, Ctrl: true, Shift: true, Alt: true);

        // Act
        var result = binding.GetDisplayName();

        // Assert
        var expected = "Ctrl+Shift+Alt+" + KeyboardInput.GetKeyName(key);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.GetDisplayName"/> includes Ctrl+Shift without Alt.
    /// </summary>
    [TestMethod]
    public void GetDisplayName_CtrlShiftModifiers_ConcatenatesCtrlShiftKey()
    {
        // Arrange
        var key = (int)UmbraKey.F5;
        var binding = new HotkeyBinding(key, Ctrl: true, Shift: true, Alt: false);

        // Act
        var result = binding.GetDisplayName();

        // Assert
        var expected = "Ctrl+Shift+" + KeyboardInput.GetKeyName(key);
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.ToString"/> delegates to <see cref="HotkeyBinding.GetDisplayName"/>.
    /// </summary>
    [TestMethod]
    public void ToString_DelegatesToGetDisplayName()
    {
        // Arrange
        var binding = new HotkeyBinding((int)UmbraKey.F5, true, false, true);

        // Act / Assert
        Assert.AreEqual(binding.GetDisplayName(), binding.ToString());
    }

    /// <summary>
    /// Tests that two <see cref="HotkeyBinding"/> instances with the same values are equal (record struct equality).
    /// </summary>
    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var a = new HotkeyBinding((int)UmbraKey.F3, true, false, true);
        var b = new HotkeyBinding((int)UmbraKey.F3, true, false, true);

        // Act / Assert
        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
    }

    /// <summary>
    /// Tests that two <see cref="HotkeyBinding"/> instances with different keys are not equal.
    /// </summary>
    [TestMethod]
    public void Equality_DifferentKey_AreNotEqual()
    {
        // Arrange
        var a = new HotkeyBinding((int)UmbraKey.F3, false, false, false);
        var b = new HotkeyBinding((int)UmbraKey.F4, false, false, false);

        // Act / Assert
        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
    }

    /// <summary>
    /// Tests that two <see cref="HotkeyBinding"/> instances with different modifiers are not equal.
    /// </summary>
    [TestMethod]
    public void Equality_DifferentModifiers_AreNotEqual()
    {
        // Arrange
        var a = new HotkeyBinding((int)UmbraKey.F3, true, false, false);
        var b = new HotkeyBinding((int)UmbraKey.F3, false, true, false);

        // Act / Assert
        Assert.AreNotEqual(a, b);
    }

    /// <summary>
    /// Tests that <see cref="HotkeyBinding.None"/> equals a default-constructed <see cref="HotkeyBinding"/>
    /// with <c>UmbraKey.None</c> and all modifiers false.
    /// </summary>
    [TestMethod]
    public void None_EqualsExplicitDefaultConstruction()
    {
        // Arrange
        var explicitNone = new HotkeyBinding((int)UmbraKey.None, false, false, false);

        // Act / Assert
        Assert.AreEqual(HotkeyBinding.None, explicitNone);
    }
}
