namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Represents one pre-built node in the configuration draw tree.
/// </summary>
/// <remarks>
/// <see cref="ConfigDrawer{TConfig}"/> walks these nodes each frame after the one-time build pass has assembled the configuration UI tree.
/// </remarks>
internal interface IDrawNode
{
    /// <summary>
    /// Emits this node's ImGui output for the current frame.
    /// </summary>
    void Draw();
}
