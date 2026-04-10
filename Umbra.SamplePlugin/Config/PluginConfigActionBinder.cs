using Umbra.Config;
using Umbra.Logging;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Binds live runtime actions to the sample plugin's delegate-backed config parameters after the
/// config store has loaded.
/// </summary>
/// <remarks>
/// Action wiring runs immediately after config load and wires actions that only need the store and
/// logger. Batch-undo wrapping is handled automatically by <see cref="ConfigUndoStack{TConfig}"/>
/// via <see cref="Attributes.UmbraBatchUndoAttribute"/> on the reset properties.
/// </remarks>
internal static class PluginConfigActionBinder
{
    /// <summary>
    /// Wires the sample plugin's action parameters to the current runtime services.
    /// </summary>
    /// <param name="config">The loaded sample-plugin config instance.</param>
    /// <param name="store">The loaded config store that owns <paramref name="config"/>.</param>
    /// <param name="log">The plugin-scoped logger used by the sample actions.</param>
    public static void Bind(PluginConfig config, ConfigStore<PluginConfig> store, PluginLogger log)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(log);

        config.LogTestMessage.Value = () => log.Info("Sample Plugin is active!");
        config.ResetAllSamples.Value = store.ResetAll;
    }
}
