namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level ImGui ID-scope operations required by <see cref="IdScopeNode"/>.
/// </summary>
/// <remarks>
/// This seam isolates ID-scope control flow from native ImGui calls so unit tests can verify child
/// ordering and scope cleanup without requiring an active ImGui frame.
/// </remarks>
internal interface IIdScopeNodeRenderer
{
    /// <summary>
    /// Pushes the specified subtree ID scope.
    /// </summary>
    /// <param name="scopeId">The stable ID scope to push.</param>
    void PushId(string scopeId);

    /// <summary>
    /// Pops the current subtree ID scope.
    /// </summary>
    void PopId();
}
