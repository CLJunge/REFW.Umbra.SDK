using System.Diagnostics;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Draw node that renders a category header and all child controls either as a flat
/// <see cref="ImGui.SeparatorText(string)"/> block or as a collapsible <see cref="ImGui.TreeNode(string)"/> scope,
/// depending on whether <paramref name="collapseAttr"/> is set at construction time.
/// </summary>
/// <param name="label">The category section label displayed in the header or tree node.</param>
/// <param name="collapseAttr">
/// When non-<see langword="null"/>, the category renders as a collapsible <see cref="ImGui.TreeNode(string)"/>
/// scope; when <see langword="null"/>, a flat <see cref="ImGui.SeparatorText(string)"/> header is used instead.
/// </param>
/// <param name="indentAttr">
/// Optional category-wide <see cref="UmbraIndentAttribute"/> that wraps the header and all child controls
/// in a matching <see cref="ImGui.Indent(float)"/>/<see cref="ImGui.Unindent(float)"/> scope.
/// </param>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class CategoryNode(
    string label,
    UmbraCollapseAsTreeAttribute? collapseAttr = null,
    UmbraIndentAttribute? indentAttr = null
) : IDrawNode
{
    internal LabelAlignmentGroup AlignmentGroup { get; } = new();

    internal readonly List<IDrawNode> Children = [];

    /// <inheritdoc/>
    public void Draw()
    {
        var hasIndent = indentAttr != null;
        if (hasIndent) ImGui.Indent(indentAttr!.Amount);
        try
        {
            if (collapseAttr is not null) DrawAsTree();
            else DrawAsHeader();
        }
        finally
        {
            if (hasIndent) ImGui.Unindent(indentAttr!.Amount);
        }
    }

    private void DrawAsHeader()
    {
        ImGui.SeparatorText(label);
        foreach (var child in Children)
            child.Draw();
    }

    private void DrawAsTree()
    {
        var flags = collapseAttr!.DefaultOpen
            ? ImGuiTreeNodeFlags.DefaultOpen
            : ImGuiTreeNodeFlags.None;
        var open = ImGui.TreeNodeEx(label, flags);
        if (!open) return;

        try
        {
            foreach (var child in Children)
                child.Draw();
        }
        finally
        {
            ImGui.TreePop();
        }
    }

    private string GetDebuggerDisplay()
    {
        var displayString = $"Category: {label}";

        if (Children.Count > 0)
            displayString += $" ({Children.Count} child node{(Children.Count > 1 ? "s" : "")})";

        return displayString;
    }
}
