using System.Runtime.CompilerServices;

namespace Umbra;

/// <summary>
/// Runs plugin startup and shutdown callbacks under Umbra's AppDomain-local single-instance guard.
/// </summary>
/// <remarks>
/// The caller-inference overloads use <see cref="PluginCallerTypeResolver"/> to resolve the plugin identity type from the static entry-point method. Load paths acquire the mutex before invoking the supplied callback, and unload paths release the mutex in a <c>finally</c> block so later reloads are not blocked by a partially failed shutdown.
/// </remarks>
public static class PluginBootstrapper
{
    /// <summary>
    /// Runs plugin initialization under the single-instance guard inferred from the calling entry-point type.
    /// </summary>
    /// <param name="initialize">The callback that performs plugin initialization after the mutex has been acquired.</param>
    /// <returns><see langword="true"/> if the inferred plugin mutex was acquired and <paramref name="initialize"/> ran; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="initialize"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The calling method cannot be resolved to a class-based plugin identity type.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
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
    /// Runs plugin initialization under the single-instance guard for <paramref name="pluginType"/>.
    /// </summary>
    /// <param name="pluginType">The plugin identity type whose assembly contributes the mutex key.</param>
    /// <param name="initialize">The callback that performs plugin initialization after the mutex has been acquired.</param>
    /// <returns><see langword="true"/> if the mutex was acquired and <paramref name="initialize"/> ran; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// If another plugin instance already holds the mutex key, this method returns <see langword="false"/> and does not invoke <paramref name="initialize"/>. If <paramref name="initialize"/> throws after acquisition succeeds, the temporary lease is disposed before the exception is rethrown.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="pluginType"/> or <paramref name="initialize"/> is <see langword="null"/>.</exception>
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
    /// Runs plugin shutdown and releases the inferred single-instance mutex.
    /// </summary>
    /// <param name="cleanup">The callback that performs plugin shutdown before the mutex is released.</param>
    /// <remarks>
    /// The inferred mutex is released even if <paramref name="cleanup"/> throws, so a later reload is not blocked by a partially failed shutdown path.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="cleanup"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The calling method cannot be resolved to a class-based plugin identity type.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
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
    /// Runs plugin shutdown and releases the single-instance mutex for <paramref name="pluginType"/>.
    /// </summary>
    /// <param name="pluginType">The plugin identity type whose assembly contributes the mutex key.</param>
    /// <param name="cleanup">The callback that performs plugin shutdown before the mutex is released.</param>
    /// <remarks>
    /// The mutex is released even if <paramref name="cleanup"/> throws, so a later reload is not blocked by a partially failed shutdown path.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="pluginType"/> or <paramref name="cleanup"/> is <see langword="null"/>.</exception>
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
