namespace Umbra;

/// <summary>
/// Represents an active single-instance lease registered for a plugin within the current process.
/// </summary>
/// <remarks>
/// <see cref="PluginInstanceGuard"/> creates these leases when acquisition succeeds. Disposing the lease removes its mutex key from the active registry so a later load from the same assembly can proceed. Disposal is idempotent.
/// </remarks>
internal sealed class PluginInstanceLease : IDisposable
{
    private int _disposed;

    internal PluginInstanceLease(Type pluginType, string mutexKey)
    {
        PluginType = pluginType;
        MutexKey = mutexKey;
    }

    /// <summary>
    /// Gets the plugin identity type that owns this lease.
    /// </summary>
    /// <value>The plugin identity type used to derive the mutex key.</value>
    public Type PluginType { get; }

    /// <summary>
    /// Gets the mutex key reserved by this lease.
    /// </summary>
    /// <value>The AppDomain-local key currently tracked by <see cref="PluginInstanceGuard"/>.</value>
    public string MutexKey { get; }

    /// <summary>
    /// Releases the mutex claim represented by this lease.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing.
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        PluginInstanceGuard.Release(this);
        GC.SuppressFinalize(this);
    }
}
