using Umbra.Logging;

namespace Umbra.Runtime;

/// <summary>
/// Provides a convenience base class for instance-based Umbra plugins.
/// </summary>
/// <remarks>
/// This base class supplies the plugin-scoped <see cref="PluginLogger"/> dependency and the core
/// Umbra lifecycle surface in addition to the <see cref="IUmbraPlugin"/> contract. Additional
/// REFramework engine callbacks remain optional through the interface's default no-op
/// <c>On&lt;CallbackName&gt;()</c> members, so derived plugins override only the callbacks they need.
/// Mutex metadata still belongs on the assembly-facing static host class via
/// <see cref="UmbraPluginAttribute"/>, not on the instance plugin type.
/// </remarks>
public abstract class UmbraPlugin : IUmbraPlugin
{
    /// <summary>
    /// Initialises a new plugin base with the logger supplied by the plugin instance.
    /// </summary>
    /// <param name="log">The plugin-specific logger.</param>
    protected UmbraPlugin(PluginLogger log)
    {
        ArgumentNullException.ThrowIfNull(log);
        Log = log;
    }

    /// <summary>
    /// Gets the plugin-specific logger supplied by the plugin instance.
    /// </summary>
    protected PluginLogger Log { get; }

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void Shutdown();

    /// <inheritdoc/>
    public abstract void OnPreImGuiDrawUI();
}
