namespace Umbra.Config;

/// <summary>
/// Describes one non-fatal issue encountered while importing a config document.
/// </summary>
public sealed class SettingsImportIssue
{
    /// <summary>
    /// Gets or sets the issue category.
    /// </summary>
    /// <remarks>
    /// Typical values are `Ignored` and `Rejected`.
    /// </remarks>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the affected config key.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable issue message.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
