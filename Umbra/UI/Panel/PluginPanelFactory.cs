using Umbra.Config;
using Umbra.UI.Config;
using Umbra.UI.Config.Search;
using Umbra.UI.Config.Transfer;
using Umbra.UI.Toast;

namespace Umbra.UI.Panel;

/// <summary>
/// Provides static factory methods for creating <see cref="PluginPanel"/> instances at varying
/// levels of convenience.
/// </summary>
/// <remarks>
/// <para>
/// Each factory method has two tiers: a <strong>simple</strong> tier that accepts boolean feature
/// flags with sensible defaults, and a <strong>custom</strong> tier that accepts a pre-built
/// <see cref="ConfigDrawerOptions"/> for fine-grained control over search, transfer, and undo
/// settings. The simple tier delegates to the custom tier internally.
/// </para>
/// <para>
/// <see cref="Create{TConfig}(string, string, ConfigDrawerOptions, string?, string?, bool, bool)"/>
/// is the highest-convenience entry point: it creates the <see cref="ConfigStore{TConfig}"/>, loads
/// the config, and builds a panel — returning a <see cref="ManagedPluginPanel{TConfig}"/> that
/// owns the full lifecycle.
/// </para>
/// <para>
/// <see cref="CreateWithConfigStore{TConfig}(TConfig, IConfigTransferStore, string, ConfigDrawerOptions, string?, string?, bool, bool)"/>
/// accepts an already-loaded config and store so the caller retains store lifecycle control.
/// </para>
/// <para>
/// For panels that need multiple sections or non-config sections, use the
/// <see cref="PluginPanel(string, string?, bool, bool)"/> constructor with
/// <see cref="ConfigSection{TConfig}.CreateWithStore(TConfig, IConfigTransferStore, ConfigDrawerOptions, string?, string?, bool, bool, bool)"/>
/// directly.
/// </para>
/// </remarks>
public static class PluginPanelFactory
{
    /// <summary>
    /// Creates a fully managed <see cref="PluginPanel"/> using the supplied
    /// <see cref="ConfigDrawerOptions"/> for full control over search, transfer, and undo settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The returned <see cref="ManagedPluginPanel{TConfig}"/> owns the config store's lifecycle.
    /// Calling <see cref="ManagedPluginPanel{TConfig}.Dispose"/> disposes the panel (which flushes
    /// the internal save controller), saves the store, and disposes the store — so the caller only
    /// needs to hold one reference.
    /// </para>
    /// <para>
    /// If panel construction fails after the store has been created, the store is disposed before
    /// the exception propagates so no resources are leaked.
    /// </para>
    /// </remarks>
    /// <typeparam name="TConfig">The configuration type rendered by the section.</typeparam>
    /// <param name="configFilePath">
    /// The absolute or relative path to the JSON file used for persisting config data.
    /// </param>
    /// <param name="panelIdScope">
    /// A globally unique identifier string for this panel. Must be non-null and non-whitespace.
    /// </param>
    /// <param name="options">
    /// A pre-built <see cref="ConfigDrawerOptions"/> controlling search, transfer, undo, and other
    /// drawer-level behaviors. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="sectionIdScope">
    /// Optional stable ImGui widget ID sub-scope for the config section. When omitted,
    /// the section derives its scope from the config type name.
    /// </param>
    /// <param name="rootNodeLabel">
    /// When non-<see langword="null"/>, all sections are rendered inside a single collapsible
    /// tree node with this label.
    /// </param>
    /// <param name="rootNodeDefaultOpen">
    /// When <see langword="true"/>, the root tree node starts expanded.
    /// Ignored when <paramref name="rootNodeLabel"/> is <see langword="null"/>.
    /// </param>
    /// <param name="drawSeparator">
    /// When <see langword="true"/> (the default), a horizontal separator is drawn after
    /// all sections.
    /// </param>
    /// <returns>
    /// A <see cref="ManagedPluginPanel{TConfig}"/> that owns the panel, config, and store lifecycle.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configFilePath"/>, <paramref name="panelIdScope"/>, or
    /// <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="configFilePath"/> or <paramref name="panelIdScope"/> is
    /// empty or whitespace.
    /// </exception>
    public static ManagedPluginPanel<TConfig> Create<TConfig>(
        string configFilePath,
        string panelIdScope,
        ConfigDrawerOptions options,
        string? sectionIdScope = null,
        string? rootNodeLabel = null,
        bool rootNodeDefaultOpen = false,
        bool drawSeparator = true)
        where TConfig : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(panelIdScope);
        ArgumentNullException.ThrowIfNull(options);

        var store = new ConfigStore<TConfig>(configFilePath);
        try
        {
            var config = store.Load();
            var section = ConfigSection<TConfig>.CreateWithStore(config, store, options, sectionIdScope);
            var panel = new PluginPanel(panelIdScope, rootNodeLabel, rootNodeDefaultOpen, drawSeparator).Add(section);

            return new ManagedPluginPanel<TConfig>(panel, config, store);
        }
        catch
        {
            store.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a fully managed <see cref="PluginPanel"/> by constructing a
    /// <see cref="ConfigStore{TConfig}"/>, loading the config, and wiring a single
    /// <see cref="ConfigSection{TConfig}"/> with the specified optional features using default settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload builds a <see cref="ConfigDrawerOptions"/> from boolean flags with default
    /// settings for each enabled feature. For fine-grained control over individual option settings,
    /// use the <see cref="Create{TConfig}(string, string, ConfigDrawerOptions, string?, string?, bool, bool)"/>
    /// overload instead.
    /// </para>
    /// <para>
    /// The returned <see cref="ManagedPluginPanel{TConfig}"/> owns the config store's lifecycle.
    /// Calling <see cref="ManagedPluginPanel{TConfig}.Dispose"/> disposes the panel (which flushes
    /// the internal save controller), saves the store, and disposes the store — so the caller only
    /// needs to hold one reference.
    /// </para>
    /// <para>
    /// If panel construction fails after the store has been created, the store is disposed before
    /// the exception propagates so no resources are leaked.
    /// </para>
    /// </remarks>
    /// <typeparam name="TConfig">The configuration type rendered by the section.</typeparam>
    /// <param name="configFilePath">
    /// The absolute or relative path to the JSON file used for persisting config data.
    /// </param>
    /// <param name="panelIdScope">
    /// A globally unique identifier string for this panel. Must be non-null and non-whitespace.
    /// </param>
    /// <param name="sectionIdScope">
    /// Optional stable ImGui widget ID sub-scope for the config section. When omitted,
    /// the section derives its scope from the config type name.
    /// </param>
    /// <param name="enableSearch">
    /// When <see langword="true"/> (the default), the built-in search bar is enabled with
    /// default <see cref="ConfigSearchOptions"/> settings.
    /// </param>
    /// <param name="enableTransfer">
    /// When <see langword="true"/> (the default), the built-in config import/export UI is
    /// enabled with default <see cref="ConfigTransferOptions"/> settings.
    /// </param>
    /// <param name="enableUndo">
    /// When <see langword="true"/> (the default), the undo stack is enabled with default
    /// <see cref="ConfigUndoOptions"/> settings.
    /// </param>
    /// <param name="toast">
    /// Optional plugin-scoped toast instance wired into the undo stack for undo/redo
    /// notifications. Ignored when <paramref name="enableUndo"/> is <see langword="false"/>.
    /// </param>
    /// <param name="rootNodeLabel">
    /// When non-<see langword="null"/>, all sections are rendered inside a single collapsible
    /// tree node with this label.
    /// </param>
    /// <param name="rootNodeDefaultOpen">
    /// When <see langword="true"/>, the root tree node starts expanded.
    /// Ignored when <paramref name="rootNodeLabel"/> is <see langword="null"/>.
    /// </param>
    /// <param name="drawSeparator">
    /// When <see langword="true"/> (the default), a horizontal separator is drawn after
    /// all sections.
    /// </param>
    /// <returns>
    /// A <see cref="ManagedPluginPanel{TConfig}"/> that owns the panel, config, and store lifecycle.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configFilePath"/> or <paramref name="panelIdScope"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="configFilePath"/> or <paramref name="panelIdScope"/> is
    /// empty or whitespace.
    /// </exception>
    public static ManagedPluginPanel<TConfig> Create<TConfig>(
        string configFilePath,
        string panelIdScope,
        string? sectionIdScope = null,
        bool enableSearch = true,
        bool enableTransfer = true,
        bool enableUndo = true,
        PluginToast? toast = null,
        string? rootNodeLabel = null,
        bool rootNodeDefaultOpen = false,
        bool drawSeparator = true)
        where TConfig : class, new()
    {
        return Create<TConfig>(
            configFilePath,
            panelIdScope,
            BuildOptions(enableSearch, enableTransfer, enableUndo, toast),
            sectionIdScope,
            rootNodeLabel,
            rootNodeDefaultOpen,
            drawSeparator);
    }

    /// <summary>
    /// Creates a <see cref="PluginPanel"/> containing a single store-backed
    /// <see cref="ConfigSection{TConfig}"/> using the supplied <see cref="ConfigDrawerOptions"/>
    /// for full control over search, transfer, and undo settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The caller retains ownership of <paramref name="store"/> and is responsible for saving
    /// and disposing it during shutdown. For fully managed lifecycle, use
    /// <see cref="Create{TConfig}(string, string, ConfigDrawerOptions, string?, string?, bool, bool)"/>
    /// instead.
    /// </para>
    /// <para>
    /// When <see cref="ConfigDrawerOptions.Undo"/> is non-<see langword="null"/> but
    /// <paramref name="store"/> is not a <see cref="ConfigStore{TConfig}"/>, the undo stack is
    /// silently omitted because undo requires the concrete store type.
    /// </para>
    /// </remarks>
    /// <typeparam name="TConfig">The configuration type rendered by the section.</typeparam>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded config store associated with <paramref name="config"/>.</param>
    /// <param name="panelIdScope">
    /// A globally unique identifier string for this panel. Must be non-null and non-whitespace.
    /// </param>
    /// <param name="options">
    /// A pre-built <see cref="ConfigDrawerOptions"/> controlling search, transfer, undo, and other
    /// drawer-level behaviors. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="sectionIdScope">
    /// Optional stable ImGui widget ID sub-scope for the config section. When omitted,
    /// the section derives its scope from the config type name.
    /// </param>
    /// <param name="rootNodeLabel">
    /// When non-<see langword="null"/>, all sections are rendered inside a single collapsible
    /// tree node with this label.
    /// </param>
    /// <param name="rootNodeDefaultOpen">
    /// When <see langword="true"/>, the root tree node starts expanded.
    /// Ignored when <paramref name="rootNodeLabel"/> is <see langword="null"/>.
    /// </param>
    /// <param name="drawSeparator">
    /// When <see langword="true"/> (the default), a horizontal separator is drawn after
    /// all sections.
    /// </param>
    /// <returns>A fully initialized <see cref="PluginPanel"/> ready for <see cref="PluginPanel.Draw"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/>, <paramref name="store"/>, or
    /// <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="panelIdScope"/>, <paramref name="config"/>, <paramref name="store"/>, or
    /// <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="panelIdScope"/> is empty or whitespace.
    /// </exception>
    public static PluginPanel CreateWithConfigStore<TConfig>(
        TConfig config,
        IConfigTransferStore store,
        string panelIdScope,
        ConfigDrawerOptions options,
        string? sectionIdScope = null,
        string? rootNodeLabel = null,
        bool rootNodeDefaultOpen = false,
        bool drawSeparator = true)
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(panelIdScope);

        var section = ConfigSection<TConfig>.CreateWithStore(config, store, options, sectionIdScope);
        return new PluginPanel(panelIdScope, rootNodeLabel, rootNodeDefaultOpen, drawSeparator).Add(section);
    }

    /// <summary>
    /// Creates a <see cref="PluginPanel"/> containing a single store-backed
    /// <see cref="ConfigSection{TConfig}"/> with the specified optional features using default settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload builds a <see cref="ConfigDrawerOptions"/> from boolean flags with default
    /// settings for each enabled feature. For fine-grained control over individual option settings,
    /// use the <see cref="CreateWithConfigStore{TConfig}(TConfig, IConfigTransferStore, string, ConfigDrawerOptions, string?, string?, bool, bool)"/>
    /// overload instead.
    /// </para>
    /// <para>
    /// The caller retains ownership of <paramref name="store"/> and is responsible for saving
    /// and disposing it during shutdown. For fully managed lifecycle, use
    /// <see cref="Create{TConfig}(string, string, string?, bool, bool, bool, PluginToast?, string?, bool, bool)"/>
    /// instead.
    /// </para>
    /// <para>
    /// When <paramref name="enableUndo"/> is <see langword="true"/> but <paramref name="store"/>
    /// is not a <see cref="ConfigStore{TConfig}"/>, the undo stack is silently omitted because
    /// undo requires the concrete store type.
    /// </para>
    /// </remarks>
    /// <typeparam name="TConfig">The configuration type rendered by the section.</typeparam>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded config store associated with <paramref name="config"/>.</param>
    /// <param name="panelIdScope">
    /// A globally unique identifier string for this panel. Must be non-null and non-whitespace.
    /// </param>
    /// <param name="sectionIdScope">
    /// Optional stable ImGui widget ID sub-scope for the config section. When omitted,
    /// the section derives its scope from the config type name.
    /// </param>
    /// <param name="enableSearch">
    /// When <see langword="true"/> (the default), the built-in search bar is enabled with
    /// default <see cref="ConfigSearchOptions"/> settings.
    /// </param>
    /// <param name="enableTransfer">
    /// When <see langword="true"/> (the default), the built-in config import/export UI is
    /// enabled with default <see cref="ConfigTransferOptions"/> settings.
    /// </param>
    /// <param name="enableUndo">
    /// When <see langword="true"/> (the default), the undo stack is enabled with default
    /// <see cref="ConfigUndoOptions"/> settings (requires <paramref name="store"/> to be
    /// a <see cref="ConfigStore{TConfig}"/>).
    /// </param>
    /// <param name="toast">
    /// Optional plugin-scoped toast instance wired into the undo stack for undo/redo
    /// notifications. Ignored when <paramref name="enableUndo"/> is <see langword="false"/>.
    /// </param>
    /// <param name="rootNodeLabel">
    /// When non-<see langword="null"/>, all sections are rendered inside a single collapsible
    /// tree node with this label.
    /// </param>
    /// <param name="rootNodeDefaultOpen">
    /// When <see langword="true"/>, the root tree node starts expanded.
    /// Ignored when <paramref name="rootNodeLabel"/> is <see langword="null"/>.
    /// </param>
    /// <param name="drawSeparator">
    /// When <see langword="true"/> (the default), a horizontal separator is drawn after
    /// all sections.
    /// </param>
    /// <returns>A fully initialized <see cref="PluginPanel"/> ready for <see cref="PluginPanel.Draw"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> or <paramref name="store"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="panelIdScope"/>, <paramref name="config"/>, or <paramref name="store"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="panelIdScope"/> is empty or whitespace.
    /// </exception>
    public static PluginPanel CreateWithConfigStore<TConfig>(
        TConfig config,
        IConfigTransferStore store,
        string panelIdScope,
        string? sectionIdScope = null,
        bool enableSearch = true,
        bool enableTransfer = true,
        bool enableUndo = true,
        PluginToast? toast = null,
        string? rootNodeLabel = null,
        bool rootNodeDefaultOpen = false,
        bool drawSeparator = true)
        where TConfig : class, new()
    {
        return CreateWithConfigStore(
            config,
            store,
            panelIdScope,
            BuildOptions(enableSearch, enableTransfer, enableUndo, toast),
            sectionIdScope,
            rootNodeLabel,
            rootNodeDefaultOpen,
            drawSeparator);
    }

    private static ConfigDrawerOptions BuildOptions(
        bool enableSearch,
        bool enableTransfer,
        bool enableUndo,
        PluginToast? toast)
    {
        return new ConfigDrawerOptions
        {
            Search = enableSearch ? new ConfigSearchOptions() : null,
            Transfer = enableTransfer ? new ConfigTransferOptions() : null,
            Undo = enableUndo ? new ConfigUndoOptions { Toast = toast } : null
        };
    }
}
