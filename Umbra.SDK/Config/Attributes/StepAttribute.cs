namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Specifies the drag speed for an unconstrained numeric settings parameter and is also
/// used to infer the display format's decimal precision when <c>[Format]</c> is absent.
/// </summary>
/// <remarks>
/// <para>
/// For <c>float</c> and <c>double</c> parameters without a <c>[Range]</c>, this value is
/// passed directly as the speed argument to <c>ImGui.DragFloat</c>. For <c>int</c>
/// parameters without a <c>[Range]</c>, it is passed to <c>ImGui.DragInt</c>. The
/// drag-speed value has no effect on slider controls (<c>SliderFloat</c> /
/// <c>SliderInt</c>), which are used when <c>[Range]</c> is also present.
/// </para>
/// <para>
/// For <c>float</c> and <c>double</c> parameters, the decimal place count of the step
/// value is used to derive the fallback printf format string (e.g. a step of <c>0.25</c>
/// produces <c>"%.2f"</c>), and this format inference applies to both drag controls and
/// sliders. For <c>int</c> parameters, format inference is not performed; the display
/// format always falls back to <c>"%d"</c> regardless of this attribute, and
/// <c>SliderInt</c> is unaffected by step entirely. Use <c>[Format]</c> to override
/// the format string explicitly on any numeric type.
/// </para>
/// </remarks>
/// <param name="step">
/// The drag speed for unconstrained numeric controls. Also used to infer the decimal
/// precision of the fallback display format string for <c>float</c> and <c>double</c> types.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class StepAttribute(double step) : Attribute
{
    /// <summary>
    /// Gets the drag speed for the parameter. Also used to infer the decimal precision
    /// of the fallback display format string for floating-point types.
    /// </summary>
    public double Step { get; } = step;
}
