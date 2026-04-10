using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.Config;

/// <summary>
/// Discovers and registers the <see cref="IParameter"/> instances exposed by a configuration object graph.
/// </summary>
/// <remarks>
/// The registrar walks public instance properties marked with <see cref="UmbraParameterAttribute"/>, respects nested-group prefixes and inherited categories, and assigns each discovered parameter its fully qualified persisted key together with resolved <see cref="ParameterMetadata"/>.
/// </remarks>
internal static class ConfigRegistrar
{
    /// <summary>
    /// Walks <paramref name="config"/> and returns a flat map of every discovered parameter keyed by fully qualified persisted name.
    /// </summary>
    /// <typeparam name="TConfig">The root configuration object type.</typeparam>
    /// <param name="config">The configuration object to inspect.</param>
    /// <returns>The registered parameter map.</returns>
    /// <remarks>
    /// If two parameters resolve to the same fully qualified key, registration fails instead of allowing the later parameter to overwrite the earlier one.
    /// </remarks>
    internal static Dictionary<string, IParameter> Register<TConfig>(TConfig config)
        where TConfig : class
    {
        var parameters = new Dictionary<string, IParameter>();
        var parameterOrigins = new Dictionary<string, string>();
        var rootType = config.GetType();
        RegisterRecursive(
            config,
            GetPrefix(rootType) ?? "",
            GetCategory(rootType),
            parameters,
            parameterOrigins,
#pragma warning disable IDE0028
            new HashSet<object>(ReferenceEqualityComparer.Instance));
#pragma warning restore IDE0028
        return parameters;
    }

    /// <summary>
    /// Recursively walks the public instance property tree of <paramref name="obj"/>, registering any
    /// <see cref="IParameter"/> properties annotated with <see cref="UmbraParameterAttribute"/>.
    /// Nested objects that are themselves decorated with <see cref="UmbraAutoRegisterAttribute"/>
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
        if (!Attribute.IsDefined(type, typeof(UmbraAutoRegisterAttribute)))
            return;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var paramAttr = prop.GetCustomAttribute<UmbraParameterAttribute>();
            if (paramAttr == null) continue;

            var value = prop.GetValue(obj);
            if (value == null) continue;

            if (value is IParameter parameter)
            {
                var key = Combine(currentPrefix, GetParameterKeySegment(prop, paramAttr));
                RegisterParameter(parameters, parameterOrigins, parameter, key, prop, currentCategory);
            }
            else
            {
                var nestedPrefix = Combine(currentPrefix, GetNestedPrefixSegment(prop, value.GetType()));
                var nestedCategory = GetCategory(prop) ?? GetCategory(value.GetType()) ?? currentCategory;
                RegisterRecursive(value, nestedPrefix, nestedCategory, parameters, parameterOrigins, visited);
            }
        }
    }

    /// <summary>
    /// Resolves the persisted key segment for a parameter property.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="UmbraParameterAttribute.KeyOverride"/> is explicitly set to an
    /// empty string.
    /// </exception>
    private static string GetParameterKeySegment(PropertyInfo property, UmbraParameterAttribute parameterAttribute)
    {
        if (parameterAttribute.KeyOverride is not null)
            return RequireNonEmptySegment(parameterAttribute.KeyOverride, property, "[UmbraParameter] key override");

        return property.Name.ToCamelCase() ?? property.Name;
    }

    /// <summary>
    /// Resolves the nested-group prefix segment contributed by a property or nested type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a nested-group <see cref="UmbraPrefixAttribute"/> is explicitly set to an
    /// empty string.
    /// </exception>
    private static string GetNestedPrefixSegment(PropertyInfo property, Type nestedType)
    {
        var propertyPrefix = GetPrefix(property);
        if (propertyPrefix is not null)
            return RequireNonEmptySegment(propertyPrefix, property, "[UmbraPrefix] on the nested-group property");

        var typePrefix = GetPrefix(nestedType);
        if (typePrefix is not null)
            return RequireNonEmptySegment(typePrefix, nestedType, "[UmbraPrefix] on the nested-group type");

        return "";
    }

    /// <summary>
    /// Registers one discovered parameter under <paramref name="key"/>, throwing when that key
    /// is already occupied by another parameter in the same configuration tree.
    /// </summary>
    /// <param name="parameters">The destination parameter map keyed by fully-qualified key.</param>
    /// <param name="parameterOrigins">
    /// Tracks the declaring property path for each registered key so duplicate-key failures can
    /// identify both colliding members.
    /// </param>
    /// <param name="parameter">The discovered parameter instance to register.</param>
    /// <param name="key">The fully-qualified key resolved for the parameter.</param>
    /// <param name="declaringProperty">The property that exposed the parameter.</param>
    /// <param name="currentCategory">The resolved category context applied to the parameter metadata.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="key"/> is already registered by a different parameter, or when
    /// <paramref name="parameter"/> does not support Umbra's registration-time identity assignment.
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
                $"Duplicate parameter key '{key}' detected while registering '{origin}'. " +
                $"The key is already used by '{existingOrigin}'. Ensure every [UmbraParameter] resolves to a unique key.");
        }

        if (parameter is not IParameterRegistration registration)
        {
            throw new InvalidOperationException(
                $"Parameter instance '{parameter.GetType().FullName ?? parameter.GetType().Name}' does not support Umbra registration.");
        }

        registration.Key = key;
        registration.Metadata = ParameterMetadataReader.ReadFrom(declaringProperty, currentCategory, key);
        parameters.Add(key, parameter);
        parameterOrigins.Add(key, origin);
    }

    /// <summary>
    /// Returns the prefix declared on <paramref name="member"/>, or <see langword="null"/> when absent.
    /// </summary>
    private static string? GetPrefix(MemberInfo member)
        => member.GetCustomAttribute<UmbraPrefixAttribute>()?.Prefix;

    /// <summary>
    /// Returns the category declared on <paramref name="member"/>, or <see langword="null"/> when absent.
    /// </summary>
    private static string? GetCategory(MemberInfo member)
        => member.GetCustomAttribute<UmbraCategoryAttribute>()?.Name;

    /// <summary>
    /// Verifies that an explicitly configured path segment is non-empty.
    /// </summary>
    /// <param name="segment">The configured segment value.</param>
    /// <param name="member">The member that supplied the segment.</param>
    /// <param name="source">The attribute source used in the error message.</param>
    /// <returns><paramref name="segment"/> when it is non-empty.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="segment"/> is an empty string.
    /// </exception>
    private static string RequireNonEmptySegment(string segment, MemberInfo member, string source)
    {
        if (!string.IsNullOrEmpty(segment))
            return segment;

        var memberName = member.DeclaringType is not null
            ? $"{member.DeclaringType.FullName}.{member.Name}"
            : member is Type type
                ? type.FullName ?? type.Name
                : member.Name;

        throw new InvalidOperationException(
            $"Member '{memberName}' declares {source} as an empty string. Config identifier segments must be non-empty.");
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
