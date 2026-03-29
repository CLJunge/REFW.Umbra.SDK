using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Applies <see cref="ConfigDrawer{TConfig}"/> ID scoping through the active ImGui frame.
/// </summary>
internal sealed class ImGuiConfigDrawerScope : IConfigDrawerScope
{
    /// <inheritdoc/>
    public void PushId(string idScope)
    {
        ImGui.PushID(idScope);
    }

    /// <inheritdoc/>
    public void PopId()
    {
        ImGui.PopID();
    }
}
