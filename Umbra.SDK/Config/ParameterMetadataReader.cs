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
    /// <returns>
    /// A <see cref="ParameterMetadata"/> instance populated from any recognized attributes
    /// found on <paramref name="member"/>. Unset attributes result in <see langword="null"/>
    /// values for the corresponding metadata properties.
    /// </returns>
    internal static ParameterMetadata ReadFrom(MemberInfo member, string? inheritedCategory = null)
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
        var order = member.GetCustomAttribute<OrderAttribute>();

        return new ParameterMetadata
        {
            DisplayName = name?.Name,
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
        };
    }
}
