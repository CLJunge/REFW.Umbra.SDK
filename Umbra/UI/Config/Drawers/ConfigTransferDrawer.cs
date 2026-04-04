using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders a cohesive import/export control for a nested config group implementing <see cref="IConfigTransferGroup"/>.
/// </summary>
/// <remarks>
/// The surrounding configuration-drawer pipeline still owns the property-level wrapper UI such as
/// category grouping, collapse behavior, and group-level visibility. This drawer owns only the
/// transfer-specific ImGui content inside that wrapper.
/// </remarks>
public sealed class ConfigTransferDrawer : INestedDrawer<IConfigTransferGroup>
{
    private const uint DefaultMaxPathLength = 256;
    private readonly IConfigTransferDrawerRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="ConfigTransferDrawer"/> that renders through the shared ImGui context.
    /// </summary>
    public ConfigTransferDrawer()
        : this(ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigTransferDrawer"/> with the specified renderer seam.
    /// </summary>
    /// <param name="renderer">The renderer used for transfer-control UI operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal ConfigTransferDrawer(IConfigTransferDrawerRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderer = renderer;
    }

    /// <inheritdoc/>
    public void Draw(IConfigTransferGroup groupInstance)
    {
        if (groupInstance is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null config transfer group)");
            return;
        }

        if (groupInstance.ImportConfig is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null import action parameter)");
            return;
        }

        if (groupInstance.ImportPath is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null 'Import Path' parameter)");
            return;
        }

        if (groupInstance.ExportConfig is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null export action parameter)");
            return;
        }

        if (groupInstance.ExportPath is null)
        {
            _renderer.TextDisabled("(ConfigTransferDrawer requires a non-null 'Export Path' parameter)");
            return;
        }

        DrawPathRow(groupInstance.ImportPath, "Import Path", "Import", groupInstance.ImportConfig.Value);
        DrawPathRow(groupInstance.ExportPath, "Export Path", "Export", groupInstance.ExportConfig.Value);
        _renderer.TextDisabled("Transfer results are reported through the existing logging flow.");
    }

    private void DrawPathRow(Parameter<string> pathParameter, string label, string buttonLabel, Action? action)
    {
        if (pathParameter is null)
        {
            _renderer.TextDisabled($"(ConfigTransferDrawer requires a non-null '{label}' parameter)");
            return;
        }

        _renderer.Text(label);
        _renderer.SameLine();
        _renderer.SetNextItemWidth(-160f);
        var currentValue = pathParameter.Value ?? string.Empty;
        if (_renderer.InputText(GetHiddenLabel(pathParameter), ref currentValue, GetMaxLength(pathParameter)))
            pathParameter.Value = currentValue;

        _renderer.SameLine();
        if (_renderer.Button($"{buttonLabel}##{pathParameter.Key}"))
            action?.Invoke();

        ValidationMessageRenderer.Draw(pathParameter, _renderer);
    }

    private static string GetHiddenLabel(IParameter parameter)
        => parameter.Metadata.HiddenLabel ?? $"##{parameter.Key}";

    private static uint GetMaxLength(IParameter parameter)
        => parameter.Metadata.MaxLength ?? DefaultMaxPathLength;
}
