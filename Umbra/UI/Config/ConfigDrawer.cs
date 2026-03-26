using System.Reflection;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Pre-builds and renders an ImGui settings UI for a typed configuration class.
/// </summary>
/// <remarks>
/// <para>
/// The draw tree is assembled once at construction time via a single reflection pass;
/// each subsequent call to <see cref="Draw"/> walks the pre-built list of nodes cheaply
/// with no per-frame reflection.
/// </para>
/// <para>
/// Pass a config instance returned by <see cref="Umbra.Config.SettingsStore{TConfig}.Load()"/> so that
/// <see cref="Umbra.Config.ParameterMetadata"/> is already populated; the drawer falls back to reading
/// attributes directly when metadata fields are absent.
/// </para>
/// <para>
/// For nested settings groups, prefer applying presentation attributes such as
/// <see cref="CategoryAttribute"/>, <see cref="CollapseAsTreeAttribute"/>,
/// <see cref="LabelMarginAttribute"/>, and <see cref="NestedGroupDrawerAttribute{TDrawer}"/>
/// to the parent property that exposes the group; equivalent type-level declarations remain
/// supported as backward-compatible fallbacks. Category names are scoped to the group that
/// declares them, so sibling nested groups may reuse the same category label without colliding.
/// When a nested-group property declares its own category, that category renders as a real parent
/// container for the group's uncategorized direct controls and any additional child categories
/// declared inside the group. Every nested-group subtree also receives its own stable ImGui ID
/// scope derived from its structural settings path, so custom nested-group drawers can safely
/// reuse local widget labels in different branches. Apply <see cref="ConfigRootNodeAttribute"/>
/// to the root config class to wrap the entire panel inside a single top-level
/// <c>ImGui.TreeNode</c>.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type, following the SDK settings attribute conventions.
/// Must have a public parameterless constructor.
/// </typeparam>
public sealed class ConfigDrawer<TConfig> : IDisposable where TConfig : class, new()
{
    private readonly List<IDrawNode> _nodes;
    private readonly List<IDisposable> _disposables;
    private readonly string _idScope;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree.
    /// </summary>
    /// <param name="config">
    /// A fully initialized configuration instance, ideally returned by
    /// <see cref="Umbra.Config.SettingsStore{TConfig}.Load()"/> so that <see cref="Umbra.Config.ParameterMetadata"/>
    /// is already populated on every parameter.
    /// </param>
    /// <param name="idScope">
    /// A plugin-unique identifier string (e.g. <c>"MyPlugin"</c>) used to scope all ImGui
    /// widget IDs rendered by this drawer via <see cref="ImGui.PushID(string)"/> / <see cref="ImGui.PopID()"/>.
    /// Every widget ID within a <see cref="Draw"/> call is internally prefixed with this
    /// string, preventing duplicate-ID warnings when multiple plugins render settings panels
    /// in the same ImGui window. Must be non-null and non-whitespace.
    /// </param>
    /// <param name="suppressRootNode">
    /// When <see langword="true"/>, the <see cref="ConfigRootNodeAttribute"/>-driven
    /// root <see cref="ImGui.TreeNode(string)"/> is not rendered even when the attribute is present on
    /// <typeparamref name="TConfig"/>. Defaults to <see langword="false"/>.
    /// Pass <see langword="true"/> when the owning <see cref="ConfigSection{TConfig}"/>
    /// is responsible for the tree node so that the wrapping is not duplicated.
    /// </param>
    public ConfigDrawer(TConfig config, string idScope, bool suppressRootNode = false)
    {
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null or whitespace when supplied.", nameof(idScope));

        _idScope = idScope;
        var builder = new ConfigDrawerBuilder();
        builder.Collect(config, typeof(TConfig));
        builder.SortAll();
        _disposables = builder.Disposables;

        var rootAttr = typeof(TConfig).GetCustomAttribute<ConfigRootNodeAttribute>();
        if (rootAttr is not null && !suppressRootNode)
        {
            var label = rootAttr.Label ?? typeof(TConfig).Name.ToDisplayName();
            _nodes = [new RootTreeNode(label, rootAttr.DefaultOpen, builder.Nodes)];
        }
        else
        {
            _nodes = builder.Nodes;
        }
    }

    /// <summary>
    /// Renders the full settings UI for one ImGui frame.
    /// Must be called from within an active ImGui window or child window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All widget IDs rendered during this call are bracketed by <c>ImGui.PushID(idScope)</c> /
    /// <c>ImGui.PopID()</c>, making every <c>##key</c> label unique across plugins without any
    /// changes to individual controls or custom drawers. The scope is always popped before this
    /// method returns, even if a node throws while drawing.
    /// </para>
    /// <para>
    /// A no-op when the instance has been disposed; logs a warning rather than throwing so
    /// a stale render callback in the game loop does not raise an unhandled exception in-process.
    /// </para>
    /// </remarks>
    public void Draw()
    {
        if (_disposed)
        {
            Logger.Warning($"ConfigDrawer<{typeof(TConfig).Name}>.Draw called on a disposed instance; skipping.");
            return;
        }
        ImGui.PushID(_idScope);
        try
        {
            foreach (var node in _nodes)
                node.Draw();
        }
        finally
        {
            ImGui.PopID();
        }
    }

    /// <summary>
    /// Disposes all stateful custom drawers collected during the draw-tree build pass,
    /// then marks this instance as disposed.
    /// Subsequent calls to <see cref="Draw"/> will log a warning and return without rendering.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var d in _disposables) d.Dispose();
        GC.SuppressFinalize(this);
    }
}
