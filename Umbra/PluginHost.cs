using Umbra.Input;
using Umbra.Logging;
using Umbra.UI.Toast;

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
    #pragma warning disable CS0649
    private static volatile TPlugin? _current;
#pragma warning restore CS0649

    private readonly Func<TPlugin> _factory;
    private volatile TPlugin? _instance;

    /// <summary>
    /// Gets the live plugin instance, or <see langword="null"/> when no instance is loaded.
    /// </summary>
    /// <value>
    /// The current <typeparamref name="TPlugin"/> instance between a successful <see cref="Load"/> and a subsequent <see cref="Unload"/>; otherwise, <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property is intended for static <c>[MethodHook]</c> methods and other static callbacks that need to forward game events to the instance-based plugin without holding a reference to the <see cref="PluginHost{TPlugin}"/> object. The backing field is <see langword="volatile"/>, so reads from any thread observe the most recently published value. Callers should use null-conditional access (<c>Current?.SomeMethod()</c>) so forwarded calls become safe no-ops after unload or before load completes.
    /// </remarks>
    public static TPlugin? Current => _current;

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
            _current = null;
            _instance = null;
        }
    }

    /// <summary>
    /// Updates the shared keyboard state tracker and forwards <see cref="IUmbraPlugin.OnPreUpdateBehavior"/> to the live plugin instance.
    /// </summary>
    /// <remarks>
    /// <see cref="KeyboardInput.Update"/> is called before the plugin callback so that edge
    /// state (pressed/released) is current for the entire tick. If no instance is currently
    /// loaded, the keyboard state is still updated so shared state stays consistent.
    /// </remarks>
    public void OnPreUpdateBehavior()
    {
        KeyboardInput.Update();
        _instance?.OnPreUpdateBehavior();
    }

    /// <summary>
    /// Forwards <see cref="IUmbraPlugin.OnPreImGuiDrawUI"/> to the live plugin instance.
    /// </summary>
    /// <remarks>
    /// If no instance is currently loaded, this method does nothing.
    /// </remarks>
    public void OnPreImGuiDrawUI() => _instance?.OnPreImGuiDrawUI();

    /// <summary>
    /// Forwards <see cref="IUmbraPlugin.OnPreImGuiRenderer"/> to the live plugin instance and then renders the shared toast overlay.
    /// </summary>
    /// <remarks>
    /// If no instance is currently loaded, this method does nothing.
    /// </remarks>
    public void OnPreImGuiRenderer()
    {
        var instance = _instance;
        if (instance is null)
            return;

        instance.OnPreImGuiRenderer();
        ToastOverlay.Draw();
    }

    /// <summary>
    /// Creates, initializes, and publishes the live plugin instance.
    /// </summary>
    private void InitializeInstance()
    {
        var instance = _factory();
        try
        {
            instance.Initialize();
            _instance = instance;
            _current = instance;
        }
        catch (Exception initializationException)
        {
            CleanupFailedInitialization(instance, initializationException);
            throw;
        }
    }

    /// <summary>
    /// Runs best-effort cleanup for a plugin instance whose <see cref="IUmbraPlugin.Initialize"/>
    /// call threw before the instance could be published.
    /// </summary>
    /// <param name="instance">The partially initialized plugin instance.</param>
    /// <param name="initializationException">The original initialization failure.</param>
    private static void CleanupFailedInitialization(TPlugin instance, Exception initializationException)
    {
        try
        {
            instance.Shutdown();
        }
        catch (Exception shutdownException)
        {
            Logger.Exception(
                shutdownException,
                "PluginHost<{0}>: Shutdown() threw while cleaning up a failed Initialize() call. Original Initialize() exception: {1}: {2}",
                typeof(TPlugin).Name,
                initializationException.GetType().Name,
                initializationException.Message);
        }
    }

    /// <summary>
    /// Clears the static <see cref="Current"/> reference.
    /// </summary>
    /// <remarks>
    /// This method exists for unit tests that need deterministic isolation between runs.
    /// </remarks>
    internal static void ResetCurrent() => _current = null;
}
