namespace Umbra;

/// <summary>
/// An immutable snapshot of a plugin's identity metadata and current lifecycle state.
/// </summary>
/// <param name="Name">The human-readable display name of the plugin.</param>
/// <param name="Version">The version string of the plugin, or <see langword="null"/> when unavailable.</param>
/// <param name="Author">The author of the plugin, or <see langword="null"/> when undeclared.</param>
/// <param name="State">The lifecycle state at the time the snapshot was taken.</param>
/// <param name="LastError">The exception from the most recent initialization failure, or <see langword="null"/>.</param>
/// <param name="LoadedAt">The UTC timestamp at which the plugin completed initialization, or <see langword="null"/>.</param>
public readonly record struct PluginStatus(
    string Name,
    string? Version,
    string? Author,
    PluginState State,
    Exception? LastError,
    DateTimeOffset? LoadedAt);
