namespace Umbra.Runtime;

/// <summary>
/// Owns a single live plugin instance and coordinates its mutex, startup, shutdown, and callback
/// dispatch.
/// </summary>
/// <typeparam name="TPlugin">The concrete plugin type.</typeparam>
/// <remarks>
/// This type centralizes the runtime behavior that previously lived in each plugin assembly's static
/// wrapper class. The assembly-specific wrapper still exists only to satisfy REFramework's static
/// entry-point requirements and supplies the mutex identity type decorated with
/// <see cref="UmbraPluginAttribute"/>.
/// </remarks>
public sealed class PluginHost<TPlugin>
    where TPlugin : class, IUmbraPlugin
{
    private readonly Type _identityType;
    private readonly Func<TPlugin> _factory;
    private volatile TPlugin? _instance;

    /// <summary>
    /// Initialises a new host for the specified plugin type.
    /// </summary>
    /// <param name="identityType">
    /// The assembly-facing host type decorated with <see cref="UmbraPluginAttribute"/>. This type
    /// defines the plugin's mutex identity independently from the instance implementation type.
    /// </param>
    /// <param name="factory">Creates the plugin instance when load succeeds.</param>
    public PluginHost(Type identityType, Func<TPlugin> factory)
    {
        ArgumentNullException.ThrowIfNull(identityType);
        ArgumentNullException.ThrowIfNull(factory);

        _identityType = identityType;
        _factory = factory;
    }

    /// <summary>
    /// Starts the plugin by acquiring its mutex and creating the live instance.
    /// </summary>
    /// <returns><see langword="true"/> when the plugin instance was created and initialised.</returns>
    public bool Load()
        => PluginBootstrapper.Load(_identityType, InitializeInstance);

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
            PluginBootstrapper.Unload(_identityType, instance.Shutdown);
        }
        finally
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Dispatches the ImGui pre-draw callback to the live instance, if present.
    /// </summary>
    public void OnPreImGuiDrawUI() => _instance?.OnPreImGuiDrawUI();

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
