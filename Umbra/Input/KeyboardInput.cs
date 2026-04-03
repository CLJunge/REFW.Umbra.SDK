using Hexa.NET.ImGui;

namespace Umbra.Input;

/// <summary>
/// Provides ImGui-backed helpers for capturing and querying keyboard input in Umbra UI flows.
/// </summary>
/// <remarks>
/// This type is used by the built-in hotkey drawers to translate current-frame ImGui key state into stored hotkey values and display names. It intentionally filters out non-keyboard <see cref="ImGuiKey"/> values such as mouse, gamepad, wheel, and modifier-alias entries.
/// </remarks>
public static class KeyboardInput
{
    private static readonly IReadOnlyList<ImGuiKey> _keyboardKeys = BuildKeyboardKeyList();
    private static readonly HashSet<int> _keyboardKeyValues = BuildKeyboardKeyValueSet();

    /// <summary>
    /// Attempts to capture the first supported keyboard key reported as pressed in the current ImGui frame.
    /// </summary>
    /// <param name="capturedKey">When this method returns <see langword="true"/>, contains the captured <see cref="ImGuiKey"/> value cast to <see cref="int"/>; otherwise, contains <c>-1</c>.</param>
    /// <returns><see langword="true"/> if a supported keyboard key was detected as pressed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method iterates over the filtered keyboard-only key list built from ImGui's named key range and returns the first key for which <see cref="ImGui.IsKeyPressed(Hexa.NET.ImGui.ImGuiKey,bool)"/> reports a press.
    /// </remarks>
    public static bool TryCaptureKeyboardKey(out int capturedKey)
    {
        foreach (var key in _keyboardKeys)
        {
            if (ImGui.IsKeyPressed(key))
            {
                capturedKey = (int)key;
                return true;
            }
        }

        capturedKey = -1;
        return false;
    }

    /// <summary>
    /// Returns a human-readable name for a stored hotkey value.
    /// </summary>
    /// <param name="key">An <see cref="ImGuiKey"/> value cast to <see cref="int"/>.</param>
    /// <returns><c>None</c> when <paramref name="key"/> equals <see cref="ImGuiKey.None"/>; otherwise, the enum member name when defined, or <c>Key(n)</c> for unknown raw values.</returns>
    public static string GetKeyName(int key)
    {
        if (key == (int)ImGuiKey.None)
            return nameof(ImGuiKey.None);

        return Enum.GetName((ImGuiKey)key) ?? $"Key({key})";
    }

    /// <summary>
    /// Determines whether <paramref name="key"/> is one of the supported keyboard keys recognized by Umbra hotkey capture.
    /// </summary>
    /// <param name="key">An <see cref="ImGuiKey"/> value cast to <see cref="int"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is present in the filtered keyboard-key set; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidKey(int key) => _keyboardKeyValues.Contains(key);

    /// <summary>
    /// Gets a value indicating whether either Ctrl key is currently held down.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="ImGuiKey.LeftCtrl"/> or <see cref="ImGuiKey.RightCtrl"/> is down; otherwise, <see langword="false"/>.</value>
    public static bool IsCtrlHeld => ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);

    /// <summary>
    /// Gets a value indicating whether either Shift key is currently held down.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="ImGuiKey.LeftShift"/> or <see cref="ImGuiKey.RightShift"/> is down; otherwise, <see langword="false"/>.</value>
    public static bool IsShiftHeld => ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

    /// <summary>
    /// Gets a value indicating whether either Alt key is currently held down.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="ImGuiKey.LeftAlt"/> or <see cref="ImGuiKey.RightAlt"/> is down; otherwise, <see langword="false"/>.</value>
    public static bool IsAltHeld => ImGui.IsKeyDown(ImGuiKey.LeftAlt) || ImGui.IsKeyDown(ImGuiKey.RightAlt);

    /// <summary>
    /// Builds the filtered list of keyboard-only <see cref="ImGuiKey"/> values from ImGui's named key range.
    /// </summary>
    /// <returns>A list containing only the named keys Umbra treats as physical keyboard keys.</returns>
    /// <remarks>
    /// Mouse buttons, gamepad inputs, joystick entries, wheel events, reserved entries, and modifier-alias names are excluded from the resulting list.
    /// </remarks>
    private static List<ImGuiKey> BuildKeyboardKeyList()
    {
        var keys = new List<ImGuiKey>();
        var start = (int)ImGuiKey.NamedKeyBegin;
        var end = (int)ImGuiKey.NamedKeyEnd;

        for (var i = start; i < end; i++)
        {
            var key = (ImGuiKey)i;
            var name = Enum.GetName(key);
            if (string.IsNullOrEmpty(name)) continue;

            if (name.StartsWith("mouse", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("pad", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("joy", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("button", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("wheel", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("reserved", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("mod", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            keys.Add(key);
        }

        return keys;
    }

    /// <summary>
    /// Builds the lookup set used by <see cref="IsValidKey(int)"/>.
    /// </summary>
    /// <returns>A set containing the integer values of every keyboard key returned by <see cref="BuildKeyboardKeyList()"/>.</returns>
    private static HashSet<int> BuildKeyboardKeyValueSet()
    {
        var keys = new HashSet<int>();
        foreach (var key in _keyboardKeys)
            keys.Add((int)key);

        return keys;
    }
}
