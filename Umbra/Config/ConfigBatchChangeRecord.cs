using System.Diagnostics;

namespace Umbra.Config;

/// <summary>
/// Captures a batch of parameter value changes that should be undone as a single atomic operation.
/// </summary>
/// <remarks>
/// Created by <see cref="ConfigUndoStack{TConfig}.EndBatch"/> when one or more parameter
/// changes occurred between <see cref="ConfigUndoStack{TConfig}.BeginBatch"/> and
/// <see cref="ConfigUndoStack{TConfig}.EndBatch"/>. Counts as a single entry for
/// undo-stack capacity purposes.
/// </remarks>
internal sealed class ConfigBatchChangeRecord : IUndoEntry
{
    /// <summary>
    /// Initializes a new batch change record.
    /// </summary>
    /// <param name="batchLabel">The human-readable label describing the batch operation.</param>
    /// <param name="records">The individual change records collected during the batch.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="batchLabel"/> or <paramref name="records"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="batchLabel"/> is empty or whitespace, or <paramref name="records"/> is empty.
    /// </exception>
    internal ConfigBatchChangeRecord(string batchLabel, List<ConfigChangeRecord> records)
    {
        ArgumentNullException.ThrowIfNull(batchLabel);
        ArgumentNullException.ThrowIfNull(records);
        if (string.IsNullOrWhiteSpace(batchLabel))
            throw new ArgumentException("Batch label cannot be empty or whitespace.", nameof(batchLabel));
        if (records.Count == 0)
            throw new ArgumentException("Batch records cannot be empty.", nameof(records));

        BatchLabel = batchLabel;
        Records = records;
        Timestamp = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Gets the human-readable label describing the batch operation (e.g. "Reset All").
    /// </summary>
    internal string BatchLabel { get; }

    /// <summary>
    /// Gets the individual change records collected during the batch, ordered chronologically.
    /// </summary>
    internal List<ConfigChangeRecord> Records { get; }

    /// <inheritdoc/>
    public long Timestamp { get; }
}
