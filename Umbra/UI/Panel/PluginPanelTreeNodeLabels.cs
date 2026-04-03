using Umbra.Logging;

namespace Umbra.UI.Panel;

/// <summary>
/// Validates and sanitizes tree-node labels used by <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// This type isolates ImGui label and ID separator handling from panel composition code. Invalid labels are warned once per owner and label pair so repeated panel construction does not flood the REFramework console with identical diagnostics.
/// </remarks>
internal static class PluginPanelTreeNodeLabels
{
    private static readonly HashSet<(string OwnerId, string TreeLabel)> _warnedInvalidLabels = [];
    private static readonly object _warningLock = new();

    /// <summary>
    /// Emits a developer warning when the section's tree-node label contains ImGui's label and ID separator token.
    /// </summary>
    /// <remarks>
    /// The warning is emitted at most once per section identifier and label pair. Rendering still sanitizes the label on every draw.
    /// </remarks>
    /// <param name="section">The section being added to the panel.</param>
    internal static void WarnIfInvalid(IPanelSection section)
        => WarnIfInvalid(section.SectionId, section.TreeNodeLabel, $"section '{section.SectionId}'");

    /// <summary>
    /// Emits a developer warning when the supplied root or section tree-node label contains ImGui's label and ID separator token.
    /// </summary>
    /// <remarks>
    /// The warning is emitted at most once per owner and label pair. Rendering still sanitizes the label on every draw.
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
    /// Removes any caller-supplied ImGui label suffix so the panel can append its own stable ID suffix.
    /// </summary>
    /// <param name="label">The caller-supplied tree-node label.</param>
    /// <returns>The label text up to, but not including, the first <c>##</c> token, or an empty string when <paramref name="label"/> is <see langword="null"/> or empty.</returns>
    internal static string Sanitize(string? label)
    {
        if (string.IsNullOrEmpty(label))
            return string.Empty;

        var hashIndex = label.IndexOf("##", StringComparison.Ordinal);
        return hashIndex >= 0 ? label[..hashIndex] : label;
    }
}
