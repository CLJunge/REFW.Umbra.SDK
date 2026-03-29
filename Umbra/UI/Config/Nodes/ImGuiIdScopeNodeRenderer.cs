using Hexa.NET.ImGui;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Renders <see cref="IdScopeNode"/> ID scoping through the active ImGui frame.
/// </summary>
internal sealed class ImGuiIdScopeNodeRenderer : IIdScopeNodeRenderer
{
    /// <inheritdoc/>
    public void PushId(string scopeId) => ImGui.PushID(scopeId);

    /// <inheritdoc/>
    public void PopId() => ImGui.PopID();
}
