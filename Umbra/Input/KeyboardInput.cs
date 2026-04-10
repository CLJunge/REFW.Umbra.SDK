namespace Umbra.Input;

/// <summary>
/// Provides hardware-backed helpers for capturing and querying keyboard input in Umbra plugin flows.
/// </summary>
/// <remarks>
/// <para>
/// This type reads physical key state via Win32 <c>GetAsyncKeyState</c> through a <see cref="KeyStateTracker"/>,
/// making it reliable regardless of ImGui frame rate, widget focus, or thread timing. Call <see cref="Update"/>
/// once per tick before reading any key state.
/// </para>
/// <para>
/// Key values exposed through the public API use <see cref="UmbraKey"/> for type-safe key identification.
/// The internal translation between <see cref="UmbraKey"/> and Windows virtual-key codes is handled by
/// <see cref="VirtualKeyMap"/>.
/// </para>
/// </remarks>
public static class KeyboardInput
{
    #region VK constants for modifier queries

#pragma warning disable IDE1006 // Naming Styles
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_LSHIFT = 0xA0;
    private const int VK_RSHIFT = 0xA1;
    private const int VK_LMENU = 0xA4;
    private const int VK_RMENU = 0xA5;
#pragma warning restore IDE1006 // Naming Styles

    #endregion

    private static readonly HashSet<int> _modifierKeyValues = BuildModifierKeyValueSet();
    private static readonly IReadOnlyList<UmbraKey> _keyboardKeys = BuildKeyboardKeyList();
    private static readonly HashSet<int> _keyboardKeyValues = BuildKeyboardKeyValueSet();
    private static KeyStateTracker _tracker = CreateDefaultTracker();

    /// <summary>
    /// Tracks the last <see cref="Environment.TickCount64"/> value at which <see cref="Update"/> ran,
    /// used to deduplicate calls within the same millisecond (same frame from multiple plugin hosts).
    /// </summary>
    private static long _lastUpdateTick;

    /// <summary>
    /// Replaces the key state provider used by the shared tracker.
    /// </summary>
    /// <param name="provider">The replacement provider.</param>
    /// <remarks>
    /// This is an internal test seam. Production code should never call this method.
    /// </remarks>
    internal static void SetKeyStateProvider(INativeKeyStateProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _tracker = new KeyStateTracker(provider, VirtualKeyMap.GetTrackedVirtualKeys());
        _lastUpdateTick = 0;
    }

    /// <summary>
    /// Restores the default <see cref="NativeKeyStateProvider"/>-backed tracker.
    /// </summary>
    internal static void ResetKeyStateProvider()
    {
        _tracker = CreateDefaultTracker();
        _lastUpdateTick = 0;
    }

    /// <summary>
    /// Updates the internal key state tracker by reading current hardware key state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Must be called once per frame tick before any key-state queries. <c>PluginHost&lt;TPlugin&gt;</c>
    /// calls this automatically at the start of <c>OnPreUpdateBehavior</c>.
    /// </para>
    /// <para>
    /// Repeated calls within the same millisecond are deduplicated so that multiple plugin hosts
    /// sharing the same process do not double-advance the edge tracker.
    /// </para>
    /// </remarks>
    public static void Update()
    {
        var now = Environment.TickCount64;
        if (now == _lastUpdateTick)
            return;

        _lastUpdateTick = now;
        _tracker.Update();
    }

    /// <summary>
    /// Attempts to capture the first supported keyboard key that transitioned to pressed this tick.
    /// </summary>
    /// <param name="capturedKey">When this method returns <see langword="true"/>, contains the captured <see cref="UmbraKey"/> value cast to <see cref="int"/>; otherwise, contains <c>-1</c>.</param>
    /// <returns><see langword="true"/> if a supported keyboard key was detected as just pressed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method queries the edge tracker for the first key-down transition this tick and maps
    /// the Windows virtual-key code back to an <see cref="UmbraKey"/> value.
    /// </remarks>
    public static bool TryCaptureKeyboardKey(out int capturedKey)
    {
        if (_tracker.TryGetFirstPressed(out var vk))
        {
            var umbraKey = VirtualKeyMap.VkToUmbraKey(vk);
            if (umbraKey != UmbraKey.None && IsValidKey((int)umbraKey))
            {
                capturedKey = (int)umbraKey;
                return true;
            }
        }

        capturedKey = -1;
        return false;
    }

    /// <summary>
    /// Returns a human-readable name for a stored hotkey value.
    /// </summary>
    /// <param name="key">An <see cref="UmbraKey"/> value cast to <see cref="int"/>.</param>
    /// <returns><c>None</c> when <paramref name="key"/> equals <see cref="UmbraKey.None"/>; otherwise, the enum member name when defined, or <c>Key(n)</c> for unknown raw values.</returns>
    public static string GetKeyName(int key) => key == (int)UmbraKey.None ? nameof(UmbraKey.None) : Enum.GetName((UmbraKey)key) ?? $"Key({key})";

    /// <summary>
    /// Determines whether <paramref name="key"/> is one of the supported keyboard keys recognized by Umbra hotkey capture.
    /// </summary>
    /// <param name="key">An <see cref="UmbraKey"/> value cast to <see cref="int"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is present in the filtered keyboard-key set; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidKey(int key) => _keyboardKeyValues.Contains(key);

    /// <summary>
    /// Determines whether <paramref name="key"/> is a modifier key (Ctrl, Shift, Alt, or Super).
    /// </summary>
    /// <param name="key">An <see cref="UmbraKey"/> value cast to <see cref="int"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is a modifier key; otherwise, <see langword="false"/>.</returns>
    public static bool IsModifierKey(int key) => _modifierKeyValues.Contains(key);

    /// <summary>
    /// Attempts to capture a full hotkey binding (key + modifiers) from the current tick.
    /// </summary>
    /// <remarks>
    /// Modifier keys (Ctrl, Shift, Alt, Super) are skipped so that pressing a modifier alone does not
    /// complete the capture. The first non-modifier key that transitioned to pressed this tick is used
    /// as the primary key, combined with the current modifier state.
    /// </remarks>
    /// <param name="binding">When this method returns <see langword="true"/>, contains the captured binding; otherwise, <see cref="HotkeyBinding.None"/>.</param>
    /// <returns><see langword="true"/> if a non-modifier primary key was detected as pressed; otherwise, <see langword="false"/>.</returns>
    public static bool TryCaptureHotkeyBinding(out HotkeyBinding binding)
    {
        var count = _tracker.JustPressedCount;
        for (var i = 0; i < count; i++)
        {
            var vk = _tracker.GetJustPressedAt(i);
            var umbraKey = VirtualKeyMap.VkToUmbraKey(vk);
            if (umbraKey == UmbraKey.None || !IsValidKey((int)umbraKey))
                continue;

            if (IsModifierKey((int)umbraKey))
                continue;

            binding = new HotkeyBinding((int)umbraKey, IsCtrlHeld, IsShiftHeld, IsAltHeld);
            return true;
        }

        binding = HotkeyBinding.None;
        return false;
    }

    /// <summary>
    /// Determines whether the specified hotkey binding is currently pressed (primary key just pressed + all required modifiers held).
    /// </summary>
    /// <param name="binding">The hotkey binding to test.</param>
    /// <returns><see langword="true"/> if the binding's key just transitioned to pressed and all required modifiers are held; otherwise, <see langword="false"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public static bool IsHotkeyPressed(HotkeyBinding binding)
    {
        if (binding.IsEmpty) return false;

        var vk = VirtualKeyMap.UmbraKeyToVk((UmbraKey)binding.Key);
        if (vk < 0 || !_tracker.JustPressed(vk)) return false;
        if (binding.Ctrl && !IsCtrlHeld) return false;
        if (binding.Shift && !IsShiftHeld) return false;
        if (binding.Alt && !IsAltHeld) return false;
        return true;
    }

    /// <summary>
    /// Determines whether the specified <see cref="UmbraKey"/> transitioned to pressed this tick.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><see langword="true"/> if the key was just pressed; otherwise, <see langword="false"/>.</returns>
    public static bool IsKeyJustPressed(UmbraKey key)
    {
        var vk = VirtualKeyMap.UmbraKeyToVk(key);
        return vk >= 0 && _tracker.JustPressed(vk);
    }

    /// <summary>
    /// Gets a value indicating whether either Ctrl key is currently held down.
    /// </summary>
    /// <value><see langword="true"/> if the left or right Ctrl key is physically pressed; otherwise, <see langword="false"/>.</value>
    public static bool IsCtrlHeld => _tracker.IsDown(VK_LCONTROL) || _tracker.IsDown(VK_RCONTROL);

    /// <summary>
    /// Gets a value indicating whether either Shift key is currently held down.
    /// </summary>
    /// <value><see langword="true"/> if the left or right Shift key is physically pressed; otherwise, <see langword="false"/>.</value>
    public static bool IsShiftHeld => _tracker.IsDown(VK_LSHIFT) || _tracker.IsDown(VK_RSHIFT);

    /// <summary>
    /// Gets a value indicating whether either Alt key is currently held down.
    /// </summary>
    /// <value><see langword="true"/> if the left or right Alt key is physically pressed; otherwise, <see langword="false"/>.</value>
    public static bool IsAltHeld => _tracker.IsDown(VK_LMENU) || _tracker.IsDown(VK_RMENU);

    /// <summary>
    /// Returns the modifier prefix for the currently held modifier keys.
    /// </summary>
    /// <returns>A string such as <c>Ctrl+Shift+</c> when those modifiers are held, or <see cref="string.Empty"/> when none are held.</returns>
    public static string GetHeldModifierPrefix()
    {
        var ctrl = IsCtrlHeld;
        var shift = IsShiftHeld;
        var alt = IsAltHeld;

        if (!ctrl && !shift && !alt)
            return string.Empty;

        var parts = new List<string>(3);
        if (ctrl) parts.Add("Ctrl");
        if (shift) parts.Add("Shift");
        if (alt) parts.Add("Alt");
        return string.Join('+', parts) + "+";
    }

    /// <summary>
    /// Builds the list of all <see cref="UmbraKey"/> values that represent physical keyboard keys.
    /// </summary>
    /// <returns>A list containing every defined <see cref="UmbraKey"/> member except <see cref="UmbraKey.None"/>.</returns>
    /// <remarks>
    /// Because <see cref="UmbraKey"/> only defines keyboard-only keys, no runtime filtering of mouse,
    /// gamepad, or modifier-alias entries is needed.
    /// </remarks>
    private static List<UmbraKey> BuildKeyboardKeyList()
    {
        var values = Enum.GetValues<UmbraKey>();
        var keys = new List<UmbraKey>(values.Length);
        foreach (var key in values)
        {
            if (key != UmbraKey.None)
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

    /// <summary>
    /// Builds the lookup set used by <see cref="IsModifierKey(int)"/>.
    /// </summary>
    /// <returns>A set containing the integer values of every modifier key in <see cref="UmbraKey"/>.</returns>
    private static HashSet<int> BuildModifierKeyValueSet() =>
    [
        (int)UmbraKey.LeftCtrl,
        (int)UmbraKey.RightCtrl,
        (int)UmbraKey.LeftShift,
        (int)UmbraKey.RightShift,
        (int)UmbraKey.LeftAlt,
        (int)UmbraKey.RightAlt,
        (int)UmbraKey.LeftSuper,
        (int)UmbraKey.RightSuper,
    ];

    private static KeyStateTracker CreateDefaultTracker()
        => new(new NativeKeyStateProvider(), VirtualKeyMap.GetTrackedVirtualKeys());
}
