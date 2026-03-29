using System.Diagnostics;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Draw node that renders a category header and all child controls either as a flat separator block
/// or as a collapsible tree scope, depending on whether collapse metadata is supplied.
/// </summary>
/// <remarks>
/// The default constructor renders through the active ImGui frame. Unit tests can replace the
/// low-level renderer through the internal constructor so header/tree behavior can be verified
/// without requiring an active ImGui frame.
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class CategoryNode : IDrawNode
{
    private readonly string _label;
    private readonly UmbraCollapseAsTreeAttribute? _collapseAttr;
    private readonly UmbraIndentAttribute? _indentAttr;
    private readonly ICategoryNodeRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="CategoryNode"/> that renders through the active ImGui frame.
    /// </summary>
    /// <param name="label">The category section label displayed in the header or tree node.</param>
    /// <param name="collapseAttr">
    /// When non-<see langword="null"/>, the category renders as a collapsible tree scope; when
    /// <see langword="null"/>, a flat separator header is used instead.
    /// </param>
    /// <param name="indentAttr">
    /// Optional category-wide indentation applied around the header and all child controls.
    /// </param>
    internal CategoryNode(
        string label,
        UmbraCollapseAsTreeAttribute? collapseAttr = null,
        UmbraIndentAttribute? indentAttr = null)
        : this(label, collapseAttr, indentAttr, new ImGuiCategoryNodeRenderer())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="CategoryNode"/> with the specified low-level renderer.
    /// </summary>
    /// <param name="label">The category section label displayed in the header or tree node.</param>
    /// <param name="collapseAttr">
    /// When non-<see langword="null"/>, the category renders as a collapsible tree scope; when
    /// <see langword="null"/>, a flat separator header is used instead.
    /// </param>
    /// <param name="indentAttr">Optional category-wide indentation metadata.</param>
    /// <param name="renderer">The renderer used for category-node UI operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal CategoryNode(
        string label,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraIndentAttribute? indentAttr,
        ICategoryNodeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _label = label;
        _collapseAttr = collapseAttr;
        _indentAttr = indentAttr;
        _renderer = renderer;
    }

    internal LabelAlignmentGroup AlignmentGroup { get; } = new();

    internal readonly List<IDrawNode> Children = [];

    /// <inheritdoc/>
    public void Draw()
    {
        var hasIndent = _indentAttr != null;
        if (hasIndent) _renderer.Indent(_indentAttr!.Amount);
        try
        {
            if (_collapseAttr is not null) DrawAsTree();
            else DrawAsHeader();
        }
        finally
        {
            if (hasIndent) _renderer.Unindent(_indentAttr!.Amount);
        }
    }

    private void DrawAsHeader()
    {
        _renderer.SeparatorText(_label);
        foreach (var child in Children)
            child.Draw();
    }

    private void DrawAsTree()
    {
        var open = _renderer.TreeNode(_label, _collapseAttr!.DefaultOpen);
        if (!open) return;

        try
        {
            foreach (var child in Children)
                child.Draw();
        }
        finally
        {
            _renderer.TreePop();
        }
    }

    private string GetDebuggerDisplay()
    {
        var displayString = $"Category: {_label}";

        if (Children.Count > 0)
            displayString += $" ({Children.Count} child node{(Children.Count > 1 ? "s" : "")})";

        return displayString;
    }
}
