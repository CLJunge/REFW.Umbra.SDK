namespace Umbra.Config;

/// <summary>
/// Represents a single entry on the undo stack. Implemented by both
/// <see cref="ConfigChangeRecord"/> (single-parameter change) and
/// <see cref="ConfigBatchChangeRecord"/> (multi-parameter batch).
/// </summary>
internal interface IUndoEntry
{
    /// <summary>
    /// Gets the <see cref="System.Diagnostics.Stopwatch"/>-based timestamp when the entry was recorded.
    /// </summary>
    long Timestamp { get; }
}
