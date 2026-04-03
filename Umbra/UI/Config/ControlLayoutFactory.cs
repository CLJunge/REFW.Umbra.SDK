using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Creates precomputed <see cref="ControlLayout"/> values for parameter rows.
/// </summary>
/// <remarks>
/// This type isolates layout-value construction from <see cref="ControlFactory"/>, including
/// hidden-label fallback resolution, description detection, default width selection, and alignment
/// group registration.
/// </remarks>
internal static class ControlLayoutFactory
{
    private const float DefaultFillControlWidth = -1f;
    private const string HiddenLabelPrefix = "##";

    /// <summary>
    /// Constructs the precomputed layout state for one parameter row.
    /// </summary>
    /// <param name="label">The resolved display label for the parameter.</param>
    /// <param name="parameter">The parameter being rendered.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>
    /// A <see cref="ControlLayout"/> capturing the label, description, alignment group, widget width,
    /// and hidden ImGui label for the row.
    /// </returns>
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
            hiddenLabel = key is null ? HiddenLabelPrefix : string.Concat(HiddenLabelPrefix, key);
        }

        alignGroup.Register(label, hasDescription);

        return new ControlLayout(
            label,
            meta.Description,
            alignGroup,
            meta.ControlWidth.GetValueOrDefault(DefaultFillControlWidth),
            hiddenLabel);
    }
}
