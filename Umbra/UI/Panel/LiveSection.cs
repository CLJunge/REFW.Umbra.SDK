using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// A <see cref="IPanelSection"/> that renders a live game state instance each frame via
/// an <see cref="ILiveSectionDrawer{T}"/> declared on the state type.
/// </summary>
/// <remarks>
/// <para>
/// The state type <typeparamref name="T"/> must be decorated with
/// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>. <see cref="LiveSectionDrawerResolver"/>
/// discovers and instantiates the drawer once at construction time and compiles a
/// zero-overhead draw delegate; no reflection occurs during rendering.
/// </para>
/// <para>
/// The draw delegate captures the exact state instance passed to the constructor and calls
/// the drawer with that same instance on every frame. For hook-driven data, keep a stable
/// holder object bound to the section and either mutate that object's fields in place or
/// publish an atomically swapped snapshot on a field or property inside the holder. Replacing
/// the holder object itself in hook code will not update the instance rendered by this section.
/// </para>
/// <para>
/// When no external writer needs access to the bound instance, the parameterless constructor
/// can be used to let the section create and own the state object internally.
/// </para>
/// <para>
/// When <c>treeNodeLabel</c> is supplied, the owning <see cref="PluginPanel"/>
/// wraps this section's output inside a collapsible <c>ImGui.TreeNode</c> with that label.
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The live state type. Must be a reference type with a public parameterless constructor
/// and be decorated with <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
/// </typeparam>
public sealed class LiveSection<T> : IPanelSection where T : class, new()
{
    private readonly string? _idScope;
    private readonly string? _treeNodeLabel;
    private readonly bool _treeNodeDefaultOpen;
    private readonly Action _drawAction;
    private readonly IDisposable _drawerDisposable;
    private readonly int _order;
    private bool _disposed;

    /// <summary>
    /// Initialises a new live section bound to the provided state instance.
    /// </summary>
    /// <param name="context">
    /// The live state instance bound to this section for its entire lifetime and read by the
    /// drawer each frame. The plugin should retain its own reference to this instance so hooks
    /// or callbacks can update it between frames.
    /// </param>
    /// <param name="idScope">
    /// Optional ImGui ID sub-scope pushed around the drawer's output. When supplied,
    /// <c>ImGui.PushID(idScope)</c> is called before rendering and <c>ImGui.PopID()</c>
    /// after. The owning <see cref="PluginPanel"/> already pushes a top-level scope, so
    /// this is only needed when two live sections of the same type exist in the same panel.
    /// </param>
    /// <param name="treeNodeLabel">
    /// Optional label for a collapsible <c>ImGui.TreeNode</c> that wraps this section's
    /// output within the owning <see cref="PluginPanel"/>. Pass <see langword="null"/>
    /// (the default) to render the section flat with no tree node.
    /// </param>
    /// <param name="treeNodeDefaultOpen">
    /// Whether the section tree node starts expanded. Ignored when
    /// <paramref name="treeNodeLabel"/> is <see langword="null"/>.
    /// Defaults to <see langword="false"/> (collapsed).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="T"/> is not decorated with
    /// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
    /// </exception>
    public LiveSection(T context, string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false)
    {
        _idScope = idScope;
        _treeNodeLabel = treeNodeLabel;
        _treeNodeDefaultOpen = treeNodeDefaultOpen;
        _order = typeof(T).GetDrawerAttribute<SectionOrderAttribute>()?.Order ?? int.MaxValue;
        _drawAction = LiveSectionDrawerResolver.Resolve(typeof(T), context, out _drawerDisposable);
    }

    /// <inheritdoc/>
    public int Order => _order;

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the explicit <c>idScope</c> when one was provided at construction, or
    /// <c>typeof(<typeparamref name="T"/>).Name</c> as the stable fallback. This is
    /// consumed by <see cref="PluginPanel"/> as the <c>ImGui.PushID</c> scope before the
    /// section's tree node, and mirrors the value the section itself pushes inside
    /// <see cref="Draw"/> when <c>idScope</c> is set.
    /// </remarks>
    public string SectionId => _idScope ?? typeof(T).Name;

    /// <inheritdoc/>
    public string? TreeNodeLabel => _treeNodeLabel;

    /// <inheritdoc/>
    public bool TreeNodeDefaultOpen => _treeNodeDefaultOpen;

    /// <summary>
    /// Initialises a new live section, constructing the bound state instance internally.
    /// Use this overload when the section owns the state and no external writer needs a
    /// reference to that instance — for example, when the drawer queries game state directly.
    /// </summary>
    /// <param name="idScope">
    /// Optional ImGui ID sub-scope. See the primary constructor for details.
    /// </param>
    /// <param name="treeNodeLabel">
    /// Optional tree node label. See the primary constructor for details.
    /// </param>
    /// <param name="treeNodeDefaultOpen">
    /// Whether the tree node starts expanded. See the primary constructor for details.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="T"/> is not decorated with
    /// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
    /// </exception>
    public LiveSection(string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false)
        : this(new T(), idScope, treeNodeLabel, treeNodeDefaultOpen) { }

    /// <inheritdoc/>
    public void Draw()
    {
        if (_disposed) return;

        var hasIdScope = _idScope is not null;
        if (hasIdScope) ImGui.PushID(_idScope);
        try
        {
            _drawAction();
        }
        finally
        {
            if (hasIdScope) ImGui.PopID();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _drawerDisposable.Dispose();
        GC.SuppressFinalize(this);
    }
}
