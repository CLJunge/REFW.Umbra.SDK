using System.Numerics;
using Umbra.Config;
using Umbra.UI.Config.Rendering;

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
    private const string ImportFileExistsStatusLabel = "OK";
    private const string ImportFileMissingStatusLabel = "X";
    private static readonly Vector4 _importFileExistsColor = new(0.35f, 1f, 0.35f, 1f);
    private static readonly Vector4 _importFileMissingColor = new(1f, 0.35f, 0.35f, 1f);
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
    {
        if (pathParameter is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null config-file path parameter)");
            return;
        }

        var importPathState = DrawPathRow(pathParameter, "Config File:", fallbackBrowseDirectory);
        DrawActionButtons(pathParameter, importAction, exportAction, importPathState);
        if (drawSeparatorBelowButtons)
            _renderer.Separator();
    }

    public void Dispose()
    {
    }

    private ImportPathState DrawPathRow(Parameter<string> pathParameter, string label, string? fallbackBrowseDirectory)
    {
        var browseButtonLabel = $"Browse...##{pathParameter.Key}";
        var availableWidth = _renderer.GetAvailableWidth();
        var spacingX = _renderer.GetItemSpacingX();
        var statusWidth = GetReservedImportStatusWidth();
        var pathInputWidth = availableWidth
            - _renderer.GetTextWidth(label)
            - _renderer.GetButtonWidth(browseButtonLabel)
            - statusWidth
            - (spacingX * 3f);

        _renderer.Text(label);
        _renderer.SameLine();
        _renderer.SetNextItemWidth(Math.Max(MinimumPathInputWidth, pathInputWidth));
        var currentValue = pathParameter.Value ?? string.Empty;
        if (_renderer.InputText(GetHiddenLabel(pathParameter), ref currentValue, GetMaxLength(pathParameter)))
            pathParameter.Value = currentValue;

        _renderer.SameLine();
        if (_renderer.Button(browseButtonLabel))
            _renderer.OpenPopup(GetBrowsePopupId(pathParameter));

        DrawBrowsePopup(pathParameter, fallbackBrowseDirectory);

        var importPathState = EvaluateImportPathState(pathParameter);
        DrawImportStatus(importPathState);
        ValidationMessageRenderer.Draw(pathParameter, _renderer);
        return importPathState;
    }

    private void DrawActionButtons(
        Parameter<string> pathParameter,
        Action? importAction,
        Action? exportAction,
        ImportPathState importPathState)
    {
        var availableWidth = _renderer.GetAvailableWidth();
        var spacingX = _renderer.GetItemSpacingX();
        var buttonWidth = Math.Max(0f, (availableWidth - spacingX) / 2f);

        _renderer.BeginDisabled(!CanImport(importPathState));
        try
        {
            if (_renderer.Button($"Import##{pathParameter.Key}", new(buttonWidth, 0f)))
                importAction?.Invoke();
        }
        finally
        {
            _renderer.EndDisabled();
        }

        _renderer.SameLine();
        if (_renderer.Button($"Export##{pathParameter.Key}", new(buttonWidth, 0f)))
            exportAction?.Invoke();
    }

    private void DrawBrowsePopup(Parameter<string> pathParameter, string? fallbackBrowseDirectory)
    {
        var popupId = GetBrowsePopupId(pathParameter);
        if (!_renderer.BeginPopup(popupId))
            return;

        try
        {
            if (_renderer.Selectable("Choose import file..."))
                ApplyPickedPath(pathParameter, fallbackBrowseDirectory, _filePicker.TryPickImportPath);

            if (_renderer.Selectable("Choose export destination..."))
                ApplyPickedPath(pathParameter, fallbackBrowseDirectory, _filePicker.TryPickExportPath);
        }
        finally
        {
            _renderer.EndPopup();
        }
    }

    private void DrawImportStatus(ImportPathState importPathState)
    {
        var statusLabel = GetImportStatusLabel(importPathState);
        if (statusLabel is null)
            return;

        _renderer.SameLine();
        _renderer.TextColored(GetImportStatusColor(importPathState), statusLabel);
    }

    private float GetReservedImportStatusWidth()
        => Math.Max(
            _renderer.GetTextWidth(ImportFileExistsStatusLabel),
            _renderer.GetTextWidth(ImportFileMissingStatusLabel));

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

    private static ImportPathState EvaluateImportPathState(Parameter<string> pathParameter)
    {
        if (pathParameter is IParameterValidationState validationState && validationState.HasValidationError)
            return ImportPathState.Invalid;

        var filePath = pathParameter.Value;
        if (string.IsNullOrWhiteSpace(filePath))
            return ImportPathState.Empty;

        return File.Exists(filePath) ? ImportPathState.Exists : ImportPathState.Missing;
    }

    private static bool CanImport(ImportPathState importPathState)
        => importPathState == ImportPathState.Exists;

    private static string? GetImportStatusLabel(ImportPathState importPathState)
        => importPathState switch
        {
            ImportPathState.Exists => ImportFileExistsStatusLabel,
            ImportPathState.Missing => ImportFileMissingStatusLabel,
            _ => null
        };

    private static Vector4 GetImportStatusColor(ImportPathState importPathState)
        => importPathState == ImportPathState.Exists ? _importFileExistsColor : _importFileMissingColor;

    private static string GetBrowsePopupId(Parameter<string> parameter)
        => $"ConfigTransferBrowse##{parameter.Key}";

    private static string GetHiddenLabel(Parameter<string> parameter)
        => parameter.Metadata.HiddenLabel ?? $"##{parameter.Key}";

    private static uint GetMaxLength(Parameter<string> parameter)
        => parameter.Metadata.MaxLength ?? DefaultMaxPathLength;

    private delegate bool TryPickPathCallback(string? currentPath, string? fallbackDirectory, out string? selectedPath);

    private enum ImportPathState
    {
        Empty,
        Invalid,
        Missing,
        Exists
    }
}
