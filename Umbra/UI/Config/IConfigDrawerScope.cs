namespace Umbra.UI.Config;

/// <summary>
/// Defines the ImGui ID-scope operations required by <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
/// <remarks>
/// This narrow seam isolates the outer draw scope from native ImGui calls so unit tests can
/// verify draw ordering and cleanup behavior without requiring an active ImGui frame.
/// </remarks>
internal interface IConfigDrawerScope
{
    /// <summary>
    /// Begins a scoped ID region for the current draw call.
    /// </summary>
    /// <param name="idScope">The unique scope identifier to push.</param>
    void PushId(string idScope);

    /// <summary>
    /// Ends the current scoped ID region.
    /// </summary>
    void PopId();
}
