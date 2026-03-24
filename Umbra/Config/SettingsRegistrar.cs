using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config;

/// <summary>
/// Discovers and registers all <see cref="IParameter"/> instances declared on a configuration
/// object by walking its property tree and respecting the SDK settings attributes.
/// </summary>
internal static class SettingsRegistrar
{
    /// <summary>
    /// Reflects over <paramref name="config"/> and returns a flat dictionary of every
    /// <see cref="IParameter"/> found, keyed by its fully-qualified dot-separated setting key.
    /// </summary>
    /// <typeparam name="TConfig">The configuration class type to inspect.</typeparam>
    /// <param name="config">The configuration instance whose properties should be registered.</param>
    /// <returns>
    /// A dictionary mapping each discovered setting key to its corresponding <see cref="IParameter"/>.
    /// Returns an empty dictionary when <typeparamref name="TConfig"/> is not decorated with
    /// <see cref="Umbra.Config.Attributes.AutoRegisterSettingsAttribute"/>.
    /// </returns>
    internal static Dictionary<string, IParameter> Register<TConfig>(TConfig config)
        where TConfig : class
    {
        var parameters = new Dictionary<string, IParameter>();
#pragma warning disable IDE0028 // Simplify collection initialization
        RegisterRecursive(config, "", null, parameters, new HashSet<object>(ReferenceEqualityComparer.Instance));
#pragma warning restore IDE0028 // Simplify collection initialization
        return parameters;
    }

    /// <summary>
    /// Recursively walks the property tree of <paramref name="obj"/>, registering any
    /// <see cref="IParameter"/> properties annotated with <see cref="SettingsParameterAttribute"/>.
    /// Nested objects that are themselves decorated with <see cref="AutoRegisterSettingsAttribute"/>
    /// are traversed automatically.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <paramref name="obj"/>'s runtime type does not carry
    /// <see cref="AutoRegisterSettingsAttribute"/>, the method returns immediately without
    /// registering any parameters, and without logging a warning.
    /// </para>
    /// <para>
    /// <paramref name="visited"/> guards against object-graph cycles: if the same instance is
    /// encountered a second time the branch is skipped, preventing a <see cref="StackOverflowException"/>.
    /// </para>
    /// </remarks>
    /// <param name="obj">The current object being inspected.</param>
    /// <param name="parentPrefix">
    /// The dot-separated key prefix accumulated from ancestor objects,
    /// used to build fully-qualified setting keys.
    /// </param>
    /// <param name="parentCategory">
    /// The category name inherited from the nearest ancestor that declared a
    /// <see cref="CategoryAttribute"/>, or <see langword="null"/> if none has been encountered yet.
    /// </param>
    /// <param name="parameters">
    /// The dictionary that discovered <see cref="IParameter"/> instances are added to.
    /// </param>
    /// <param name="visited">
    /// The set of object instances already visited in the current walk, keyed by reference identity.
    /// Used to detect and short-circuit cycles in the configuration object graph.
    /// </param>
    private static void RegisterRecursive(
        object obj,
        string parentPrefix,
        string? parentCategory,
        Dictionary<string, IParameter> parameters,
        HashSet<object> visited)
    {
        if (!visited.Add(obj)) return; // cycle guard — same instance seen twice, skip

        var type = obj.GetType();
        if (!Attribute.IsDefined(type, typeof(AutoRegisterSettingsAttribute)))
            return;

        var prefixAttr = type.GetCustomAttribute<SettingsPrefixAttribute>();
        var currentPrefix = Combine(parentPrefix, prefixAttr?.Prefix ?? "");

        var categoryAttr = type.GetCustomAttribute<CategoryAttribute>();
        var currentCategory = categoryAttr?.Name ?? parentCategory;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var paramAttr = prop.GetCustomAttribute<SettingsParameterAttribute>();
            if (paramAttr == null) continue;

            var value = prop.GetValue(obj);
            if (value == null) continue;

            if (value is IParameter parameter)
            {
                var key = Combine(currentPrefix, paramAttr.KeyOverride ?? prop.Name.ToCamelCase());
                parameter.Key = key;
                parameter.Metadata = ParameterMetadataReader.ReadFrom(prop, currentCategory, key);
                parameters[key] = parameter;
            }
            else
            {
                RegisterRecursive(value, currentPrefix, currentCategory, parameters, visited);
            }
        }
    }

    /// <summary>
    /// Combines two dot-separated key segments into a single key, omitting the separator
    /// when either segment is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="a">The left-hand segment (prefix).</param>
    /// <param name="b">The right-hand segment (suffix).</param>
    /// <returns>
    /// <paramref name="b"/> if <paramref name="a"/> is empty;
    /// <paramref name="a"/> if <paramref name="b"/> is empty;
    /// otherwise <c>"a.b"</c>.
    /// </returns>
    private static string Combine(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b;
        if (string.IsNullOrEmpty(b)) return a;
        return $"{a}.{b}";
    }
}
