using Umbra.Config;
using Umbra.Config.Attributes;

namespace Umbra.Tests.TestConfigs;

[UmbraAutoRegisterSettings]
[UmbraSettingsPrefix("tests")]
internal record BasicConfig
{
    [UmbraSettingsParameter]
    public Parameter<bool> Enabled { get; set; } = new(true);

    [UmbraSettingsParameter]
    public Parameter<int> Count { get; set; } = new(5);
}

[UmbraAutoRegisterSettings]
[UmbraSettingsPrefix("dup")]
internal record DuplicateKeyConfig
{
    [UmbraSettingsParameter("same")]
    public Parameter<int> First { get; set; } = new(1);

    [UmbraSettingsParameter("same")]
    public Parameter<int> Second { get; set; } = new(2);
}

internal enum NullableEnumValue
{
    First,
    Second,
    Third
}

[UmbraAutoRegisterSettings]
[UmbraSettingsPrefix("nullableEnum")]
internal record NullableEnumConfig
{
    [UmbraSettingsParameter]
    public Parameter<NullableEnumValue?> Value { get; set; } = new(NullableEnumValue.First);
}
