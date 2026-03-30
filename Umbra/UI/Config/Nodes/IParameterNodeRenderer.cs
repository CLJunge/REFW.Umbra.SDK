namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level spacing operations required by <see cref="ParameterNode"/>.
/// </summary>
/// <remarks>
/// This seam isolates parameter-node spacing from native ImGui calls so unit tests can verify draw
/// ordering and spacing counts without requiring an active ImGui frame.
/// </remarks>
internal interface IParameterNodeRenderer
{
    /// <summary>
    /// Emits one spacing unit before or after a parameter draw action.
    /// </summary>
    void Spacing();
}
