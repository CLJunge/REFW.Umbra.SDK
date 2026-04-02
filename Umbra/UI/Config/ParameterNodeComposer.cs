using Umbra.Config;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Creates draw nodes for leaf configuration parameters.
/// </summary>
/// <remarks>
/// This type isolates parameter-row composition from <see cref="ConfigDrawerBuilder"/> so the
/// builder remains focused on traversing the configuration object graph.
/// </remarks>
internal static class ParameterNodeComposer
{
    private static readonly Func<bool> s_alwaysVisible = static () => true;

    /// <summary>
    /// Creates the draw node for one leaf <see cref="IParameter"/> along with any disposable
    /// resource produced by the resolved control drawer.
    /// </summary>
    /// <param name="parameter">The parameter to render.</param>
    /// <param name="owner">The configuration object instance that owns the parameter.</param>
    /// <param name="alignmentGroup">The alignment group shared by the current scope or category.</param>
    /// <param name="classIndentAmount">The type-level indent amount applied when the parameter itself has no explicit indent.</param>
    /// <param name="classLabelMarginPixels">The type-level label margin applied to the shared alignment group.</param>
    /// <returns>
    /// A tuple containing the composed <see cref="ParameterNode"/> and any optional disposable
    /// resource created while resolving the control drawer.
    /// </returns>
    internal static (ParameterNode Node, IDisposable? Resource) Create(
        IParameter parameter,
        object owner,
        LabelAlignmentGroup alignmentGroup,
        float? classIndentAmount,
        float? classLabelMarginPixels)
    {
        var meta = parameter.Metadata;
        if (classLabelMarginPixels.HasValue && alignmentGroup.Margin != classLabelMarginPixels.Value)
            alignmentGroup.Margin = classLabelMarginPixels.Value;

        var (draw, resource) = ControlFactory.BuildDrawAction(parameter, meta.ResolvedLabel, alignmentGroup);

        var indentAmount = meta.Indent ?? classIndentAmount;
        var isVisible = meta.HideIf is not null
            ? VisibilityPredicateResolver.Build(meta.HideIf, owner)
            : s_alwaysVisible;

        return (
            new ParameterNode(
                isVisible,
                draw,
                meta.Order ?? int.MaxValue,
                meta.SpacingBefore,
                meta.SpacingAfter,
                indentAmount),
            resource);
    }
}
