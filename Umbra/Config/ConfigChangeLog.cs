namespace Umbra.Config;

/// <summary>
/// Thread-safe fixed-capacity circular buffer of <see cref="ConfigChangeRecord"/> entries.
/// </summary>
/// <remarks>
/// <para>
/// When the buffer is full, the oldest entry is overwritten. <see cref="GetEntries"/> returns
/// a snapshot ordered from oldest to newest. All public methods are thread-safe.
/// </para>
/// <para>
/// This type is the shared data structure used by both
/// <see cref="ConfigUndoStack{TConfig}"/> (through its own internal stack) and the
/// parameter change monitor UI to display a scrolling log of recent changes.
/// </para>
/// </remarks>
public sealed class ConfigChangeLog
{
    /// <summary>
    /// The default maximum number of entries retained in the log.
    /// </summary>
    public const int DefaultCapacity = 64;

    private readonly object _lock = new();
    private readonly ConfigChangeRecord[] _buffer;
    private int _head;
    private int _count;

    /// <summary>
    /// Initializes a new circular change log with the specified capacity.
    /// </summary>
    /// <param name="capacity">
    /// Maximum number of entries to retain. When exceeded, the oldest entry is overwritten.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> is less than 1.
    /// </exception>
    public ConfigChangeLog(int capacity = DefaultCapacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");

        _buffer = new ConfigChangeRecord[capacity];
    }

    /// <summary>
    /// Gets the maximum number of entries this log can hold.
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// Gets the current number of entries in the log.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
                return _count;
        }
    }

    /// <summary>
    /// Appends a change record to the log. When the log is at capacity, the oldest entry
    /// is overwritten.
    /// </summary>
    /// <param name="record">The change record to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/>.</exception>
    public void Push(ConfigChangeRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_lock)
        {
            _buffer[_head] = record;
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length)
                _count++;
        }
    }

    /// <summary>
    /// Returns a snapshot of all entries ordered from oldest to newest.
    /// </summary>
    /// <returns>A new list containing the current entries.</returns>
    public List<ConfigChangeRecord> GetEntries()
    {
        lock (_lock)
        {
            var result = new List<ConfigChangeRecord>(_count);
            if (_count == 0) return result;

            int start = _count < _buffer.Length ? 0 : _head;
            for (int i = 0; i < _count; i++)
            {
                int idx = (start + i) % _buffer.Length;
                result.Add(_buffer[idx]);
            }

            return result;
        }
    }

    /// <summary>
    /// Removes all entries from the log.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _head = 0;
            _count = 0;
        }
    }
}
