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
    /// <remarks>
    /// If two parameters resolve to the same fully-qualified key, registration fails with an
    /// <see cref="InvalidOperationException"/> rather than silently letting the later parameter
    /// overwrite the earlier one.
    /// </remarks>
    internal static Dictionary<string, IParameter> Register<TConfig>(TConfig config)
        where TConfig : class
    {
        var parameters = new Dictionary<string, IParameter>();
        var parameterOrigins = new Dictionary<string, string>();
#pragma warning disable IDE0028
        var rootType = config.GetType();
        RegisterRecursive(
            config,
            GetSettingsPrefix(rootType) ?? "",
            GetCategory(rootType),
            parameters,
            parameterOrigins,
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
        Dictionary<string, string> parameterOrigins,
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
                RegisterParameter(parameters, parameterOrigins, parameter, key, prop, currentCategory);
            }
            else
            {
                var nestedPrefix = Combine(currentPrefix, GetSettingsPrefix(prop) ?? GetSettingsPrefix(value.GetType()) ?? "");
                var nestedCategory = GetCategory(prop) ?? GetCategory(value.GetType()) ?? currentCategory;
                RegisterRecursive(value, nestedPrefix, nestedCategory, parameters, parameterOrigins, visited);
            }
        }
    }

    /// <summary>
    /// Registers one discovered parameter under <paramref name="key"/>, throwing when that key
    /// is already occupied by another parameter in the same configuration tree.
    /// </summary>
    /// <param name="parameters">The destination parameter map keyed by fully-qualified setting key.</param>
    /// <param name="parameterOrigins">
    /// Tracks the declaring property path for each registered key so duplicate-key failures can
    /// identify both colliding members.
    /// </param>
    /// <param name="parameter">The discovered parameter instance to register.</param>
    /// <param name="key">The fully-qualified settings key resolved for the parameter.</param>
    /// <param name="declaringProperty">The property that exposed the parameter.</param>
    /// <param name="currentCategory">The resolved category context applied to the parameter metadata.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="key"/> is already registered by a different parameter.
    /// </exception>
    private static void RegisterParameter(
        Dictionary<string, IParameter> parameters,
        Dictionary<string, string> parameterOrigins,
        IParameter parameter,
        string key,
        PropertyInfo declaringProperty,
        string? currentCategory)
    {
        var origin = $"{declaringProperty.DeclaringType?.FullName ?? declaringProperty.ReflectedType?.FullName ?? "<unknown>"}.{declaringProperty.Name}";
        if (parameterOrigins.TryGetValue(key, out var existingOrigin))
        {
            throw new InvalidOperationException(
                $"Duplicate settings key '{key}' detected while registering '{origin}'. " +
                $"The key is already used by '{existingOrigin}'. Ensure every [UmbraSettingsParameter] resolves to a unique key.");
        }

        parameter.Key = key;
        parameter.Metadata = ParameterMetadataReader.ReadFrom(declaringProperty, currentCategory, key);
        parameters.Add(key, parameter);
        parameterOrigins.Add(key, origin);
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
