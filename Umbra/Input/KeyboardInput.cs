using Hexa.NET.ImGui;

namespace Umbra.Input;

/// <summary>
/// Provides utilities for capturing and querying keyboard input through ImGui.
/// </summary>
public static class KeyboardInput
{
    private static readonly IReadOnlyList<ImGuiKey> _keyboardKeys = BuildKeyboardKeyList();
    private static readonly HashSet<int> _keyboardKeyValues = BuildKeyboardKeyValueSet();

    /// <summary>
    /// Attempts to capture a keyboard key that is currently pressed.
    /// Mouse, gamepad, and other non-keyboard keys are excluded.
    /// </summary>
    /// <param name="capturedKey">
    /// When this method returns <see langword="true"/>, contains the <see cref="ImGuiKey"/> value cast to
    /// <see cref="int"/> of the pressed key; otherwise <c>-1</c>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a keyboard key was detected as pressed this frame; otherwise <see langword="false"/>.
    /// </returns>
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
    /// Returns a human-readable name for the given key value.
    /// </summary>
    /// <param name="key">An <see cref="ImGuiKey"/> value cast to <see cref="int"/>.</param>
    /// <returns>
    /// <c>None</c> when <paramref name="key"/> is zero; otherwise the enum member name if defined;
    /// otherwise <c>Key(n)</c> where <c>n</c> is the raw integer value.
    /// </returns>
    public static string GetKeyName(int key)
    {
        if (key == (int)ImGuiKey.None)
            return nameof(ImGuiKey.None);

        return Enum.GetName((ImGuiKey)key) ?? $"Key({key})";
    }

    /// <summary>
    /// Determines whether the given key value represents a supported keyboard key.
    /// </summary>
    /// <param name="key">An <see cref="ImGuiKey"/> value cast to <see cref="int"/>.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="key"/> is one of the keyboard-only
    /// <see cref="ImGuiKey"/> values that <see cref="TryCaptureKeyboardKey"/> may return;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This excludes non-keyboard named keys such as mouse buttons, gamepad inputs, wheel events,
    /// modifier aliases, and arbitrary positive integers that do not map to a supported keyboard key.
    /// </remarks>
    public static bool IsValidKey(int key) => _keyboardKeyValues.Contains(key);

    /// <summary>
    /// Gets a value indicating whether the left or right Ctrl key is currently held down.
    /// </summary>
    public static bool IsCtrlHeld => ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);

    /// <summary>
    /// Gets a value indicating whether the left or right Shift key is currently held down.
    /// </summary>
    public static bool IsShiftHeld => ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

    /// <summary>
    /// Gets a value indicating whether the left or right Alt key is currently held down.
    /// </summary>
    public static bool IsAltHeld => ImGui.IsKeyDown(ImGuiKey.LeftAlt) || ImGui.IsKeyDown(ImGuiKey.RightAlt);

    /// <summary>
    /// Builds the filtered list of keyboard-only <see cref="ImGuiKey"/> values from the named key range,
    /// excluding mouse buttons, gamepad inputs, joystick axes, scroll wheel events, reserved entries, and modifier aliases.
    /// </summary>
    /// <returns>A list of <see cref="ImGuiKey"/> values that correspond to physical keyboard keys.</returns>
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
    /// Builds a lookup set of supported keyboard key values for fast validity checks.
    /// </summary>
    /// <returns>
    /// A set containing the integer values of every keyboard-only <see cref="ImGuiKey"/> returned
    /// by <see cref="BuildKeyboardKeyList"/>.
    /// </returns>
    private static HashSet<int> BuildKeyboardKeyValueSet()
    {
        var keys = new HashSet<int>();
        foreach (var key in _keyboardKeys)
            keys.Add((int)key);

        return keys;
    }
}
