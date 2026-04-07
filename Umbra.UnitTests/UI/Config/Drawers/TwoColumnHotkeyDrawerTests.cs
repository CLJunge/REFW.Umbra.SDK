using Umbra.Config;
using Umbra.Input;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="TwoColumnHotkeyDrawer"/>.
/// </summary>
[TestClass]
public sealed class TwoColumnHotkeyDrawerTests
{
    private TestHotkeyDrawerRenderer _renderer = null!;
    private TestHotkeyInputSource _inputSource = null!;

    /// <summary>
    /// Resets the shared capture state and creates deterministic test doubles before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        HotkeyCaptureState.WaitingCount = 0;
        _renderer = new TestHotkeyDrawerRenderer();
        _inputSource = new TestHotkeyInputSource();
    }

    /// <summary>
    /// Resets the shared capture state after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup() => HotkeyCaptureState.WaitingCount = 0;

    /// <summary>
    /// Verifies that drawing a disposed drawer returns immediately without rendering.
    /// </summary>
    [TestMethod]
    public void Draw_DisposedDrawer_ReturnsEarly()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        drawer.Dispose();

        // Act
        drawer.Draw(new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)));

        // Assert
        Assert.IsEmpty(_renderer.DisabledTexts);
        Assert.IsEmpty(_renderer.Texts);
        Assert.IsEmpty(_renderer.Buttons);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Verifies that null or wrong-typed parameters render disabled text.
    /// </summary>
    [TestMethod]
    public void Draw_WrongParameterType_ShowsDisabledText()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);

        // Act
        drawer.Draw(null!);

        // Assert
        Assert.HasCount(1, _renderer.DisabledTexts);
        Assert.AreEqual("(TwoColumnHotkeyDrawer requires Parameter<HotkeyBinding>)", _renderer.DisabledTexts[0]);
    }

    /// <summary>
    /// Verifies that a non-waiting drawer renders the current binding name and a change button.
    /// </summary>
    [TestMethod]
    public void Draw_ValidParameterHotkeyBinding_RendersKeyNameAndChangeButton()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        _inputSource.SetBindingDisplayName(new HotkeyBinding(70, false, false, false), "F2");
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testHotkey" };

        // Act
        drawer.Draw(parameter);

        // Assert
        Assert.HasCount(1, _renderer.Texts);
        Assert.AreEqual("F2", _renderer.Texts[0]);
        Assert.HasCount(1, _renderer.Buttons);
        Assert.AreEqual("Change##testHotkey", _renderer.Buttons[0]);
        Assert.AreEqual(1, _renderer.SameLineCount);
        Assert.AreEqual(0, _inputSource.BindingCaptureCallCount);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Verifies that clicking Change enters waiting mode and increments the shared waiting count.
    /// </summary>
    [TestMethod]
    public void Draw_WhenChangeButtonClicked_EntersWaitingMode()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testHotkey" };

        // Act
        drawer.Draw(parameter);

        // Assert
        Assert.AreEqual(1, HotkeyCaptureState.WaitingCount);

        // Act again to observe waiting UI.
        drawer.Draw(parameter);

        // Assert
        Assert.AreEqual("Press any key...", _renderer.Texts[1]);
        Assert.AreEqual("Cancel##testHotkey", _renderer.Buttons[1]);
    }

    /// <summary>
    /// Verifies that another waiting drawer prevents this drawer from entering capture mode.
    /// </summary>
    [TestMethod]
    public void Draw_WhenAnotherDrawerIsWaiting_DoesNotEnterWaitingMode()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        HotkeyCaptureState.WaitingCount = 1;
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testHotkey" };

        // Act
        drawer.Draw(parameter);

        // Assert
        Assert.AreEqual(1, HotkeyCaptureState.WaitingCount);
        drawer.Draw(parameter);
        Assert.AreEqual("Change##testHotkey", _renderer.Buttons[1]);
    }

    /// <summary>
    /// Verifies that clicking Cancel exits waiting mode without changing the stored hotkey value.
    /// </summary>
    [TestMethod]
    public void Draw_WhenCancelClicked_LeavesWaitingModeWithoutChangingValue()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        _renderer.ButtonResults.Enqueue(true);
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testHotkey" };

        // Act
        drawer.Draw(parameter);
        drawer.Draw(parameter);

        // Assert
        Assert.AreEqual(new HotkeyBinding(70, false, false, false), parameter.Value);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
        drawer.Draw(parameter);
        Assert.AreEqual("Key(70)", _renderer.Texts[2]);
    }

    /// <summary>
    /// Verifies that a captured binding updates the parameter value and exits waiting mode.
    /// </summary>
    [TestMethod]
    public void Draw_WhenKeyIsCaptured_UpdatesValueAndLeavesWaitingMode()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        _renderer.ButtonResults.Enqueue(false);
        _inputSource.QueueCapturedBinding(new HotkeyBinding(71, false, false, false));
        _inputSource.SetBindingDisplayName(new HotkeyBinding(71, false, false, false), "F3");
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testHotkey" };

        // Act
        drawer.Draw(parameter);
        drawer.Draw(parameter);
        drawer.Draw(parameter);

        // Assert
        Assert.AreEqual(new HotkeyBinding(71, false, false, false), parameter.Value);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
        Assert.AreEqual("F3", _renderer.Texts[2]);
        Assert.AreEqual(1, _inputSource.BindingCaptureCallCount);
    }

    /// <summary>
    /// Verifies that multiple calls without user interaction leave the stored value unchanged.
    /// </summary>
    [TestMethod]
    public void Draw_MultipleCallsSameParameter_DoesNotModifyValue()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testKey" };

        // Act
        drawer.Draw(parameter);
        drawer.Draw(parameter);
        drawer.Draw(parameter);

        // Assert
        Assert.AreEqual(new HotkeyBinding(70, false, false, false), parameter.Value);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
        Assert.AreEqual(0, _inputSource.BindingCaptureCallCount);
    }

    /// <summary>
    /// Verifies that disposing a waiting drawer decrements the shared waiting count exactly once.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenWaiting_DecrementsWaitingCountOnce()
    {
        // Arrange
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testKey" };
        drawer.Draw(parameter);

        // Act
        drawer.Dispose();
        drawer.Dispose();

        // Assert
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Verifies that repeated draws while not waiting do not change the displayed hotkey label.
    /// </summary>
    [TestMethod]
    public void Draw_RepeatedWithoutInteraction_KeepsCurrentKeyLabel()
    {
        var drawer = new TwoColumnHotkeyDrawer(_renderer, _inputSource);
        _inputSource.SetBindingDisplayName(new HotkeyBinding(70, false, false, false), "F2");
        var parameter = new Parameter<HotkeyBinding>(new HotkeyBinding(70, false, false, false)) { Key = "testKey" };

        drawer.Draw(parameter);
        drawer.Draw(parameter);

        Assert.AreEqual("F2", _renderer.Texts[0]);
        Assert.AreEqual("F2", _renderer.Texts[1]);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new TwoColumnHotkeyDrawer(null!, new TestHotkeyInputSource()));

        Assert.AreEqual("renderer", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null input source.
    /// </summary>
    [TestMethod]
    public void Constructor_NullInputSource_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new TwoColumnHotkeyDrawer(new TestHotkeyDrawerRenderer(), null!));

        Assert.AreEqual("inputSource", exception.ParamName);
    }
}
