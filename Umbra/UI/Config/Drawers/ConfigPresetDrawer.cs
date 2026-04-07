using System.Numerics;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders the built-in config preset selection, navigation, export, and import controls.
/// </summary>
/// <remarks>
/// The surrounding configuration UI owns section composition and feature enablement. This drawer owns
/// only the preset-specific ImGui content for the combo selector, navigation buttons, and
/// export/import buttons.
/// </remarks>
internal sealed class ConfigPresetDrawer
{
    private const float MinimumComboWidth = 64f;
    private const string PreviousButtonLabel = "<##ConfigPresetPrev";
    private const string NextButtonLabel = ">##ConfigPresetNext";
    private const string ExportButtonLabel = "Export##ConfigPresetExport";
    private const string ImportButtonLabel = "Import##ConfigPresetImport";
    private const string ComboLabel = "##ConfigPresetCombo";
    private const string NoPresetsLabel = "(no presets)";

    private readonly IConfigPresetDrawerRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="ConfigPresetDrawer"/> that renders through the shared ImGui context.
    /// </summary>
    internal ConfigPresetDrawer()
        : this(ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigPresetDrawer"/> with the specified renderer seam.
    /// </summary>
    /// <param name="renderer">The renderer used for preset-control UI operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal ConfigPresetDrawer(IConfigPresetDrawerRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderer = renderer;
    }

    /// <summary>
    /// Draws the preset UI controls.
    /// </summary>
    /// <param name="presetNames">The sorted list of available preset names.</param>
    /// <param name="selectedPresetName">The currently selected preset name, or <see langword="null"/> when no preset is active.</param>
    /// <param name="onPresetSelected">Callback invoked when the user selects a preset from the combo or navigates with buttons. Receives the new preset name.</param>
    /// <param name="onExportClicked">Callback invoked when the user clicks the Export button.</param>
    /// <param name="onImportClicked">Callback invoked when the user clicks the Import button.</param>
    /// <param name="showSeparatorBelowButtons">When <see langword="true"/>, a separator is drawn below the button row.</param>
    /// <param name="disableControls">When <see langword="true"/>, all preset controls are rendered disabled.</param>
    internal void Draw(
        List<string> presetNames,
        string? selectedPresetName,
        Action<string> onPresetSelected,
        Action onExportClicked,
        Action onImportClicked,
        bool showSeparatorBelowButtons,
        bool disableControls = false)
    {
        var hasPresets = presetNames.Count > 0;
        _renderer.BeginDisabled(disableControls);
        try
        {
            DrawComboRow(presetNames, selectedPresetName, onPresetSelected, hasPresets);
            DrawActionRow(selectedPresetName, onExportClicked, onImportClicked, hasPresets);
            if (showSeparatorBelowButtons)
                _renderer.Separator();
        }
        finally
        {
            _renderer.EndDisabled();
        }
    }

    private void DrawComboRow(
        List<string> presetNames,
        string? selectedPresetName,
        Action<string> onPresetSelected,
        bool hasPresets)
    {
        var prevWidth = _renderer.GetButtonWidth(PreviousButtonLabel);
        var nextWidth = _renderer.GetButtonWidth(NextButtonLabel);
        var spacingX = _renderer.GetItemSpacingX();
        var availableWidth = _renderer.GetAvailableWidth();
        var comboWidth = availableWidth - prevWidth - nextWidth - (spacingX * 2f);

        // Previous button
        _renderer.BeginDisabled(!hasPresets);
        try
        {
            if (_renderer.Button(PreviousButtonLabel))
            {
                var idx = FindIndex(presetNames, selectedPresetName);
                var newIdx = idx <= 0 ? presetNames.Count - 1 : idx - 1;
                onPresetSelected(presetNames[newIdx]);
            }
        }
        finally
        {
            _renderer.EndDisabled();
        }

        // Combo
        _renderer.SameLine();
        _renderer.SetNextItemWidth(Math.Max(MinimumComboWidth, comboWidth));

        if (hasPresets)
        {
            var items = ToStringArray(presetNames);
            var selectedIndex = FindIndex(presetNames, selectedPresetName);
            if (_renderer.Combo(ComboLabel, ref selectedIndex, items, items.Length))
            {
                if (selectedIndex >= 0 && selectedIndex < presetNames.Count)
                    onPresetSelected(presetNames[selectedIndex]);
            }
        }
        else
        {
            var emptyItems = new[] { NoPresetsLabel };
            var noIndex = 0;
            _renderer.BeginDisabled(true);
            try
            {
                _renderer.Combo(ComboLabel, ref noIndex, emptyItems, 1);
            }
            finally
            {
                _renderer.EndDisabled();
            }
        }

        // Next button
        _renderer.SameLine();
        _renderer.BeginDisabled(!hasPresets);
        try
        {
            if (_renderer.Button(NextButtonLabel))
            {
                var idx = FindIndex(presetNames, selectedPresetName);
                var newIdx = (idx + 1) >= presetNames.Count ? 0 : idx + 1;
                onPresetSelected(presetNames[newIdx]);
            }
        }
        finally
        {
            _renderer.EndDisabled();
        }
    }

    private void DrawActionRow(
        string? selectedPresetName,
        Action onExportClicked,
        Action onImportClicked,
        bool hasPresets)
    {
        var availableWidth = _renderer.GetAvailableWidth();
        var spacingX = _renderer.GetItemSpacingX();
        var halfWidth = (availableWidth - spacingX) / 2f;
        var buttonSize = new Vector2(Math.Max(0f, halfWidth), 0f);

        var canExport = hasPresets && !string.IsNullOrWhiteSpace(selectedPresetName);

        _renderer.BeginDisabled(!canExport);
        try
        {
            if (_renderer.Button(ExportButtonLabel, buttonSize))
                onExportClicked();
        }
        finally
        {
            _renderer.EndDisabled();
        }

        _renderer.SameLine();

        if (_renderer.Button(ImportButtonLabel, buttonSize))
            onImportClicked();
    }

    private static int FindIndex(List<string> names, string? target)
    {
        if (target is null)
            return -1;

        for (var i = 0; i < names.Count; i++)
        {
            if (string.Equals(names[i], target, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    private static string[] ToStringArray(List<string> names)
    {
        var result = new string[names.Count];
        for (var i = 0; i < names.Count; i++)
            result[i] = names[i];

        return result;
    }
}
