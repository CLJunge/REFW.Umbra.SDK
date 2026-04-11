namespace Umbra.Input;

/// <summary>
/// Represents a keyboard hotkey combination consisting of a primary key and optional Ctrl, Shift, and Alt modifiers.
/// </summary>
/// <param name="Key">The primary <see cref="UmbraKey"/> value cast to <see cref="int"/>. Use <c>(int)UmbraKey.None</c> for an unbound hotkey.</param>
/// <param name="Ctrl">Whether the Ctrl modifier must be held.</param>
/// <param name="Shift">Whether the Shift modifier must be held.</param>
/// <param name="Alt">Whether the Alt modifier must be held.</param>
public readonly record struct HotkeyBinding(int Key, bool Ctrl = false, bool Shift = false, bool Alt = false)
{
    /// <summary>
    /// A hotkey binding with no key assigned and no modifiers.
    /// </summary>
    public static readonly HotkeyBinding None = new((int)UmbraKey.None, false, false, false);

    /// <summary>
    /// Gets a value indicating whether this binding has no primary key assigned.
    /// </summary>
    public bool IsEmpty => Key == (int)UmbraKey.None;

    /// <summary>
    /// Returns a human-readable display string such as <c>Ctrl+Shift+F5</c>.
    /// </summary>
    public string GetDisplayName()
    {
        if (IsEmpty) return "None";

        var keyName = KeyboardInput.GetKeyName(Key);
        if (!Ctrl && !Shift && !Alt) return keyName;

        var modifiers = (Ctrl, Shift, Alt) switch
        {
            (true, true, true) => "Ctrl+Shift+Alt+",
            (true, true, false) => "Ctrl+Shift+",
            (true, false, true) => "Ctrl+Alt+",
            (false, true, true) => "Shift+Alt+",
            (true, false, false) => "Ctrl+",
            (false, true, false) => "Shift+",
            (false, false, true) => "Alt+",
            _ => ""
        };

        return string.Concat(modifiers, keyName);
    }

    /// <inheritdoc/>
    public override string ToString() => GetDisplayName();
}
