using System.Numerics;
using System.Reflection;
using Umbra.SDK.Config.Attributes;

namespace Umbra.SDK.Config;

/// <summary>
/// Provides utility methods for reading <see cref="ParameterMetadata"/> from
/// reflected <see cref="MemberInfo"/> instances by inspecting known metadata attributes.
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
    /// An optional category name to fall back to when no <see cref="CategoryAttribute"/>
    /// is present on <paramref name="member"/>. Typically inherited from the declaring type.
    /// </param>
    /// <param name="parameterKey">
    /// The fully-qualified dot-separated setting key assigned to this parameter by
    /// <c>SettingsRegistrar</c>. When provided, <see cref="ParameterMetadata.HiddenLabel"/> is
    /// pre-computed as <c>"##" + parameterKey</c>, avoiding a <c>string.Concat</c> allocation
    /// per <c>ConfigDrawer{TConfig}</c> construction. Pass <see langword="null"/> (the default)
    /// when the key is not yet known (e.g. direct or test calls).
    /// </param>
    /// <returns>
    /// A <see cref="ParameterMetadata"/> instance populated from any recognized attributes
    /// found on <paramref name="member"/>. Unset attributes result in <see langword="null"/>
    /// values for the corresponding metadata properties.
    /// </returns>
    internal static ParameterMetadata ReadFrom(MemberInfo member, string? inheritedCategory = null, string? parameterKey = null)
    {
        var maxLength = member.GetCustomAttribute<MaxLengthAttribute>();
        var range = member.GetCustomAttribute<RangeAttribute>();
        var step = member.GetCustomAttribute<StepAttribute>();
        var name = member.GetCustomAttribute<DisplayNameAttribute>();
        var desc = member.GetCustomAttribute<DescriptionAttribute>();
        var category = member.GetCustomAttribute<CategoryAttribute>();
        var format = member.GetCustomAttribute<FormatAttribute>();
        var buttonStyle = member.GetCustomAttribute<ButtonStyleAttribute>();
        var customButtonColors = member.GetCustomAttribute<CustomButtonColorsAttribute>();
        var buttonWidth = member.GetCustomAttribute<ButtonWidthAttribute>();
        // ButtonWidthAttribute derives from ControlWidthAttribute, so GetCustomAttribute<ControlWidthAttribute>()
        // would also match [ButtonWidth] decorated members. Resolve ControlWidth only when ButtonWidthAttribute
        // is absent so the two metadata slots remain independent.
        var controlWidth = buttonWidth is null ? member.GetCustomAttribute<ControlWidthAttribute>() : null;
        var multiline = member.GetCustomAttribute<MultilineAttribute>();
        var order = member.GetCustomAttribute<ParameterOrderAttribute>();
        var spacingBefore = member.GetCustomAttribute<SpacingBeforeAttribute>();
        var spacingAfter = member.GetCustomAttribute<SpacingAfterAttribute>();
        var indent = member.GetCustomAttribute<IndentAttribute>();

        // Interface-typed attributes cannot be found via GetCustomAttribute<T>.
        // Scan the full attribute list once to capture all three in a single pass.
        Type? customDrawerType = null;
        Type? twoColumnCustomDrawerType = null;
        IHideIfAttribute? hideIf = null;
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            if (attr is ICustomDrawerAttribute cd) { customDrawerType = cd.DrawerType; continue; }
            if (attr is ITwoColumnCustomDrawerAttribute tcd) { twoColumnCustomDrawerType = tcd.DrawerType; continue; }
            if (attr is IHideIfAttribute h) hideIf = h;
        }

        // Precompute the effective float format once. This collapses Number.FormatFloat
        // (called by step.Value.ToString("G") inside the old FallbackFloatFormat) out of
        // the draw-tree construction path entirely.
        var inferredFloatFormat = format?.Format ?? FallbackFloatFormat(step?.Step);

        return new ParameterMetadata
        {
            DisplayName = name?.Name,
            ResolvedLabel = name?.Name ?? member.Name.ToDisplayName(),
            Description = desc?.Text,
            MaxLength = maxLength?.Length,
            Min = range?.Min,
            Max = range?.Max,
            Step = step?.Step,
            Category = category?.Name ?? inheritedCategory,
            Format = format?.Format,
            ButtonStyle = buttonStyle?.Style,
            CustomButtonColors = customButtonColors is null ? null : (
                new Vector4(customButtonColors.NormalR, customButtonColors.NormalG, customButtonColors.NormalB, customButtonColors.NormalA),
                new Vector4(customButtonColors.HoveredR, customButtonColors.HoveredG, customButtonColors.HoveredB, customButtonColors.HoveredA),
                new Vector4(customButtonColors.ActiveR, customButtonColors.ActiveG, customButtonColors.ActiveB, customButtonColors.ActiveA)
            ),
            ButtonWidth = buttonWidth?.Width,
            ControlWidth = controlWidth?.Width,
            MultilineLines = multiline?.Lines,
            Order = order?.Order,
            SpacingBefore = spacingBefore?.Count ?? 0,
            SpacingAfter = spacingAfter?.Count ?? 0,
            Indent = indent?.Amount,
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
