using System.Numerics;
using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config;

/// <summary>
/// Reads <see cref="ParameterMetadata"/> from Umbra configuration attributes on reflected members.
/// </summary>
/// <remarks>
/// This helper centralizes metadata extraction so registration resolves labels, ranges, drawer types, visibility rules, and cached render strings once instead of rediscovering them during UI construction.
/// </remarks>
internal static class ParameterMetadataReader
{
    /// <summary>
    /// Builds a <see cref="ParameterMetadata"/> instance from the attributes applied to <paramref name="member"/>.
    /// </summary>
    /// <param name="member">The reflected settings member whose attributes should be read.</param>
    /// <param name="inheritedCategory">The category inherited from an enclosing settings scope, or <see langword="null"/> when none applies.</param>
    /// <param name="parameterKey">The fully qualified persisted key, used to precompute cached UI labels when available.</param>
    /// <returns>The resolved metadata for <paramref name="member"/>.</returns>
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

        Type? drawerType = null;
        Type? twoColumnDrawerType = null;
        IHideIfAttribute? hideIf = null;
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            if (attr is IDrawerAttribute cd) { drawerType = cd.DrawerType; continue; }
            if (attr is ITwoColumnDrawerAttribute tcd) { twoColumnDrawerType = tcd.DrawerType; continue; }
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
            DrawerType = drawerType,
            TwoColumnDrawerType = twoColumnDrawerType,
            HideIf = hideIf,
            InferredFloatFormat = inferredFloatFormat,
            HiddenLabel = parameterKey is not null ? string.Concat("##", parameterKey) : null,
        };
    }

    /// <summary>
    /// Derives the fallback ImGui float format string for a numeric parameter from <paramref name="step"/>.
    /// </summary>
    /// <param name="step">The configured numeric step value, or <see langword="null"/> when no step is declared.</param>
    /// <returns>A printf-style float format string.</returns>
    /// <remarks>
    /// Returns <c>"%.2f"</c> when <paramref name="step"/> is <see langword="null"/> or zero.
    /// </remarks>
    private static string FallbackFloatFormat(double? step)
    {
        if (step is null or 0) return "%.2f";
        var s = step.Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        var dot = s.IndexOf('.');
        return dot < 0 ? "%.0f" : $"%.{s.Length - dot - 1}f";
    }
}
