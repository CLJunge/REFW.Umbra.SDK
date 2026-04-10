using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Toast;

/// <summary>
/// Renders each active toast entry in its own ImGui overlay window, anchored to the
/// top-right corner of the game viewport. Each window stacks vertically and fades
/// independently, providing per-toast background alpha for smooth fade-in and fade-out
/// effects. A small level-colored circle icon is drawn to the left of each message.
/// </summary>
internal sealed class ImGuiToastRenderer : IToastRenderer
{
    private const float _windowWidth = 320f;
    private const float _backgroundAlpha = 0.85f;
    private const float _fadeInEndProgress = 0.08f;
    private const float _fadeOutStartProgress = 0.75f;
    private const float _iconGap = 6f;
    private const float _windowPadding = 12f;
    private const float _itemSpacing = 4f;

    private static readonly ImGuiWindowFlags _windowFlags =
        ImGuiWindowFlags.NoDecoration
      | ImGuiWindowFlags.NoInputs
      | ImGuiWindowFlags.NoNav
      | ImGuiWindowFlags.NoMove
      | ImGuiWindowFlags.NoSavedSettings
      | ImGuiWindowFlags.AlwaysAutoResize
      | ImGuiWindowFlags.NoFocusOnAppearing
      | ImGuiWindowFlags.NoBringToFrontOnFocus;

    /// <inheritdoc />
    public void Draw(List<ToastEntry> entries)
    {
        if (entries.Count == 0) return;

        var displaySize = ImGui.GetIO().DisplaySize;
        var xPos = displaySize.X - _windowWidth - _windowPadding;
        var yOffset = _windowPadding;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var alpha = CalculateAlpha(entry.GetProgress());

            ImGui.SetNextWindowPos(new Vector2(xPos, yOffset));
            ImGui.SetNextWindowSize(new Vector2(_windowWidth, 0));
            ImGui.SetNextWindowBgAlpha(_backgroundAlpha * alpha);

            if (!ImGui.Begin($"##UmbraToast_{i}", _windowFlags))
            {
                ImGui.End();
                continue;
            }

            DrawEntry(entry, alpha);

            yOffset += ImGui.GetWindowSize().Y + _itemSpacing;
            ImGui.End();
        }
    }

    private static void DrawEntry(ToastEntry entry, float alpha)
    {
        DrawLevelIcon(entry.Level, alpha);

        var color = GetColor(entry.Level, alpha);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextWrapped(entry.Message);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Draws a filled circle indicating the toast severity level.
    /// </summary>
    /// <remarks>
    /// Captures the cursor position before emitting an <see cref="ImGui.Dummy"/> spacer, which reserves
    /// horizontal room for the icon so that subsequent wrapped text starts to the right of it.
    /// The circle is rendered directly on the window draw list to remain font-independent.
    /// </remarks>
    private static void DrawLevelIcon(ToastLevel level, float alpha)
    {
        var fontHeight = ImGui.GetFontSize();
        var radius = fontHeight * 0.35f;
        var diameter = radius * 2f;

        var iconScreenPos = ImGui.GetCursorScreenPos();

        ImGui.Dummy(new Vector2(diameter + _iconGap, fontHeight));
        ImGui.SameLine(0, 0);

        var center = new Vector2(
            iconScreenPos.X + radius,
            iconScreenPos.Y + fontHeight / 2f);

        var color = ImGui.ColorConvertFloat4ToU32(GetColor(level, alpha));
        ImGui.GetWindowDrawList().AddCircleFilled(center, radius, color);
    }

    private static float CalculateAlpha(float progress)
    {
        return progress switch
        {
            < _fadeInEndProgress => progress / _fadeInEndProgress,
            >= _fadeOutStartProgress => 1f - (progress - _fadeOutStartProgress) / (1f - _fadeOutStartProgress),
            _ => 1f
        };
    }

    private static Vector4 GetColor(ToastLevel level, float alpha)
    {
        return level switch
        {
            ToastLevel.Success => new Vector4(0.3f, 1.0f, 0.3f, alpha),
            ToastLevel.Warning => new Vector4(1.0f, 0.85f, 0.2f, alpha),
            ToastLevel.Error => new Vector4(1.0f, 0.3f, 0.3f, alpha),
            _ => new Vector4(0.5f, 0.8f, 1.0f, alpha),
        };
    }
}
