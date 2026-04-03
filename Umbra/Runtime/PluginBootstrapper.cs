namespace Umbra.Runtime;

/// <summary>
/// Provides a higher-level plugin lifecycle wrapper that owns single-instance mutex acquisition and
/// release on behalf of plugin authors.
/// </summary>
/// <remarks>
/// <para>
/// Use this helper from a plugin's static <c>[PluginEntryPoint]</c> and <c>[PluginExitPoint]</c>
/// methods to keep the plugin class free of manual lease storage and disposal logic.
/// </para>
/// <para>
/// The convenience overloads delegate caller-type inference to <see cref="PluginCallerTypeResolver"/>,
/// acquire the mutex before running the load callback, release it automatically if initialization
/// fails, and always release it after the unload callback finishes.
/// </para>
/// </remarks>
public static class PluginBootstrapper
{
    /// <summary>
    /// Runs a plugin's initialization code under the single-instance guard inferred from the calling
    /// entry-point method.
    /// </summary>
    /// <param name="initialize">The plugin initialization callback.</param>
    /// <returns>
    /// <see langword="true"/> when the plugin acquired its mutex and the initialization callback ran;
    /// otherwise <see langword="false"/> when another instance already holds the mutex.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialize"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the calling method cannot be resolved to a class-based plugin host type.</exception>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static bool Load(Action initialize)
    {
        ArgumentNullException.ThrowIfNull(initialize);

        var pluginType = PluginCallerTypeResolver.ResolveCallingPluginType(
            typeof(PluginBootstrapper),
            nameof(Load),
            $"{nameof(Load)}(Type, Action)");

        return Load(pluginType, initialize);
    }

    /// <summary>
    /// Runs a plugin's initialization code under the single-instance guard for <paramref name="pluginType"/>.
    /// </summary>
    /// <param name="pluginType">
    /// The plugin identity type whose assembly contributes the mutex key used for single-instance
    /// enforcement.
    /// </param>
    /// <param name="initialize">The plugin initialization callback.</param>
    /// <returns>
    /// <see langword="true"/> when the plugin acquired its mutex and the initialization callback ran;
    /// otherwise <see langword="false"/> when another instance already holds the mutex.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pluginType"/> or <paramref name="initialize"/> is <see langword="null"/>.</exception>
    public static bool Load(Type pluginType, Action initialize)
    {
        ArgumentNullException.ThrowIfNull(pluginType);
        ArgumentNullException.ThrowIfNull(initialize);

        if (!PluginInstanceGuard.TryAcquire(pluginType, out var lease))
            return false;

        try
        {
            initialize();
            return true;
        }
        catch
        {
            lease.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Runs a plugin's shutdown code and then releases its single-instance mutex inferred from the
    /// calling entry-point method.
    /// </summary>
    /// <param name="cleanup">The plugin cleanup callback.</param>
    /// <remarks>
    /// The mutex is released even if <paramref name="cleanup"/> throws, so a later reload is not
    /// blocked by a partially failed shutdown path.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cleanup"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the calling method cannot be resolved to a class-based plugin host type.</exception>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void Unload(Action cleanup)
    {
        ArgumentNullException.ThrowIfNull(cleanup);

        var pluginType = PluginCallerTypeResolver.ResolveCallingPluginType(
            typeof(PluginBootstrapper),
            nameof(Unload),
            $"{nameof(Unload)}(Type, Action)");

        Unload(pluginType, cleanup);
    }

    /// <summary>
    /// Runs a plugin's shutdown code and then releases its single-instance mutex.
    /// </summary>
    /// <param name="pluginType">
    /// The plugin identity type whose assembly contributes the mutex key used for single-instance
    /// enforcement.
    /// </param>
    /// <param name="cleanup">The plugin cleanup callback.</param>
    /// <remarks>
    /// The mutex is released even if <paramref name="cleanup"/> throws, so a later reload is not
    /// blocked by a partially failed shutdown path.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pluginType"/> or <paramref name="cleanup"/> is <see langword="null"/>.</exception>
    public static void Unload(Type pluginType, Action cleanup)
    {
        ArgumentNullException.ThrowIfNull(pluginType);
        ArgumentNullException.ThrowIfNull(cleanup);

        try
        {
            cleanup();
        }
        finally
        {
            PluginInstanceGuard.Release(pluginType);
        }
    }
}
