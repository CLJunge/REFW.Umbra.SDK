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
    private static readonly object s_scopeLock = new();

    /// <summary>
    /// Registers <paramref name="idScope"/> and logs a developer warning when the same scope is
    /// already active in another panel.
    /// </summary>
    /// <param name="idScope">The globally unique panel scope to register.</param>
    /// <returns><see langword="true"/> when the scope was newly registered; otherwise <see langword="false"/>.</returns>
    internal static bool TryRegister(string idScope)
    {
        bool registered;
        lock (s_scopeLock)
        {
            registered = s_registeredScopes.Add(idScope);
        }

        if (!registered)
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
    /// Releases a previously registered panel scope.
    /// </summary>
    /// <param name="idScope">The scope to release.</param>
    internal static void Release(string idScope)
    {
        lock (s_scopeLock)
        {
            s_registeredScopes.Remove(idScope);
        }
    }
}
