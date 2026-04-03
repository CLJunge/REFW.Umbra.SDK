using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// Renders a panel's ordered section list once the shared <see cref="PluginPanel"/> ID scope is active.
/// </summary>
/// <remarks>
/// This type isolates optional root-node rendering, per-section tree-node wrapping, and trailing separator placement from <see cref="PluginPanel"/>, leaving the panel focused on lifetime and top-level scope management.
/// </remarks>
internal sealed class PluginPanelDrawPipeline
{
    private readonly string? _rootNodeLabel;
    private readonly bool _rootNodeDefaultOpen;
    private readonly bool _drawSeparator;
    private readonly IPluginPanelRenderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginPanelDrawPipeline"/> class.
    /// </summary>
    /// <param name="rootNodeLabel">The optional root-node label that wraps the full section list.</param>
    /// <param name="rootNodeDefaultOpen"><see langword="true"/> to start the optional root node in the open state; otherwise, <see langword="false"/>.</param>
    /// <param name="drawSeparator"><see langword="true"/> to draw a trailing separator after the section list; otherwise, <see langword="false"/>.</param>
    /// <param name="renderer">The renderer used for tree-node and separator operations.</param>
    /// <remarks>
    /// When <paramref name="rootNodeLabel"/> contains ImGui's <c>"##"</c> separator, <see cref="PluginPanelTreeNodeLabels.Sanitize(string?)"/> strips the suffix before rendering so the panel keeps full control over tree-node identity semantics.
    /// </remarks>
    internal PluginPanelDrawPipeline(string? rootNodeLabel, bool rootNodeDefaultOpen, bool drawSeparator, IPluginPanelRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        _rootNodeLabel = rootNodeLabel is null
            ? null
            : PluginPanelTreeNodeLabels.Sanitize(rootNodeLabel);

        _rootNodeDefaultOpen = rootNodeDefaultOpen;
        _drawSeparator = drawSeparator;
        _renderer = renderer;
    }

    /// <summary>
    /// Renders the supplied section list using the configured root-node and separator policy.
    /// </summary>
    /// <param name="sections">The ordered sections to draw.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sections"/> is <see langword="null"/>.</exception>
    internal void Draw(IReadOnlyList<IPanelSection> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);

        if (_rootNodeLabel is not null)
        {
            var flags = _rootNodeDefaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
            if (_renderer.TreeNode(_rootNodeLabel, flags))
            {
                try
                {
                    DrawSections(sections);
                    if (_drawSeparator)
                        _renderer.Separator();
                }
                finally
                {
                    _renderer.TreePop();
                }
            }

            return;
        }

        DrawSections(sections);
        if (_drawSeparator)
            _renderer.Separator();
    }

    /// <summary>
    /// Renders each section, optionally wrapping it in a section-specific tree node.
    /// </summary>
    /// <param name="sections">The ordered sections to draw.</param>
    private void DrawSections(IReadOnlyList<IPanelSection> sections)
    {
        foreach (var section in sections)
        {
            var label = section.TreeNodeLabel;
            if (label is not null)
            {
                label = PluginPanelTreeNodeLabels.Sanitize(label);

                var flags = section.TreeNodeDefaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
                if (_renderer.TreeNode($"{label}##{section.SectionId}", flags))
                {
                    try
                    {
                        section.Draw();
                    }
                    finally
                    {
                        _renderer.TreePop();
                    }
                }
            }
            else
            {
                section.Draw();
            }
        }
    }
}
