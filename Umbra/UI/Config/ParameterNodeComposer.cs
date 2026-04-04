using Umbra.Config;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Composes the draw node used for one leaf configuration parameter.
/// </summary>
/// <remarks>
/// This helper combines control resolution, label-alignment configuration, wrapper metadata, and visibility handling into the final <see cref="ParameterNode"/> consumed by the configuration draw tree.
/// </remarks>
internal static class ParameterNodeComposer
{
    /// <summary>
    /// Creates the draw node for one registered parameter together with any disposable resource created while resolving its renderer.
    /// </summary>
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
        if (meta.HideIf is null)
            return (
                new ParameterNode(
                    draw,
                    meta.Order ?? int.MaxValue,
                    meta.SpacingBefore,
                    meta.SpacingAfter,
                    indentAmount,
                    parameter.Key),
                resource);

        var isVisible = VisibilityPredicateResolver.Build(meta.HideIf, owner);

        return (
            new ParameterNode(
                isVisible,
                draw,
                meta.Order ?? int.MaxValue,
                meta.SpacingBefore,
                meta.SpacingAfter,
                indentAmount,
                parameter.Key),
            resource);
    }
}
