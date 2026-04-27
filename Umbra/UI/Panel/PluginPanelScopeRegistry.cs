using Umbra.Logging;

namespace Umbra.UI.Panel;

/// <summary>
/// Tracks live <see cref="PluginPanel"/> ID scopes across the shared managed plugin host.
/// </summary>
/// <remarks>
/// This registry isolates duplicate-scope detection and release from <see cref="PluginPanel"/> so the panel can remain focused on section composition and rendering. Scope registration is AppDomain-local and exists to prevent accidental ImGui ID collisions between plugins loaded in the same REFramework process.
/// </remarks>
internal static class PluginPanelScopeRegistry
{
    private static readonly HashSet<string> _registeredScopes = [];
    private static readonly HashSet<string> _warnedDuplicateScopes = [];
    private static readonly Lock _scopeLock = new();

    /// <summary>
    /// Registers <paramref name="idScope"/> and emits a developer warning the first time the same active scope is detected as a duplicate.
    /// </summary>
    /// <remarks>
    /// The duplicate-scope warning includes a stack trace to help locate the conflicting panel construction site. The warning is emitted at most once per still-active duplicate scope and is re-armed when the original scope is released.
    /// </remarks>
    /// <param name="idScope">The globally unique panel scope to register.</param>
    /// <returns><see langword="true"/> if the scope was newly registered; otherwise, <see langword="false"/>.</returns>
    internal static bool TryRegister(string idScope)
    {
        bool registered;
        bool shouldWarn;
        lock (_scopeLock)
        {
            registered = _registeredScopes.Add(idScope);
            shouldWarn = !registered && _warnedDuplicateScopes.Add(idScope);
        }

        if (shouldWarn)
        {
            Logger.Warning(
                $"[PluginPanel] DEVELOPER WARNING — Duplicate idScope '{idScope}' detected.\n" +
                $"\n" +
                $"  Impact : All ImGui widget IDs produced by this panel share the same hash as the\n" +
                $"           existing panel using the same scope. Buttons, sliders, checkboxes, and\n" +
                $"           tree nodes in both panels will silently share state across plugins.\n" +
                $"\n" +
                $"  Fix    : Pass a globally unique string to new PluginPanel(idScope), e.g.:\n" +
                $"             new PluginPanel(nameof(MyPlugin))\n" +
                $"             new PluginPanel(typeof(MyPlugin).FullName!)\n" +
                $"\n" +
                $"  Stack  :\n{Environment.StackTrace}");
        }

        return registered;
    }

    /// <summary>
    /// Releases a previously registered panel scope and re-arms duplicate diagnostics for future reuse of the same scope.
    /// </summary>
    /// <param name="idScope">The scope to release.</param>
    internal static void Release(string idScope)
    {
        lock (_scopeLock)
        {
            _registeredScopes.Remove(idScope);
            _warnedDuplicateScopes.Remove(idScope);
        }
    }
}
