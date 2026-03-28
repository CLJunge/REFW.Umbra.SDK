using System.Text.Json;
using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class SettingsStoreTests
{
    [Fact]
    public void Load_CreatesDefaultsFile_WhenDirectoryDoesNotExist()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "nested", "config.json");

        using var store = new SettingsStore<BasicConfig>(filePath);
        var config = store.Load();

        Assert.NotNull(config);
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Load_Throws_WhenResolvedKeysAreDuplicate()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "duplicate.json");

        using var store = new SettingsStore<DuplicateKeyConfig>(filePath);
        var exception = Assert.Throws<InvalidOperationException>(() => store.Load());

        Assert.Contains("dup.same", exception.Message);
    }

    [Fact]
    public void Load_BacksUpUnreadableJson_AndRewritesDefaults()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        File.WriteAllText(filePath, "{ this is not valid json }");

        using var store = new SettingsStore<BasicConfig>(filePath);
        var config = store.Load();

        Assert.NotNull(config);
        Assert.True(File.Exists(filePath));

        var files = Directory.GetFiles(temp.Path, "config.invalid-*.json");
        Assert.Single(files);
        Assert.Equal("{ this is not valid json }", File.ReadAllText(files[0]));

        var json = File.ReadAllText(filePath);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.True(root.TryGetProperty("tests.enabled", out var enabled));
        Assert.True(enabled.GetBoolean());
        Assert.True(root.TryGetProperty("tests.count", out var count));
        Assert.Equal(5, count.GetInt32());
    }

    [Fact]
    public void Load_RewritesTrueDefaults_AfterFailureThatPartiallyAppliedValues()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        File.WriteAllText(filePath, """
        {
          "tests.count": 42,
          "tests.enabled": "not-a-bool"
        }
        """);

        using var store = new SettingsStore<BasicConfig>(filePath);
        var config = store.Load();

        Assert.NotNull(config);
        Assert.True(config.Enabled.Value);
        Assert.Equal(5, config.Count.Value);

        var files = Directory.GetFiles(temp.Path, "config.invalid-*.json");
        Assert.Single(files);

        var rewrittenJson = File.ReadAllText(filePath);
        using var document = JsonDocument.Parse(rewrittenJson);
        var root = document.RootElement;
        Assert.True(root.TryGetProperty("tests.enabled", out var enabled));
        Assert.True(enabled.GetBoolean());
        Assert.True(root.TryGetProperty("tests.count", out var count));
        Assert.Equal(5, count.GetInt32());
    }
}
