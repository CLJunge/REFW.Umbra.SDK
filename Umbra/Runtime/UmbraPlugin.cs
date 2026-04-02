using Umbra.Logging;

namespace Umbra.Runtime;

/// <summary>
/// Provides a convenience base class for instance-based Umbra plugins.
/// </summary>
/// <remarks>
/// This base class supplies the plugin-scoped <see cref="PluginLogger"/> dependency and surfaces the
/// core Umbra lifecycle defined by the <see cref="IUmbraPlugin"/> contract. Derived plugins are free
/// to implement any additional REFramework callbacks they require on their own types.
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
    public virtual void Initialize() { }

    /// <inheritdoc/>
    public virtual void Shutdown() { }

    /// <inheritdoc/>
    public virtual void OnPreUpdateBehavior() { }

    /// <inheritdoc/>
    public virtual void OnPreImGuiDrawUI() { }

    /// <inheritdoc/>
    public virtual void OnPreImGuiRenderer() { }
}
