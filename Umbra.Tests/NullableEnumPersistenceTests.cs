using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class NullableEnumPersistenceTests
{
    [Fact]
    public void Load_RestoresNullableEnumValue_FromJsonString()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        File.WriteAllText(filePath, """
        {
          "nullableEnum.value": "Second"
        }
        """);

        using var store = new SettingsStore<NullableEnumConfig>(filePath);
        var config = store.Load();

        Assert.Equal(NullableEnumValue.Second, config.Value.Value);
    }

    [Fact]
    public void Load_KeepsDefaultNullableEnumValue_WhenJsonStringIsUnknown()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        File.WriteAllText(filePath, """
        {
          "nullableEnum.value": "UnknownValue"
        }
        """);

        using var store = new SettingsStore<NullableEnumConfig>(filePath);
        var config = store.Load();

        Assert.Equal(NullableEnumValue.First, config.Value.Value);
    }
}
