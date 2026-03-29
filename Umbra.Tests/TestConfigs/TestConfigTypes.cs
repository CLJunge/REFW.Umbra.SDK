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

[UmbraAutoRegisterSettings]
[UmbraSettingsPrefix("rootOrder")]
internal record RootParameterOrderConfig
{
    [UmbraSettingsParameter]
    public Parameter<int> UnorderedFirst { get; set; } = new(10);

    [UmbraSettingsParameter]
    [UmbraParameterOrder(1)]
    public Parameter<int> OrderedSecond { get; set; } = new(20);

    [UmbraSettingsParameter]
    public Parameter<int> UnorderedSecond { get; set; } = new(30);

    [UmbraSettingsParameter]
    [UmbraParameterOrder(0)]
    public Parameter<int> OrderedFirst { get; set; } = new(40);
}

[UmbraAutoRegisterSettings]
[UmbraSettingsPrefix("nestedOrder")]
internal record NestedParameterOrderConfig
{
    [UmbraSettingsParameter]
    [UmbraSettingsPrefix("nested")]
    public NestedParameterOrderGroup Nested { get; set; } = new();
}

[UmbraAutoRegisterSettings]
internal record NestedParameterOrderGroup
{
    [UmbraSettingsParameter]
    public Parameter<int> UnorderedFirst { get; set; } = new(10);

    [UmbraSettingsParameter]
    [UmbraParameterOrder(1)]
    public Parameter<int> OrderedSecond { get; set; } = new(20);

    [UmbraSettingsParameter]
    public Parameter<int> UnorderedSecond { get; set; } = new(30);

    [UmbraSettingsParameter]
    [UmbraParameterOrder(0)]
    public Parameter<int> OrderedFirst { get; set; } = new(40);
}
