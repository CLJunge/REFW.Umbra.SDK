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
        Assert.False(store.IsLoaded);
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

    [Fact]
    public void Save_IsSuppressed_ForSession_WhenUnreadableFileCouldNotBeBackedUp()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        File.WriteAllText(filePath, "{ this is not valid json }");

        using var backupBlocker = CreateBackupBlocker(filePath);
        using var store = new SettingsStore<BasicConfig>(filePath);
        var config = store.Load();

        Assert.NotNull(config);
        backupBlocker.Dispose();

        config.Enabled.Value = false;
        store.Save();

        Assert.Equal("{ this is not valid json }", File.ReadAllText(filePath));
        Assert.Empty(Directory.GetFiles(temp.Path, "config.invalid-*.json"));
    }

    [Fact]
    public void Load_ResetsToTrueDefaults_WhenBackupFails_AfterPartialApplication()
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

        using var backupBlocker = CreateBackupBlocker(filePath);
        using var store = new SettingsStore<BasicConfig>(filePath);
        var config = store.Load();

        Assert.NotNull(config);
        Assert.True(config.Enabled.Value);
        Assert.Equal(5, config.Count.Value);

        backupBlocker.Dispose();
        config.Count.Value = 99;
        store.Save();

        var json = File.ReadAllText(filePath);
        Assert.Contains("\"tests.count\": 42", json);
        Assert.DoesNotContain("\"tests.count\": 99", json);
        Assert.Empty(Directory.GetFiles(temp.Path, "config.invalid-*.json"));
    }

    private static IDisposable CreateBackupBlocker(string filePath)
    {
        if (OperatingSystem.IsWindows())
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

        return new DirectoryWritePermissionScope(Path.GetDirectoryName(filePath)!);
    }

    private sealed class DirectoryWritePermissionScope : IDisposable
    {
        private readonly string _directoryPath;
        private readonly UnixFileMode _originalMode;
        private bool _disposed;

        public DirectoryWritePermissionScope(string directoryPath)
        {
            if (OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("Unix directory permissions are only used on non-Windows test runners.");

            _directoryPath = directoryPath;
            _originalMode = File.GetUnixFileMode(directoryPath);

            File.SetUnixFileMode(
                directoryPath,
                _originalMode & ~(UnixFileMode.UserWrite | UnixFileMode.GroupWrite | UnixFileMode.OtherWrite));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (OperatingSystem.IsWindows())
                return;

            _disposed = true;
            File.SetUnixFileMode(_directoryPath, _originalMode);
        }
    }
}
