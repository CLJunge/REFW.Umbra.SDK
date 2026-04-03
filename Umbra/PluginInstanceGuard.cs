using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Umbra.Logging;

namespace Umbra;

/// <summary>
/// Coordinates AppDomain-local single-instance enforcement for plugin identity types.
/// </summary>
/// <remarks>
/// This registry is process-local to the current managed plugin host. The preferred entry point for
/// plugin authors is <see cref="PluginBootstrapper"/>, which owns both acquisition and release.
/// <see cref="PluginInstanceLease"/> remains available for advanced or test-oriented scenarios where
/// explicit lease handling is desirable. Mutex keys are derived from the supplied plugin type's
/// assembly identity, so only the first successful acquisition for a given assembly key is allowed
/// to proceed. The convenience overload delegates caller-type inference to
/// <see cref="PluginCallerTypeResolver"/>.
/// </remarks>
internal static class PluginInstanceGuard
{
    private static readonly object _sync = new();
#pragma warning disable IDE0028
    private static readonly Dictionary<string, PluginInstanceLease> _activeLeases = new(StringComparer.Ordinal);
#pragma warning restore IDE0028

    /// <summary>
    /// Attempts to acquire the single-instance mutex for the decorated host type that owns the
    /// calling entry-point method.
    /// </summary>
    /// <remarks>
    /// This overload is intended for direct use inside a plugin's <c>[PluginEntryPoint]</c> method.
    /// It delegates caller-type inference to <see cref="PluginCallerTypeResolver"/>, then applies the
    /// same class validation and mutex-key resolution as
    /// <see cref="TryAcquire(Type, out PluginInstanceLease?)"/>.
    /// </remarks>
    /// <param name="lease">
    /// Receives the acquired lease when the method returns <see langword="true"/>; otherwise
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the caller's plugin mutex key was acquired successfully;
    /// otherwise <see langword="false"/> when another plugin instance already holds that key.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the caller cannot be resolved to a declaring plugin type, when that type is not a
    /// class, or when a mutex key cannot be derived from the supplied type.
    /// </exception>
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
    /// <param name="pluginType">
    /// The plugin identity type whose assembly contributes the mutex key.
    /// </param>
    /// <param name="lease">
    /// Receives the acquired lease when the method returns <see langword="true"/>; otherwise
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the mutex key was acquired successfully; otherwise
    /// <see langword="false"/> when another plugin instance already holds that key.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="pluginType"/> is not a class or when a mutex key cannot be
    /// derived from the supplied type.
    /// </exception>
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
    /// <param name="lease">The lease to release.</param>
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
    /// This is the release primitive used by <see cref="PluginBootstrapper"/> so plugin authors do
    /// not need to store or dispose a lease object in their own code.
    /// </remarks>
    /// <param name="pluginType">
    /// The plugin identity type whose assembly contributes the mutex key.
    /// </param>
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
    /// This exists for unit tests that need deterministic isolation between test methods.
    /// </remarks>
    internal static void Reset()
    {
        lock (_sync)
            _activeLeases.Clear();
    }

    /// <summary>
    /// Emits the duplicate-load warning through the global logger.
    /// </summary>
    /// <param name="pluginName">The plugin name used in the warning.</param>
    /// <param name="mutexKey">The mutex key that is already held.</param>
    /// <param name="ownerName">The plugin type currently holding the mutex key.</param>
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
    /// <returns>The fully-qualified type name when available; otherwise the simple type name.</returns>
    private static string GetTypeDisplayName(Type pluginType)
        => pluginType.FullName ?? pluginType.Name;
}
