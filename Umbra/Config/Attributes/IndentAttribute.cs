using Hexa.NET.ImGui;

namespace Umbra.Config.Attributes;

/// <summary>
/// Indents the decorated parameter control, or all parameter controls within a settings
/// group class, in the settings UI.
/// </summary>
/// <remarks>
/// <para>
/// When applied to a discovered <see cref="Parameter{T}"/> property, only that individual
/// control is indented.
/// </para>
/// <para>
/// When applied to a settings group class decorated with <see cref="AutoRegisterSettingsAttribute"/>,
/// all parameters in that group inherit the indentation. A property-level <c>[Indent]</c> on
/// an individual parameter within the group overrides the class-level value for that control.
/// </para>
/// <para>
/// Wraps the control with <see cref="ImGui.Indent(float)"/> / <see cref="ImGui.Unindent(float)"/>.
/// When <paramref name="amount"/> is <c>0</c>, ImGui's default indent spacing
/// (<c>ImGui.GetStyle().IndentSpacing</c>) is used.
/// </para>
/// <para>
/// Although this attribute permits field targets, the built-in settings registrar and drawer
/// inspect public instance properties and types only.
/// </para>
/// </remarks>
/// <param name="amount">
/// The indentation width in pixels, or <c>0</c> to use ImGui's default indent spacing.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
[Obsolete("Use UmbraIndentAttribute instead for the collision-safe Umbra-prefixed name.")]
public class IndentAttribute(float amount = 0f) : Attribute
{
    /// <summary>Gets the indentation width in pixels. <c>0</c> means use ImGui's default.</summary>
    public float Amount { get; } = amount;
}
