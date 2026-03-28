using System.Reflection;
using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class NullableEnumControlTests
{
    [Fact]
    public void BuildDrawAction_UsesNullableEnumCombo_WithNoneOption()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<NullableEnumConfig>(filePath);
        var config = store.Load();
        config.Value.Value = null;

        var groupType = typeof(Umbra.UI.Config.ConfigDrawer<>).Assembly.GetType("Umbra.UI.Config.LabelAlignmentGroup")!;
        var controlFactoryType = typeof(Umbra.UI.Config.ConfigDrawer<>).Assembly.GetType("Umbra.UI.Config.ControlFactory")!;
        var alignGroup = Activator.CreateInstance(groupType, nonPublic: true)!;
        var buildMethod = controlFactoryType.GetMethod(
            "BuildDrawAction",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        var result = buildMethod.Invoke(null, [config.Value, "Optional Enum", alignGroup])!;
        var action = (Action)result.GetType().GetField("Item1")!.GetValue(result)!;

        Assert.NotNull(action.Target);
        Assert.Contains("EnumControlBuilder", action.Target.GetType().FullName, StringComparison.Ordinal);

        var targetFields = action.Target.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        string[]? names = null;
        object?[]? values = null;

        foreach (var field in targetFields)
        {
            if (field.FieldType == typeof(string[]))
                names = (string[]?)field.GetValue(action.Target);
            else if (field.FieldType == typeof(object[]))
                values = (object?[]?)field.GetValue(action.Target);
        }

        Assert.NotNull(names);
        Assert.NotNull(values);
        Assert.Equal("<None>", names![0]);
        Assert.Null(values![0]);
        Assert.Contains("Second", names);
    }
}
