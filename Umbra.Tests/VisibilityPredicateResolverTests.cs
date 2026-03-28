using System.Collections;
using System.Reflection;
using Umbra.Config.Attributes;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class VisibilityPredicateResolverTests
{
    [Fact]
    public void Build_TracksEachInvalidHideIfBindingOnlyOnce()
    {
        using var _ = new LoggerTestScope();
        var resolverType = typeof(Umbra.UI.Config.ConfigDrawer<>).Assembly.GetType("Umbra.UI.Config.VisibilityPredicateResolver")!;
        var warningsField = resolverType.GetField("s_invalidAccessorWarnings", BindingFlags.Static | BindingFlags.NonPublic)!;
        var warningCache = (IDictionary)warningsField.GetValue(null)!;
        warningCache.Clear();

        var buildMethod = resolverType.GetMethod("Build", BindingFlags.Static | BindingFlags.NonPublic)!;
        var hideIf = new UmbraHideIfAttribute<bool>("MissingFlag");
        var owner = new HideIfOwner();

        buildMethod.Invoke(null, [hideIf, owner]);
        buildMethod.Invoke(null, [hideIf, owner]);

        Assert.Single(warningCache);
    }

    private sealed class HideIfOwner
    {
        public bool ExistingFlag { get; set; }
    }
}
