using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds per-frame ImGui draw actions for built-in enum parameter types.
/// </summary>
/// <remarks>
/// This type isolates enum combo-box composition from <see cref="ControlFactory"/>.
/// </remarks>
internal static class EnumControlBuilder
{
    /// <summary>
    /// Builds a per-frame draw action that renders a <c>Combo</c> box for an enum parameter.
    /// </summary>
    internal static Action Build(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var enumType = parameter.ValueType;
        var names = Enum.GetNames(enumType);
        var rawValues = Enum.GetValues(enumType);
        var values = new object[rawValues.Length];
        for (var i = 0; i < rawValues.Length; i++)
            values[i] = rawValues.GetValue(i)!;

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
