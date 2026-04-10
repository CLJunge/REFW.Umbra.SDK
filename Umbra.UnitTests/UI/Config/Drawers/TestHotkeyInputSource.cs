using Umbra.Input;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Provides deterministic key names and captured keys for hotkey-drawer unit tests.
/// </summary>
internal sealed class TestHotkeyInputSource : IHotkeyInputSource
{
    private readonly Dictionary<int, string> _keyNames = [];
    private readonly Queue<(bool Success, int Key)> _capturedKeys = new();
    private readonly Dictionary<HotkeyBinding, string> _bindingNames = [];
    private readonly Queue<(bool Success, HotkeyBinding Binding)> _capturedBindings = new();

    public int CaptureCallCount { get; private set; }
    public int BindingCaptureCallCount { get; private set; }
    public string HeldModifierPrefix { get; set; } = "";

    public void SetKeyName(int key, string name) => _keyNames[key] = name;

    public void QueueCapturedKey(int key) => _capturedKeys.Enqueue((true, key));

    public void QueueNoCapturedKey() => _capturedKeys.Enqueue((false, -1));

    public void SetBindingDisplayName(HotkeyBinding binding, string name) => _bindingNames[binding] = name;

    public void QueueCapturedBinding(HotkeyBinding binding) => _capturedBindings.Enqueue((true, binding));

    public void QueueNoCapturedBinding() => _capturedBindings.Enqueue((false, HotkeyBinding.None));

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

    public bool TryCaptureHotkeyBinding(out HotkeyBinding binding)
    {
        BindingCaptureCallCount++;
        if (_capturedBindings.Count == 0)
        {
            binding = HotkeyBinding.None;
            return false;
        }

        var next = _capturedBindings.Dequeue();
        binding = next.Binding;
        return next.Success;
    }

    public string GetBindingDisplayName(HotkeyBinding binding)
    {
        if (_bindingNames.TryGetValue(binding, out var name))
            return name;

        return $"Key({binding.Key})";
    }

    public string GetHeldModifierPrefix() => HeldModifierPrefix;
}
