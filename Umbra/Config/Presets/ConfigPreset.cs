namespace Umbra.Config.Presets;

/// <summary>
/// Represents a named config preset consisting of a snapshot of parameter values.
/// </summary>
/// <param name="Name">The user-assigned preset name.</param>
/// <param name="Values">The captured parameter values keyed by fully qualified persisted key.</param>
public sealed record ConfigPreset(string Name, Dictionary<string, object?> Values);
