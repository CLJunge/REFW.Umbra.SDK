using Umbra.Logging;

namespace Umbra;

/// <summary>
/// Provides a base implementation of <see cref="IUmbraPlugin"/> for instance-based plugins that need a per-plugin <see cref="PluginLogger"/>.
/// </summary>
/// <remarks>
/// This base class stores the logger supplied by the concrete plugin and leaves all lifecycle methods as overridable no-ops. It is typically paired with <see cref="PluginHost{TPlugin}"/> or another assembly-facing static host that satisfies REFramework's static entry-point requirements.
/// </remarks>
public abstract class UmbraPlugin : IUmbraPlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbraPlugin"/> class.
    /// </summary>
    /// <param name="log">The logger scoped to the derived plugin instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
    protected UmbraPlugin(PluginLogger log)
    {
        ArgumentNullException.ThrowIfNull(log);
        Log = log;
    }

    /// <summary>
    /// Gets the logger scoped to this plugin instance.
    /// </summary>
    /// <value>The logger supplied to the constructor.</value>
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
