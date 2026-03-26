using Hexa.NET.ImGui;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Draw node that wraps a subtree in a stable ImGui ID scope.
/// </summary>
/// <remarks>
/// The scope ID is typically a dot-separated structural path derived from the owning nested
/// settings-group property and its configured prefix. This keeps repeated local widget labels and
/// custom nested-group drawer IDs isolated across sibling branches of the configuration tree.
/// <see cref="ImGui.PopID()"/> is guaranteed to run even if a child node throws while drawing.
/// </remarks>
/// <param name="scopeId">The stable ImGui ID pushed before drawing the subtree.</param>
/// <param name="children">The child nodes that should render inside the pushed ID scope.</param>
internal sealed class IdScopeNode(string scopeId, List<IDrawNode> children) : IDrawNode
{
    /// <inheritdoc/>
    public void Draw()
    {
        ImGui.PushID(scopeId);
        try
        {
            foreach (var child in children)
                child.Draw();
        }
        finally
        {
            ImGui.PopID();
        }
    }
}
