using Umbra.Config;

namespace Umbra.UI.Config.Drawers.UnitTests;


/// <summary>
/// Unit tests for <see cref="HotkeyDrawer"/>.
/// </summary>
[TestClass]
public sealed class HotkeyDrawerTests
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
    public void Draw_WhenDisposed_ReturnsEarlyWithoutRendering()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        drawer.Dispose();

        // Act
        drawer.Draw("Hotkey", new Parameter<int>(70));

        // Assert
        Assert.IsEmpty(_renderer.DisabledTexts);
        Assert.IsEmpty(_renderer.Texts);
        Assert.IsEmpty(_renderer.Buttons);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Verifies that a null or wrong-typed parameter renders disabled text.
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterIsNotParameterOfInt_RendersDisabledText()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);

        // Act
        drawer.Draw("Hotkey", null!);

        // Assert
        Assert.HasCount(1, _renderer.DisabledTexts);
        Assert.AreEqual("Hotkey: (HotkeyDrawer requires Parameter<int>)", _renderer.DisabledTexts[0]);
    }

    /// <summary>
    /// Verifies that a non-waiting drawer renders the current key name and a change button.
    /// </summary>
    [TestMethod]
    public void Draw_WhenNotWaiting_RendersCurrentKeyNameAndChangeButton()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        _inputSource.SetKeyName(70, "F2");
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.HasCount(1, _renderer.Texts);
        Assert.AreEqual("Hotkey: F2", _renderer.Texts[0]);
        Assert.HasCount(1, _renderer.Buttons);
        Assert.AreEqual("Change##testKey", _renderer.Buttons[0]);
        Assert.AreEqual(1, _renderer.SameLineCount);
        Assert.AreEqual(0, _inputSource.CaptureCallCount);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Verifies that clicking Change enters waiting mode and increments the shared waiting count.
    /// </summary>
    [TestMethod]
    public void Draw_WhenChangeButtonClicked_EntersWaitingMode()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.AreEqual(1, HotkeyCaptureState.WaitingCount);

        // Act again to observe waiting UI.
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.AreEqual("Hotkey: Press any key...", _renderer.Texts[1]);
        Assert.AreEqual("Cancel##testKey", _renderer.Buttons[1]);
    }

    /// <summary>
    /// Verifies that another waiting drawer prevents this drawer from entering capture mode.
    /// </summary>
    [TestMethod]
    public void Draw_WhenAnotherDrawerIsWaiting_DoesNotEnterWaitingMode()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        HotkeyCaptureState.WaitingCount = 1;
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.AreEqual(1, HotkeyCaptureState.WaitingCount);
        drawer.Draw("Hotkey", parameter);
        Assert.AreEqual("Change##testKey", _renderer.Buttons[1]);
    }

    /// <summary>
    /// Verifies that clicking Cancel exits waiting mode without changing the stored hotkey value.
    /// </summary>
    [TestMethod]
    public void Draw_WhenCancelClicked_LeavesWaitingModeWithoutChangingValue()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        _renderer.ButtonResults.Enqueue(true);
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.AreEqual(70, parameter.Value);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
        drawer.Draw("Hotkey", parameter);
        Assert.AreEqual("Hotkey: Key(70)", _renderer.Texts[2]);
    }

    /// <summary>
    /// Verifies that a captured key updates the parameter value and exits waiting mode.
    /// </summary>
    [TestMethod]
    public void Draw_WhenKeyIsCaptured_UpdatesValueAndLeavesWaitingMode()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        _renderer.ButtonResults.Enqueue(false);
        _inputSource.QueueCapturedKey(71);
        _inputSource.SetKeyName(71, "F3");
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.AreEqual(71, parameter.Value);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
        Assert.AreEqual("Hotkey: F3", _renderer.Texts[2]);
        Assert.AreEqual(1, _inputSource.CaptureCallCount);
    }

    /// <summary>
    /// Verifies that an active description renders an inline help marker.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDescriptionIsProvided_RendersHelpMarker()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Description = "Press a key to bind"
            }
        };

        // Act
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.HasCount(1, _renderer.HelpMarkers);
        Assert.AreEqual("Press a key to bind", _renderer.HelpMarkers[0]);
        Assert.AreEqual(2, _renderer.SameLineCount);
    }

    /// <summary>
    /// Verifies that no help marker is rendered when the parameter has no description.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDescriptionIsNull_DoesNotRenderHelpMarker()
    {
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata
            {
                Description = null
            }
        };

        drawer.Draw("Hotkey", parameter);

        Assert.IsEmpty(_renderer.HelpMarkers);
        Assert.AreEqual(1, _renderer.SameLineCount);
    }

    /// <summary>
    /// Verifies that disposing a waiting drawer decrements the shared waiting count exactly once.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenWaiting_DecrementsWaitingCountOnce()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        _renderer.ButtonResults.Enqueue(true);
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };
        drawer.Draw("Hotkey", parameter);

        // Act
        drawer.Dispose();
        drawer.Dispose();

        // Assert
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
    }

    /// <summary>
    /// Verifies that repeated draws without user interaction leave the stored value unchanged.
    /// </summary>
    [TestMethod]
    public void Draw_WithNoUserInteraction_DoesNotModifyParameterValue()
    {
        // Arrange
        var drawer = new HotkeyDrawer(_renderer, _inputSource);
        var parameter = new Parameter<int>(70)
        {
            Key = "testKey",
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);
        drawer.Draw("Hotkey", parameter);

        // Assert
        Assert.AreEqual(70, parameter.Value);
        Assert.AreEqual(0, HotkeyCaptureState.WaitingCount);
        Assert.AreEqual(0, _inputSource.CaptureCallCount);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new HotkeyDrawer(null!, new TestHotkeyInputSource()));

        Assert.AreEqual("renderer", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null input source.
    /// </summary>
    [TestMethod]
    public void Constructor_NullInputSource_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new HotkeyDrawer(new TestHotkeyDrawerRenderer(), null!));

        Assert.AreEqual("inputSource", exception.ParamName);
    }
}
