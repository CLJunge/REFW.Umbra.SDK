using Umbra.Logging;

namespace Umbra.UI.Panel;

/// <summary>
/// Tracks live <see cref="PluginPanel"/> ID scopes across the shared REFramework AppDomain.
/// </summary>
/// <remarks>
/// This type isolates duplicate-scope registration and release from <see cref="PluginPanel"/> so
/// the panel can remain focused on section composition and rendering.
/// </remarks>
internal static class PluginPanelScopeRegistry
{
    private static readonly HashSet<string> s_registeredScopes = [];
    private static readonly HashSet<string> s_warnedDuplicateScopes = [];
    private static readonly object s_scopeLock = new();

    /// <summary>
    /// Registers <paramref name="idScope"/> and logs a developer warning the first time the same
    /// active scope is detected as a duplicate.
    /// </summary>
    /// <remarks>
    /// The detailed duplicate-scope warning includes a stack trace so plugin authors can identify
    /// the conflicting panel construction site. To avoid spamming the REFramework console, that
    /// warning is emitted only once per still-active duplicate scope and is re-armed when the
    /// original scope is eventually released.
    /// </remarks>
    /// <param name="idScope">The globally unique panel scope to register.</param>
    /// <returns><see langword="true"/> when the scope was newly registered; otherwise <see langword="false"/>.</returns>
    internal static bool TryRegister(string idScope)
    {
        bool registered;
        bool shouldWarn;
        lock (s_scopeLock)
        {
            registered = s_registeredScopes.Add(idScope);
            shouldWarn = !registered && s_warnedDuplicateScopes.Add(idScope);
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
    /// Releases a previously registered panel scope and re-arms duplicate diagnostics for any
    /// future reuse of the same scope.
    /// </summary>
    /// <param name="idScope">The scope to release.</param>
    internal static void Release(string idScope)
    {
        lock (s_scopeLock)
        {
            s_registeredScopes.Remove(idScope);
            s_warnedDuplicateScopes.Remove(idScope);
        }
    }
}
