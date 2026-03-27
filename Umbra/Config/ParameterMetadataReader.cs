using System.Numerics;
using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config;

#pragma warning disable CS0618 // Reader must continue supporting legacy unprefixed attributes for backwards compatibility.

/// <summary>
/// Provides utility methods for reading <see cref="ParameterMetadata"/> from
/// reflected <see cref="MemberInfo"/> instances by inspecting known legacy and
/// Umbra-prefixed metadata attributes.
/// </summary>
internal static class ParameterMetadataReader
{
    /// <summary>
    /// Reads and constructs a <see cref="ParameterMetadata"/> instance from the
    /// custom attributes applied to the specified <paramref name="member"/>.
    /// </summary>
    /// <param name="member">
    /// The <see cref="MemberInfo"/> (e.g. a property or field) to read metadata attributes from.
    /// </param>
    /// <param name="inheritedCategory">
    /// An optional category name to fall back to when no category attribute
    /// is present on <paramref name="member"/>. Typically inherited from the declaring type.
    /// </param>
    /// <param name="parameterKey">
    /// The fully-qualified dot-separated setting key assigned to this parameter by
    /// <see cref="SettingsRegistrar"/>. When provided, <see cref="ParameterMetadata.HiddenLabel"/> is
    /// pre-computed as <c>"##" + parameterKey</c>, avoiding a <c>string.Concat</c> allocation
    /// per <see cref="UI.Config.ConfigDrawer{TConfig}"/> construction. Pass <see langword="null"/> (the default)
    /// when the key is not yet known (e.g. direct or test calls).
    /// </param>
    /// <returns>
    /// A <see cref="ParameterMetadata"/> instance populated from any recognized attributes
    /// found on <paramref name="member"/>. Unset attributes result in <see langword="null"/>
    /// values for the corresponding metadata properties.
    /// </returns>
    internal static ParameterMetadata ReadFrom(MemberInfo member, string? inheritedCategory = null, string? parameterKey = null)
    {
        uint? maxLength = null;
        double? min = null;
        double? max = null;
        double? step = null;
        string? displayName = null;
        string? description = null;
        string? category = null;
        string? format = null;
        ButtonStyle? buttonStyle = null;
        (Vector4 Normal, Vector4 Hovered, Vector4 Active)? customButtonColors = null;
        float? controlWidth = null;
        int? multilineLines = null;
        int? order = null;
        var spacingBefore = 0;
        var spacingAfter = 0;
        float? indent = null;
        Type? customDrawerType = null;
        Type? twoColumnCustomDrawerType = null;
        IHideIfAttribute? hideIf = null;

        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            switch (attr)
            {
                case MaxLengthAttribute legacyMaxLength:
                    maxLength = legacyMaxLength.Length;
                    continue;
                case UmbraMaxLengthAttribute prefixedMaxLength:
                    maxLength = prefixedMaxLength.Length;
                    continue;
                case RangeAttribute legacyRange:
                    min = legacyRange.Min;
                    max = legacyRange.Max;
                    continue;
                case UmbraRangeAttribute prefixedRange:
                    min = prefixedRange.Min;
                    max = prefixedRange.Max;
                    continue;
                case StepAttribute legacyStep:
                    step = legacyStep.Step;
                    continue;
                case UmbraStepAttribute prefixedStep:
                    step = prefixedStep.Step;
                    continue;
                case DisplayNameAttribute legacyDisplayName:
                    displayName = legacyDisplayName.Name;
                    continue;
                case UmbraDisplayNameAttribute prefixedDisplayName:
                    displayName = prefixedDisplayName.Name;
                    continue;
                case DescriptionAttribute legacyDescription:
                    description = legacyDescription.Text;
                    continue;
                case UmbraDescriptionAttribute prefixedDescription:
                    description = prefixedDescription.Text;
                    continue;
                case CategoryAttribute legacyCategory:
                    category = legacyCategory.Name;
                    continue;
                case UmbraCategoryAttribute prefixedCategory:
                    category = prefixedCategory.Name;
                    continue;
                case FormatAttribute legacyFormat:
                    format = legacyFormat.Format;
                    continue;
                case UmbraFormatAttribute prefixedFormat:
                    format = prefixedFormat.Format;
                    continue;
                case ButtonStyleAttribute legacyButtonStyle:
                    buttonStyle = legacyButtonStyle.Style;
                    continue;
                case UmbraButtonStyleAttribute prefixedButtonStyle:
                    buttonStyle = prefixedButtonStyle.Style;
                    continue;
                case CustomButtonColorsAttribute legacyCustomColors:
                    customButtonColors = (
                        new Vector4(legacyCustomColors.NormalR, legacyCustomColors.NormalG, legacyCustomColors.NormalB, legacyCustomColors.NormalA),
                        new Vector4(legacyCustomColors.HoveredR, legacyCustomColors.HoveredG, legacyCustomColors.HoveredB, legacyCustomColors.HoveredA),
                        new Vector4(legacyCustomColors.ActiveR, legacyCustomColors.ActiveG, legacyCustomColors.ActiveB, legacyCustomColors.ActiveA));
                    continue;
                case UmbraCustomButtonColorsAttribute prefixedCustomColors:
                    customButtonColors = (
                        new Vector4(prefixedCustomColors.NormalR, prefixedCustomColors.NormalG, prefixedCustomColors.NormalB, prefixedCustomColors.NormalA),
                        new Vector4(prefixedCustomColors.HoveredR, prefixedCustomColors.HoveredG, prefixedCustomColors.HoveredB, prefixedCustomColors.HoveredA),
                        new Vector4(prefixedCustomColors.ActiveR, prefixedCustomColors.ActiveG, prefixedCustomColors.ActiveB, prefixedCustomColors.ActiveA));
                    continue;
                case ButtonWidthAttribute legacyButtonWidth:
                    controlWidth = legacyButtonWidth.Width;
                    continue;
                case ControlWidthAttribute legacyControlWidth:
                    controlWidth = legacyControlWidth.Width;
                    continue;
                case UmbraControlWidthAttribute prefixedControlWidth:
                    controlWidth = prefixedControlWidth.Width;
                    continue;
                case MultilineAttribute legacyMultiline:
                    multilineLines = legacyMultiline.Lines;
                    continue;
                case UmbraMultilineAttribute prefixedMultiline:
                    multilineLines = prefixedMultiline.Lines;
                    continue;
                case ParameterOrderAttribute legacyOrder:
                    order = legacyOrder.Order;
                    continue;
                case UmbraParameterOrderAttribute prefixedOrder:
                    order = prefixedOrder.Order;
                    continue;
                case SpacingBeforeAttribute legacySpacingBefore:
                    spacingBefore = legacySpacingBefore.Count;
                    continue;
                case UmbraSpacingBeforeAttribute prefixedSpacingBefore:
                    spacingBefore = prefixedSpacingBefore.Count;
                    continue;
                case SpacingAfterAttribute legacySpacingAfter:
                    spacingAfter = legacySpacingAfter.Count;
                    continue;
                case UmbraSpacingAfterAttribute prefixedSpacingAfter:
                    spacingAfter = prefixedSpacingAfter.Count;
                    continue;
                case IndentAttribute legacyIndent:
                    indent = legacyIndent.Amount;
                    continue;
                case UmbraIndentAttribute prefixedIndent:
                    indent = prefixedIndent.Amount;
                    continue;
            }

            if (attr is ICustomDrawerAttribute cd) { customDrawerType = cd.DrawerType; continue; }
            if (attr is ITwoColumnCustomDrawerAttribute tcd) { twoColumnCustomDrawerType = tcd.DrawerType; continue; }
            if (attr is IHideIfAttribute h) hideIf = h;
        }

        var inferredFloatFormat = format ?? FallbackFloatFormat(step);

        return new ParameterMetadata
        {
            DisplayName = displayName,
            ResolvedLabel = displayName ?? member.Name.ToDisplayName(),
            Description = description,
            MaxLength = maxLength,
            Min = min,
            Max = max,
            Step = step,
            Category = category ?? inheritedCategory,
            Format = format,
            ButtonStyle = buttonStyle,
            CustomButtonColors = customButtonColors,
            ControlWidth = controlWidth,
            MultilineLines = multilineLines,
            Order = order,
            SpacingBefore = spacingBefore,
            SpacingAfter = spacingAfter,
            Indent = indent,
            CustomDrawerType = customDrawerType,
            TwoColumnCustomDrawerType = twoColumnCustomDrawerType,
            HideIf = hideIf,
            InferredFloatFormat = inferredFloatFormat,
            HiddenLabel = parameterKey is not null ? string.Concat("##", parameterKey) : null,
        };
    }

    /// <summary>
    /// Derives a printf float format string from the decimal-place count of <paramref name="step"/>.
    /// Returns <c>"%.2f"</c> when <paramref name="step"/> is <see langword="null"/> or zero.
    /// </summary>
    /// <param name="step">
    /// The step value whose decimal-place count determines the format precision,
    /// or <see langword="null"/> / <c>0</c> to use the default <c>"%.2f"</c>.
    /// </param>
    /// <returns>
    /// A printf-style format string such as <c>"%.2f"</c> (default), <c>"%.0f"</c> (integer step),
    /// or <c>"%.Nf"</c> where N is the decimal-place count of <paramref name="step"/>.
    /// </returns>
    private static string FallbackFloatFormat(double? step)
    {
        if (step is null or 0) return "%.2f";
        var s = step.Value.ToString("G");
        var dot = s.IndexOf('.');
        return dot < 0 ? "%.0f" : $"%.{s.Length - dot - 1}f";
    }
}

#pragma warning restore CS0618
