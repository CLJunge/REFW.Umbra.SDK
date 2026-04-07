namespace Umbra.UI.ChangeMonitor;

/// <summary>
/// Stores optional change-monitor settings for a <see cref="ParameterChangeMonitorState"/>.
/// </summary>
/// <remarks>
/// This options class is used directly by the options-based overload of
/// <see cref="ParameterChangeMonitorState"/>.<c>Create</c>
/// rather than being composed into <c>ConfigDrawerOptions</c>, because the change monitor
/// is a <c>LiveStateSection</c> rather than a config-drawer feature.
/// </remarks>
public sealed class ConfigChangeMonitorOptions
{
    /// <summary>
    /// The default maximum number of entries retained in the change log.
    /// </summary>
    public const int DefaultLogCapacity = Umbra.Config.ConfigChangeLog.DefaultCapacity;

    /// <summary>
    /// The default display height (in pixels) for the scrollable change list.
    /// </summary>
    public const float DefaultDisplayHeight = 200f;

    private readonly int _logCapacity = DefaultLogCapacity;

    /// <summary>
    /// Gets the maximum number of entries retained in the change log.
    /// </summary>
    /// <remarks>
    /// When set to a value less than 1, <see cref="DefaultLogCapacity"/> is used.
    /// </remarks>
    public int LogCapacity
    {
        get => _logCapacity;
        init => _logCapacity = value < 1 ? DefaultLogCapacity : value;
    }

    private readonly float _displayHeight = DefaultDisplayHeight;

    /// <summary>
    /// Gets the height (in pixels) of the scrollable change-monitor child window.
    /// </summary>
    /// <remarks>
    /// When set to a value less than or equal to zero, <see cref="DefaultDisplayHeight"/> is used.
    /// </remarks>
    public float DisplayHeight
    {
        get => _displayHeight;
        init => _displayHeight = value <= 0f ? DefaultDisplayHeight : value;
    }
}
