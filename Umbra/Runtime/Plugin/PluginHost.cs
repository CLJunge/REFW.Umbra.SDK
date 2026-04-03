namespace Umbra.Runtime.Plugin;

/// <summary>
/// Owns a single live plugin instance and coordinates its mutex, startup, shutdown, and callback
/// dispatch.
/// </summary>
/// <typeparam name="TPlugin">The concrete plugin type.</typeparam>
/// <remarks>
/// <para>
/// This type centralizes the runtime behavior that previously lived in each plugin assembly's static
/// wrapper class. The assembly-specific wrapper still exists only to satisfy REFramework's static
/// entry-point requirements; it delegates to this host for all lifecycle and callback behavior.
/// </para>
/// <para>
/// <typeparamref name="TPlugin"/> is used as the mutex identity type for single-instance
/// enforcement. The mutex key is derived from the assembly name of <typeparamref name="TPlugin"/>,
/// so all types from the same assembly share one key. Ensure that the plugin type's assembly name
/// is stable and unique across all plugins loaded in the same process session.
/// </para>
/// </remarks>
public sealed class PluginHost<TPlugin>
    where TPlugin : class, IUmbraPlugin
{
    private readonly Func<TPlugin> _factory;
    private volatile TPlugin? _instance;

    /// <summary>
    /// Initialises a new host for the specified plugin type, using <typeparamref name="TPlugin"/>
    /// as the mutex identity type for single-instance enforcement.
    /// </summary>
    /// <param name="factory">Creates the plugin instance when load succeeds.</param>
    public PluginHost(Func<TPlugin> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <summary>
    /// Starts the plugin by acquiring its mutex and creating the live instance.
    /// </summary>
    /// <returns><see langword="true"/> when the plugin instance was created and initialised.</returns>
    public bool Load()
        => PluginBootstrapper.Load(typeof(TPlugin), InitializeInstance);

    /// <summary>
    /// Stops the plugin, runs its shutdown logic, and releases its mutex.
    /// </summary>
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
    /// Invokes the pre-update behavior logic on the underlying instance, if available.
    /// </summary>
    /// <remarks>This method delegates to the underlying instance's OnPreUpdateBehavior method if the instance
    /// is not null. Call this method to perform any actions required before the main update behavior is
    /// executed.</remarks>
    public void OnPreUpdateBehavior() => _instance?.OnPreUpdateBehavior();

    /// <summary>
    /// Invokes the pre-draw logic for the ImGui UI, if an instance is available.
    /// </summary>
    /// <remarks>This method should be called before rendering the ImGui-based user interface to allow any
    /// necessary setup or state updates. If no instance is present, the method performs no action.</remarks>
    public void OnPreImGuiDrawUI() => _instance?.OnPreImGuiDrawUI();

    /// <summary>
    /// Invokes the pre-ImGui rendering logic for the current instance, if available.
    /// </summary>
    /// <remarks>Call this method before rendering ImGui UI elements for an ingame overlay.
    /// If no instance is set, this method has no effect.</remarks>
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
