namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines the ImGui ID-scope push and pop operations used by the configuration UI pipeline.
/// </summary>
internal interface IIdScopeOps
{
    /// <summary>
    /// Pushes the specified ID scope.
    /// </summary>
    /// <param name="id">The scope identifier to push.</param>
    void PushId(string id);

    /// <summary>
    /// Pops the current ID scope.
    /// </summary>
    void PopId();
}
