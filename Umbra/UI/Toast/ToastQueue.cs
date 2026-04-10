using System.Diagnostics;

namespace Umbra.UI.Toast;

/// <summary>
/// Thread-safe queue of timed toast notifications consumed by the overlay renderer.
/// </summary>
/// <remarks>
/// Push from any thread (hooks, callbacks, UI). The renderer calls <see cref="GetActiveEntries"/>
/// once per frame to receive the current visible set; expired entries are pruned automatically.
/// </remarks>
public static class ToastQueue
{
    private const int _maxCapacity = 8;

    /// <summary>
    /// The default display duration when none is specified.
    /// </summary>
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(3);

    private static readonly Lock _lock = new();
    private static readonly List<ToastEntry> _entries = [];

    /// <summary>
    /// Enqueues a toast notification.
    /// </summary>
    /// <param name="message">The notification text.</param>
    /// <param name="level">The severity level. Defaults to <see cref="ToastLevel.Info"/>.</param>
    /// <param name="duration">
    /// How long the toast stays visible. Defaults to <see cref="DefaultDuration"/> when <see langword="null"/>.
    /// </param>
    public static void Push(string message, ToastLevel level = ToastLevel.Info, TimeSpan? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var entry = new ToastEntry(message, level, Stopwatch.GetTimestamp(), duration ?? DefaultDuration);

        lock (_lock)
        {
            _entries.Add(entry);
            TrimOldest();
        }
    }

    /// <summary>
    /// Returns a snapshot of the currently active (non-expired) entries and prunes expired ones.
    /// </summary>
    /// <returns>A list of active entries ordered oldest-first.</returns>
    public static List<ToastEntry> GetActiveEntries()
    {
        lock (_lock)
        {
            PruneExpired();
            return [.. _entries];
        }
    }

    /// <summary>
    /// Removes all entries from the queue.
    /// </summary>
    public static void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }

    /// <summary>
    /// Gets the current number of entries in the queue (including expired ones not yet pruned).
    /// </summary>
    internal static int Count
    {
        get
        {
            lock (_lock)
            {
                return _entries.Count;
            }
        }
    }

    private static void PruneExpired()
    {
        for (var i = _entries.Count - 1; i >= 0; i--)
        {
            if (_entries[i].IsExpired())
                _entries.RemoveAt(i);
        }
    }

    private static void TrimOldest()
    {
        while (_entries.Count > _maxCapacity)
            _entries.RemoveAt(0);
    }
}
