using System.Diagnostics;

namespace Umbra.UI.Toast;

/// <summary>
/// Represents one queued toast notification with its creation timestamp and display duration.
/// </summary>
/// <param name="Message">The notification text.</param>
/// <param name="Level">The severity level that determines rendering color.</param>
/// <param name="CreatedAt">The <see cref="Stopwatch"/> timestamp captured when the entry was created.</param>
/// <param name="Duration">How long the toast remains visible after creation.</param>
public readonly record struct ToastEntry(string Message, ToastLevel Level, long CreatedAt, TimeSpan Duration)
{
    /// <summary>
    /// Determines whether this entry has exceeded its display duration.
    /// </summary>
    /// <returns><see langword="true"/> if the elapsed time since creation exceeds <see cref="Duration"/>; otherwise, <see langword="false"/>.</returns>
    public bool IsExpired() => Stopwatch.GetElapsedTime(CreatedAt) >= Duration;

    /// <summary>
    /// Gets the fraction of the display duration that has elapsed, clamped to [0, 1].
    /// </summary>
    /// <returns>A value between 0 (just created) and 1 (fully expired).</returns>
    public float GetProgress()
    {
        var elapsed = Stopwatch.GetElapsedTime(CreatedAt);
        if (Duration <= TimeSpan.Zero) return 1f;
        var ratio = elapsed / Duration;
        return ratio > 1.0 ? 1f : (float)ratio;
    }
}
