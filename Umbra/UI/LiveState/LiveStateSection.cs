using System.Reflection;
using Hexa.NET.ImGui;
using Umbra.UI.Panel;

namespace Umbra.UI.LiveState;

/// <summary>
/// Renders a live-state instance each frame through the drawer declared on <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <para>
/// <typeparamref name="T"/> must be decorated with <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>. <see cref="LiveStateSectionDrawerResolver"/> resolves the drawer once at construction time and compiles the delegate used for per-frame drawing.
/// </para>
/// <para>
/// The section keeps using the exact state instance passed to the constructor. For hook-driven data, keep that instance stable for the section's lifetime and update its contents in place or through members that publish swapped snapshots.
/// </para>
/// <para>
/// When a tree-node label is supplied, the owning <see cref="PluginPanel"/> wraps this section's output in a collapsible tree node.
/// </para>
/// </remarks>
/// <typeparam name="T">The live-state type rendered by this section. It must be a reference type decorated with <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>.</typeparam>
public sealed class LiveStateSection<T> : IPanelSection where T : class
{
    private readonly string? _idScope;
    private readonly string? _treeNodeLabel;
    private readonly bool _treeNodeDefaultOpen;
    private readonly Action _drawAction;
    private readonly IDisposable _drawerDisposable;
    private readonly int _order;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveStateSection{T}"/> class bound to the provided state instance.
    /// </summary>
    /// <param name="context">The live-state instance rendered by this section for its entire lifetime.</param>
    /// <param name="idScope">The optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="treeNodeLabel">The optional label for a collapsible tree node that wraps this section in the owning <see cref="PluginPanel"/>.</param>
    /// <param name="treeNodeDefaultOpen"><see langword="true"/> to start the optional section tree node expanded; otherwise, <see langword="false"/>.</param>
    /// <remarks>
    /// <para>
    /// When <paramref name="idScope"/> is omitted, <c>typeof(<typeparamref name="T"/>).FullName</c> is used, falling back to <c>typeof(<typeparamref name="T"/>).Name</c> when the full name is unavailable. Supply an explicit value only when multiple live-state sections of the same type appear in one panel.
    /// </para>
    /// <para>
    /// The plugin should keep its own reference to <paramref name="context"/> when hooks or callbacks need to update it between frames.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> is not decorated with <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>.</exception>
    public LiveStateSection(T context, string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (idScope is not null && string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be empty or whitespace when supplied.", nameof(idScope));
        _idScope = idScope;
        _treeNodeLabel = treeNodeLabel;
        _treeNodeDefaultOpen = treeNodeDefaultOpen;
        _order = typeof(T).GetDrawerAttribute<UmbraSectionOrderAttribute>()?.Order ?? int.MaxValue;
        _drawAction = LiveStateSectionDrawerResolver.Resolve(typeof(T), context, out _drawerDisposable);
    }

    /// <inheritdoc/>
    public int Order => _order;

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the explicit constructor-supplied scope when one was provided; otherwise, it returns the runtime-type fallback used for this section. The owning <see cref="PluginPanel"/> also uses this value to disambiguate tree-node identity when <see cref="IPanelSection.TreeNodeLabel"/> is set.
    /// </remarks>
    public string SectionId => _idScope ?? typeof(T).FullName ?? typeof(T).Name;

    /// <inheritdoc/>
    public string? TreeNodeLabel => _treeNodeLabel;

    /// <inheritdoc/>
    public bool TreeNodeDefaultOpen => _treeNodeDefaultOpen;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveStateSection{T}"/> class and creates the bound state instance internally.
    /// </summary>
    /// <remarks>
    /// Use this overload when the section owns the state instance and no external writer needs a reference to it. <typeparamref name="T"/> must expose a public parameterless constructor; otherwise, use <see cref="LiveStateSection{T}(T, string?, string?, bool)"/>.
    /// </remarks>
    /// <param name="idScope">The optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="treeNodeLabel">The optional label for a collapsible tree node that wraps this section in the owning <see cref="PluginPanel"/>.</param>
    /// <param name="treeNodeDefaultOpen"><see langword="true"/> to start the optional section tree node expanded; otherwise, <see langword="false"/>.</param>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> does not expose a public parameterless constructor, activation fails, or <typeparamref name="T"/> is not decorated with <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>.</exception>
    public LiveStateSection(string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false)
        : this(CreateOwnedContext(), idScope, treeNodeLabel, treeNodeDefaultOpen) { }

    /// <inheritdoc/>
    /// <remarks>
    /// After <see cref="Dispose()"/> has been called, this method becomes a silent no-op.
    /// </remarks>
    public void Draw()
    {
        if (_disposed) return;

        ImGui.PushID(SectionId);
        try
        {
            _drawAction();
        }
        finally
        {
            ImGui.PopID();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Disposes the resolved drawer once. Repeated calls after the first one do nothing.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _drawerDisposable.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates the internally owned state instance used by the constructor overload that does not accept a context.
    /// </summary>
    /// <returns>A new <typeparamref name="T"/> instance.</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> does not expose a public parameterless constructor, the constructor throws during activation, or activation fails for any other reason.</exception>
    private static T CreateOwnedContext()
    {
        try
        {
            return Activator.CreateInstance<T>();
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException(
                $"LiveStateSection<{typeof(T).Name}> requires a public parameterless constructor when using the parameterless section constructor. " +
                $"Use {nameof(LiveStateSection<T>)}(T, string?, string?, bool) to supply the state instance explicitly.",
                ex);
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"LiveStateSection<{typeof(T).Name}> state constructor threw an exception during activation. " +
                $"Use {nameof(LiveStateSection<T>)}(T, string?, string?, bool) to supply a pre-constructed instance.",
                ex.InnerException ?? ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"LiveStateSection<{typeof(T).Name}> could not activate the state type ({ex.GetType().Name}). " +
                $"Use {nameof(LiveStateSection<T>)}(T, string?, string?, bool) to supply the state instance explicitly.",
                ex);
        }
    }
}
