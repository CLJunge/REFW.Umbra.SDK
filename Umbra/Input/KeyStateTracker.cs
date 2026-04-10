namespace Umbra.Input;

/// <summary>
/// Tracks keyboard key state using snapshot-diff edge detection. Each <see cref="Update"/> call
/// reads the current hardware state via an <see cref="INativeKeyStateProvider"/>, compares it
/// against the previous snapshot, and exposes key-down (pressed), key-up (released), and held edges.
/// </summary>
/// <remarks>
/// This tracker is the core of Umbra's reliable keyboard input. Unlike polling
/// <c>ImGui.IsKeyPressed</c> which returns <see langword="true"/> for a single ImGui frame,
/// the tracker derives edges from two consecutive hardware snapshots, making it resilient
/// to low frame rates and ImGui widget key consumption.
/// </remarks>
internal sealed class KeyStateTracker
{
    private readonly INativeKeyStateProvider _provider;
    private readonly int[] _trackedKeys;

    private readonly HashSet<int> _previouslyDown;
    private readonly HashSet<int> _currentlyDown;
    private readonly List<int> _justPressed;
    private readonly List<int> _justReleased;

    /// <summary>
    /// Initializes a new <see cref="KeyStateTracker"/>.
    /// </summary>
    /// <param name="provider">The native key state source.</param>
    /// <param name="trackedKeys">The Windows virtual-key codes to track each update.</param>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> or <paramref name="trackedKeys"/> is <see langword="null"/>.</exception>
    public KeyStateTracker(INativeKeyStateProvider provider, int[] trackedKeys)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(trackedKeys);

        _provider = provider;
        _trackedKeys = trackedKeys;

        _previouslyDown = [with(trackedKeys.Length)];
        _currentlyDown = [with(trackedKeys.Length)];
        _justPressed = [with(8)];
        _justReleased = [with(8)];
    }

    /// <summary>
    /// Snapshots the current hardware key state and computes edge transitions.
    /// </summary>
    /// <remarks>
    /// Call this exactly once per frame tick before reading edge state.
    /// After this call, <see cref="JustPressed"/>, <see cref="JustReleased"/>,
    /// and <see cref="IsDown"/> reflect the current tick.
    /// </remarks>
    public void Update()
    {
        _justPressed.Clear();
        _justReleased.Clear();
        _previouslyDown.Clear();

        // Swap: current → previous
        foreach (var key in _currentlyDown)
            _previouslyDown.Add(key);

        _currentlyDown.Clear();

        // Sample current hardware state
        foreach (var vk in _trackedKeys)
        {
            if (!_provider.IsKeyDown(vk))
                continue;

            _currentlyDown.Add(vk);

            if (!_previouslyDown.Contains(vk))
                _justPressed.Add(vk);
        }

        // Detect releases: was down, now up
        foreach (var vk in _previouslyDown)
        {
            if (!_currentlyDown.Contains(vk))
                _justReleased.Add(vk);
        }
    }

    /// <summary>
    /// Determines whether the specified virtual key is currently held down.
    /// </summary>
    /// <param name="virtualKeyCode">The Windows virtual-key code to check.</param>
    /// <returns><see langword="true"/> if the key was down during the last <see cref="Update"/> call; otherwise, <see langword="false"/>.</returns>
    public bool IsDown(int virtualKeyCode) => _currentlyDown.Contains(virtualKeyCode);

    /// <summary>
    /// Determines whether the specified virtual key transitioned from up to down this tick (key-down edge).
    /// </summary>
    /// <param name="virtualKeyCode">The Windows virtual-key code to check.</param>
    /// <returns><see langword="true"/> if the key was just pressed; otherwise, <see langword="false"/>.</returns>
    public bool JustPressed(int virtualKeyCode)
    {
        foreach (var vk in _justPressed)
        {
            if (vk == virtualKeyCode)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the number of keys that transitioned from up to down this tick.
    /// </summary>
    public int JustPressedCount => _justPressed.Count;

    /// <summary>
    /// Returns the virtual-key code of the just-pressed key at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index into the just-pressed list.</param>
    /// <returns>The Windows virtual-key code at the given position.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or greater than or equal to <see cref="JustPressedCount"/>.</exception>
    public int GetJustPressedAt(int index) => _justPressed[index];

    /// <summary>
    /// Determines whether the specified virtual key transitioned from down to up this tick (key-up edge).
    /// </summary>
    /// <param name="virtualKeyCode">The Windows virtual-key code to check.</param>
    /// <returns><see langword="true"/> if the key was just released; otherwise, <see langword="false"/>.</returns>
    public bool JustReleased(int virtualKeyCode)
    {
        foreach (var vk in _justReleased)
        {
            if (vk == virtualKeyCode)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the first virtual key that transitioned from up to down this tick.
    /// </summary>
    /// <param name="virtualKeyCode">When this method returns <see langword="true"/>, contains the VK code; otherwise, <c>-1</c>.</param>
    /// <returns><see langword="true"/> if at least one key was just pressed; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstPressed(out int virtualKeyCode)
    {
        if (_justPressed.Count > 0)
        {
            virtualKeyCode = _justPressed[0];
            return true;
        }

        virtualKeyCode = -1;
        return false;
    }
}
