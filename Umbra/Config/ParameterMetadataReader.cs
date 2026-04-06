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
    private readonly record struct ValidationMetadataValues(
        bool Required,
        bool AllowWhitespace,
        uint? MinLength,
        uint? MaxLength,
        double? Min,
        double? Max,
        string? RegexPattern,
        string? RegexMessage,
        Type? ValidatorType);

    /// <summary>
    /// Builds a <see cref="ParameterMetadata"/> instance from the attributes applied to <paramref name="member"/>.
    /// </summary>
    /// <param name="member">The reflected config member whose attributes should be read.</param>
    /// <param name="inheritedCategory">The category inherited from an enclosing config scope, or <see langword="null"/> when none applies.</param>
    /// <param name="parameterKey">The fully qualified persisted key, used to precompute cached UI labels when available.</param>
    /// <returns>The resolved metadata for <paramref name="member"/>.</returns>
    internal static ParameterMetadata ReadFrom(MemberInfo member, string? inheritedCategory = null, string? parameterKey = null)
    {
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
        var validation = ReadValidationMetadata(member);

        Type? drawerType = null;
        Type? twoColumnDrawerType = null;
        IHideIfAttribute? hideIf = null;
        IDisableIfAttribute? disableIf = null;
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            if (attr is IDrawerAttribute cd) { drawerType = cd.DrawerType; continue; }
            if (attr is ITwoColumnDrawerAttribute tcd) { twoColumnDrawerType = tcd.DrawerType; continue; }
            if (attr is IHideIfAttribute h) { hideIf = h; continue; }
            if (attr is IDisableIfAttribute d) disableIf = d;
        }

        var inferredFloatFormat = format?.Format ?? FallbackFloatFormat(step?.Step);

        return new ParameterMetadata
        {
            DisplayName = name?.Name,
            ResolvedLabel = name?.Name ?? member.Name.ToDisplayName(),
            Description = desc?.Text,
            Required = validation.Required,
            AllowWhitespace = validation.AllowWhitespace,
            MinLength = validation.MinLength,
            MaxLength = validation.MaxLength,
            Min = validation.Min,
            Max = validation.Max,
            Step = step?.Step,
            Category = category?.Name ?? inheritedCategory,
            Format = format?.Format,
            RegexPattern = validation.RegexPattern,
            RegexMessage = validation.RegexMessage,
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
            ValidatorType = validation.ValidatorType,
            HideIf = hideIf,
            DisableIf = disableIf,
            InferredFloatFormat = inferredFloatFormat,
            HiddenLabel = parameterKey is not null ? string.Concat("##", parameterKey) : null,
        };
    }

    /// <summary>
    /// Reads the validation-specific attribute metadata applied to <paramref name="member"/>.
    /// </summary>
    /// <param name="member">The reflected config member whose validation attributes should be read.</param>
    /// <returns>The resolved validation-specific metadata values.</returns>
    private static ValidationMetadataValues ReadValidationMetadata(MemberInfo member)
    {
        var required = member.GetCustomAttribute<UmbraRequiredAttribute>();
        var minLength = member.GetCustomAttribute<UmbraMinLengthAttribute>();
        var maxLength = member.GetCustomAttribute<UmbraMaxLengthAttribute>();
        var range = member.GetCustomAttribute<UmbraRangeAttribute>();
        var regex = member.GetCustomAttribute<UmbraRegexAttribute>();

        Type? validatorType = null;
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            if (attr is IValidatorAttribute validator)
            {
                validatorType = validator.ValidatorType;
                break;
            }
        }

        return new ValidationMetadataValues(
            required is not null,
            required?.AllowWhitespace ?? false,
            minLength?.Length,
            maxLength?.Length,
            range?.Min,
            range?.Max,
            regex?.Pattern,
            regex?.Message,
            validatorType);
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
