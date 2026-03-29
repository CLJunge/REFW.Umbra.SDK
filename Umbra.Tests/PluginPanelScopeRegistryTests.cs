using System.Reflection;
using Umbra.Tests.TestSupport;
using Umbra.UI.Panel;
using Xunit;

namespace Umbra.Tests;

public sealed class PluginPanelScopeRegistryTests
{
    [Fact]
    public void TryRegister_WarnsOncePerActiveDuplicateScope_AndRearmsAfterRelease()
    {
        using var _ = new LoggerTestScope();
        var registryType = typeof(PluginPanel).Assembly.GetType("Umbra.UI.Panel.PluginPanelScopeRegistry")!;
        var registeredField = registryType.GetField("s_registeredScopes", BindingFlags.Static | BindingFlags.NonPublic)!;
        var warnedField = registryType.GetField("s_warnedDuplicateScopes", BindingFlags.Static | BindingFlags.NonPublic)!;
        var registeredScopes = (HashSet<string>)registeredField.GetValue(null)!;
        var warnedScopes = (HashSet<string>)warnedField.GetValue(null)!;
        registeredScopes.Clear();
        warnedScopes.Clear();

        var tryRegister = registryType.GetMethod("TryRegister", BindingFlags.Static | BindingFlags.NonPublic)!;
        var release = registryType.GetMethod("Release", BindingFlags.Static | BindingFlags.NonPublic)!;

        Assert.True((bool)tryRegister.Invoke(null, ["MyScope"])!);
        Assert.False((bool)tryRegister.Invoke(null, ["MyScope"])!);
        Assert.Single(warnedScopes);

        release.Invoke(null, ["MyScope"]);
        Assert.Empty(warnedScopes);

        Assert.True((bool)tryRegister.Invoke(null, ["MyScope"])!);
        Assert.False((bool)tryRegister.Invoke(null, ["MyScope"])!);
        Assert.Single(warnedScopes);
    }
}
