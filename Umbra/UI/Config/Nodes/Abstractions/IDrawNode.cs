namespace Umbra.UI.Config.Nodes;

/// <summary>Pre-built draw node that emits one frame of ImGui output.</summary>
internal interface IDrawNode
{
    /// <summary>Emits this node's ImGui calls for the current frame.</summary>
    void Draw();
}
