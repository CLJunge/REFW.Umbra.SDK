namespace Umbra;

/// <summary>
/// Describes the current lifecycle phase of a plugin managed by <see cref="PluginHost{TPlugin}"/>.
/// </summary>
public enum PluginState
{
    /// <summary>
    /// The plugin is not loaded. This is the initial state and the state after a successful unload.
    /// </summary>
    Unloaded,

    /// <summary>
    /// The plugin is currently executing its <see cref="IUmbraPlugin.Initialize"/> method.
    /// </summary>
    Loading,

    /// <summary>
    /// The plugin initialized successfully and is actively receiving callbacks.
    /// </summary>
    Loaded,

    /// <summary>
    /// The plugin's <see cref="IUmbraPlugin.Initialize"/> method threw an exception.
    /// </summary>
    Failed,

    /// <summary>
    /// The plugin is currently executing its <see cref="IUmbraPlugin.Shutdown"/> method.
    /// </summary>
    Unloading
}
