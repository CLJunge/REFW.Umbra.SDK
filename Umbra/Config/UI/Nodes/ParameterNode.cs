using Hexa.NET.ImGui;

namespace Umbra.Config.UI.Nodes;

/// <summary>
/// Draw node that conditionally invokes a per-frame draw <see cref="Action"/> based on a visibility predicate.
/// </summary>
/// <param name="isVisible">
/// Predicate evaluated each frame; the draw action is invoked only when it returns <see langword="true"/>.
/// </param>
/// <param name="draw">The per-frame ImGui draw action to invoke when the parameter is visible.</param>
/// <param name="order">
/// Sort key used to order this node within its category context during the build pass.
/// Lower values appear first; defaults to <see cref="int.MaxValue"/> so unordered nodes sort after
/// all explicitly ordered ones while preserving original declaration order via stable sort.
/// </param>
/// <param name="spacingBefore">
/// Number of <c>ImGui.Spacing()</c> calls emitted before the draw action when the parameter is visible.
/// Absorbed from <see cref="Umbra.Config.Attributes.SpacingBeforeAttribute"/> during the build pass. Defaults to <c>0</c>.
/// </param>
/// <param name="spacingAfter">
/// Number of <c>ImGui.Spacing()</c> calls emitted after the draw action when the parameter is visible.
/// Absorbed from <see cref="Umbra.Config.Attributes.SpacingAfterAttribute"/> during the build pass. Defaults to <c>0</c>.
/// </param>
internal sealed class ParameterNode(
    Func<bool> isVisible,
    Action draw,
    int order = int.MaxValue,
    int spacingBefore = 0,
    int spacingAfter = 0) : IDrawNode
{
    /// <summary>Gets the sort key for this node within its category context.</summary>
    internal int Order { get; } = order;

    /// <inheritdoc/>
    public void Draw()
    {
        if (!isVisible()) return;
        for (var i = 0; i < spacingBefore; i++) ImGui.Spacing();
        draw();
        for (var i = 0; i < spacingAfter; i++) ImGui.Spacing();
    }
}
