namespace Umbra.Input;

/// <summary>
/// Provides a bidirectional mapping between <see cref="UmbraKey"/> values and Windows virtual-key codes.
/// </summary>
/// <remarks>
/// The mapping covers all keyboard-only keys tracked by Umbra. Mouse, gamepad, wheel, reserved,
/// and modifier-alias keys are intentionally excluded because <see cref="UmbraKey"/> only defines
/// the keyboard subset.
/// </remarks>
internal static class VirtualKeyMap
{
    // --- Windows virtual-key constants ---
    private const int VK_BACK = 0x08;
    private const int VK_TAB = 0x09;
    private const int VK_RETURN = 0x0D;
    private const int VK_PAUSE = 0x13;
    private const int VK_CAPITAL = 0x14;
    private const int VK_ESCAPE = 0x1B;
    private const int VK_SPACE = 0x20;
    private const int VK_PRIOR = 0x21;
    private const int VK_NEXT = 0x22;
    private const int VK_END = 0x23;
    private const int VK_HOME = 0x24;
    private const int VK_LEFT = 0x25;
    private const int VK_UP = 0x26;
    private const int VK_RIGHT = 0x27;
    private const int VK_DOWN = 0x28;
    private const int VK_SNAPSHOT = 0x2C;
    private const int VK_INSERT = 0x2D;
    private const int VK_DELETE = 0x2E;
    private const int VK_0 = 0x30;
    private const int VK_A = 0x41;
    private const int VK_NUMPAD0 = 0x60;
    private const int VK_MULTIPLY = 0x6A;
    private const int VK_ADD = 0x6B;
    private const int VK_SUBTRACT = 0x6D;
    private const int VK_DECIMAL = 0x6E;
    private const int VK_DIVIDE = 0x6F;
    private const int VK_F1 = 0x70;
    private const int VK_NUMLOCK = 0x90;
    private const int VK_SCROLL = 0x91;
    private const int VK_LSHIFT = 0xA0;
    private const int VK_RSHIFT = 0xA1;
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_LMENU = 0xA4;
    private const int VK_RMENU = 0xA5;
    private const int VK_OEM_1 = 0xBA;
    private const int VK_OEM_PLUS = 0xBB;
    private const int VK_OEM_COMMA = 0xBC;
    private const int VK_OEM_MINUS = 0xBD;
    private const int VK_OEM_PERIOD = 0xBE;
    private const int VK_OEM_2 = 0xBF;
    private const int VK_OEM_3 = 0xC0;
    private const int VK_OEM_4 = 0xDB;
    private const int VK_OEM_5 = 0xDC;
    private const int VK_OEM_6 = 0xDD;
    private const int VK_OEM_7 = 0xDE;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;
    private const int VK_APPS = 0x5D;

    /// <summary>
    /// Lookup from <see cref="UmbraKey"/> int value to Windows VK code.
    /// Built once at class load.
    /// </summary>
    private static readonly Dictionary<int, int> _umbraToVk = BuildUmbraToVkMap();

    /// <summary>
    /// Reverse lookup from Windows VK code to <see cref="UmbraKey"/> int value.
    /// Built once at class load.
    /// </summary>
    private static readonly Dictionary<int, int> _vkToUmbra = BuildVkToUmbraMap();

    /// <summary>
    /// Cached array of all tracked VK codes. Built once at class load.
    /// </summary>
    private static readonly int[] _trackedVirtualKeys = BuildTrackedVirtualKeys();

    /// <summary>
    /// Converts an <see cref="UmbraKey"/> to the corresponding Windows virtual-key code.
    /// </summary>
    /// <param name="key">The Umbra key to map.</param>
    /// <returns>The Windows VK code, or <c>-1</c> if the key has no mapping.</returns>
    public static int UmbraKeyToVk(UmbraKey key)
    {
        if (_umbraToVk.TryGetValue((int)key, out var vk))
            return vk;
        return -1;
    }

    /// <summary>
    /// Converts a Windows virtual-key code to the corresponding <see cref="UmbraKey"/>.
    /// </summary>
    /// <param name="vk">The Windows virtual-key code to map.</param>
    /// <returns>The corresponding <see cref="UmbraKey"/>, or <see cref="UmbraKey.None"/> if unmapped.</returns>
    public static UmbraKey VkToUmbraKey(int vk)
    {
        if (_vkToUmbra.TryGetValue(vk, out var umbraKey))
            return (UmbraKey)umbraKey;
        return UmbraKey.None;
    }

    /// <summary>
    /// Returns the array of all Windows virtual-key codes that correspond to tracked keyboard-only Umbra keys.
    /// </summary>
    /// <returns>An array of VK codes. The caller must not modify the returned array.</returns>
    public static int[] GetTrackedVirtualKeys() => _trackedVirtualKeys;

    private static Dictionary<int, int> BuildUmbraToVkMap()
    {
        var map = new Dictionary<int, int>(128);

        // Navigation
        map[(int)UmbraKey.Tab] = VK_TAB;
        map[(int)UmbraKey.LeftArrow] = VK_LEFT;
        map[(int)UmbraKey.RightArrow] = VK_RIGHT;
        map[(int)UmbraKey.UpArrow] = VK_UP;
        map[(int)UmbraKey.DownArrow] = VK_DOWN;
        map[(int)UmbraKey.PageUp] = VK_PRIOR;
        map[(int)UmbraKey.PageDown] = VK_NEXT;
        map[(int)UmbraKey.Home] = VK_HOME;
        map[(int)UmbraKey.End] = VK_END;
        map[(int)UmbraKey.Insert] = VK_INSERT;
        map[(int)UmbraKey.Delete] = VK_DELETE;
        map[(int)UmbraKey.Backspace] = VK_BACK;
        map[(int)UmbraKey.Space] = VK_SPACE;
        map[(int)UmbraKey.Enter] = VK_RETURN;
        map[(int)UmbraKey.Escape] = VK_ESCAPE;

        // Punctuation / symbols
        map[(int)UmbraKey.Apostrophe] = VK_OEM_7;
        map[(int)UmbraKey.Comma] = VK_OEM_COMMA;
        map[(int)UmbraKey.Minus] = VK_OEM_MINUS;
        map[(int)UmbraKey.Period] = VK_OEM_PERIOD;
        map[(int)UmbraKey.Slash] = VK_OEM_2;
        map[(int)UmbraKey.Semicolon] = VK_OEM_1;
        map[(int)UmbraKey.Equal] = VK_OEM_PLUS;
        map[(int)UmbraKey.LeftBracket] = VK_OEM_4;
        map[(int)UmbraKey.Backslash] = VK_OEM_5;
        map[(int)UmbraKey.RightBracket] = VK_OEM_6;
        map[(int)UmbraKey.GraveAccent] = VK_OEM_3;

        // Lock keys
        map[(int)UmbraKey.CapsLock] = VK_CAPITAL;
        map[(int)UmbraKey.ScrollLock] = VK_SCROLL;
        map[(int)UmbraKey.NumLock] = VK_NUMLOCK;
        map[(int)UmbraKey.PrintScreen] = VK_SNAPSHOT;
        map[(int)UmbraKey.Pause] = VK_PAUSE;

        // Numpad
        for (var i = 0; i < 10; i++)
            map[(int)UmbraKey.Keypad0 + i] = VK_NUMPAD0 + i;

        map[(int)UmbraKey.KeypadDecimal] = VK_DECIMAL;
        map[(int)UmbraKey.KeypadDivide] = VK_DIVIDE;
        map[(int)UmbraKey.KeypadMultiply] = VK_MULTIPLY;
        map[(int)UmbraKey.KeypadSubtract] = VK_SUBTRACT;
        map[(int)UmbraKey.KeypadAdd] = VK_ADD;
        map[(int)UmbraKey.KeypadEnter] = VK_RETURN; // Same VK as Enter — known limitation

        // 0-9
        for (var i = 0; i < 10; i++)
            map[(int)UmbraKey.Key0 + i] = VK_0 + i;

        // A-Z
        for (var i = 0; i < 26; i++)
            map[(int)UmbraKey.A + i] = VK_A + i;

        // F1-F24
        for (var i = 0; i < 24; i++)
            map[(int)UmbraKey.F1 + i] = VK_F1 + i;

        // Modifiers (individual keys, not aliases)
        map[(int)UmbraKey.LeftCtrl] = VK_LCONTROL;
        map[(int)UmbraKey.LeftShift] = VK_LSHIFT;
        map[(int)UmbraKey.LeftAlt] = VK_LMENU;
        map[(int)UmbraKey.LeftSuper] = VK_LWIN;
        map[(int)UmbraKey.RightCtrl] = VK_RCONTROL;
        map[(int)UmbraKey.RightShift] = VK_RSHIFT;
        map[(int)UmbraKey.RightAlt] = VK_RMENU;
        map[(int)UmbraKey.RightSuper] = VK_RWIN;
        map[(int)UmbraKey.Menu] = VK_APPS;

        return map;
    }

    private static Dictionary<int, int> BuildVkToUmbraMap()
    {
        var reverse = new Dictionary<int, int>(_umbraToVk.Count);
        foreach (var pair in _umbraToVk)
        {
            // For duplicate VK mappings (Enter/KeypadEnter → VK_RETURN),
            // the first one wins. Enter is added before KeypadEnter, so
            // VK_RETURN maps back to UmbraKey.Enter.
            reverse.TryAdd(pair.Value, pair.Key);
        }
        return reverse;
    }

    private static int[] BuildTrackedVirtualKeys()
    {
        // Deduplicate VK codes (Enter and KeypadEnter share VK_RETURN).
        var unique = new HashSet<int>();
        foreach (var pair in _umbraToVk)
            unique.Add(pair.Value);

        var result = new int[unique.Count];
        var index = 0;
        foreach (var vk in unique)
            result[index++] = vk;

        return result;
    }
}
