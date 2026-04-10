using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Umbra.Logging;

namespace Umbra;

/// <summary>
/// Tracks active single-instance leases for plugin identity types inside the current managed plugin host.
/// </summary>
/// <remarks>
/// <see cref="PluginBootstrapper"/> is the preferred entry point for plugin authors because it owns both acquisition and release. This registry is process-local to the current AppDomain, and mutex keys are derived from the supplied plugin type's assembly identity with type-name fallbacks when the assembly name is unavailable.
/// </remarks>
internal static class PluginInstanceGuard
{
    private static readonly Lock _sync = new();
    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Code Cleanup tries to use collection initializer syntax preview features; production code avoids preview syntax")]
    private static readonly Dictionary<string, PluginInstanceLease> _activeLeases = new(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to acquire the single-instance mutex for the plugin type that owns the calling entry-point method.
    /// </summary>
    /// <remarks>
    /// This overload delegates caller-type inference to <see cref="PluginCallerTypeResolver"/>, then applies the same class validation and mutex-key resolution as <see cref="TryAcquire(Type, out PluginInstanceLease?)"/>.
    /// </remarks>
    /// <param name="lease">When this method returns, contains the acquired lease if the method returned <see langword="true"/>; otherwise, <see langword="null"/>. This parameter is treated as uninitialized.</param>
    /// <returns><see langword="true"/> if the caller's plugin mutex key was acquired; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">The caller cannot be resolved to a declaring plugin type, the resolved type is not a class, or no mutex key can be derived from that type.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryAcquire([NotNullWhen(true)] out PluginInstanceLease? lease)
    {
        var pluginType = PluginCallerTypeResolver.ResolveCallingPluginType(
            typeof(PluginInstanceGuard),
            nameof(TryAcquire),
            $"{nameof(TryAcquire)}(Type, out PluginInstanceLease?)");

        return TryAcquire(pluginType, out lease);
    }

    /// <summary>
    /// Attempts to acquire the single-instance mutex declared by <paramref name="pluginType"/>.
    /// </summary>
    /// <param name="pluginType">The plugin identity type whose assembly contributes the mutex key.</param>
    /// <param name="lease">When this method returns, contains the acquired lease if the method returned <see langword="true"/>; otherwise, <see langword="null"/>. This parameter is treated as uninitialized.</param>
    /// <returns><see langword="true"/> if the mutex key was acquired; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pluginType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="pluginType"/> is not a class or no mutex key can be derived from it.</exception>
    public static bool TryAcquire(Type pluginType, [NotNullWhen(true)] out PluginInstanceLease? lease)
    {
        ArgumentNullException.ThrowIfNull(pluginType);

        var mutexKey = ResolveMutexKey(pluginType);
        var pluginName = GetTypeDisplayName(pluginType);

        lock (_sync)
        {
            if (_activeLeases.TryGetValue(mutexKey, out var activeLease))
            {
                WarnDuplicateLoad(pluginName, mutexKey, GetTypeDisplayName(activeLease.PluginType));
                lease = null;
                return false;
            }

            lease = new PluginInstanceLease(pluginType, mutexKey);
            _activeLeases.Add(mutexKey, lease);
            return true;
        }
    }

    /// <summary>
    /// Releases an active lease when the owning plugin unloads.
    /// </summary>
    /// <param name="lease">The lease to remove if it still matches the registry entry for its mutex key.</param>
    internal static void Release(PluginInstanceLease lease)
    {
        lock (_sync)
        {
            if (_activeLeases.TryGetValue(lease.MutexKey, out var activeLease)
                && ReferenceEquals(activeLease, lease))
            {
                _activeLeases.Remove(lease.MutexKey);
            }
        }
    }

    /// <summary>
    /// Releases any active lease held by <paramref name="pluginType"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="PluginBootstrapper"/> uses this overload so plugin authors do not need to store or dispose a <see cref="PluginInstanceLease"/> themselves.
    /// </remarks>
    /// <param name="pluginType">The plugin identity type whose assembly contributes the mutex key.</param>
    /// <exception cref="ArgumentNullException"><paramref name="pluginType"/> is <see langword="null"/>.</exception>
    internal static void Release(Type pluginType)
    {
        ArgumentNullException.ThrowIfNull(pluginType);

        var mutexKey = ResolveMutexKey(pluginType);

        lock (_sync)
        {
            _activeLeases.Remove(mutexKey);
        }
    }

    /// <summary>
    /// Clears all active leases.
    /// </summary>
    /// <remarks>
    /// This method exists for unit tests that need deterministic isolation between runs.
    /// </remarks>
    internal static void Reset()
    {
        lock (_sync)
            _activeLeases.Clear();
    }

    /// <summary>
    /// Emits the duplicate-load warning through the global logger.
    /// </summary>
    /// <param name="pluginName">The plugin name shown in the warning.</param>
    /// <param name="mutexKey">The mutex key that is already held.</param>
    /// <param name="ownerName">The plugin type currently holding <paramref name="mutexKey"/>.</param>
    private static void WarnDuplicateLoad(string pluginName, string mutexKey, string ownerName)
    {
        Logger.Warning(
            "Skipped load for plugin '{0}' because mutex key '{1}' is already held by '{2}'.",
            pluginName,
            mutexKey,
            ownerName);
    }

    /// <summary>
    /// Resolves the mutex key contributed by the supplied plugin identity type.
    /// </summary>
    /// <param name="pluginType">The plugin identity type.</param>
    /// <returns>The non-empty mutex key used for acquisition.</returns>
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Guard clauses with early returns are more readable than a single chained conditional expression for multi-condition validation.")]
    private static string ResolveMutexKey(Type pluginType)
    {
        if (!pluginType.IsClass)
        {
            throw new InvalidOperationException(
                $"Plugin type '{GetTypeDisplayName(pluginType)}' must be a class to participate in single-instance enforcement.");
        }

        var assemblyName = pluginType.Assembly.GetName().Name;
        if (!string.IsNullOrWhiteSpace(assemblyName))
            return assemblyName;

        var fullName = pluginType.FullName;
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName;

        if (!string.IsNullOrWhiteSpace(pluginType.Name))
            return pluginType.Name;

        throw new InvalidOperationException("Unable to derive a plugin mutex key from the supplied plugin type.");
    }

    /// <summary>
    /// Returns a stable display name for diagnostic messages.
    /// </summary>
    /// <param name="pluginType">The plugin type to format.</param>
    /// <returns>The fully qualified type name when available; otherwise, the simple type name.</returns>
    private static string GetTypeDisplayName(Type pluginType)
        => pluginType.FullName ?? pluginType.Name;
}
