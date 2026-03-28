using System.Numerics;
using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Shared cached button drawer used by the default <see cref="Action"/>-parameter dispatch path.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Umbra.UI.Config.ControlFactory"/> uses this singleton when a
/// <see cref="Parameter{T}"/> has value type <see cref="Action"/> and no explicit custom drawer
/// has been selected. This avoids allocating a separate <see cref="ButtonDrawer"/> instance for
/// every default action button while preserving <see cref="ButtonDrawer"/> itself for explicit
/// <c>[UmbraCustomDrawer&lt;ButtonDrawer&gt;]</c> usage.
/// </para>
/// <para>
/// The one-time warning emitted for <see cref="ButtonStyle.Custom"/> without
/// <see cref="UmbraCustomButtonColorsAttribute"/> is tracked per parameter instance via
/// <see cref="ConditionalWeakTable{TKey,TValue}"/> so the shared drawer does not suppress warnings
/// for unrelated buttons and does not keep dead parameter objects alive.
/// </para>
/// </remarks>
internal sealed class CachedButtonDrawer : IParameterDrawer
{
    private sealed class MissingCustomColorsWarningState;

    private readonly object _warningLock = new();
    private readonly ConditionalWeakTable<IParameter, MissingCustomColorsWarningState> _missingColorWarnings = [];

    /// <summary>Gets the shared cached drawer instance used for implicit action-button rendering.</summary>
    internal static CachedButtonDrawer Instance { get; } = new();

    private CachedButtonDrawer()
    {
    }

    /// <inheritdoc/>
    public void Draw(string label, IParameter parameter)
    {
        if (parameter is not Parameter<Action> p)
        {
            ImGui.TextDisabled($"{label}: (CachedButtonDrawer requires Parameter<Action>)");
            return;
        }

        var meta = parameter.Metadata;
        var style = meta.ButtonStyle ?? ButtonStyle.Default;
        var size = new Vector2(meta.ControlWidth ?? 0f, 0f);
        bool colorsPushed;

        if (style == ButtonStyle.Custom && meta.CustomButtonColors is null)
        {
            if (ShouldWarnAboutMissingColors(parameter))
            {
                Logger.Warning(
                    $"CachedButtonDrawer: '{label}' uses ButtonStyle.Custom without a [UmbraCustomButtonColors] attribute; " +
                    "falling back to ButtonStyle.Default. Add [UmbraCustomButtonColors(...)] to suppress this warning.");
            }

            style = ButtonStyle.Default;
        }

        if (meta.CustomButtonColors is { } custom)
            colorsPushed = ButtonStyleColors.Push(custom.Normal, custom.Hovered, custom.Active);
        else
            colorsPushed = ButtonStyleColors.Push(style);

        var clicked = ImGui.Button(label, size);
        if (colorsPushed) ButtonStyleColors.Pop();

        if (clicked) p.Value?.Invoke();

        if (meta.Description is not null)
        {
            ImGui.SameLine();
            ImGuiWidgets.DrawHelpMarker(meta.Description);
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> only the first time a specific parameter instance is found to
    /// be misconfigured with <see cref="ButtonStyle.Custom"/> but no custom colors.
    /// </summary>
    private bool ShouldWarnAboutMissingColors(IParameter parameter)
    {
        lock (_warningLock)
        {
            if (_missingColorWarnings.TryGetValue(parameter, out _))
                return false;

            _missingColorWarnings.Add(parameter, new MissingCustomColorsWarningState());
            return true;
        }
    }
}
