using System.Diagnostics;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.Config.UI.Nodes;

/// <summary>
/// Draw node that renders a category header and all child controls either as a flat
/// <c>ImGui.SeparatorText</c> block or as a collapsible <c>ImGui.TreeNode</c> scope,
/// depending on whether <paramref name="collapseAttr"/> is set at construction time.
/// </summary>
/// <remarks>
/// <para>
/// When <paramref name="indentAttr"/> is non-<see langword="null"/>, the entire category
/// block — both the header and all child controls — is wrapped inside a matching
/// <c>ImGui.Indent</c>/<c>ImGui.Unindent</c> scope using the attribute's pixel amount.
/// </para>
/// </remarks>
/// <param name="label">The category section label displayed in the header or tree node.</param>
/// <param name="collapseAttr">
/// When non-<see langword="null"/>, the category renders as a collapsible <c>ImGui.TreeNode</c>
/// scope; when <see langword="null"/>, a flat <c>ImGui.SeparatorText</c> header is used instead.
/// </param>
/// <param name="indentAttr">
/// Optional category-wide <see cref="IndentAttribute"/> that, when non-<see langword="null"/>,
/// wraps the header and all child controls in a matching <c>ImGui.Indent</c>/<c>ImGui.Unindent</c>
/// scope using the attribute's pixel amount.
/// </param>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class CategoryNode(
    string label,
    CollapseAsTreeAttribute? collapseAttr = null,
    IndentAttribute? indentAttr = null
) : IDrawNode
{
    /// <summary>
    /// The label alignment group shared by all parameter controls in this category.
    /// <see cref="ControlFactory"/> observes each label into this group each frame so that
    /// all editing widgets within the category are aligned to a common column x position.
    /// </summary>
    internal LabelAlignmentGroup AlignmentGroup { get; } = new();

    /// <summary>
    /// Child draw nodes (parameter controls, spacing) belonging to this category.
    /// Populated by <see cref="ConfigDrawerBuilder"/> during the construction-time reflection pass.
    /// </summary>
    internal readonly List<IDrawNode> Children = [];

    /// <inheritdoc/>
    public void Draw()
    {
        if (indentAttr != null) ImGui.Indent(indentAttr.Amount);
        if (collapseAttr is not null) DrawAsTree();
        else DrawAsHeader();
        if (indentAttr != null) ImGui.Unindent(indentAttr.Amount);
    }

    /// <summary>Renders the category as an <c>ImGui.SeparatorText</c> header followed by its child controls.</summary>
    private void DrawAsHeader()
    {
        ImGui.SeparatorText(label);
        foreach (var child in Children)
            child.Draw();
    }

    /// <summary>
    /// Renders the category as an <c>ImGui.TreeNode</c>, drawing all child controls inside the
    /// expanded scope. <c>ImGui.TreePop()</c> is always called when the node is open.
    /// </summary>
    private void DrawAsTree()
    {
        var flags = collapseAttr!.DefaultOpen
            ? ImGuiTreeNodeFlags.DefaultOpen
            : ImGuiTreeNodeFlags.None;
        var open = ImGui.TreeNodeEx(label, flags);
        if (!open) return;

        foreach (var child in Children)
            child.Draw();

        ImGui.TreePop();
    }

    private string GetDebuggerDisplay()
    {
        var displayString = $"Category: {label}";

        if (Children.Count > 0)
            displayString += $" ({Children.Count} child node{(Children.Count > 1 ? "s" : "")})";

        return displayString;
    }
}
