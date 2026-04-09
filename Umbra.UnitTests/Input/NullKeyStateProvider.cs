using Umbra.Input;

namespace Umbra.UnitTests.Input;

/// <summary>
/// A no-op key state provider that always reports all keys as released.
/// Used to isolate tests from the Win32 <c>GetAsyncKeyState</c> P/Invoke
/// so they can run on non-Windows platforms.
/// </summary>
internal sealed class NullKeyStateProvider : INativeKeyStateProvider
{
    /// <inheritdoc/>
    public bool IsKeyDown(int virtualKeyCode) => false;
}
