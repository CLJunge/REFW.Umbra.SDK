using System.Numerics;
using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config;

/// <summary>
/// Provides utility methods for reading <see cref="ParameterMetadata"/> from
/// reflected <see cref="MemberInfo"/> instances by inspecting the Umbra-prefixed metadata attributes.
/// </summary>
internal static class ParameterMetadataReader
{
    /// <summary>
    /// Reads and constructs a <see cref="ParameterMetadata"/> instance from the
    /// custom attributes applied to the specified <paramref name="member"/>.
    /// </summary>
    internal static ParameterMetadata ReadFrom(MemberInfo member, string? inheritedCategory = null, string? parameterKey = null)
    {
        var maxLength = member.GetCustomAttribute<UmbraMaxLengthAttribute>();
        var range = member.GetCustomAttribute<UmbraRangeAttribute>();
        var step = member.GetCustomAttribute<UmbraStepAttribute>();
        var name = member.GetCustomAttribute<UmbraDisplayNameAttribute>();
        var desc = member.GetCustomAttribute<UmbraDescriptionAttribute>();
        var category = member.GetCustomAttribute<UmbraCategoryAttribute>();
        var format = member.GetCustomAttribute<UmbraFormatAttribute>();
        var buttonStyle = member.GetCustomAttribute<UmbraButtonStyleAttribute>();
        var customButtonColors = member.GetCustomAttribute<UmbraCustomButtonColorsAttribute>();
        var controlWidth = member.GetCustomAttribute<UmbraControlWidthAttribute>();
        var multiline = member.GetCustomAttribute<UmbraMultilineAttribute>();
        var order = member.GetCustomAttribute<UmbraParameterOrderAttribute>();
        var spacingBefore = member.GetCustomAttribute<UmbraSpacingBeforeAttribute>();
        var spacingAfter = member.GetCustomAttribute<UmbraSpacingAfterAttribute>();
        var indent = member.GetCustomAttribute<UmbraIndentAttribute>();

        Type? customDrawerType = null;
        Type? twoColumnCustomDrawerType = null;
        IHideIfAttribute? hideIf = null;
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            if (attr is ICustomDrawerAttribute cd) { customDrawerType = cd.DrawerType; continue; }
            if (attr is ITwoColumnCustomDrawerAttribute tcd) { twoColumnCustomDrawerType = tcd.DrawerType; continue; }
            if (attr is IHideIfAttribute h) hideIf = h;
        }

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
    private static string FallbackFloatFormat(double? step)
    {
        if (step is null or 0) return "%.2f";
        var s = step.Value.ToString("G");
        var dot = s.IndexOf('.');
        return dot < 0 ? "%.0f" : $"%.{s.Length - dot - 1}f";
    }
}
