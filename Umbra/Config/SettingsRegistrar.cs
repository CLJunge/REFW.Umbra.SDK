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
    /// <see cref="AutoRegisterSettingsAttribute"/>.
    /// </returns>
    internal static Dictionary<string, IParameter> Register<TConfig>(TConfig config)
        where TConfig : class
    {
        var parameters = new Dictionary<string, IParameter>();
#pragma warning disable IDE0028 // Simplify collection initialization
        var rootType = config.GetType();
        RegisterRecursive(
            config,
            GetSettingsPrefix(rootType) ?? "",
            GetCategory(rootType),
            parameters,
            new HashSet<object>(ReferenceEqualityComparer.Instance));
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
    /// <para>
    /// Prefix resolution for nested groups uses a property-first strategy: if
    /// <see cref="SettingsPrefixAttribute"/> is present on the parent property, that value is
    /// used. If not, the attribute is looked up on the nested type itself (backwards-compatible
    /// fallback). Placing the prefix on the property is the preferred approach.
    /// </para>
    /// <para>
    /// Category resolution follows the same priority order: property-level
    /// <see cref="CategoryAttribute"/> wins over type-level.
    /// </para>
    /// </remarks>
    /// <param name="obj">The current object being inspected.</param>
    /// <param name="currentPrefix">
    /// The fully resolved dot-separated key prefix for <paramref name="obj"/>, already including
    /// the prefix segment selected for this branch of the object tree. For nested groups the
    /// segment is sourced from the parent property's <see cref="SettingsPrefixAttribute"/> when
    /// present, or from the nested type's attribute as a fallback.
    /// </param>
    /// <param name="currentCategory">
    /// The effective category inherited by child parameters of <paramref name="obj"/>, resolved
    /// from the parent property's <see cref="CategoryAttribute"/> when present, or from the
    /// nested type's attribute as a fallback.
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
        string currentPrefix,
        string? currentCategory,
        Dictionary<string, IParameter> parameters,
        HashSet<object> visited)
    {
        if (!visited.Add(obj)) return; // cycle guard — same instance seen twice, skip

        var type = obj.GetType();
        if (!Attribute.IsDefined(type, typeof(AutoRegisterSettingsAttribute)))
            return;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var paramAttr = prop.GetCustomAttribute<SettingsParameterAttribute>();
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
    /// Returns the <see cref="SettingsPrefixAttribute.Prefix"/> declared on <paramref name="member"/>,
    /// or <see langword="null"/> when the member declares no <see cref="SettingsPrefixAttribute"/>.
    /// </summary>
    /// <param name="member">The reflected property or type to inspect.</param>
    private static string? GetSettingsPrefix(MemberInfo member)
        => member.GetCustomAttribute<SettingsPrefixAttribute>()?.Prefix;

    /// <summary>
    /// Returns the <see cref="CategoryAttribute.Name"/> declared on <paramref name="member"/>, or
    /// <see langword="null"/> when the member declares no <see cref="CategoryAttribute"/>.
    /// </summary>
    /// <param name="member">The reflected property or type to inspect.</param>
    private static string? GetCategory(MemberInfo member)
        => member.GetCustomAttribute<CategoryAttribute>()?.Name;

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
