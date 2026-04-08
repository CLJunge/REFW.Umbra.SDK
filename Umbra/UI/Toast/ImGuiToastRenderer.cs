using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Toast;

/// <summary>
/// Renders each active toast entry in its own ImGui overlay window, anchored to the top-right
/// corner of the game viewport. Each window stacks vertically and fades independently,
/// providing per-toast background alpha for smooth fade-in and fade-out effects.
/// </summary>
internal sealed class ImGuiToastRenderer : IToastRenderer
{
    private const float _padding = 12f;
    private const float _itemSpacing = 4f;
    private const float _windowWidth = 320f;
    private const float _backgroundAlpha = 0.85f;
    private const float _fadeInEndProgress = 0.08f;
    private const float _fadeOutStartProgress = 0.75f;

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
        var xPos = displaySize.X - _windowWidth - _padding;
        var yOffset = _padding;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var alpha = CalculateAlpha(entry.GetProgress());

            ImGui.SetNextWindowPos(new Vector2(xPos, yOffset));
            ImGui.SetNextWindowSize(new Vector2(_windowWidth, 0));
            ImGui.SetNextWindowBgAlpha(_backgroundAlpha * alpha);

            if (!ImGui.Begin($"##UmbraToast_{i}", _windowFlags))
                continue;

            var color = GetColor(entry.Level, alpha);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(entry.Message);
            ImGui.PopStyleColor();

            yOffset += ImGui.GetWindowSize().Y + _itemSpacing;
            ImGui.End();
        }
    }

    private static float CalculateAlpha(float progress)
    {
        if (progress < _fadeInEndProgress)
            return progress / _fadeInEndProgress;

        if (progress >= _fadeOutStartProgress)
            return 1f - ((progress - _fadeOutStartProgress) / (1f - _fadeOutStartProgress));

        return 1f;
    }

    private static Vector4 GetColor(ToastLevel level, float alpha)
    {
        return level switch
        {
            ToastLevel.Success => new Vector4(0.3f, 1.0f, 0.3f, alpha),
            ToastLevel.Warning => new Vector4(1.0f, 0.85f, 0.2f, alpha),
            ToastLevel.Error => new Vector4(1.0f, 0.3f, 0.3f, alpha),
            _ => new Vector4(1.0f, 1.0f, 1.0f, alpha),
        };
    }
}
