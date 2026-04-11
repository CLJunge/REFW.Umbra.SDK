namespace Umbra;

/// <summary>
/// Provides an immutable <see cref="PluginStatus"/> snapshot from a plugin host.
/// </summary>
/// <remarks>
/// This non-generic interface allows <see cref="PluginRegistry"/> to hold heterogeneous
/// <see cref="PluginHost{TPlugin}"/> instances and query their status without knowing
/// the concrete <c>TPlugin</c> type.
/// </remarks>
public interface IPluginStatusProvider
{
    /// <summary>
    /// Returns an immutable snapshot of the provider's current plugin metadata and lifecycle state.
    /// </summary>
    /// <returns>A <see cref="PluginStatus"/> describing the plugin at the moment of the call.</returns>
    PluginStatus GetStatus();
}
