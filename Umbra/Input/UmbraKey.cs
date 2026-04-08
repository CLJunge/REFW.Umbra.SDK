namespace Umbra.Input;

/// <summary>
/// Identifies a keyboard key in the Umbra input system.
/// </summary>
/// <remarks>
/// <para>
/// This enum covers the keyboard-only key set tracked by Umbra. Mouse, gamepad, joystick, wheel,
/// reserved, and modifier-alias entries found in third-party key enums are intentionally excluded.
/// </para>
/// <para>
/// Integer values are aligned with the <c>Hexa.NET.ImGui.ImGuiKey</c> enum at the time of creation
/// so that previously serialized hotkey bindings (stored as <see cref="int"/> in JSON config files)
/// remain valid without migration.
/// </para>
/// </remarks>
public enum UmbraKey
{
    /// <summary>No key.</summary>
    None = 0,

    // ── Navigation ──────────────────────────────────────────

    /// <summary>Tab key.</summary>
    Tab = 512,

    /// <summary>Left arrow key.</summary>
    LeftArrow = 513,

    /// <summary>Right arrow key.</summary>
    RightArrow = 514,

    /// <summary>Up arrow key.</summary>
    UpArrow = 515,

    /// <summary>Down arrow key.</summary>
    DownArrow = 516,

    /// <summary>Page Up key.</summary>
    PageUp = 517,

    /// <summary>Page Down key.</summary>
    PageDown = 518,

    /// <summary>Home key.</summary>
    Home = 519,

    /// <summary>End key.</summary>
    End = 520,

    /// <summary>Insert key.</summary>
    Insert = 521,

    /// <summary>Delete key.</summary>
    Delete = 522,

    /// <summary>Backspace key.</summary>
    Backspace = 523,

    /// <summary>Space bar.</summary>
    Space = 524,

    /// <summary>Enter / Return key.</summary>
    Enter = 525,

    /// <summary>Escape key.</summary>
    Escape = 526,

    // ── Modifiers (individual keys) ─────────────────────────

    /// <summary>Left Ctrl key.</summary>
    LeftCtrl = 527,

    /// <summary>Left Shift key.</summary>
    LeftShift = 528,

    /// <summary>Left Alt key.</summary>
    LeftAlt = 529,

    /// <summary>Left Super / Windows key.</summary>
    LeftSuper = 530,

    /// <summary>Right Ctrl key.</summary>
    RightCtrl = 531,

    /// <summary>Right Shift key.</summary>
    RightShift = 532,

    /// <summary>Right Alt key.</summary>
    RightAlt = 533,

    /// <summary>Right Super / Windows key.</summary>
    RightSuper = 534,

    /// <summary>Menu / Apps key.</summary>
    Menu = 535,

    // ── Digit keys ──────────────────────────────────────────

    /// <summary>Digit 0 key (top row).</summary>
    Key0 = 536,

    /// <summary>Digit 1 key (top row).</summary>
    Key1 = 537,

    /// <summary>Digit 2 key (top row).</summary>
    Key2 = 538,

    /// <summary>Digit 3 key (top row).</summary>
    Key3 = 539,

    /// <summary>Digit 4 key (top row).</summary>
    Key4 = 540,

    /// <summary>Digit 5 key (top row).</summary>
    Key5 = 541,

    /// <summary>Digit 6 key (top row).</summary>
    Key6 = 542,

    /// <summary>Digit 7 key (top row).</summary>
    Key7 = 543,

    /// <summary>Digit 8 key (top row).</summary>
    Key8 = 544,

    /// <summary>Digit 9 key (top row).</summary>
    Key9 = 545,

    // ── Letter keys ─────────────────────────────────────────

    /// <summary>A key.</summary>
    A = 546,

    /// <summary>B key.</summary>
    B = 547,

    /// <summary>C key.</summary>
    C = 548,

    /// <summary>D key.</summary>
    D = 549,

    /// <summary>E key.</summary>
    E = 550,

    /// <summary>F key.</summary>
    F = 551,

    /// <summary>G key.</summary>
    G = 552,

    /// <summary>H key.</summary>
    H = 553,

    /// <summary>I key.</summary>
    I = 554,

    /// <summary>J key.</summary>
    J = 555,

    /// <summary>K key.</summary>
    K = 556,

    /// <summary>L key.</summary>
    L = 557,

    /// <summary>M key.</summary>
    M = 558,

    /// <summary>N key.</summary>
    N = 559,

    /// <summary>O key.</summary>
    O = 560,

    /// <summary>P key.</summary>
    P = 561,

    /// <summary>Q key.</summary>
    Q = 562,

    /// <summary>R key.</summary>
    R = 563,

    /// <summary>S key.</summary>
    S = 564,

    /// <summary>T key.</summary>
    T = 565,

    /// <summary>U key.</summary>
    U = 566,

    /// <summary>V key.</summary>
    V = 567,

    /// <summary>W key.</summary>
    W = 568,

    /// <summary>X key.</summary>
    X = 569,

    /// <summary>Y key.</summary>
    Y = 570,

    /// <summary>Z key.</summary>
    Z = 571,

    // ── Function keys ───────────────────────────────────────

    /// <summary>F1 key.</summary>
    F1 = 572,

    /// <summary>F2 key.</summary>
    F2 = 573,

    /// <summary>F3 key.</summary>
    F3 = 574,

    /// <summary>F4 key.</summary>
    F4 = 575,

    /// <summary>F5 key.</summary>
    F5 = 576,

    /// <summary>F6 key.</summary>
    F6 = 577,

    /// <summary>F7 key.</summary>
    F7 = 578,

    /// <summary>F8 key.</summary>
    F8 = 579,

    /// <summary>F9 key.</summary>
    F9 = 580,

    /// <summary>F10 key.</summary>
    F10 = 581,

    /// <summary>F11 key.</summary>
    F11 = 582,

    /// <summary>F12 key.</summary>
    F12 = 583,

    /// <summary>F13 key.</summary>
    F13 = 584,

    /// <summary>F14 key.</summary>
    F14 = 585,

    /// <summary>F15 key.</summary>
    F15 = 586,

    /// <summary>F16 key.</summary>
    F16 = 587,

    /// <summary>F17 key.</summary>
    F17 = 588,

    /// <summary>F18 key.</summary>
    F18 = 589,

    /// <summary>F19 key.</summary>
    F19 = 590,

    /// <summary>F20 key.</summary>
    F20 = 591,

    /// <summary>F21 key.</summary>
    F21 = 592,

    /// <summary>F22 key.</summary>
    F22 = 593,

    /// <summary>F23 key.</summary>
    F23 = 594,

    /// <summary>F24 key.</summary>
    F24 = 595,

    // ── Punctuation / symbols ───────────────────────────────

    /// <summary>Apostrophe / single-quote key.</summary>
    Apostrophe = 596,

    /// <summary>Comma key.</summary>
    Comma = 597,

    /// <summary>Minus / hyphen key.</summary>
    Minus = 598,

    /// <summary>Period / full-stop key.</summary>
    Period = 599,

    /// <summary>Slash / forward-slash key.</summary>
    Slash = 600,

    /// <summary>Semicolon key.</summary>
    Semicolon = 601,

    /// <summary>Equal / plus key.</summary>
    Equal = 602,

    /// <summary>Left bracket key.</summary>
    LeftBracket = 603,

    /// <summary>Backslash key.</summary>
    Backslash = 604,

    /// <summary>Right bracket key.</summary>
    RightBracket = 605,

    /// <summary>Grave accent / tilde key.</summary>
    GraveAccent = 606,

    // ── Lock / utility keys ─────────────────────────────────

    /// <summary>Caps Lock key.</summary>
    CapsLock = 607,

    /// <summary>Scroll Lock key.</summary>
    ScrollLock = 608,

    /// <summary>Num Lock key.</summary>
    NumLock = 609,

    /// <summary>Print Screen key.</summary>
    PrintScreen = 610,

    /// <summary>Pause / Break key.</summary>
    Pause = 611,

    // ── Numpad ──────────────────────────────────────────────

    /// <summary>Numpad 0 key.</summary>
    Keypad0 = 612,

    /// <summary>Numpad 1 key.</summary>
    Keypad1 = 613,

    /// <summary>Numpad 2 key.</summary>
    Keypad2 = 614,

    /// <summary>Numpad 3 key.</summary>
    Keypad3 = 615,

    /// <summary>Numpad 4 key.</summary>
    Keypad4 = 616,

    /// <summary>Numpad 5 key.</summary>
    Keypad5 = 617,

    /// <summary>Numpad 6 key.</summary>
    Keypad6 = 618,

    /// <summary>Numpad 7 key.</summary>
    Keypad7 = 619,

    /// <summary>Numpad 8 key.</summary>
    Keypad8 = 620,

    /// <summary>Numpad 9 key.</summary>
    Keypad9 = 621,

    /// <summary>Numpad decimal / dot key.</summary>
    KeypadDecimal = 622,

    /// <summary>Numpad divide key.</summary>
    KeypadDivide = 623,

    /// <summary>Numpad multiply key.</summary>
    KeypadMultiply = 624,

    /// <summary>Numpad subtract key.</summary>
    KeypadSubtract = 625,

    /// <summary>Numpad add key.</summary>
    KeypadAdd = 626,

    /// <summary>Numpad Enter key.</summary>
    KeypadEnter = 627,
}
