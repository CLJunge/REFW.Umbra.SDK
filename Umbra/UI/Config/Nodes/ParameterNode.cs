using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Renders one configuration-row draw action with optional visibility, spacing, and indentation behavior.
/// </summary>
/// <remarks>
/// The default constructors render through the shared ImGui render context. Tests can supply a renderer seam to verify visibility, spacing, and indentation behavior without requiring an active ImGui frame.
/// </remarks>
internal sealed class ParameterNode : IDrawNode
{
    private readonly Func<bool>? _isVisible;
    private readonly bool _alwaysVisible;
    private readonly Action _draw;
    private readonly float? _indentAmount;
    private readonly int _spacingBefore;
    private readonly int _spacingAfter;
    private readonly IParameterNodeRenderer _renderer;

    /// <summary>
    /// Initializes a new always-visible <see cref="ParameterNode"/> that renders through the shared ImGui render context.
    /// </summary>
    internal ParameterNode(
        Action draw,
        int order = int.MaxValue,
        int spacingBefore = 0,
        int spacingAfter = 0,
        float? indentAmount = null)
        : this(draw, order, spacingBefore, spacingAfter, ImGuiConfigRenderContext.Instance, indentAmount)
    {
    }

    /// <summary>
    /// Initializes a new always-visible <see cref="ParameterNode"/> with the specified renderer seam.
    /// </summary>
    /// <param name="draw">The per-frame draw action to invoke.</param>
    /// <param name="order">The sort key for this node within its local rendered scope.</param>
    /// <param name="spacingBefore">The number of spacing calls emitted before drawing.</param>
    /// <param name="spacingAfter">The number of spacing calls emitted after drawing.</param>
    /// <param name="renderer">The renderer used for spacing and indentation operations.</param>
    /// <param name="indentAmount">The optional indentation width applied around the draw action.</param>
    /// <exception cref="ArgumentNullException"><paramref name="draw"/> or <paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal ParameterNode(
        Action draw,
        int order,
        int spacingBefore,
        int spacingAfter,
        IParameterNodeRenderer renderer,
        float? indentAmount = null)
    {
        ArgumentNullException.ThrowIfNull(draw);
        ArgumentNullException.ThrowIfNull(renderer);
        _alwaysVisible = true;
        _draw = draw;
        _indentAmount = indentAmount;
        _spacingBefore = spacingBefore;
        _spacingAfter = spacingAfter;
        _renderer = renderer;
        Order = order;
    }

    /// <summary>
    /// Initializes a new conditionally visible <see cref="ParameterNode"/> that renders through the shared ImGui render context.
    /// </summary>
    internal ParameterNode(
        Func<bool> isVisible,
        Action draw,
        int order = int.MaxValue,
        int spacingBefore = 0,
        int spacingAfter = 0,
        float? indentAmount = null)
        : this(isVisible, draw, order, spacingBefore, spacingAfter, ImGuiConfigRenderContext.Instance, indentAmount)
    {
    }

    /// <summary>
    /// Initializes a new conditionally visible <see cref="ParameterNode"/> with the specified renderer seam.
    /// </summary>
    /// <param name="isVisible">The predicate evaluated each frame to determine whether drawing should occur.</param>
    /// <param name="draw">The per-frame draw action to invoke when visible.</param>
    /// <param name="order">The sort key for this node within its local rendered scope.</param>
    /// <param name="spacingBefore">The number of spacing calls emitted before drawing.</param>
    /// <param name="spacingAfter">The number of spacing calls emitted after drawing.</param>
    /// <param name="renderer">The renderer used for spacing and indentation operations.</param>
    /// <param name="indentAmount">The optional indentation width applied around the draw action.</param>
    /// <exception cref="ArgumentNullException"><paramref name="isVisible"/>, <paramref name="draw"/>, or <paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal ParameterNode(
        Func<bool> isVisible,
        Action draw,
        int order,
        int spacingBefore,
        int spacingAfter,
        IParameterNodeRenderer renderer,
        float? indentAmount = null)
    {
        ArgumentNullException.ThrowIfNull(isVisible);
        ArgumentNullException.ThrowIfNull(draw);
        ArgumentNullException.ThrowIfNull(renderer);
        _isVisible = isVisible;
        _draw = draw;
        _indentAmount = indentAmount;
        _spacingBefore = spacingBefore;
        _spacingAfter = spacingAfter;
        _renderer = renderer;
        Order = order;
    }

    /// <summary>
    /// Gets the sort key for this node within its local rendered scope.
    /// </summary>
    internal int Order { get; }

    /// <inheritdoc/>
    public void Draw()
    {
        if (!_alwaysVisible && !_isVisible!()) return;
        for (var i = 0; i < _spacingBefore; i++) _renderer.Spacing();

        if (_indentAmount.HasValue)
        {
            var amount = _indentAmount.Value;
            _renderer.Indent(amount);
            try
            {
                _draw();
            }
            finally
            {
                _renderer.Unindent(amount);
            }
        }
        else
        {
            _draw();
        }

        for (var i = 0; i < _spacingAfter; i++) _renderer.Spacing();
    }
}
