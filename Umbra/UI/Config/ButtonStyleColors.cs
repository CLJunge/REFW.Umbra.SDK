using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Maps <see cref="ButtonStyle"/> values to the ImGui color triples used when rendering styled buttons.
/// </summary>
/// <remarks>
/// This helper also abstracts the low-level color push and pop operations through <see cref="IButtonStyleColorSink"/> so tests can validate style selection without invoking native ImGui entry points.
/// </remarks>
internal static class ButtonStyleColors
{
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
    private static IButtonStyleColorSink? _colorSink;

    /// <summary>
    /// Replaces the low-level sink used for button style-color push/pop operations.
    /// </summary>
    /// <remarks>
    /// This exists primarily for unit tests that need to validate button-style selection logic
    /// without touching native ImGui entry points.
    /// </remarks>
    /// <param name="colorSink">The sink that should receive future push/pop operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="colorSink"/> is <see langword="null"/>.</exception>
    internal static void SetColorSink(IButtonStyleColorSink colorSink)
    {
        ArgumentNullException.ThrowIfNull(colorSink);
        Interlocked.Exchange(ref _colorSink, colorSink);
    }

    /// <summary>
    /// Restores the default ImGui-backed color sink.
    /// </summary>
    internal static void ResetColorSink() => Interlocked.Exchange(ref _colorSink, null);

    /// <summary>
    /// Returns the currently active color sink, creating the shared ImGui-backed sink on first use.
    /// </summary>
    /// <returns>The sink used for subsequent push/pop operations.</returns>
    internal static IButtonStyleColorSink GetColorSink()
    {
        var colorSink = Volatile.Read(ref _colorSink);
        if (colorSink != null)
            return colorSink;

        colorSink = Rendering.ImGuiConfigRenderContext.Instance;
        var existing = Interlocked.CompareExchange(ref _colorSink, colorSink, null);
        return existing ?? colorSink;
    }

    /// <summary>
    /// Pushes the ImGui button colors for <paramref name="style"/> when the style defines an explicit color table.
    /// </summary>
    /// <param name="style">The button style whose colors should be applied.</param>
    /// <returns><see langword="true"/> if colors were pushed and <see cref="Pop"/> must be called; otherwise, <see langword="false"/>.</returns>
    internal static bool Push(ButtonStyle style)
    {
        if (!_colors.TryGetValue(style, out var c)) return false;
        var colorSink = GetColorSink();
        colorSink.PushStyleColor(ImGuiCol.Button, c.Normal);
        colorSink.PushStyleColor(ImGuiCol.ButtonHovered, c.Hovered);
        colorSink.PushStyleColor(ImGuiCol.ButtonActive, c.Active);
        return true;
    }

    /// <summary>
    /// Pushes an explicit normal, hovered, and active color triple for the next button draw.
    /// </summary>
    /// <param name="normal">The color applied to <see cref="ImGuiCol.Button"/>.</param>
    /// <param name="hovered">The color applied to <see cref="ImGuiCol.ButtonHovered"/>.</param>
    /// <param name="active">The color applied to <see cref="ImGuiCol.ButtonActive"/>.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    internal static bool Push(Vector4 normal, Vector4 hovered, Vector4 active)
    {
        var colorSink = GetColorSink();
        colorSink.PushStyleColor(ImGuiCol.Button, normal);
        colorSink.PushStyleColor(ImGuiCol.ButtonHovered, hovered);
        colorSink.PushStyleColor(ImGuiCol.ButtonActive, active);
        return true;
    }

    /// <summary>
    /// Pops the three button color entries previously pushed by <see cref="Push(ButtonStyle)"/> or <see cref="Push(Vector4, Vector4, Vector4)"/>.
    /// </summary>
    internal static void Pop() => GetColorSink().PopStyleColor(3);
}
