using Hexa.NET.ImGui;
using Umbra.SDK.UI;

namespace Umbra.SDK.Config.UI.ParameterDrawers;

/// <summary>
/// An <see cref="IParameterDrawer"/> implementation that renders a hotkey-capture control
/// for a <see cref="Parameter{T}"/> of type <see cref="int"/>, where the value represents
/// an <c>ImGuiKey</c> cast to <c>int</c>.
/// </summary>
/// <remarks>
/// At most one hotkey-capture drawer may be in capture mode at any given frame.
/// A shared static counter in <see cref="HotkeyCaptureState"/> enforces mutual exclusion
/// across all <see cref="HotkeyDrawer"/> and <see cref="TwoColumnHotkeyDrawer"/> instances
/// in the same assembly. <see cref="Dispose"/> must be called (via the owning
/// <see cref="ConfigDrawer{TConfig}"/>) on plugin unload so that any in-progress capture
/// does not permanently block future captures.
/// </remarks>
public sealed class HotkeyDrawer : IParameterDrawer
{
    private bool _waiting;
    private bool _disposed;

    /// <inheritdoc/>
    public void Draw(string label, IParameter parameter)
    {
        if (_disposed) return;

        if (parameter is not Parameter<int> p)
        {
            ImGui.TextDisabled($"{label}: (HotkeyDrawer requires Parameter<int>)");
            return;
        }

        var v = p.Value;
        var prev = v;
        var wasWaiting = _waiting;

        // Prevent multiple drawers from capturing input simultaneously.
        // HotkeyCaptureState.WaitingCount is shared with TwoColumnHotkeyDrawer.
        var otherWaiting = HotkeyCaptureState.WaitingCount > (wasWaiting ? 1 : 0);

        // Use the parameter key as the stable unique button ID so two parameters with the
        // same display label do not share an ImGui button ID within the same window.
        ImGuiControls.DrawHotKeySetting(label, p.Key, ref _waiting, ref v, otherWaiting);

        // Keep the shared counter in sync when this drawer's capture state changes.
        if (_waiting != wasWaiting)
            HotkeyCaptureState.WaitingCount += _waiting ? 1 : -1;

        if (v != prev) p.Value = v;

        var metadata = parameter.Metadata;
        if (metadata.Description is not null)
        {
            ImGui.SameLine();
            ImGuiControls.DrawHelpMarker(metadata.Description);
        }
    }

    /// <summary>
    /// Releases this drawer's contribution to the shared capture counter in
    /// <see cref="HotkeyCaptureState"/>. Must be called when the owning
    /// <see cref="ConfigDrawer{TConfig}"/> is disposed so that a mid-capture plugin unload
    /// does not permanently block future captures.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_waiting)
        {
            HotkeyCaptureState.WaitingCount--;
            _waiting = false;
        }
    }
}
