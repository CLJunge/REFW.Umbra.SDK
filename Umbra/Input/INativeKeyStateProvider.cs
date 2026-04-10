namespace Umbra.Input;

/// <summary>
/// Abstracts native keyboard state queries so <see cref="KeyStateTracker"/> can be unit-tested
/// without requiring P/Invoke calls to the Windows API.
/// </summary>
internal interface INativeKeyStateProvider
{
    /// <summary>
    /// Determines whether the specified virtual key is currently held down.
    /// </summary>
    /// <param name="virtualKeyCode">The Windows virtual-key code to query.</param>
    /// <returns><see langword="true"/> if the key is physically pressed; otherwise, <see langword="false"/>.</returns>
    bool IsKeyDown(int virtualKeyCode);
}
