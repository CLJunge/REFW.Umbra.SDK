using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves <see cref="UmbraDisableIfAttribute{T}"/> declarations into per-frame disabled-state predicates.
/// </summary>
/// <remarks>
/// Disable rules reuse the shared conditional-member resolution pipeline so they stay behaviorally aligned with hide rules while remaining a separate enabled-state concern.
/// </remarks>
internal static class DisablePredicateResolver
{
    /// <summary>
    /// Builds the disabled-state predicate for the supplied cached disable condition and configuration owner instance.
    /// </summary>
    /// <param name="disableIf">The cached disable-condition metadata, or <see langword="null"/> when no disable rule applies.</param>
    /// <param name="owner">The configuration object that owns the annotated parameter.</param>
    /// <returns>A predicate that returns <see langword="true"/> when the parameter should render disabled; otherwise, <see langword="false"/>.</returns>
    internal static Func<bool> Build(IDisableIfAttribute? disableIf, object owner)
    {
        return disableIf is null
            ? (static () => false)
            : ConditionalMemberPredicateResolver.BuildIsMatchPredicate(
                disableIf.MemberName,
                disableIf.HasValue,
                disableIf.BoxedValue,
                owner,
                "DisableIf");
    }
}
