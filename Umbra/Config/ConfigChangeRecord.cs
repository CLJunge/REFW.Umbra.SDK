using System.Diagnostics;

namespace Umbra.Config;

/// <summary>
/// Captures a single parameter value change for undo and change-monitoring purposes.
/// </summary>
/// <param name="ParameterKey">The fully qualified persisted key of the changed parameter.</param>
/// <param name="DisplayLabel">The human-readable label resolved from parameter metadata.</param>
/// <param name="OldValue">The value before the change.</param>
/// <param name="NewValue">The value after the change.</param>
/// <param name="Timestamp">
/// The <see cref="Stopwatch"/>-based timestamp when the change was recorded,
/// obtained from <see cref="Stopwatch.GetTimestamp"/>.
/// </param>
public sealed record ConfigChangeRecord(
    string ParameterKey,
    string DisplayLabel,
    object? OldValue,
    object? NewValue,
    long Timestamp) : IUndoEntry;
