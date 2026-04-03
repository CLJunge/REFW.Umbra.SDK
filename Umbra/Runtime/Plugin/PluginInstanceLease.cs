namespace Umbra.Runtime.Plugin;

/// <summary>
/// Represents an active single-instance claim held by a plugin within the current AppDomain.
/// </summary>
/// <remarks>
/// Dispose the lease from the plugin's <c>[PluginExitPoint]</c> method to release the mutex key and
/// allow a future load to acquire it again. Disposal is idempotent.
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
    /// Gets the plugin type that owns this lease.
    /// </summary>
    public Type PluginType { get; }

    /// <summary>
    /// Gets the mutex key reserved by this lease.
    /// </summary>
    public string MutexKey { get; }

    /// <summary>
    /// Releases the mutex claim represented by this lease.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        PluginInstanceGuard.Release(this);
        GC.SuppressFinalize(this);
    }
}
