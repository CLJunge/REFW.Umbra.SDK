using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Logging;

namespace Umbra.UI.LiveState.LogConsole;

/// <summary>
/// Renders <see cref="LogConsoleState"/> as an in-game scrollable log console using ImGui.
/// </summary>
/// <remarks>
/// <para>
/// The drawer reads entries from the state's <see cref="LogBuffer"/> each frame, applies the
/// <see cref="LogConsoleState.MinDisplayLevel"/> filter, and renders them as color-coded text
/// inside a scrolling child region. A toolbar at the top provides level filtering, a clear
/// button, and an auto-scroll toggle.
/// </para>
/// <para>
/// Color mapping: <see cref="LogLevel.Debug"/> = gray, <see cref="LogLevel.Info"/> = white,
/// <see cref="LogLevel.Warning"/> = yellow, <see cref="LogLevel.Error"/> = red.
/// </para>
/// </remarks>
public sealed class LogConsoleDrawer : ILiveStateSectionDrawer<LogConsoleState>
{
    private static readonly Vector4 _colorDebug = new(0.6f, 0.6f, 0.6f, 1.0f);
    private static readonly Vector4 _colorInfo = new(1.0f, 1.0f, 1.0f, 1.0f);
    private static readonly Vector4 _colorWarning = new(1.0f, 0.9f, 0.3f, 1.0f);
    private static readonly Vector4 _colorError = new(1.0f, 0.3f, 0.3f, 1.0f);

    private static readonly string[] _levelLabels = ["All", "Info+", "Warn+", "Error"];
    private static readonly LogLevel[] _levelValues = [LogLevel.Debug, LogLevel.Info, LogLevel.Warning, LogLevel.Error];

    private readonly List<LogEntry> _frameEntries = new(256);

    /// <inheritdoc/>
    public void Draw(LogConsoleState state)
    {
        DrawToolbar(state);
        ImGui.Separator();
        DrawLogRegion(state);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);

    private static void DrawToolbar(LogConsoleState state)
    {
        DrawLevelFilter(state);

        ImGui.SameLine();
        if (ImGui.Button("Clear"))
            state.Buffer.Clear();

        ImGui.SameLine();
        var autoScroll = state.AutoScroll;
        if (ImGui.Checkbox("Auto-scroll", ref autoScroll))
            state.AutoScroll = autoScroll;

        ImGui.SameLine();
        ImGui.TextDisabled($"({state.Buffer.Count})");
    }

    private static void DrawLevelFilter(LogConsoleState state)
    {
        var current = 0;
        for (var i = 0; i < _levelValues.Length; i++)
        {
            if (_levelValues[i] == state.MinDisplayLevel)
            {
                current = i;
                break;
            }
        }

        ImGui.SetNextItemWidth(80);
        if (ImGui.Combo("##LogLevel", ref current, _levelLabels, _levelLabels.Length))
            state.MinDisplayLevel = _levelValues[current];
    }

    private void DrawLogRegion(LogConsoleState state)
    {
        _frameEntries.Clear();
        state.Buffer.GetEntries(_frameEntries);

        if (!ImGui.BeginChild("##LogConsoleScroll", new Vector2(0, 200), ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        var minLevel = state.MinDisplayLevel;
        for (var i = 0; i < _frameEntries.Count; i++)
        {
            var entry = _frameEntries[i];
            if (entry.Level < minLevel)
                continue;

            var color = GetLevelColor(entry.Level);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted(FormatEntry(entry));
            ImGui.PopStyleColor();
        }

        if (state.AutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 20f)
            ImGui.SetScrollHereY(1.0f);

        ImGui.EndChild();
    }

    private static Vector4 GetLevelColor(LogLevel level) => level switch
    {
        LogLevel.Debug => _colorDebug,
        LogLevel.Info => _colorInfo,
        LogLevel.Warning => _colorWarning,
        LogLevel.Error => _colorError,
        _ => _colorInfo,
    };

    private static string FormatEntry(LogEntry entry)
    {
        var tag = entry.Level switch
        {
            LogLevel.Debug => "DBG",
            LogLevel.Info => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            _ => "???",
        };
        return $"[{entry.Timestamp:HH:mm:ss.fff}] [{tag}] {entry.Message}";
    }
}
