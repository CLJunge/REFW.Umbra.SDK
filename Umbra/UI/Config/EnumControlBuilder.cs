using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds combo-box draw actions for enum and nullable-enum parameters.
/// </summary>
/// <remarks>
/// Nullable-enum parameters are rendered with an additional synthetic <c>&lt;None&gt;</c> entry that maps back to <see langword="null"/>.
/// </remarks>
internal static class EnumControlBuilder
{
    /// <summary>
    /// Builds the per-frame draw action for an enum or nullable-enum parameter.
    /// </summary>
    internal static Action Build(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var enumType = Nullable.GetUnderlyingType(parameter.ValueType) ?? parameter.ValueType;
        var isNullableEnum = enumType != parameter.ValueType;
        var rawNames = Enum.GetNames(enumType);
        var rawValues = Enum.GetValues(enumType);
        var names = new string[rawNames.Length + (isNullableEnum ? 1 : 0)];
        var values = new object?[rawValues.Length + (isNullableEnum ? 1 : 0)];

        var offset = 0;
        if (isNullableEnum)
        {
            names[0] = "<None>";
            values[0] = null;
            offset = 1;
        }

        for (var i = 0; i < rawValues.Length; i++)
        {
            names[i + offset] = rawNames[i];
            values[i + offset] = rawValues.GetValue(i)!;
        }

        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);
        return () =>
        {
            var current = parameter.GetValue();
            var idx = Array.IndexOf(values, current);
            if (idx < 0) idx = 0;
            layout.Pre();
            if (ImGui.Combo(layout.HiddenLabel, ref idx, names, names.Length))
                parameter.SetValue(values[idx]);
        };
    }
}
