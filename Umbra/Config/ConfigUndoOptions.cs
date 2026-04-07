namespace Umbra.Config;

/// <summary>
/// Stores optional undo-stack settings for a config drawer or section.
/// </summary>
/// <remarks>
/// When supplied as a non-<see langword="null"/> value to <see cref="UI.Config.ConfigDrawerOptions.Undo"/>,
/// the undo stack is created with the configured settings. When <see langword="null"/>,
/// the undo feature is disabled.
/// </remarks>
public sealed class ConfigUndoOptions
{
    /// <summary>
    /// The default maximum number of change records retained before the oldest is dropped.
    /// </summary>
    public const int DefaultCapacity = 32;

    /// <summary>
    /// The default value for <see cref="ShowToastOnUndo"/>.
    /// </summary>
    public const bool DefaultShowToastOnUndo = true;

    private readonly int _capacity = DefaultCapacity;

    /// <summary>
    /// Gets the maximum number of change records to retain on the undo stack.
    /// </summary>
    /// <remarks>
    /// When set to a value less than 1, <see cref="DefaultCapacity"/> is used.
    /// </remarks>
    public int Capacity
    {
        get => _capacity;
        init => _capacity = value < 1 ? DefaultCapacity : value;
    }

    /// <summary>
    /// Gets a value indicating whether a toast notification is displayed after a successful undo.
    /// </summary>
    public bool ShowToastOnUndo { get; init; } = DefaultShowToastOnUndo;
}
