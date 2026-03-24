using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.Config.UI;

/// <summary>
/// Defines the ImGui color table for each <see cref="ButtonStyle"/> variant
/// and provides push/pop color scope helpers for rendering styled buttons.
/// </summary>
internal static class ButtonStyleColors
{
    // Three ImGui color slots (normal / hovered / active) for each non-default style variant.
    private static readonly Dictionary<ButtonStyle, (Vector4 Normal, Vector4 Hovered, Vector4 Active)> _colors =
        new()
        {
            [ButtonStyle.Primary] = (
                new Vector4(0.20f, 0.45f, 0.80f, 1f),
                new Vector4(0.30f, 0.55f, 0.90f, 1f),
                new Vector4(0.15f, 0.38f, 0.72f, 1f)),
            [ButtonStyle.Success] = (
                new Vector4(0.18f, 0.55f, 0.18f, 1f),
                new Vector4(0.26f, 0.66f, 0.26f, 1f),
                new Vector4(0.12f, 0.46f, 0.12f, 1f)),
            [ButtonStyle.Warning] = (
                new Vector4(0.78f, 0.50f, 0.08f, 1f),
                new Vector4(0.88f, 0.60f, 0.14f, 1f),
                new Vector4(0.68f, 0.42f, 0.04f, 1f)),
            [ButtonStyle.Danger] = (
                new Vector4(0.72f, 0.15f, 0.15f, 1f),
                new Vector4(0.86f, 0.25f, 0.25f, 1f),
                new Vector4(0.60f, 0.10f, 0.10f, 1f)),
        };

    /// <summary>
    /// Pushes the three button color slots (<see cref="ImGuiCol.Button"/>,
    /// <see cref="ImGuiCol.ButtonHovered"/>, <see cref="ImGuiCol.ButtonActive"/>)
    /// for the requested <paramref name="style"/>.
    /// </summary>
    /// <param name="style">The style variant whose colors should be applied.</param>
    /// <returns>
    /// <see langword="true"/> when colors were pushed and <see cref="Pop"/> must be called;
    /// <see langword="false"/> for <see cref="ButtonStyle.Default"/>, which leaves the
    /// active ImGui theme untouched.
    /// </returns>
    internal static bool Push(ButtonStyle style)
    {
        if (!_colors.TryGetValue(style, out var c)) return false;
        ImGui.PushStyleColor(ImGuiCol.Button, c.Normal);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, c.Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, c.Active);
        return true;
    }

    /// <summary>
    /// Pushes fully custom RGBA colors into the three button color slots
    /// (<see cref="ImGuiCol.Button"/>, <see cref="ImGuiCol.ButtonHovered"/>,
    /// <see cref="ImGuiCol.ButtonActive"/>).
    /// Always returns <see langword="true"/>; <see cref="Pop"/> must always be called
    /// after the button widget is rendered.
    /// </summary>
    /// <param name="normal">Color applied to <see cref="ImGuiCol.Button"/>.</param>
    /// <param name="hovered">Color applied to <see cref="ImGuiCol.ButtonHovered"/>.</param>
    /// <param name="active">Color applied to <see cref="ImGuiCol.ButtonActive"/>.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    internal static bool Push(Vector4 normal, Vector4 hovered, Vector4 active)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, normal);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, active);
        return true;
    }

    /// <summary>
    /// Pops the three color slots pushed by <see cref="Push(ButtonStyle)"/> or <see cref="Push(Vector4, Vector4, Vector4)"/>.
    /// Must only be called when <see cref="Push(ButtonStyle)"/> or <see cref="Push(Vector4, Vector4, Vector4)"/> returned <see langword="true"/>.
    /// </summary>
    internal static void Pop() => ImGui.PopStyleColor(3);
}
