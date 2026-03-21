using Hexa.NET.ImGui;

namespace Umbra.SDK.UI.Panel;

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
/// The live state instance is typically owned by the plugin class and written to by
/// <c>[MethodHook]</c> callbacks between frames. Pass the plugin-owned instance to the
/// constructor so the same reference is shared between the hook and the drawer. When no
/// instance is supplied, the section constructs one internally.
/// </para>
/// <para>
/// Use the swap-instance pattern in hooks that update multiple fields to guarantee the
/// drawer always reads a consistent snapshot.
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The live state type. Must be a reference type with a public parameterless constructor
/// and be decorated with <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
/// </typeparam>
public sealed class LiveSection<T> : IPanelSection where T : class, new()
{
    private readonly string? _idScope;
    private readonly Action _drawAction;
    private readonly IDisposable _drawerDisposable;
    private readonly int _order;
    private bool _disposed;

    /// <summary>
    /// Initialises a new live section bound to the provided state instance.
    /// </summary>
    /// <param name="context">
    /// The live state instance written to by hooks and read by the drawer each frame.
    /// The plugin should retain its own reference to this instance so hooks can write to it.
    /// </param>
    /// <param name="idScope">
    /// Optional ImGui ID sub-scope pushed around the drawer's output. When supplied,
    /// <c>ImGui.PushID(idScope)</c> is called before rendering and <c>ImGui.PopID()</c>
    /// after. The owning <see cref="PluginPanel"/> already pushes a top-level scope, so
    /// this is only needed when two live sections of the same type exist in the same panel.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="T"/> is not decorated with
    /// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
    /// </exception>
    public LiveSection(T context, string? idScope = null)
    {
        _idScope = idScope;
        _order = typeof(T).GetDrawerAttribute<SectionOrderAttribute>()?.Order ?? int.MaxValue;
        _drawAction = LiveSectionDrawerResolver.Resolve(typeof(T), context, out _drawerDisposable);
    }

    /// <inheritdoc/>
    public int Order => _order;

    /// <summary>
    /// Initialises a new live section, constructing the state instance internally.
    /// Use this overload when the section owns the state and no external writer (hook) needs
    /// the reference — for example, when the drawer queries game state directly.
    /// </summary>
    /// <param name="idScope">
    /// Optional ImGui ID sub-scope. See the primary constructor for details.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="T"/> is not decorated with
    /// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
    /// </exception>
    public LiveSection(string? idScope = null) : this(new T(), idScope) { }

    /// <inheritdoc/>
    public void Draw()
    {
        if (_disposed) return;

        if (_idScope is not null) ImGui.PushID(_idScope);
        _drawAction();
        if (_idScope is not null) ImGui.PopID();
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
