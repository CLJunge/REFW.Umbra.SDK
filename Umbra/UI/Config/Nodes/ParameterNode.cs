namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Draw node that conditionally invokes a per-frame draw action based on a visibility predicate.
/// </summary>
/// <remarks>
/// The default constructor renders spacing through the active ImGui frame. Unit tests can replace
/// the low-level renderer through the internal constructor so visibility and spacing behavior can be
/// verified without requiring an active ImGui frame.
/// </remarks>
internal sealed class ParameterNode : IDrawNode
{
    private readonly Func<bool> _isVisible;
    private readonly Action _draw;
    private readonly int _spacingBefore;
    private readonly int _spacingAfter;
    private readonly IParameterNodeRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="ParameterNode"/> that renders spacing through the active ImGui frame.
    /// </summary>
    internal ParameterNode(
        Func<bool> isVisible,
        Action draw,
        int order = int.MaxValue,
        int spacingBefore = 0,
        int spacingAfter = 0)
        : this(isVisible, draw, order, spacingBefore, spacingAfter, new ImGuiParameterNodeRenderer())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ParameterNode"/> with the specified low-level renderer.
    /// </summary>
    /// <param name="isVisible">Predicate evaluated each frame to determine visibility.</param>
    /// <param name="draw">The per-frame draw action to invoke when visible.</param>
    /// <param name="order">The sort key for this node within its local rendered scope.</param>
    /// <param name="spacingBefore">The number of spacing calls emitted before the draw action.</param>
    /// <param name="spacingAfter">The number of spacing calls emitted after the draw action.</param>
    /// <param name="renderer">The renderer used for spacing operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="isVisible"/>, <paramref name="draw"/>, or <paramref name="renderer"/> is <see langword="null"/>.
    /// </exception>
    internal ParameterNode(
        Func<bool> isVisible,
        Action draw,
        int order,
        int spacingBefore,
        int spacingAfter,
        IParameterNodeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(isVisible);
        ArgumentNullException.ThrowIfNull(draw);
        ArgumentNullException.ThrowIfNull(renderer);
        _isVisible = isVisible;
        _draw = draw;
        _spacingBefore = spacingBefore;
        _spacingAfter = spacingAfter;
        _renderer = renderer;
        Order = order;
    }

    /// <summary>Gets the sort key for this node within its local rendered scope.</summary>
    internal int Order { get; }

    /// <inheritdoc/>
    public void Draw()
    {
        if (!_isVisible()) return;
        for (var i = 0; i < _spacingBefore; i++) _renderer.Spacing();
        _draw();
        for (var i = 0; i < _spacingAfter; i++) _renderer.Spacing();
    }
}
