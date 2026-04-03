using Umbra.Runtime;

namespace Umbra;

/// <summary>
/// Owns one live <typeparamref name="TPlugin"/> instance and coordinates single-instance startup, shutdown, and callback forwarding.
/// </summary>
/// <typeparam name="TPlugin">The concrete plugin type created by this host.</typeparam>
/// <remarks>
/// <para>
/// This type centralizes the runtime behavior that otherwise lives in an assembly-facing static wrapper required by REFramework. The wrapper delegates lifecycle and forwarded callbacks to this host, while the live plugin state remains on the <typeparamref name="TPlugin"/> instance.
/// </para>
/// <para>
/// <typeparamref name="TPlugin"/> is also used as the mutex identity type for <see cref="PluginBootstrapper"/>. The mutex key is derived from the assembly identity of <typeparamref name="TPlugin"/>, so types from the same assembly participate in the same single-instance guard.
/// </para>
/// </remarks>
public sealed class PluginHost<TPlugin>
    where TPlugin : class, IUmbraPlugin
{
    private readonly Func<TPlugin> _factory;
    private volatile TPlugin? _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginHost{TPlugin}"/> class.
    /// </summary>
    /// <param name="factory">The callback that creates the plugin instance after the mutex has been acquired.</param>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
    public PluginHost(Func<TPlugin> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <summary>
    /// Acquires the plugin mutex, creates the live instance, and runs <see cref="IUmbraPlugin.Initialize"/>.
    /// </summary>
    /// <remarks>
    /// If another plugin instance already holds the mutex for <typeparamref name="TPlugin"/>'s assembly, this method returns <see langword="false"/> and does not invoke the factory. If initialization throws, <see cref="PluginBootstrapper"/> releases the acquired mutex before the exception is rethrown.
    /// </remarks>
    /// <returns><see langword="true"/> if the plugin instance was created and initialized; otherwise, <see langword="false"/>.</returns>
    public bool Load()
        => PluginBootstrapper.Load(typeof(TPlugin), InitializeInstance);

    /// <summary>
    /// Runs <see cref="IUmbraPlugin.Shutdown"/> for the live instance and releases its mutex.
    /// </summary>
    /// <remarks>
    /// If <see cref="Load"/> has not published an instance, this method does nothing. The cached instance reference is cleared even when shutdown throws so later forwarded callbacks become safe no-ops.
    /// </remarks>
    public void Unload()
    {
        var instance = _instance;
        if (instance is null)
            return;

        try
        {
            PluginBootstrapper.Unload(typeof(TPlugin), instance.Shutdown);
        }
        finally
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Forwards <see cref="IUmbraPlugin.OnPreUpdateBehavior"/> to the live plugin instance.
    /// </summary>
    /// <remarks>
    /// If no instance is currently loaded, this method does nothing.
    /// </remarks>
    public void OnPreUpdateBehavior() => _instance?.OnPreUpdateBehavior();

    /// <summary>
    /// Forwards <see cref="IUmbraPlugin.OnPreImGuiDrawUI"/> to the live plugin instance.
    /// </summary>
    /// <remarks>
    /// If no instance is currently loaded, this method does nothing.
    /// </remarks>
    public void OnPreImGuiDrawUI() => _instance?.OnPreImGuiDrawUI();

    /// <summary>
    /// Forwards <see cref="IUmbraPlugin.OnPreImGuiRenderer"/> to the live plugin instance.
    /// </summary>
    /// <remarks>
    /// If no instance is currently loaded, this method does nothing.
    /// </remarks>
    public void OnPreImGuiRenderer() => _instance?.OnPreImGuiRenderer();

    /// <summary>
    /// Creates, initializes, and publishes the live plugin instance.
    /// </summary>
    private void InitializeInstance()
    {
        var instance = _factory();
        instance.Initialize();
        _instance = instance;
    }
}
