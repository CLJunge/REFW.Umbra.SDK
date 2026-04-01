namespace Umbra.Runtime;

/// <summary>
/// Declares Umbra-specific metadata for a plugin class and optionally places that plugin in a
/// single-instance mutex group within the current AppDomain.
/// </summary>
/// <remarks>
/// Apply this attribute to the static class that owns the REFramework entry and exit points. When
/// <see cref="PluginInstanceGuard"/> acquires a lease for the decorated type, the guard uses
/// <see cref="MutexKey"/> when provided; otherwise it derives a default key from the plugin's
/// assembly identity.
/// </remarks>
/// <param name="mutexKey">
/// An optional explicit mutex key shared by all plugin types that must be mutually exclusive. When
/// <see langword="null"/>, <see cref="PluginInstanceGuard"/> derives a default key from the plugin
/// assembly name.
/// </param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class UmbraPluginAttribute(string? mutexKey = null) : Attribute
{
    /// <summary>
    /// Gets the explicit mutex key that should be used for single-instance enforcement, or
    /// <see langword="null"/> when the guard should derive a default key from the plugin type.
    /// </summary>
    public string? MutexKey { get; } = mutexKey;
}
