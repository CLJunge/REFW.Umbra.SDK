namespace Umbra.UI.Config;

/// <summary>
/// Stores one stable branch identifier that can be force-opened by search navigation.
/// </summary>
/// <param name="BranchId">The stable branch identifier.</param>
/// <param name="ParentBranchId">The parent branch identifier, or <see langword="null"/> for a top-level branch.</param>
internal readonly record struct ConfigSearchBranch(string BranchId, string? ParentBranchId);
