namespace Umbra.UI.Config;

/// <summary>
/// Stores one searchable configuration result in the drawer's flat search index.
/// </summary>
/// <param name="ResultId">The stable semantic identifier for the searchable result.</param>
/// <param name="NormalizedSearchText">The pre-normalized searchable text for the result.</param>
/// <param name="DrawOrderIndex">The stable draw-order index assigned during draw-tree construction.</param>
/// <param name="AncestorBranchIds">The stable branch identifiers that must be opened for this result to become visible.</param>
internal readonly record struct ConfigSearchEntry(
    string ResultId,
    string NormalizedSearchText,
    int DrawOrderIndex,
    string[] AncestorBranchIds);
