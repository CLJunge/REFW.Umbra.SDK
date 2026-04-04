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

    internal void Draw(Parameter<string>? pathParameter, Action? importAction, Action? exportAction)
    {
        if (pathParameter is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null config-file path parameter)");
            return;
        }

        DrawPathRow(pathParameter, "Config File");
        DrawActionButtons(pathParameter, importAction, exportAction);
    }

    public void Dispose()
    {
    }

    private void DrawPathRow(Parameter<string> pathParameter, string label)
    {
        if (pathParameter is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null config-file path parameter)");
            return;
        }

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
            _renderer.OpenPopup(GetBrowsePopupId(pathParameter));

        DrawBrowsePopup(pathParameter);

        ValidationMessageRenderer.Draw(pathParameter, _renderer);
    }

    private void DrawActionButtons(Parameter<string> pathParameter, Action? importAction, Action? exportAction)
    {
        var availableWidth = _renderer.GetAvailableWidth();
        var spacingX = _renderer.GetItemSpacingX();
        var buttonWidth = Math.Max(0f, (availableWidth - spacingX) / 2f);
        if (_renderer.Button($"Import##{pathParameter.Key}", new(buttonWidth, 0f)))
            importAction?.Invoke();

        _renderer.SameLine();
        if (_renderer.Button($"Export##{pathParameter.Key}", new(buttonWidth, 0f)))
            exportAction?.Invoke();
    }

    private void DrawBrowsePopup(Parameter<string> pathParameter)
    {
        var popupId = GetBrowsePopupId(pathParameter);
        if (!_renderer.BeginPopup(popupId))
            return;

        try
        {
            if (_renderer.Selectable("Choose import file..."))
                ApplyPickedPath(pathParameter, _filePicker.TryPickImportPath);

            if (_renderer.Selectable("Choose export destination..."))
                ApplyPickedPath(pathParameter, _filePicker.TryPickExportPath);
        }
        finally
        {
            _renderer.EndPopup();
        }
    }

    private void ApplyPickedPath(
        Parameter<string> pathParameter,
        TryPickPathCallback tryPickPath)
    {
        if (!tryPickPath(pathParameter.Value, out var selectedPath)
            || string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        pathParameter.Value = selectedPath;
    }

    private static string GetBrowsePopupId(IParameter parameter)
        => $"ConfigTransferBrowse##{parameter.Key}";

    private static string GetHiddenLabel(IParameter parameter)
        => parameter.Metadata.HiddenLabel ?? $"##{parameter.Key}";

    private static uint GetMaxLength(IParameter parameter)
        => parameter.Metadata.MaxLength ?? DefaultMaxPathLength;

    private delegate bool TryPickPathCallback(string? currentPath, out string? selectedPath);
}
