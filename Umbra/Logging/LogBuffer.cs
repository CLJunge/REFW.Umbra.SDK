namespace Umbra.Logging;

/// <summary>
/// Thread-safe fixed-capacity circular buffer of <see cref="LogEntry"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// When the buffer is full, the oldest entry is silently overwritten. All public methods
/// are thread-safe. <see cref="GetEntries(IList{LogEntry})"/> returns entries ordered from
/// oldest to newest.
/// </para>
/// <para>
/// This type is designed to capture log messages for in-game display without unbounded
/// allocation. The capacity is fixed at construction time and cannot be changed.
/// </para>
/// </remarks>
public sealed class LogBuffer
{
    /// <summary>
    /// The default maximum number of entries retained in the buffer.
    /// </summary>
    public const int DefaultCapacity = 256;

    private readonly Lock _lock = new();
    private readonly LogEntry[] _buffer;
    private int _head;
    private int _count;

    /// <summary>
    /// Initializes a new circular log buffer with the specified capacity.
    /// </summary>
    /// <param name="capacity">Maximum number of entries to retain. When exceeded, the oldest entry is overwritten.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1.</exception>
    public LogBuffer(int capacity = DefaultCapacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");

        _buffer = new LogEntry[capacity];
    }

    /// <summary>
    /// Gets the maximum number of entries this buffer can hold.
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// Gets the current number of entries in the buffer.
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
    /// Records a new log entry in the buffer.
    /// </summary>
    /// <param name="level">The severity level of the message.</param>
    /// <param name="message">The fully formatted log message.</param>
    public void Add(LogLevel level, string message)
    {
        var entry = new LogEntry(level, message, DateTimeOffset.UtcNow);
        lock (_lock)
        {
            _buffer[_head] = entry;
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length)
                _count++;
        }
    }

    /// <summary>
    /// Copies all buffered entries into <paramref name="destination"/> ordered from oldest to newest.
    /// </summary>
    /// <param name="destination">The caller-supplied list that receives the entries. Existing contents are not cleared.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <see langword="null"/>.</exception>
    public void GetEntries(IList<LogEntry> destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        lock (_lock)
        {
            if (_count == 0)
                return;

            var start = _count < _buffer.Length ? 0 : _head;
            for (var i = 0; i < _count; i++)
                destination.Add(_buffer[(start + i) % _buffer.Length]);
        }
    }

    /// <summary>
    /// Removes all entries from the buffer.
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
