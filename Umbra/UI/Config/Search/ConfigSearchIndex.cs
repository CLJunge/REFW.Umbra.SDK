namespace Umbra.UI.Config.Search;

/// <summary>
/// Stores the flat searchable result set built alongside one configuration draw tree.
/// </summary>
/// <remarks>
/// This index is built from the semantic configuration traversal, not from the final rendered node
/// graph. It provides deterministic search matching and stores ancestor branch ids for later
/// expansion and navigation behavior.
/// </remarks>
internal sealed class ConfigSearchIndex
{
    private readonly List<ConfigSearchEntry> _entries = [];
    private readonly Dictionary<string, ConfigSearchEntry> _entriesByResultId = [];
    private readonly List<ConfigSearchBranch> _branches = [];
    private readonly HashSet<string> _branchIds = [];
    private int _nextDrawOrderIndex;

    internal IReadOnlyList<ConfigSearchEntry> Entries => _entries;

    internal IReadOnlyList<ConfigSearchBranch> Branches => _branches;

    internal void Clear()
    {
        _entries.Clear();
        _entriesByResultId.Clear();
        _branches.Clear();
        _branchIds.Clear();
        _nextDrawOrderIndex = 0;
    }

    internal void AddParameterResult(string? resultId, string label, string? description, string? category, string groupPath)
    {
        var ancestorBranchIds = BuildAncestorBranchIds(groupPath, category);
        var stableResultId = string.IsNullOrWhiteSpace(resultId)
            ? $"result:{_nextDrawOrderIndex}"
            : resultId;

        var entry = new ConfigSearchEntry(
            stableResultId,
            NormalizeSearchText(label, description, category),
            _nextDrawOrderIndex++,
            ancestorBranchIds);
        _entries.Add(entry);
        _entriesByResultId[stableResultId] = entry;
    }

    internal void PrependRootBranch(string branchId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(branchId);

        RegisterBranch(branchId, null);
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            var ancestorBranchIds = new string[entry.AncestorBranchIds.Length + 1];
            ancestorBranchIds[0] = branchId;
            Array.Copy(entry.AncestorBranchIds, 0, ancestorBranchIds, 1, entry.AncestorBranchIds.Length);
            var updatedEntry = entry with { AncestorBranchIds = ancestorBranchIds };
            _entries[i] = updatedEntry;
            _entriesByResultId[updatedEntry.ResultId] = updatedEntry;
        }
    }

    internal bool TryGetEntry(string resultId, out ConfigSearchEntry entry)
        => _entriesByResultId.TryGetValue(resultId, out entry);

    internal List<string> FindMatches(string normalizedQuery)
    {
        var matches = new List<string>();
        if (string.IsNullOrEmpty(normalizedQuery))
            return matches;

        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (entry.NormalizedSearchText.Contains(normalizedQuery, StringComparison.Ordinal))
                matches.Add(entry.ResultId);
        }

        return matches;
    }

    private string[] BuildAncestorBranchIds(string groupPath, string? category)
    {
        var branchIds = new List<string>();
        AddGroupBranches(groupPath, branchIds);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryBranchId = BuildCategoryBranchId(groupPath, category);
            var parentBranchId = branchIds.Count > 0 ? branchIds[^1] : null;
            RegisterBranch(categoryBranchId, parentBranchId);
            branchIds.Add(categoryBranchId);
        }

        return [.. branchIds];
    }

    private void AddGroupBranches(string groupPath, List<string> branchIds)
    {
        if (string.IsNullOrEmpty(groupPath))
            return;

        var segments = groupPath.Split('.');
        var currentPath = string.Empty;
        string? parentBranchId = null;
        foreach (var segment in segments)
        {
            currentPath = string.IsNullOrEmpty(currentPath)
                ? segment
                : $"{currentPath}.{segment}";

            var branchId = BuildGroupBranchId(currentPath);
            RegisterBranch(branchId, parentBranchId);
            branchIds.Add(branchId);
            parentBranchId = branchId;
        }
    }

    private void RegisterBranch(string branchId, string? parentBranchId)
    {
        if (!_branchIds.Add(branchId))
            return;

        _branches.Add(new ConfigSearchBranch(branchId, parentBranchId));
    }

    private static string NormalizeSearchText(string label, string? description, string? category)
    {
        var buffer = label.ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(description))
            buffer = string.Concat(buffer, "\n", description.Trim().ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(category))
            buffer = string.Concat(buffer, "\n", category.Trim().ToUpperInvariant());
        return buffer;
    }

    private static string BuildGroupBranchId(string groupPath) => $"group:{groupPath}";

    private static string BuildCategoryBranchId(string groupPath, string category)
        => string.IsNullOrEmpty(groupPath)
            ? $"category:{category}"
            : $"category:{groupPath}|{category}";
}
