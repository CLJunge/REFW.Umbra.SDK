using Umbra.Logging;

namespace Umbra.UI.Panel;

/// <summary>
/// Validates and sanitizes panel section tree-node labels.
/// </summary>
/// <remarks>
/// This type isolates ImGui label/ID separator handling from <see cref="PluginPanel"/>.
/// Invalid labels are warned once per section-id/label pair so repeated panel construction does
/// not flood the REFramework console with identical stack-trace diagnostics.
/// </remarks>
internal static class PluginPanelTreeNodeLabels
{
    private static readonly HashSet<(string OwnerId, string TreeLabel)> _warnedInvalidLabels = [];
    private static readonly object _warningLock = new();

    /// <summary>
    /// Logs a developer warning when the section's tree-node label contains ImGui's label/ID
    /// separator token.
    /// </summary>
    /// <remarks>
    /// The warning is emitted only once per section-id/label pair. The panel still sanitizes the
    /// label at render time on every draw.
    /// </remarks>
    /// <param name="section">The section being added to the panel.</param>
    internal static void WarnIfInvalid(IPanelSection section)
        => WarnIfInvalid(section.SectionId, section.TreeNodeLabel, $"section '{section.SectionId}'");

    /// <summary>
    /// Logs a developer warning when the supplied root or section tree-node label contains ImGui's
    /// label/ID separator token.
    /// </summary>
    /// <remarks>
    /// The warning is emitted only once per owner-id/label pair. The panel still sanitizes the
    /// label at render time on every draw.
    /// </remarks>
    /// <param name="ownerId">The stable panel or section identifier associated with the label.</param>
    /// <param name="treeLabel">The root or section tree-node label to validate.</param>
    /// <param name="ownerDescription">The owner description used in the warning text.</param>
    internal static void WarnIfInvalid(string ownerId, string? treeLabel, string ownerDescription)
    {
        if (treeLabel is not { } || !treeLabel.Contains("##", StringComparison.Ordinal))
            return;

        var shouldWarn = false;
        lock (_warningLock)
        {
            shouldWarn = _warnedInvalidLabels.Add((ownerId, treeLabel));
        }

        if (!shouldWarn)
            return;

        Logger.Warning(
            $"[PluginPanel] DEVELOPER WARNING — {ownerDescription} has a tree-node label containing \"##\".\n" +
            $"\n" +
            $"  Impact : ImGui treats the first \"##\" in a label as the visible-label/ID separator,\n" +
            $"           so any \"##\" already present in the tree-node label causes the panel's\n" +
            $"           own ImGui ID disambiguation to be silently ignored.\n" +
            $"           Two tree nodes with identical label prefixes would then share the same\n" +
            $"           persisted open/closed state and the visible label may be truncated.\n" +
            $"\n" +
            $"  Fix    : Remove \"##\" from the tree-node label for {ownerDescription}.\n" +
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
    internal static string Sanitize(string? label)
    {
        if (string.IsNullOrEmpty(label))
            return string.Empty;

        var hashIndex = label.IndexOf("##", StringComparison.Ordinal);
        return hashIndex >= 0 ? label[..hashIndex] : label;
    }
}
