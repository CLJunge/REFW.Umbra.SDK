namespace Umbra.Config;

/// <summary>
/// Reports the outcome of a config import attempt.
/// </summary>
public sealed class SettingsImportReport
{
    /// <summary>
    /// Gets or sets a value indicating whether the import operation completed without a document-level failure.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the imported document used Umbra's legacy flat `key -> value` JSON shape instead of the versioned exchange envelope.
    /// </summary>
    public bool IsLegacyDocument { get; init; }

    /// <summary>
    /// Gets or sets the imported document's schema identifier when one was available.
    /// </summary>
    public string? SchemaId { get; init; }

    /// <summary>
    /// Gets or sets the imported document's schema version when one was available.
    /// </summary>
    public int? SchemaVersion { get; init; }

    /// <summary>
    /// Gets or sets the number of keys successfully accepted by the current store.
    /// </summary>
    public int AppliedCount { get; init; }

    /// <summary>
    /// Gets or sets the number of keys ignored because they are not compatible with the current store shape.
    /// </summary>
    public int IgnoredCount { get; init; }

    /// <summary>
    /// Gets or sets the number of keys rejected because conversion or validation failed.
    /// </summary>
    public int RejectedCount { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the store persisted the accepted imported state after import.
    /// </summary>
    public bool Saved { get; set; }

    /// <summary>
    /// Gets or sets the document-level failure reason when the import did not complete successfully.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Gets or sets the per-key issues collected during import.
    /// </summary>
    public IReadOnlyList<SettingsImportIssue> Issues { get; init; } = [];
}
