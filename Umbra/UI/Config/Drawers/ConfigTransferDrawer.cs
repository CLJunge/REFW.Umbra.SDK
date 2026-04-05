using System.Numerics;
using Umbra.Config;
using Umbra.UI.Config.Rendering;
using Umbra.UI.Config.Transfer;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders Umbra's built-in config import/export control.
/// </summary>
/// <remarks>
/// The surrounding configuration UI owns section composition and feature enablement. This drawer owns
/// only the transfer-specific ImGui content for the shared path field, browse workflow, and action buttons.
/// </remarks>
internal sealed class ConfigTransferDrawer : IDisposable
{
    private const uint DefaultMaxPathLength = 256;
    private const float MinimumPathInputWidth = 64f;
    private const string DefaultModeLabel = "Action";
    private const string DefaultPathLabel = "Config File";
    private static readonly string[] _modeLabels = [nameof(ConfigTransferMode.Import), nameof(ConfigTransferMode.Export)];
    private static readonly ConfigTransferMode[] _modes = [ConfigTransferMode.Import, ConfigTransferMode.Export];
    private readonly IConfigTransferDrawerRenderer _renderer;
    private readonly IConfigTransferFilePicker _filePicker;

    /// <summary>
    /// Initializes a new <see cref="ConfigTransferDrawer"/> that renders through the shared ImGui context.
    /// </summary>
    internal ConfigTransferDrawer()
        : this(ImGuiConfigRenderContext.Instance, new WindowsConfigTransferFilePicker())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigTransferDrawer"/> with the specified renderer seam.
    /// </summary>
    /// <param name="renderer">The renderer used for transfer-control UI operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal ConfigTransferDrawer(IConfigTransferDrawerRenderer renderer)
        : this(renderer, new WindowsConfigTransferFilePicker())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigTransferDrawer"/> with the specified renderer seam and file picker.
    /// </summary>
    /// <param name="renderer">The renderer used for transfer-control UI operations.</param>
    /// <param name="filePicker">The native file picker used by the browse workflow.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="filePicker"/> is <see langword="null"/>.</exception>
    internal ConfigTransferDrawer(IConfigTransferDrawerRenderer renderer, IConfigTransferFilePicker filePicker)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(filePicker);
        _renderer = renderer;
        _filePicker = filePicker;
    }

    internal void Draw(
        Parameter<string>? pathParameter,
        Action? importAction,
        Action? exportAction,
        string? fallbackBrowseDirectory = null,
        bool drawSeparatorBelowButtons = true)
        => Draw(null, pathParameter, importAction, exportAction, fallbackBrowseDirectory, drawSeparatorBelowButtons);

    internal void Draw(
        Parameter<ConfigTransferMode>? modeParameter,
        Parameter<string>? pathParameter,
        Action? importAction,
        Action? exportAction,
        string? fallbackBrowseDirectory = null,
        bool drawSeparatorBelowButtons = true)
    {
        if (pathParameter is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null config-file path parameter)");
            return;
        }

        var mode = DrawModeRow(modeParameter);
        var actionState = DrawPathRow(mode, pathParameter, fallbackBrowseDirectory);
        DrawActionButton(mode, pathParameter, importAction, exportAction, actionState);
        if (drawSeparatorBelowButtons)
            _renderer.Separator();
    }

    public void Dispose()
    {
    }

    private ConfigTransferMode DrawModeRow(Parameter<ConfigTransferMode>? modeParameter)
    {
        if (modeParameter is null)
            return ConfigTransferMode.Import;

        var label = GetLabel(modeParameter, DefaultModeLabel);
        var availableWidth = _renderer.GetAvailableWidth();
        var spacingX = _renderer.GetItemSpacingX();
        var comboWidth = availableWidth - _renderer.GetTextWidth(label) - spacingX;
        var selectedMode = GetTransferMode(modeParameter);
        var selectedIndex = GetModeIndex(selectedMode);

        _renderer.Text(label);
        _renderer.SameLine();
        _renderer.SetNextItemWidth(Math.Max(MinimumPathInputWidth, comboWidth));
        if (_renderer.Combo(GetModeHiddenLabel(modeParameter), ref selectedIndex, _modeLabels, _modeLabels.Length))
            modeParameter.Value = _modes[selectedIndex];

        return GetTransferMode(modeParameter);
    }

    private TransferActionState DrawPathRow(
        ConfigTransferMode mode,
        Parameter<string> pathParameter,
        string? fallbackBrowseDirectory)
    {
        var label = GetLabel(pathParameter, DefaultPathLabel);
        var browseButtonLabel = $"Browse...##{pathParameter.Key}";
        var availableWidth = _renderer.GetAvailableWidth();
        var spacingX = _renderer.GetItemSpacingX();
        var pathInputWidth = availableWidth
            - _renderer.GetTextWidth(label)
            - _renderer.GetButtonWidth(browseButtonLabel)
            - (spacingX * 2f);

        _renderer.Text(label);
        _renderer.SameLine();
        _renderer.SetNextItemWidth(Math.Max(MinimumPathInputWidth, pathInputWidth));
        var currentValue = pathParameter.Value ?? string.Empty;
        if (_renderer.InputText(GetHiddenLabel(pathParameter), ref currentValue, GetMaxLength(pathParameter)))
            pathParameter.Value = currentValue;

        _renderer.SameLine();
        if (_renderer.Button(browseButtonLabel))
            ApplyPickedPath(pathParameter, fallbackBrowseDirectory, GetBrowsePicker(mode));

        var actionState = EvaluateActionState(mode, pathParameter);
        ValidationMessageRenderer.Draw(pathParameter, _renderer);
        return actionState;
    }

    private void DrawActionButton(
        ConfigTransferMode mode,
        Parameter<string> pathParameter,
        Action? importAction,
        Action? exportAction,
        TransferActionState actionState)
    {
        var buttonWidth = Math.Max(0f, _renderer.GetAvailableWidth());
        var buttonLabel = GetActionButtonLabel(mode, pathParameter);

        _renderer.BeginDisabled(!CanExecute(actionState));
        try
        {
            if (!_renderer.Button(buttonLabel, new(buttonWidth, 0f)))
                return;

            if (mode == ConfigTransferMode.Import)
                importAction?.Invoke();
            else
                exportAction?.Invoke();
        }
        finally
        {
            _renderer.EndDisabled();
        }
    }

    private static void ApplyPickedPath(
        Parameter<string> pathParameter,
        string? fallbackBrowseDirectory,
        TryPickPathCallback tryPickPath)
    {
        if (!tryPickPath(pathParameter.Value, fallbackBrowseDirectory, out var selectedPath)
            || string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        pathParameter.Value = selectedPath;
    }

    private static TransferActionState EvaluateActionState(ConfigTransferMode mode, Parameter<string> pathParameter)
    {
        if (pathParameter is IParameterValidationState validationState && validationState.HasValidationError)
            return TransferActionState.Invalid;

        var filePath = pathParameter.Value;
        if (string.IsNullOrWhiteSpace(filePath))
            return TransferActionState.Empty;

        if (mode == ConfigTransferMode.Import)
            return File.Exists(filePath) ? TransferActionState.Ready : TransferActionState.Missing;

        return HasJsonExtension(filePath) ? TransferActionState.Ready : TransferActionState.InvalidExtension;
    }

    private static bool CanExecute(TransferActionState actionState)
        => actionState == TransferActionState.Ready;

    private static string GetHiddenLabel(Parameter<string> parameter)
        => parameter.Metadata.HiddenLabel ?? $"##{parameter.Key}";

    private static string GetModeHiddenLabel(Parameter<ConfigTransferMode> parameter)
        => parameter.Metadata.HiddenLabel ?? $"##{parameter.Key}";

    private static uint GetMaxLength(Parameter<string> parameter)
        => parameter.Metadata.MaxLength ?? DefaultMaxPathLength;

    private static ConfigTransferMode GetTransferMode(Parameter<ConfigTransferMode> modeParameter)
    {
        var currentMode = modeParameter.Value;
        return Array.IndexOf(_modes, currentMode) >= 0 ? currentMode : ConfigTransferMode.Import;
    }

    private static int GetModeIndex(ConfigTransferMode mode)
    {
        var index = Array.IndexOf(_modes, mode);
        return index >= 0 ? index : 0;
    }

    private static string GetActionButtonLabel(ConfigTransferMode mode, Parameter<string> pathParameter)
        => $"{mode}##{pathParameter.Key}";

    private TryPickPathCallback GetBrowsePicker(ConfigTransferMode mode)
        => mode == ConfigTransferMode.Import ? _filePicker.TryPickImportPath : _filePicker.TryPickExportPath;

    private static string GetLabel<T>(Parameter<T> parameter, string fallback)
    {
        var resolvedLabel = parameter.Metadata.ResolvedLabel;
        if (!string.IsNullOrWhiteSpace(resolvedLabel))
            return resolvedLabel;

        var displayName = parameter.Metadata.DisplayName;
        return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName;
    }

    private static bool HasJsonExtension(string filePath)
        => string.Equals(Path.GetExtension(filePath), ".json", StringComparison.OrdinalIgnoreCase);

    private delegate bool TryPickPathCallback(string? currentPath, string? fallbackDirectory, out string? selectedPath);

    private enum TransferActionState
    {
        Empty,
        Invalid,
        Missing,
        InvalidExtension,
        Ready
    }
}
