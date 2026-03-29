using Hexa.NET.ImGui;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Renders <see cref="ParameterNode"/> spacing through the active ImGui frame.
/// </summary>
internal sealed class ImGuiParameterNodeRenderer : IParameterNodeRenderer
{
    /// <inheritdoc/>
    public void Spacing()
    {
        ImGui.Spacing();
    }
}
