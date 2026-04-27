using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Creates the precomputed <see cref="ControlLayout"/> values used for parameter rows.
/// </summary>
/// <remarks>
/// This helper resolves hidden-label fallbacks, description presence, default control widths, and alignment-group registration so the control builders can stay focused on widget-specific behavior.
/// </remarks>
internal static class ControlLayoutFactory
{
    private const float _defaultFillControlWidth = -1f;
    private const string _hiddenLabelPrefix = "##";

    /// <summary>
    /// Builds the precomputed layout state for one parameter row.
    /// </summary>
    /// <param name="label">The resolved display label.</param>
    /// <param name="parameter">The parameter being rendered.</param>
    /// <param name="alignGroup">The alignment group shared by the current scope.</param>
    /// <returns>The precomputed layout state for the row.</returns>
    internal static ControlLayout Create(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(alignGroup);

        var meta = parameter.Metadata;
        var hasDescription = meta.Description is not null;
        var hiddenLabel = meta.HiddenLabel;
        if (hiddenLabel is null)
        {
            var key = parameter.Key;
            hiddenLabel = key is null ? _hiddenLabelPrefix : string.Concat(_hiddenLabelPrefix, key);
        }

        alignGroup.Register(label, hasDescription);

        return new ControlLayout(
            label,
            meta.Description,
            alignGroup,
            meta.ControlWidth.GetValueOrDefault(_defaultFillControlWidth),
            hiddenLabel);
    }
}
