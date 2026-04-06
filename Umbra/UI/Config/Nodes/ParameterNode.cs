using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.UI.Config.Rendering;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Renders one configuration-row draw action with optional visibility, spacing, and indentation behavior.
/// </summary>
/// <remarks>
/// The default constructors render through the shared ImGui render context. Tests can supply a renderer seam to verify visibility, spacing, indentation, highlight, scroll, and focused-control handoff behavior without requiring an active ImGui frame.
/// </remarks>
internal sealed class ParameterNode : IDrawNode, IConfigSearchNode
{
    private static readonly Vector4 MatchTextColor = new(1f, 0.95f, 0.60f, 1f);
    private static readonly Vector4 MatchFrameColor = new(0.35f, 0.28f, 0.08f, 0.70f);
    private static readonly Vector4 MatchFrameHoveredColor = new(0.42f, 0.34f, 0.10f, 0.82f);
    private static readonly Vector4 MatchFrameActiveColor = new(0.48f, 0.40f, 0.12f, 0.90f);
    private static readonly Vector4 FocusedTextColor = new(1f, 1f, 0.78f, 1f);
    private static readonly Vector4 FocusedFrameColor = new(0.50f, 0.32f, 0.08f, 0.88f);
    private static readonly Vector4 FocusedFrameHoveredColor = new(0.58f, 0.38f, 0.10f, 0.94f);
    private static readonly Vector4 FocusedFrameActiveColor = new(0.66f, 0.44f, 0.12f, 1f);

    private readonly Func<bool>? _isVisible;
    private readonly bool _alwaysVisible;
    private readonly Action _draw;
    private readonly List<IDrawNode>? _children;
    private readonly float? _indentAmount;
    private readonly int _spacingBefore;
    private readonly int _spacingAfter;
    private readonly string? _resultId;
    private readonly IParameterNodeRenderer _renderer;
    private bool _searchVisible = true;
    private bool _scrollIntoView;
    private bool _focusControl;
    private SearchMatchVisualState _searchVisualState;

    /// <summary>
    /// Initializes a new always-visible <see cref="ParameterNode"/> that renders through the shared ImGui render context.
    /// </summary>
    /// <param name="draw">The per-frame draw action to invoke.</param>
    /// <param name="order">The sort key for this node within its local rendered scope.</param>
    /// <param name="spacingBefore">The number of spacing calls emitted before drawing.</param>
    /// <param name="spacingAfter">The number of spacing calls emitted after drawing.</param>
    /// <param name="indentAmount">The optional indentation width applied around the draw action.</param>
    /// <param name="resultId">The stable search-result identifier for this row, or <see langword="null"/> when the node does not participate as a searchable leaf result.</param>
    /// <param name="children">The optional child nodes routed through this parameter node when it acts as a searchable container wrapper.</param>
    internal ParameterNode(
        Action draw,
        int order = int.MaxValue,
        int spacingBefore = 0,
        int spacingAfter = 0,
        float? indentAmount = null,
        string? resultId = null,
        List<IDrawNode>? children = null)
        : this(draw, order, spacingBefore, spacingAfter, ImGuiConfigRenderContext.Instance, indentAmount, resultId, children)
    {
    }

    /// <summary>
    /// Initializes a new always-visible <see cref="ParameterNode"/> with the specified renderer seam.
    /// </summary>
    /// <param name="draw">The per-frame draw action to invoke.</param>
    /// <param name="order">The sort key for this node within its local rendered scope.</param>
    /// <param name="spacingBefore">The number of spacing calls emitted before drawing.</param>
    /// <param name="spacingAfter">The number of spacing calls emitted after drawing.</param>
    /// <param name="renderer">The renderer used for spacing, indentation, highlight, scroll, and focus operations.</param>
    /// <param name="indentAmount">The optional indentation width applied around the draw action.</param>
    /// <param name="resultId">The stable search-result identifier for this row, or <see langword="null"/> when the node does not participate as a searchable leaf result.</param>
    /// <param name="children">The optional child nodes routed through this parameter node when it acts as a searchable container wrapper.</param>
    /// <exception cref="ArgumentNullException"><paramref name="draw"/> or <paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal ParameterNode(
        Action draw,
        int order,
        int spacingBefore,
        int spacingAfter,
        IParameterNodeRenderer renderer,
        float? indentAmount = null,
        string? resultId = null,
        List<IDrawNode>? children = null)
    {
        ArgumentNullException.ThrowIfNull(draw);
        ArgumentNullException.ThrowIfNull(renderer);
        _alwaysVisible = true;
        _draw = draw;
        _children = children;
        _indentAmount = indentAmount;
        _spacingBefore = spacingBefore;
        _spacingAfter = spacingAfter;
        _resultId = resultId;
        _renderer = renderer;
        Order = order;
    }

    /// <summary>
    /// Initializes a new conditionally visible <see cref="ParameterNode"/> that renders through the shared ImGui render context.
    /// </summary>
    /// <param name="isVisible">The predicate evaluated each frame to determine whether drawing should occur.</param>
    /// <param name="draw">The per-frame draw action to invoke when visible.</param>
    /// <param name="order">The sort key for this node within its local rendered scope.</param>
    /// <param name="spacingBefore">The number of spacing calls emitted before drawing.</param>
    /// <param name="spacingAfter">The number of spacing calls emitted after drawing.</param>
    /// <param name="indentAmount">The optional indentation width applied around the draw action.</param>
    /// <param name="resultId">The stable search-result identifier for this row, or <see langword="null"/> when the node does not participate as a searchable leaf result.</param>
    /// <param name="children">The optional child nodes routed through this parameter node when it acts as a searchable container wrapper.</param>
    internal ParameterNode(
        Func<bool> isVisible,
        Action draw,
        int order = int.MaxValue,
        int spacingBefore = 0,
        int spacingAfter = 0,
        float? indentAmount = null,
        string? resultId = null,
        List<IDrawNode>? children = null)
        : this(isVisible, draw, order, spacingBefore, spacingAfter, ImGuiConfigRenderContext.Instance, indentAmount, resultId, children)
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
    /// <param name="renderer">The renderer used for spacing, indentation, highlight, scroll, and focus operations.</param>
    /// <param name="indentAmount">The optional indentation width applied around the draw action.</param>
    /// <param name="resultId">The stable search-result identifier for this row, or <see langword="null"/> when the node does not participate as a searchable leaf result.</param>
    /// <param name="children">The optional child nodes routed through this parameter node when it acts as a searchable container wrapper.</param>
    /// <exception cref="ArgumentNullException"><paramref name="isVisible"/>, <paramref name="draw"/>, or <paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal ParameterNode(
        Func<bool> isVisible,
        Action draw,
        int order,
        int spacingBefore,
        int spacingAfter,
        IParameterNodeRenderer renderer,
        float? indentAmount = null,
        string? resultId = null,
        List<IDrawNode>? children = null)
    {
        ArgumentNullException.ThrowIfNull(isVisible);
        ArgumentNullException.ThrowIfNull(draw);
        ArgumentNullException.ThrowIfNull(renderer);
        _isVisible = isVisible;
        _draw = draw;
        _children = children;
        _indentAmount = indentAmount;
        _spacingBefore = spacingBefore;
        _spacingAfter = spacingAfter;
        _resultId = resultId;
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
        if (!_searchVisible) return;
        if (!IsRuntimeVisible()) return;

        for (var i = 0; i < _spacingBefore; i++) _renderer.Spacing();

        var highlightDepth = PushSearchHighlight();
        try
        {
            if (_indentAmount.HasValue)
            {
                var amount = _indentAmount.Value;
                _renderer.Indent(amount);
                try
                {
                    DrawCore();
                }
                finally
                {
                    _renderer.Unindent(amount);
                }
            }
            else
            {
                DrawCore();
            }
        }
        finally
        {
            if (highlightDepth > 0)
                _renderer.PopStyleColor(highlightDepth);
        }

        for (var i = 0; i < _spacingAfter; i++) _renderer.Spacing();
    }

    bool IConfigSearchNode.ApplySearch(ConfigSearchRenderState? searchState)
    {
        _scrollIntoView = false;
        _focusControl = false;

        if (searchState is null || !searchState.HasActiveQuery)
        {
            _searchVisible = true;
            _searchVisualState = SearchMatchVisualState.None;

            if (_children is null)
                return true;

            for (var i = 0; i < _children.Count; i++)
            {
                if (_children[i] is IConfigSearchNode searchNode)
                    searchNode.ApplySearch(null);
            }

            return true;
        }

        if (_children is not null)
        {
            var hasVisibleChild = false;
            for (var i = 0; i < _children.Count; i++)
            {
                if (_children[i] is IConfigSearchNode searchNode && searchNode.ApplySearch(searchState))
                    hasVisibleChild = true;
            }

            _searchVisible = hasVisibleChild && IsRuntimeVisible();
            _searchVisualState = SearchMatchVisualState.None;
            return _searchVisible;
        }

        var isMatch = searchState.IsMatch(_resultId) && IsRuntimeVisible();
        _searchVisible = isMatch;
        _searchVisualState = !isMatch
            ? SearchMatchVisualState.None
            : searchState.IsFocused(_resultId)
                ? SearchMatchVisualState.FocusedMatch
                : SearchMatchVisualState.Match;

        if (isMatch && searchState.ShouldScrollIntoView(_resultId))
        {
            _scrollIntoView = true;
            searchState.MarkScrolled(_resultId);
        }

        if (isMatch && searchState.ShouldFocusControl(_resultId))
        {
            _focusControl = true;
            searchState.MarkFocused(_resultId);
        }

        return isMatch;
    }

    private bool IsRuntimeVisible()
        => _alwaysVisible || _isVisible!();

    private void DrawCore()
    {
        if (_focusControl)
        {
            _renderer.SetKeyboardFocusHere();
            _focusControl = false;
        }

        _draw();
        if (_scrollIntoView)
        {
            _renderer.SetScrollHereY(0.5f);
            _scrollIntoView = false;
        }
    }

    private int PushSearchHighlight()
    {
        if (_searchVisualState == SearchMatchVisualState.None)
            return 0;

        if (_searchVisualState == SearchMatchVisualState.FocusedMatch)
        {
            _renderer.PushStyleColor(ImGuiCol.Text, FocusedTextColor);
            _renderer.PushStyleColor(ImGuiCol.FrameBg, FocusedFrameColor);
            _renderer.PushStyleColor(ImGuiCol.FrameBgHovered, FocusedFrameHoveredColor);
            _renderer.PushStyleColor(ImGuiCol.FrameBgActive, FocusedFrameActiveColor);
            return 4;
        }

        _renderer.PushStyleColor(ImGuiCol.Text, MatchTextColor);
        _renderer.PushStyleColor(ImGuiCol.FrameBg, MatchFrameColor);
        _renderer.PushStyleColor(ImGuiCol.FrameBgHovered, MatchFrameHoveredColor);
        _renderer.PushStyleColor(ImGuiCol.FrameBgActive, MatchFrameActiveColor);
        return 4;
    }
}
