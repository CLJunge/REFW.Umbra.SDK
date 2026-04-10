using System.Runtime.InteropServices;

namespace Umbra.Input;

/// <summary>
/// Queries physical keyboard state by calling the Win32 <c>GetAsyncKeyState</c> function.
/// </summary>
/// <remarks>
/// <c>GetAsyncKeyState</c> reads the hardware key state at the moment of the call, independent
/// of any window message processing or ImGui frame state. This makes it reliable for detecting
/// key presses even when the game is running at low frame rates or when ImGui widgets consume
/// key events.
/// </remarks>
internal sealed class NativeKeyStateProvider : INativeKeyStateProvider
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    /// <inheritdoc/>
    public bool IsKeyDown(int virtualKeyCode)
        => (GetAsyncKeyState(virtualKeyCode) & 0x8000) != 0;
}
