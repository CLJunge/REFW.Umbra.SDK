namespace Umbra.Runtime;

/// <summary>
/// Provides utilities for resolving native game object addresses into managed
/// typed references.
/// </summary>
/// <remarks>
/// Production resolution is performed through an internal REFramework-backed bridge. Unit tests can
/// replace that bridge with a deterministic implementation so the resolver's control flow can be
/// verified without requiring the game runtime host.
/// </remarks>
public static class ManagedObjectResolver
{
    private static IManagedObjectBridge? _bridge;

    /// <summary>
    /// Resolves the native game object at <paramref name="address"/> to a
    /// strongly-typed managed reference, or <see langword="null"/> on failure.
    /// </summary>
    /// <remarks>
    /// This is a convenience wrapper over <see cref="TryResolve{T}(ulong, out T)"/> for call
    /// sites that prefer a nullable return value over an explicit success flag.
    /// </remarks>
    /// <typeparam name="T">
    /// The managed type to cast the resolved object to. Must be a reference type.
    /// </typeparam>
    /// <param name="address">
    /// The native memory address of the game object to resolve.
    /// </param>
    /// <returns>
    /// The resolved instance cast to <typeparamref name="T"/>, or
    /// <see langword="null"/> if the address is invalid or the object's runtime type
    /// is incompatible with <typeparamref name="T"/>.
    /// </returns>
    public static T? Resolve<T>(ulong address) where T : class
        => TryResolve(address, out T? value) ? value : null;

    /// <summary>
    /// Replaces the low-level bridge used to resolve managed objects.
    /// </summary>
    /// <remarks>
    /// This exists primarily for unit tests that need to exercise resolver behavior without loading
    /// the REFramework runtime assemblies or entering the game process.
    /// </remarks>
    /// <param name="bridge">The replacement bridge to use for subsequent resolutions.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="bridge"/> is <see langword="null"/>.
    /// </exception>
    internal static void SetBridge(IManagedObjectBridge bridge)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        Interlocked.Exchange(ref _bridge, bridge);
    }

    /// <summary>
    /// Restores the default REFramework-backed resolution bridge.
    /// </summary>
    internal static void ResetBridge()
    {
        Interlocked.Exchange(ref _bridge, null);
    }

    /// <summary>
    /// Returns the currently active resolution bridge, creating the default REFramework-backed
    /// bridge on first use.
    /// </summary>
    /// <returns>The bridge used for subsequent managed-object resolution.</returns>
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
    /// Attempts to resolve the native game object at <paramref name="address"/> to a
    /// strongly-typed managed reference.
    /// </summary>
    /// <remarks>
    /// Internally delegates to the active managed-object bridge, which uses
    /// <see cref="REFrameworkManagedObjectBridge"/> by default inside the game process.
    /// <para>
    /// The method returns <see langword="false"/> in three distinct cases:
    /// <list type="bullet">
    ///   <item><description>
    ///     <paramref name="address"/> is zero — returned immediately without entering the
    ///     exception-handling path.
    ///   </description></item>
    ///   <item><description>
    ///     The underlying bridge returns <see langword="false"/> — for example when the runtime
    ///     type of the object at <paramref name="address"/> is not compatible with
    ///     <typeparamref name="T"/>.
    ///   </description></item>
    ///   <item><description>
    ///     The underlying bridge throws — for example when <paramref name="address"/> is otherwise
    ///     invalid or the runtime host is unavailable — and the exception is swallowed so the
    ///     game-facing call site can stay on a simple failure path.
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This API is useful when callers need to distinguish success from failure without relying on
    /// a nullable return value alone.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The managed type to cast the resolved object to. Must be a reference type.
    /// </typeparam>
    /// <param name="address">The native memory address of the game object to resolve.</param>
    /// <param name="value">
    /// Receives the resolved instance cast to <typeparamref name="T"/> when the method returns
    /// <see langword="true"/>; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the object was resolved and cast successfully; otherwise
    /// <see langword="false"/>.
    /// </returns>
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
        catch
        {
            value = null;
            return false;
        }
    }
}
