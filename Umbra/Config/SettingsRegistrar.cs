using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config;

/// <summary>
/// Discovers and registers all <see cref="IParameter"/> instances declared on a configuration
/// object by walking its public instance property tree and respecting the Umbra settings attributes.
/// </summary>
internal static class SettingsRegistrar
{
    /// <summary>
    /// Reflects over <paramref name="config"/> and returns a flat dictionary of every
    /// <see cref="IParameter"/> found, keyed by its fully-qualified dot-separated setting key.
    /// </summary>
    internal static Dictionary<string, IParameter> Register<TConfig>(TConfig config)
        where TConfig : class
    {
        var parameters = new Dictionary<string, IParameter>();
#pragma warning disable IDE0028
        var rootType = config.GetType();
        RegisterRecursive(
            config,
            GetSettingsPrefix(rootType) ?? "",
            GetCategory(rootType),
            parameters,
            new HashSet<object>(ReferenceEqualityComparer.Instance));
#pragma warning restore IDE0028
        return parameters;
    }

    /// <summary>
    /// Recursively walks the public instance property tree of <paramref name="obj"/>, registering any
    /// <see cref="IParameter"/> properties annotated with <see cref="UmbraSettingsParameterAttribute"/>.
    /// Nested objects that are themselves decorated with <see cref="UmbraAutoRegisterSettingsAttribute"/>
    /// are traversed automatically.
    /// </summary>
    private static void RegisterRecursive(
        object obj,
        string currentPrefix,
        string? currentCategory,
        Dictionary<string, IParameter> parameters,
        HashSet<object> visited)
    {
        if (!visited.Add(obj)) return;

        var type = obj.GetType();
        if (!Attribute.IsDefined(type, typeof(UmbraAutoRegisterSettingsAttribute)))
            return;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var paramAttr = prop.GetCustomAttribute<UmbraSettingsParameterAttribute>();
            if (paramAttr == null) continue;

            var value = prop.GetValue(obj);
            if (value == null) continue;

            if (value is IParameter parameter)
            {
                var key = Combine(currentPrefix, paramAttr.KeyOverride ?? prop.Name.ToCamelCase()!);
                parameter.Key = key;
                parameter.Metadata = ParameterMetadataReader.ReadFrom(prop, currentCategory, key);
                parameters[key] = parameter;
            }
            else
            {
                var nestedPrefix = Combine(currentPrefix, GetSettingsPrefix(prop) ?? GetSettingsPrefix(value.GetType()) ?? "");
                var nestedCategory = GetCategory(prop) ?? GetCategory(value.GetType()) ?? currentCategory;
                RegisterRecursive(value, nestedPrefix, nestedCategory, parameters, visited);
            }
        }
    }

    /// <summary>
    /// Returns the prefix declared on <paramref name="member"/>, or <see langword="null"/> when absent.
    /// </summary>
    private static string? GetSettingsPrefix(MemberInfo member)
        => member.GetCustomAttribute<UmbraSettingsPrefixAttribute>()?.Prefix;

    /// <summary>
    /// Returns the category declared on <paramref name="member"/>, or <see langword="null"/> when absent.
    /// </summary>
    private static string? GetCategory(MemberInfo member)
        => member.GetCustomAttribute<UmbraCategoryAttribute>()?.Name;

    /// <summary>
    /// Combines two dot-separated key segments into a single key, omitting the separator
    /// when either segment is <see langword="null"/> or empty.
    /// </summary>
    private static string Combine(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b;
        if (string.IsNullOrEmpty(b)) return a;
        return $"{a}.{b}";
    }
}
