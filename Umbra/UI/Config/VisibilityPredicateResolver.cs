using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves <see cref="UmbraHideIfAttribute{T}"/> declarations into per-frame visibility predicates.
/// </summary>
/// <remarks>
/// Hide rules reuse the shared conditional-member resolution pipeline so member-access caching and diagnostics stay aligned with other condition-driven UI state.
/// </remarks>
internal static class VisibilityPredicateResolver
{
    /// <summary>
    /// Builds the visibility predicate for the supplied cached hide condition and configuration owner instance.
    /// </summary>
    /// <param name="hideIf">The cached hide-condition metadata, or <see langword="null"/> when no hide rule applies.</param>
    /// <param name="owner">The configuration object that owns the annotated parameter.</param>
    /// <returns>A predicate that returns <see langword="true"/> when the parameter should be rendered; otherwise, <see langword="false"/>.</returns>
    internal static Func<bool> Build(IHideIfAttribute? hideIf, object owner)
    {
        if (hideIf is null)
            return static () => true;

        var isMatch = ConditionalMemberPredicateResolver.BuildIsMatchPredicate(
            hideIf.MemberName,
            hideIf.HasValue,
            hideIf.BoxedValue,
            owner,
            "HideIf");

        return () => !isMatch();
    }
}
