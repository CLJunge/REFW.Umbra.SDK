namespace Umbra.Runtime;

/// <summary>
/// Provides the public API for resolving native game object addresses into strongly typed managed references.
/// </summary>
/// <remarks>
/// Production resolution is delegated to an internal REFramework-backed bridge. Tests can replace that bridge with a deterministic implementation so resolver control flow can be exercised without the game runtime host.
/// </remarks>
public static class ManagedObjectResolver
{
    private static IManagedObjectBridge? _bridge;

    /// <summary>
    /// Gets or sets an optional observer that receives exceptions swallowed by <see cref="TryResolve{T}(ulong, out T)"/>.
    /// </summary>
    /// <remarks>
    /// This hook is intended for opt-in diagnostics when callers need visibility into bridge failures that Umbra still suppresses to keep game-facing code on a simple failure path. Exceptions thrown by the observer are swallowed as well.
    /// </remarks>
    public static Action<Exception>? SuppressedResolutionFailureObserver { get; set; }

    /// <summary>
    /// Resolves the native object at <paramref name="address"/> to a managed reference compatible with <typeparamref name="T"/>, or returns <see langword="null"/> on failure.
    /// </summary>
    /// <typeparam name="T">The managed reference type to request.</typeparam>
    /// <param name="address">The native memory address of the object to resolve.</param>
    /// <returns>The resolved managed instance, or <see langword="null"/> when resolution fails.</returns>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="TryResolve{T}(ulong, out T)"/> for call sites that prefer a nullable return value over an explicit success flag.
    /// </remarks>
    public static T? Resolve<T>(ulong address) where T : class
        => TryResolve(address, out T? value) ? value : null;

    /// <summary>
    /// Replaces the low-level bridge used for future managed-object resolutions.
    /// </summary>
    /// <param name="bridge">The replacement bridge.</param>
    /// <remarks>
    /// This method exists primarily for tests that need to exercise resolver behavior without loading the REFramework runtime assemblies.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bridge"/> is <see langword="null"/>.</exception>
    internal static void SetBridge(IManagedObjectBridge bridge)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        Interlocked.Exchange(ref _bridge, bridge);
    }

    /// <summary>
    /// Restores the default REFramework-backed bridge.
    /// </summary>
    internal static void ResetBridge() => Interlocked.Exchange(ref _bridge, null);

    /// <summary>
    /// Returns the currently active managed-object bridge, creating the default bridge on first use.
    /// </summary>
    /// <returns>The bridge used for subsequent resolutions.</returns>
    internal static IManagedObjectBridge GetBridge()
    {
        var bridge = Volatile.Read(ref _bridge);
        if (bridge != null)
            return bridge;

        bridge = new REFrameworkManagedObjectBridge();
        var existing = Interlocked.CompareExchange(ref _bridge, bridge, null);
        return existing ?? bridge;
    }

    /// <summary>
    /// Attempts to resolve the native object at <paramref name="address"/> to a managed reference compatible with <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The managed reference type to request.</typeparam>
    /// <param name="address">The native memory address of the object to resolve.</param>
    /// <param name="value">When this method returns <see langword="true"/>, contains the resolved managed instance; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the object was resolved and cast successfully; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// The method returns <see langword="false"/> when <paramref name="address"/> is zero, when the active bridge cannot resolve a compatible object, or when the active bridge throws and the exception is suppressed.
    /// </remarks>
    public static bool TryResolve<T>(ulong address, out T? value) where T : class
    {
        if (address == 0)
        {
            value = null;
            return false;
        }

        try
        {
            return GetBridge().TryResolve(address, out value);
        }
        catch (Exception ex)
        {
            ReportSuppressedResolutionFailure(ex);
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Reports a suppressed bridge exception to the optional observer.
    /// </summary>
    /// <param name="exception">The suppressed exception.</param>
    private static void ReportSuppressedResolutionFailure(Exception exception)
    {
        var observer = SuppressedResolutionFailureObserver;
        if (observer is null)
            return;

        try
        {
            observer(exception);
        }
        catch
        {
        }
    }
}
