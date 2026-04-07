namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigPresetDrawer"/>.
/// </summary>
[TestClass]
public sealed class ConfigPresetDrawerTests
{
    private TestConfigPresetDrawerRenderer _renderer = null!;
    private List<string> _presetNames = null!;
    private List<string> _selectedCallbacks = null!;
    private int _exportCallCount;
    private int _importCallCount;

    [TestInitialize]
    public void TestInitialize()
    {
        _renderer = new TestConfigPresetDrawerRenderer();
        _presetNames = [];
        _selectedCallbacks = [];
        _exportCallCount = 0;
        _importCallCount = 0;
    }

    // --- Layout: Combo row ---

    /// <summary>
    /// Verifies that the combo row reserves width for the prev/next buttons and the combo fills the remaining space.
    /// </summary>
    [TestMethod]
    public void Draw_Layout_ComboWidthFillsRemainingSpace()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta"];
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - SetNextItemWidth should have been called once for the combo
        Assert.HasCount(1, _renderer.Widths);
        // Width = available(600) - prevBtn - nextBtn - 2*spacing
        // prevBtn = "<" visible label => 1*8+16 = 24; nextBtn = ">" => 1*8+16 = 24; 2*spacing = 16
        Assert.AreEqual(600f - 24f - 24f - 16f, _renderer.Widths[0]);
    }

    /// <summary>
    /// Verifies that the combo row uses SameLine twice (once before combo, once before next button).
    /// </summary>
    [TestMethod]
    public void Draw_ComboRow_UsesSameLineTwice()
    {
        // Arrange
        _presetNames = ["Alpha"];
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - combo row: SameLine before combo + SameLine before next; action row: SameLine before import
        Assert.AreEqual(3, _renderer.SameLineCount);
    }

    // --- Empty preset list ---

    /// <summary>
    /// Verifies that when no presets exist, the combo shows "(no presets)" in a disabled state and nav buttons are disabled.
    /// </summary>
    [TestMethod]
    public void Draw_NoPresets_ShowsDisabledComboAndButtons()
    {
        // Arrange
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, null, OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - outer scope is enabled, then prev/combo/next are individually disabled
        Assert.IsGreaterThanOrEqualTo(4, _renderer.DisabledScopes.Count);
        Assert.IsFalse(_renderer.DisabledScopes[0]); // outer scope enabled
        Assert.IsTrue(_renderer.DisabledScopes[1]); // prev disabled
        Assert.IsTrue(_renderer.DisabledScopes[2]); // combo disabled
        Assert.IsTrue(_renderer.DisabledScopes[3]); // next disabled

        // Combo should show "(no presets)"
        Assert.HasCount(1, _renderer.Combos);
        Assert.AreEqual("(no presets)", _renderer.Combos[0].Items[0]);
    }

    /// <summary>
    /// Verifies that when no presets exist, clicking prev/next does not trigger any callback.
    /// </summary>
    [TestMethod]
    public void Draw_NoPresets_NavButtonClicksAreIgnored()
    {
        // Arrange
        _renderer.ButtonResults.Enqueue(true);
        _renderer.ButtonResults.Enqueue(true);
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, null, OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - disabled buttons suppress clicks
        Assert.HasCount(0, _selectedCallbacks);
    }

    // --- Combo selection ---

    /// <summary>
    /// Verifies that selecting a different preset from the combo invokes the preset-selected callback.
    /// </summary>
    [TestMethod]
    public void Draw_ComboSelectionChanged_InvokesCallback()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta", "Gamma"];
        _renderer.ComboResults.Enqueue((true, 2));
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.HasCount(1, _selectedCallbacks);
        Assert.AreEqual("Gamma", _selectedCallbacks[0]);
    }

    /// <summary>
    /// Verifies that when the combo selection does not change, no callback is invoked.
    /// </summary>
    [TestMethod]
    public void Draw_ComboSelectionUnchanged_DoesNotInvokeCallback()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta"];
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.HasCount(0, _selectedCallbacks);
    }

    // --- Navigation buttons ---

    /// <summary>
    /// Verifies that clicking Previous from the first preset wraps to the last preset.
    /// </summary>
    [TestMethod]
    public void Draw_PreviousFromFirst_WrapsToLast()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta", "Gamma"];
        _renderer.ButtonResults.Enqueue(true); // prev clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.HasCount(1, _selectedCallbacks);
        Assert.AreEqual("Gamma", _selectedCallbacks[0]);
    }

    /// <summary>
    /// Verifies that clicking Previous from the middle preset goes to the previous preset.
    /// </summary>
    [TestMethod]
    public void Draw_PreviousFromMiddle_GoesToPrevious()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta", "Gamma"];
        _renderer.ButtonResults.Enqueue(true); // prev clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Beta", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.HasCount(1, _selectedCallbacks);
        Assert.AreEqual("Alpha", _selectedCallbacks[0]);
    }

    /// <summary>
    /// Verifies that clicking Next from the last preset wraps to the first preset.
    /// </summary>
    [TestMethod]
    public void Draw_NextFromLast_WrapsToFirst()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta", "Gamma"];
        // prev not clicked (default false), next clicked
        _renderer.ButtonResults.Enqueue(false); // prev not clicked
        _renderer.ButtonResults.Enqueue(true);  // next clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Gamma", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.HasCount(1, _selectedCallbacks);
        Assert.AreEqual("Alpha", _selectedCallbacks[0]);
    }

    /// <summary>
    /// Verifies that clicking Next from the middle preset goes to the next preset.
    /// </summary>
    [TestMethod]
    public void Draw_NextFromMiddle_GoesToNext()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta", "Gamma"];
        _renderer.ButtonResults.Enqueue(false); // prev not clicked
        _renderer.ButtonResults.Enqueue(true);  // next clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.HasCount(1, _selectedCallbacks);
        Assert.AreEqual("Beta", _selectedCallbacks[0]);
    }

    /// <summary>
    /// Verifies that when no preset is currently selected and prev is clicked, it wraps to last.
    /// </summary>
    [TestMethod]
    public void Draw_PreviousWithNoSelection_WrapsToLast()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta"];
        _renderer.ButtonResults.Enqueue(true); // prev clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, null, OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - FindIndex returns -1 for null, so prev wraps: (-1 <= 0) => count-1 = 1 => "Beta"
        Assert.HasCount(1, _selectedCallbacks);
        Assert.AreEqual("Beta", _selectedCallbacks[0]);
    }

    /// <summary>
    /// Verifies that when no preset is currently selected and next is clicked, it goes to the first.
    /// </summary>
    [TestMethod]
    public void Draw_NextWithNoSelection_GoesToFirst()
    {
        // Arrange
        _presetNames = ["Alpha", "Beta"];
        _renderer.ButtonResults.Enqueue(false); // prev not clicked
        _renderer.ButtonResults.Enqueue(true);  // next clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, null, OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - FindIndex returns -1, next: (-1+1)=0 => "Alpha"
        Assert.HasCount(1, _selectedCallbacks);
        Assert.AreEqual("Alpha", _selectedCallbacks[0]);
    }

    // --- Action row: Export ---

    /// <summary>
    /// Verifies that clicking Export when a preset is selected invokes the export callback.
    /// </summary>
    [TestMethod]
    public void Draw_ExportWithSelectedPreset_InvokesCallback()
    {
        // Arrange
        _presetNames = ["Alpha"];
        _renderer.SizedButtonResults.Enqueue(true);  // export clicked
        _renderer.SizedButtonResults.Enqueue(false); // import not clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.AreEqual(1, _exportCallCount);
    }

    /// <summary>
    /// Verifies that the Export button is disabled when no preset is selected.
    /// </summary>
    [TestMethod]
    public void Draw_ExportWithNoSelectedPreset_IsDisabled()
    {
        // Arrange
        _presetNames = ["Alpha"];
        _renderer.SizedButtonResults.Enqueue(true); // would click export if not disabled
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, null, OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - export button should be disabled (no selected preset)
        Assert.AreEqual(0, _exportCallCount);
        // outer(false), prev(false), next(false), export(true)
        Assert.IsGreaterThanOrEqualTo(4, _renderer.DisabledScopes.Count);
        Assert.IsTrue(_renderer.DisabledScopes[3]); // export disabled
    }

    /// <summary>
    /// Verifies that the Export button is disabled when no presets exist.
    /// </summary>
    [TestMethod]
    public void Draw_ExportWithNoPresets_IsDisabled()
    {
        // Arrange
        _renderer.SizedButtonResults.Enqueue(true);
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, null, OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.AreEqual(0, _exportCallCount);
    }

    // --- Action row: Import ---

    /// <summary>
    /// Verifies that clicking Import invokes the import callback.
    /// </summary>
    [TestMethod]
    public void Draw_ImportClicked_InvokesCallback()
    {
        // Arrange
        _presetNames = ["Alpha"];
        _renderer.SizedButtonResults.Enqueue(false); // export not clicked
        _renderer.SizedButtonResults.Enqueue(true);  // import clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.AreEqual(1, _importCallCount);
    }

    /// <summary>
    /// Verifies that Import is always enabled regardless of preset selection state.
    /// </summary>
    [TestMethod]
    public void Draw_ImportWithNoSelection_IsStillEnabled()
    {
        // Arrange
        _presetNames = [];
        // Export is disabled (no presets), so the renderer short-circuits without dequeuing.
        _renderer.SizedButtonResults.Enqueue(true);  // import clicked
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, null, OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.AreEqual(1, _importCallCount);
    }

    // --- Action row: Button sizing ---

    /// <summary>
    /// Verifies that both action buttons use half-width sizing.
    /// </summary>
    [TestMethod]
    public void Draw_ActionButtons_UseHalfWidth()
    {
        // Arrange
        _presetNames = ["Alpha"];
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert - two sized buttons (Export + Import)
        Assert.HasCount(2, _renderer.SizedButtons);
        // halfWidth = (600 - 8) / 2 = 296
        var expectedHalfWidth = (600f - 8f) / 2f;
        Assert.AreEqual(expectedHalfWidth, _renderer.SizedButtons[0].Size.X);
        Assert.AreEqual(expectedHalfWidth, _renderer.SizedButtons[1].Size.X);
    }

    // --- Separator ---

    /// <summary>
    /// Verifies that a separator is drawn below the action row when requested.
    /// </summary>
    [TestMethod]
    public void Draw_ShowSeparatorTrue_DrawsSeparator()
    {
        // Arrange
        _presetNames = ["Alpha"];
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, true);

        // Assert
        Assert.AreEqual(1, _renderer.SeparatorCount);
    }

    /// <summary>
    /// Verifies that no separator is drawn when not requested.
    /// </summary>
    [TestMethod]
    public void Draw_ShowSeparatorFalse_DoesNotDrawSeparator()
    {
        // Arrange
        _presetNames = ["Alpha"];
        var drawer = new ConfigPresetDrawer(_renderer);

        // Act
        drawer.Draw(_presetNames, "Alpha", OnPresetSelected, OnExportClicked, OnImportClicked, false);

        // Assert
        Assert.AreEqual(0, _renderer.SeparatorCount);
    }

    // --- Constructor guard ---

    /// <summary>
    /// Verifies that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException() => Assert.ThrowsExactly<ArgumentNullException>(() => new ConfigPresetDrawer(null!));

    // --- Helpers ---

    private void OnPresetSelected(string name) => _selectedCallbacks.Add(name);

    private void OnExportClicked() => _exportCallCount++;

    private void OnImportClicked() => _importCallCount++;
}
