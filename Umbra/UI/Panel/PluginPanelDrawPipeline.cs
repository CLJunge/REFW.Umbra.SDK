using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// Renders the section body of a <see cref="PluginPanel"/> once the shared panel ID scope is active.
/// </summary>
/// <remarks>
/// This type isolates root-node rendering, separator placement, and per-section tree-node behavior
/// from <see cref="PluginPanel"/>, leaving the panel focused on lifetime and scope management.
/// </remarks>
internal sealed class PluginPanelDrawPipeline
{
    private readonly string? _rootNodeLabel;
    private readonly bool _rootNodeDefaultOpen;
    private readonly bool _drawSeparator;
    private readonly IPluginPanelRenderer _renderer;

    /// <summary>
    /// Initializes a new draw pipeline for one panel configuration.
    /// </summary>
    /// <param name="rootNodeLabel">
    /// The optional root node label wrapping the full section list.
    /// When the label contains ImGui's <c>"##"</c> separator, the caller-supplied suffix is stripped
    /// before rendering so the panel retains full control over tree-node identity semantics.
    /// </param>
    /// <param name="rootNodeDefaultOpen">Whether the optional root node starts expanded.</param>
    /// <param name="drawSeparator">Whether a trailing separator should be rendered after the sections.</param>
    /// <param name="renderer">The low-level renderer used for tree-node and separator operations.</param>
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
    /// Renders the supplied <paramref name="sections"/> using the configured root-node and separator policy.
    /// </summary>
    /// <param name="sections">The ordered sections to draw.</param>
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
    /// Iterates over all sections and renders each one, optionally wrapping it inside a per-section tree node.
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
