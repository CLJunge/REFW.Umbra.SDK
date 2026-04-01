using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Umbra.Logging;

namespace Umbra.Runtime;

/// <summary>
/// Coordinates AppDomain-local single-instance enforcement for plugins decorated with
/// <see cref="UmbraPluginAttribute"/>.
/// </summary>
/// <remarks>
/// This registry is process-local to the current managed plugin host. The preferred entry point for
/// plugin authors is <see cref="PluginBootstrapper"/>, which owns both acquisition and release.
/// <see cref="PluginInstanceLease"/> remains available for advanced or test-oriented scenarios where
/// explicit lease handling is desirable. When two plugin types resolve to the same mutex key, only
/// the first successful acquisition is allowed to proceed.
/// </remarks>
public static class PluginInstanceGuard
{
    private static readonly object _sync = new();
    private static readonly Dictionary<string, PluginInstanceLease> _activeLeases = new(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to acquire the single-instance mutex for the plugin class that owns the calling
    /// entry-point method.
    /// </summary>
    /// <remarks>
    /// This overload is intended for direct use inside a plugin's <c>[PluginEntryPoint]</c> method.
    /// It inspects the immediate caller, resolves that method's declaring type, and then applies the
    /// same <see cref="UmbraPluginAttribute"/> validation and mutex-key resolution as
    /// <see cref="TryAcquire(Type, PluginLogger, out PluginInstanceLease?)"/>.
    /// </remarks>
    /// <param name="log">The logger used to emit duplicate-load warnings.</param>
    /// <param name="lease">
    /// Receives the acquired lease when the method returns <see langword="true"/>; otherwise
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the caller's plugin mutex key was acquired successfully;
    /// otherwise <see langword="false"/> when another plugin instance already holds that key.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the caller cannot be resolved to a declaring plugin type, when that type is not
    /// decorated with <see cref="UmbraPluginAttribute"/>, or when the declared mutex key is empty or whitespace.
    /// </exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryAcquire(PluginLogger log, [NotNullWhen(true)] out PluginInstanceLease? lease)
    {
        ArgumentNullException.ThrowIfNull(log);

        var callerMethod = new StackFrame(1, false).GetMethod()
            ?? throw new InvalidOperationException(
                $"Unable to resolve the calling method for {nameof(PluginInstanceGuard)}.{nameof(TryAcquire)}.");

        var pluginType = callerMethod.DeclaringType
            ?? throw new InvalidOperationException(
                $"Calling method '{callerMethod.Name}' does not declare a plugin type. " +
                $"Use {nameof(TryAcquire)}(Type, PluginLogger, out PluginInstanceLease?) when caller inference is not available.");

        return TryAcquire(pluginType, log, out lease);
    }

    /// <summary>
    /// Attempts to acquire the single-instance mutex declared by <paramref name="pluginType"/>.
    /// </summary>
    /// <param name="pluginType">The plugin class decorated with <see cref="UmbraPluginAttribute"/>.</param>
    /// <param name="log">The logger used to emit duplicate-load warnings.</param>
    /// <param name="lease">
    /// Receives the acquired lease when the method returns <see langword="true"/>; otherwise
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the mutex key was acquired successfully; otherwise
    /// <see langword="false"/> when another plugin instance already holds that key.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="pluginType"/> or <paramref name="log"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="pluginType"/> is not decorated with <see cref="UmbraPluginAttribute"/>,
    /// or when the declared mutex key is empty or whitespace.
    /// </exception>
    public static bool TryAcquire(Type pluginType, PluginLogger log, [NotNullWhen(true)] out PluginInstanceLease? lease)
    {
        ArgumentNullException.ThrowIfNull(pluginType);
        ArgumentNullException.ThrowIfNull(log);

        var mutexKey = ResolveMutexKey(pluginType);
        var pluginName = GetTypeDisplayName(pluginType);

        lock (_sync)
        {
            if (_activeLeases.TryGetValue(mutexKey, out var activeLease))
            {
                log.Warning(
                    "Skipped load for plugin '{0}' because mutex key '{1}' is already held by '{2}'.",
                    pluginName,
                    mutexKey,
                    GetTypeDisplayName(activeLease.PluginType));
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
    /// <param name="pluginType">The plugin class decorated with <see cref="UmbraPluginAttribute"/>.</param>
    internal static void Release(Type pluginType)
    {
        ArgumentNullException.ThrowIfNull(pluginType);

        var mutexKey = ResolveMutexKey(pluginType);

        lock (_sync)
        {
            if (!_activeLeases.TryGetValue(mutexKey, out var activeLease))
                return;

            if (!ReferenceEquals(activeLease.PluginType, pluginType))
                return;

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
    /// Resolves the mutex key contributed by the plugin type and its <see cref="UmbraPluginAttribute"/>.
    /// </summary>
    /// <param name="pluginType">The decorated plugin type.</param>
    /// <returns>The non-empty mutex key used for acquisition.</returns>
    private static string ResolveMutexKey(Type pluginType)
    {
        if (!pluginType.IsClass)
        {
            throw new InvalidOperationException(
                $"Plugin type '{GetTypeDisplayName(pluginType)}' must be a class to use [UmbraPlugin].");
        }

        var attribute = pluginType.GetCustomAttribute<UmbraPluginAttribute>(false)
            ?? throw new InvalidOperationException(
                $"Plugin type '{GetTypeDisplayName(pluginType)}' must be decorated with [UmbraPlugin] before calling {nameof(TryAcquire)}.");

        if (attribute.MutexKey is { } explicitMutexKey)
        {
            if (!string.IsNullOrWhiteSpace(explicitMutexKey))
                return explicitMutexKey;

            throw new InvalidOperationException(
                $"Plugin type '{GetTypeDisplayName(pluginType)}' declares [UmbraPlugin] with an empty or whitespace mutex key. " +
                "Supply a non-empty key or omit the constructor argument.");
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
