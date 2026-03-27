using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config;

#pragma warning disable CS0618 // Registrar must continue supporting legacy unprefixed attributes for backwards compatibility.

/// <summary>
/// Discovers and registers all <see cref="IParameter"/> instances declared on a configuration
/// object by walking its public instance property tree and respecting the SDK settings attributes.
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
    /// an auto-register settings attribute.
    /// </returns>
    /// <remarks>
    /// Only public instance properties participate in the built-in discovery walk.
    /// Fields are ignored even if they carry settings metadata attributes.
    /// </remarks>
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
    /// Recursively walks the public instance property tree of <paramref name="obj"/>, registering any
    /// <see cref="IParameter"/> properties annotated with a settings-parameter attribute.
    /// Nested objects that are themselves decorated with an auto-register settings attribute
    /// are traversed automatically.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <paramref name="obj"/>'s runtime type does not carry an auto-register settings attribute,
    /// the method returns immediately without registering any parameters, and without logging a warning.
    /// </para>
    /// <para>
    /// <paramref name="visited"/> guards against object-graph cycles: if the same instance is
    /// encountered a second time the branch is skipped, preventing a <see cref="StackOverflowException"/>.
    /// </para>
    /// <para>
    /// Prefix resolution for nested groups uses a property-first strategy: if a settings-prefix
    /// attribute is present on the parent property, that value is used. If not, the attribute is
    /// looked up on the nested type itself (backwards-compatible fallback). Placing the prefix on
    /// the property is the preferred approach.
    /// </para>
    /// <para>
    /// Category resolution follows the same priority order: property-level category metadata wins
    /// over type-level.
    /// </para>
    /// </remarks>
    private static void RegisterRecursive(
        object obj,
        string currentPrefix,
        string? currentCategory,
        Dictionary<string, IParameter> parameters,
        HashSet<object> visited)
    {
        if (!visited.Add(obj)) return;

        var type = obj.GetType();
        if (!HasAutoRegisterSettings(type))
            return;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var keyOverride = GetSettingsParameterKeyOverride(prop);
            if (keyOverride == MissingSettingsParameterSentinel)
                continue;

            var value = prop.GetValue(obj);
            if (value == null) continue;

            if (value is IParameter parameter)
            {
                var key = Combine(currentPrefix, keyOverride ?? prop.Name.ToCamelCase()!);
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

    private const string MissingSettingsParameterSentinel = "\u0000";

    /// <summary>
    /// Gets whether <paramref name="member"/> is decorated with either the legacy or the
    /// preferred Umbra-prefixed auto-register settings attribute.
    /// </summary>
    private static bool HasAutoRegisterSettings(MemberInfo member)
    {
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            if (attr is AutoRegisterSettingsAttribute or UmbraAutoRegisterSettingsAttribute)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the settings-parameter key override for <paramref name="member"/>, or a sentinel
    /// when the member is not marked as a settings parameter.
    /// </summary>
    private static string? GetSettingsParameterKeyOverride(MemberInfo member)
    {
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            switch (attr)
            {
                case SettingsParameterAttribute legacy:
                    return legacy.KeyOverride;
                case UmbraSettingsParameterAttribute prefixed:
                    return prefixed.KeyOverride;
            }
        }

        return MissingSettingsParameterSentinel;
    }

    /// <summary>
    /// Returns the prefix declared on <paramref name="member"/>, or <see langword="null"/> when none is present.
    /// </summary>
    private static string? GetSettingsPrefix(MemberInfo member)
    {
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            switch (attr)
            {
                case SettingsPrefixAttribute legacy:
                    return legacy.Prefix;
                case UmbraSettingsPrefixAttribute prefixed:
                    return prefixed.Prefix;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the category declared on <paramref name="member"/>, or <see langword="null"/> when none is present.
    /// </summary>
    private static string? GetCategory(MemberInfo member)
    {
        foreach (var attr in member.GetCustomAttributes(inherit: false))
        {
            switch (attr)
            {
                case CategoryAttribute legacy:
                    return legacy.Name;
                case UmbraCategoryAttribute prefixed:
                    return prefixed.Name;
            }
        }

        return null;
    }

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

#pragma warning restore CS0618
