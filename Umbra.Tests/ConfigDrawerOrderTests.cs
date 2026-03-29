using System.Collections;
using System.Reflection;
using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Umbra.UI.Config;
using Xunit;

namespace Umbra.Tests;

public sealed class ConfigDrawerOrderTests
{
    [Fact]
    public void Constructor_OrdersParametersByAttribute_WithinRootScope()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "root-order.json");

        using var store = new SettingsStore<RootParameterOrderConfig>(filePath);
        var config = store.Load();
        var keys = GetParameterKeysInDrawOrder(config);

        Assert.Equal(
            [
                "rootOrder.orderedFirst",
                "rootOrder.orderedSecond",
                "rootOrder.unorderedFirst",
                "rootOrder.unorderedSecond"
            ],
            keys);
    }

    [Fact]
    public void Constructor_OrdersParametersByAttribute_WithinNestedUncategorizedScope()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "nested-order.json");

        using var store = new SettingsStore<NestedParameterOrderConfig>(filePath);
        var config = store.Load();
        var keys = GetParameterKeysInDrawOrder(config);

        Assert.Equal(
            [
                "nestedOrder.nested.orderedFirst",
                "nestedOrder.nested.orderedSecond",
                "nestedOrder.nested.unorderedFirst",
                "nestedOrder.nested.unorderedSecond"
            ],
            keys);
    }

    private static List<string> GetParameterKeysInDrawOrder<TConfig>(TConfig config)
        where TConfig : class, new()
    {
        using var drawer = new ConfigDrawer<TConfig>(config, "TestScope");
        var nodesField = typeof(ConfigDrawer<TConfig>).GetField("_nodes", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(nodesField);
        var nodes = Assert.IsType<IList>(nodesField!.GetValue(drawer), exactMatch: false);
        var keys = new List<string>();

        for (var i = 0; i < nodes.Count; i++)
            CollectParameterKeys(nodes[i]!, keys);

        return keys;
    }

    private static void CollectParameterKeys(object node, List<string> keys)
    {
        if (node.GetType().FullName == "Umbra.UI.Config.Nodes.ParameterNode")
        {
            keys.Add(GetParameterKey(node));
            return;
        }

        if (TryGetChildNodes(node) is not { } childNodes)
            return;

        for (var i = 0; i < childNodes.Count; i++)
            CollectParameterKeys(childNodes[i]!, keys);
    }

    private static IList? TryGetChildNodes(object node)
    {
        var type = node.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];

            if (property.GetIndexParameters().Length != 0 || !IsDrawNodeListType(property.PropertyType))
                continue;

            if (property.GetValue(node) is IList list)
                return list;
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (!IsDrawNodeListType(field.FieldType))
                continue;

            var value = field.GetValue(node);
            if (value is IList list)
                return list;
        }

        return null;
    }

    private static bool IsDrawNodeListType(Type type)
    {
        if (!typeof(IList).IsAssignableFrom(type) || !type.IsGenericType)
            return false;

        var arguments = type.GetGenericArguments();
        return arguments.Length == 1 && arguments[0].FullName == "Umbra.UI.Config.Nodes.IDrawNode";
    }

    private static string GetParameterKey(object parameterNode)
    {
        var fields = parameterNode.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        for (var i = 0; i < fields.Length; i++)
        {
            if (fields[i].GetValue(parameterNode) is not Action draw)
                continue;

#pragma warning disable IDE0028
            var key = FindCapturedParameterKey(draw.Target, new HashSet<object>(ReferenceEqualityComparer.Instance));
#pragma warning restore IDE0028
            if (key is not null)
                return key;
        }

        throw new Xunit.Sdk.XunitException("Could not resolve the parameter key from the parameter node draw action.");
    }

    private static string? FindCapturedParameterKey(object? value, HashSet<object> visited)
    {
        if (value is null)
            return null;

        if (value is IParameter parameter)
            return parameter.Key;

        if (value is string)
            return null;

        var type = value.GetType();
        if (type.IsPrimitive || type.IsEnum)
            return null;

        if (!visited.Add(value))
            return null;

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        for (var i = 0; i < fields.Length; i++)
        {
            var key = FindCapturedParameterKey(fields[i].GetValue(value), visited);
            if (key is not null)
                return key;
        }

        return null;
    }
}
