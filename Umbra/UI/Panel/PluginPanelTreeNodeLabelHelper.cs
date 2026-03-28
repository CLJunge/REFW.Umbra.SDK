using Umbra.Logging;

namespace Umbra.UI.Panel;

/// <summary>
/// Provides helper methods for validating and sanitizing panel section tree-node labels.
/// </summary>
/// <remarks>
/// This type isolates ImGui label/ID separator handling from <see cref="PluginPanel"/>.
/// </remarks>
internal static class PluginPanelTreeNodeLabelHelper
{
    /// <summary>
    /// Logs a developer warning when the section's tree-node label contains ImGui's label/ID
    /// separator token.
    /// </summary>
    /// <param name="section">The section being added to the panel.</param>
    internal static void WarnIfInvalid(IPanelSection section)
    {
        if (section.TreeNodeLabel is not { } treeLabel || !treeLabel.Contains("##", StringComparison.Ordinal))
            return;

        Logger.Warning(
            $"[PluginPanel] DEVELOPER WARNING — Section '{section.SectionId}' has a TreeNodeLabel containing \"##\".\n" +
            $"\n" +
            $"  Impact : ImGui treats the first \"##\" in a label as the visible-label/ID separator,\n" +
            $"           so any \"##\" already present in TreeNodeLabel causes the appended\n" +
            $"           \"##{section.SectionId}\" disambiguation suffix to be silently ignored.\n" +
            $"           Two sections with identical label prefixes would then share the same\n" +
            $"           persisted open/closed state and the visible label may be truncated.\n" +
            $"\n" +
            $"  Fix    : Remove \"##\" from the TreeNodeLabel of section '{section.SectionId}'.\n" +
            $"           The panel strips the \"##...\" portion at render time as a fallback.\n" +
            $"\n" +
            $"  Stack  :\n{Environment.StackTrace}");
    }

    /// <summary>
    /// Removes any caller-supplied ImGui label/ID suffix so the panel can append its own stable
    /// <c>##{SectionId}</c> disambiguation suffix.
    /// </summary>
    /// <param name="label">The caller-supplied tree-node label.</param>
    /// <returns>
    /// The sanitized label text with everything from the first <c>##</c> separator token onwards
    /// removed, matching ImGui's label/ID separator semantics.
    /// </returns>
    internal static string Sanitize(string label)
    {
        var hashIndex = label.IndexOf("##", StringComparison.Ordinal);
        return hashIndex >= 0 ? label[..hashIndex] : label;
    }
}
