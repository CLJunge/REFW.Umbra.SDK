using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class SettingsStoreCopyValuesTests
{
    [Fact]
    public void CopyValuesTo_Throws_WhenTargetIsNull()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var sourcePath = Path.Combine(temp.Path, "source.json");
        using var source = new SettingsStore<BasicConfig>(sourcePath);
        source.Load();

        var exception = Assert.Throws<ArgumentNullException>(() => source.CopyValuesTo(null!));

        Assert.Equal("target", exception.ParamName);
    }

    [Fact]
    public void CopyValuesTo_Throws_WhenTargetHasNotBeenLoaded()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var sourcePath = Path.Combine(temp.Path, "source.json");
        var targetPath = Path.Combine(temp.Path, "target.json");
        using var source = new SettingsStore<BasicConfig>(sourcePath);
        using var target = new SettingsStore<BasicConfig>(targetPath);
        source.Load();

        var exception = Assert.Throws<InvalidOperationException>(() => source.CopyValuesTo(target));

        Assert.Contains("completed Load()", exception.Message);
    }

    [Fact]
    public void CopyValuesTo_Throws_WhenTargetIsDisposed()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var sourcePath = Path.Combine(temp.Path, "source.json");
        var targetPath = Path.Combine(temp.Path, "target.json");
        using var source = new SettingsStore<BasicConfig>(sourcePath);
        var target = new SettingsStore<BasicConfig>(targetPath);
        source.Load();
        target.Load();
        target.Dispose();

        Assert.Throws<ObjectDisposedException>(() => source.CopyValuesTo(target));
    }

    [Fact]
    public void CopyValuesTo_CopiesValues_WhenTargetIsLoaded()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var sourcePath = Path.Combine(temp.Path, "source.json");
        var targetPath = Path.Combine(temp.Path, "target.json");
        using var source = new SettingsStore<BasicConfig>(sourcePath);
        using var target = new SettingsStore<BasicConfig>(targetPath);
        var sourceConfig = source.Load();
        var targetConfig = target.Load();

        sourceConfig.Enabled.Value = false;
        sourceConfig.Count.Value = 42;

        source.CopyValuesTo(target);

        Assert.False(targetConfig.Enabled.Value);
        Assert.Equal(42, targetConfig.Count.Value);
    }
}
