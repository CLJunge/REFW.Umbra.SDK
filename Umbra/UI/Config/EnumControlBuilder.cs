using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds per-frame ImGui draw actions for built-in enum parameter types.
/// </summary>
/// <remarks>
/// This type isolates enum combo-box composition from <see cref="ControlFactory"/>.
/// Nullable enum parameters are also supported: the combo prepends a synthetic <c>&lt;None&gt;</c>
/// option that maps back to <see langword="null"/>.
/// </remarks>
internal static class EnumControlBuilder
{
    /// <summary>
    /// Builds a per-frame draw action that renders a <c>Combo</c> box for an enum or nullable-enum parameter.
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
