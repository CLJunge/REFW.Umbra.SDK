using Umbra.Config;
using Umbra.Logging;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Binds live runtime actions to the sample plugin's delegate-backed config parameters after the
/// config store has loaded.
/// </summary>
/// <remarks>
/// <para>
/// Action wiring happens in two phases. <see cref="Bind"/> runs immediately after config load
/// and wires actions that only need the store and logger. <see cref="BindBatchUndo"/> runs
/// after the <see cref="ConfigUndoStack{TConfig}"/> is available (e.g. from
/// <see cref="Umbra.UI.Config.ConfigSection{TConfig}.UndoStack"/>) and wraps existing reset
/// delegates with batch-undo boundaries so each reset button becomes a single atomic undo step.
/// </para>
/// <para>
/// This two-phase approach keeps the config types entirely free of undo-stack knowledge.
/// </para>
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

    /// <summary>
    /// Wraps existing per-category and global reset actions with batch-undo boundaries so each
    /// reset button produces a single atomic undo entry.
    /// </summary>
    /// <remarks>
    /// Call this after the undo stack is available. Each reset delegate that was wired in
    /// <see cref="Bind"/> or in the config constructors is replaced by a wrapped version that
    /// brackets the original action with
    /// <see cref="ConfigUndoStack{TConfig}.BeginBatch"/> / <see cref="ConfigUndoStack{TConfig}.EndBatch"/>.
    /// </remarks>
    /// <param name="config">The loaded sample-plugin config instance.</param>
    /// <param name="undoStack">The undo stack that provides batch-undo wrapping.</param>
    public static void BindBatchUndo(PluginConfig config, ConfigUndoStack<PluginConfig> undoStack)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(undoStack);

        config.ResetAllSamples.Value = undoStack.WrapWithBatch("Reset All Samples", config.ResetAllSamples.Value!);
        config.General.ResetGeneral.Value = undoStack.WrapWithBatch("Reset General", config.General.ResetGeneral.Value!);
        config.Booleans.ResetBooleans.Value = undoStack.WrapWithBatch("Reset Booleans", config.Booleans.ResetBooleans.Value!);
    }
}
