namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Provides deterministic key names and captured keys for hotkey-drawer unit tests.
/// </summary>
internal sealed class TestHotkeyInputSource : IHotkeyInputSource
{
    private readonly Dictionary<int, string> _keyNames = [];
    private readonly Queue<(bool Success, int Key)> _capturedKeys = new();

    public int CaptureCallCount { get; private set; }

    public void SetKeyName(int key, string name) => _keyNames[key] = name;

    public void QueueCapturedKey(int key) => _capturedKeys.Enqueue((true, key));

    public void QueueNoCapturedKey() => _capturedKeys.Enqueue((false, -1));

    public bool TryCaptureKeyboardKey(out int capturedKey)
    {
        CaptureCallCount++;
        if (_capturedKeys.Count == 0)
        {
            capturedKey = -1;
            return false;
        }

        var next = _capturedKeys.Dequeue();
        capturedKey = next.Key;
        return next.Success;
    }

    public string GetKeyName(int key)
    {
        if (_keyNames.TryGetValue(key, out var name))
            return name;

        return $"Key({key})";
    }
}
