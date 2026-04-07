using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Toast;

/// <summary>
/// Renders active toast entries as a stack of color-coded text items anchored to the top-right
/// corner of the game viewport via an ImGui overlay window.
/// </summary>
internal sealed class ImGuiToastRenderer : IToastRenderer
{
    private const string _windowId = "##UmbraToasts";
    private const float _padding = 12f;
    private const float _itemSpacing = 4f;
    private const float _windowWidth = 320f;
    private const float _fadeStartProgress = 0.75f;

    /// <inheritdoc />
    public void Draw(List<ToastEntry> entries)
    {
        if (entries.Count == 0) return;

        var io = ImGui.GetIO();
        var displaySize = io.DisplaySize;

        ImGui.SetNextWindowPos(new Vector2(displaySize.X - _windowWidth - _padding, _padding));
        ImGui.SetNextWindowSize(new Vector2(_windowWidth, 0));

        var flags = ImGuiWindowFlags.NoDecoration
                  | ImGuiWindowFlags.NoInputs
                  | ImGuiWindowFlags.NoNav
                  | ImGuiWindowFlags.NoMove
                  | ImGuiWindowFlags.NoSavedSettings
                  | ImGuiWindowFlags.AlwaysAutoResize
                  | ImGuiWindowFlags.NoFocusOnAppearing
                  | ImGuiWindowFlags.NoBringToFrontOnFocus;

        ImGui.SetNextWindowBgAlpha(0.85f);

        if (!ImGui.Begin(_windowId, flags)) return;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var progress = entry.GetProgress();
            var alpha = progress >= _fadeStartProgress
                ? 1f - ((progress - _fadeStartProgress) / (1f - _fadeStartProgress))
                : 1f;

            var color = GetColor(entry.Level, alpha);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(entry.Message);
            ImGui.PopStyleColor();

            if (i < entries.Count - 1)
                ImGui.Spacing();
        }

        ImGui.End();
    }

    private static Vector4 GetColor(ToastLevel level, float alpha)
    {
        return level switch
        {
            ToastLevel.Success => new Vector4(0.3f, 1.0f, 0.3f, alpha),
            ToastLevel.Warning => new Vector4(1.0f, 0.85f, 0.2f, alpha),
            ToastLevel.Error   => new Vector4(1.0f, 0.3f, 0.3f, alpha),
            _                  => new Vector4(1.0f, 1.0f, 1.0f, alpha),
        };
    }
}
