namespace Umbra.UI.Config.Search;

/// <summary>
/// Stores optional built-in config-drawer search bar settings.
/// </summary>
/// <remarks>
/// When supplied as a non-<see langword="null"/> value to <see cref="ConfigDrawerOptions.Search"/>,
/// the built-in search bar is rendered with the configured settings. When <see langword="null"/>,
/// the search bar is hidden.
/// </remarks>
public sealed class ConfigSearchOptions
{
    /// <summary>
    /// The default maximum number of characters accepted by the search input field.
    /// </summary>
    public const uint DefaultMaxInputLength = 256;

    /// <summary>
    /// The default minimum pixel width of the search input field.
    /// </summary>
    public const float DefaultMinimumSearchInputWidth = 64f;

    private readonly uint _maxInputLength = DefaultMaxInputLength;
    private readonly float _minimumSearchInputWidth = DefaultMinimumSearchInputWidth;

    /// <summary>
    /// Gets the maximum number of characters accepted by the search input field.
    /// </summary>
    /// <remarks>
    /// When set to zero, <see cref="DefaultMaxInputLength"/> is used.
    /// </remarks>
    public uint MaxInputLength
    {
        get => _maxInputLength;
        init => _maxInputLength = value == 0 ? DefaultMaxInputLength : value;
    }

    /// <summary>
    /// Gets the minimum pixel width of the search input field.
    /// </summary>
    /// <remarks>
    /// When set to zero or a negative value, <see cref="DefaultMinimumSearchInputWidth"/> is used.
    /// </remarks>
    public float MinimumSearchInputWidth
    {
        get => _minimumSearchInputWidth;
        init => _minimumSearchInputWidth = value <= 0f ? DefaultMinimumSearchInputWidth : value;
    }
}
