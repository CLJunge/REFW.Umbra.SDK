using Umbra.Config;
using Umbra.Config.Presets;
using Umbra.UI.Config.Search;
using Umbra.UI.Config.Transfer;

namespace Umbra.UI.Config;

/// <summary>
/// Stores optional configuration-drawer behavior flags.
/// </summary>
/// <remarks>
/// This options object exists so new drawer-level behaviors can be added without expanding constructor
/// parameter lists with unrelated Boolean flags. All options default to the current behavior so existing
/// call sites remain unchanged unless they opt in explicitly.
/// </remarks>
public sealed class ConfigDrawerOptions
{
    internal static ConfigDrawerOptions Default { get; } = new()
    {
        Search = null,
        SuppressRootNode = false,
        Transfer = null,
        Undo = null,
        UndoInputSource = null,
        NumericEditUndoSink = null,
        Presets = null
    };

    /// <summary>
    /// Gets or sets the optional built-in search bar settings.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the built-in search bar is hidden.
    /// When non-<see langword="null"/>, the search bar is rendered with the configured settings.
    /// </remarks>
    public ConfigSearchOptions? Search { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the root-node-attribute-driven root tree wrapper is suppressed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to suppress the root tree wrapper even when the configuration type would
    /// otherwise render one; otherwise, <see langword="false"/>.
    /// </value>
    public bool SuppressRootNode { get; init; }

    /// <summary>
    /// Gets or sets the optional built-in config transfer UI settings.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/> or when <see cref="ConfigTransferOptions.Enabled"/> is <see langword="false"/>,
    /// the wrapped drawer or section renders no built-in config transfer UI.
    /// </remarks>
    public ConfigTransferOptions? Transfer { get; init; }

    /// <summary>
    /// Gets or sets the optional undo-stack settings.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the undo feature is disabled.
    /// When non-<see langword="null"/>, an undo stack is created with the configured settings.
    /// </remarks>
    public ConfigUndoOptions? Undo { get; init; }

    /// <summary>
    /// Gets or sets the optional preset-store settings.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the preset feature is disabled.
    /// When non-<see langword="null"/>, a preset store is created with the configured settings.
    /// </remarks>
    public ConfigPresetOptions? Presets { get; init; }

    /// <summary>
    /// Gets or sets the internal undo-shortcut input source used by store-backed config sections.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the built-in production undo input source is used if and only if
    /// <see cref="Undo"/> is enabled and a section creates an undo stack. This property exists so tests
    /// can inject deterministic keyboard state without adding undo-specific constructor parameters to
    /// sections that do not use undo.
    /// </remarks>
    internal IUndoShortcutInputSource? UndoInputSource { get; init; }

    /// <summary>
    /// Gets or sets the internal numeric-edit undo sink used to group one numeric mouse interaction into one undo record.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, built-in numeric controls render without grouped numeric undo tracking.
    /// When non-<see langword="null"/>, built-in numeric controls notify this sink of interaction begin and end events.
    /// This property remains internal because grouped numeric undo is an Umbra-owned optional feature.
    /// </remarks>
    internal INumericEditUndoSink? NumericEditUndoSink { get; init; }

    /// <summary>Initializes a new instance of <see cref="ConfigDrawerOptions"/> with all options set to their defaults.</summary>
    public ConfigDrawerOptions() { }

    private ConfigDrawerOptions(ConfigDrawerOptions source)
    {
        Search = source.Search;
        SuppressRootNode = source.SuppressRootNode;
        Transfer = source.Transfer;
        Undo = source.Undo;
        UndoInputSource = source.UndoInputSource;
        NumericEditUndoSink = source.NumericEditUndoSink;
        Presets = source.Presets;
    }

    internal ConfigDrawerOptions WithSearch(ConfigSearchOptions? search) => new(this) { Search = search };

    internal ConfigDrawerOptions WithSuppressRootNode(bool suppressRootNode) => new(this) { SuppressRootNode = suppressRootNode };

    internal ConfigDrawerOptions WithTransfer(ConfigTransferOptions? transfer) => new(this) { Transfer = transfer };

    internal ConfigDrawerOptions WithUndo(ConfigUndoOptions? undo) => new(this) { Undo = undo };

    internal ConfigDrawerOptions WithUndoInputSource(IUndoShortcutInputSource? undoInputSource) => new(this) { UndoInputSource = undoInputSource };

    internal ConfigDrawerOptions WithNumericEditUndoSink(INumericEditUndoSink? numericEditUndoSink) => new(this) { NumericEditUndoSink = numericEditUndoSink };

    internal ConfigDrawerOptions WithPresets(ConfigPresetOptions? presets) => new(this) { Presets = presets };
}
