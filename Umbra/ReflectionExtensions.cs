using System.Reflection;

namespace Umbra;

/// <summary>
/// Provides reflection extension methods for inspecting custom attributes on
/// <see cref="MemberInfo"/> and <see cref="PropertyInfo"/> instances.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Searches the custom attributes on <paramref name="member"/> for an attribute whose
    /// runtime type is a closed generic constructed from <paramref name="genericType"/>.
    /// </summary>
    /// <param name="member">The member whose custom attributes are inspected.</param>
    /// <param name="genericType">
    /// The open generic type definition to match (e.g. <c>typeof(HideIfAttribute&lt;&gt;)</c>).
    /// </param>
    /// <returns>
    /// The first matching attribute instance, or <see langword="null"/> when no match is found.
    /// </returns>
    public static Attribute? GetCustomGenericAttribute(this MemberInfo member, Type genericType)
    {
        foreach (var a in member.GetCustomAttributes(false))
        {
            if (a is not Attribute candidate) continue;
            var t = candidate.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == genericType)
                return candidate;
        }

        return null;
    }

    /// <summary>
    /// Returns the first custom attribute on <paramref name="property"/> that is assignable to
    /// <typeparamref name="T"/>, or <see langword="null"/> when no match is found.
    /// </summary>
    /// <remarks>
    /// Use this to locate interface-typed attributes (e.g. <see cref="Config.Attributes.ICustomDrawerAttribute"/>)
    /// without knowing the concrete generic type argument at the call site.
    /// </remarks>
    /// <typeparam name="T">The attribute type or interface to search for. Must be a reference type.</typeparam>
    /// <param name="property">The property whose custom attributes are inspected.</param>
    /// <returns>
    /// The first attribute instance that is assignable to <typeparamref name="T"/>,
    /// or <see langword="null"/> when no such attribute is present.
    /// </returns>
    public static T? GetDrawerAttribute<T>(this PropertyInfo property) where T : class
    {
        foreach (var a in property.GetCustomAttributes(false))
        {
            if (a is T ca)
                return ca;
        }

        return null;
    }

    /// <summary>
    /// Returns the first custom attribute on <paramref name="type"/> that is assignable to
    /// <typeparamref name="T"/>, or <see langword="null"/> when no match is found.
    /// </summary>
    /// <remarks>
    /// Use this to locate interface-typed class-level attributes (e.g.
    /// <see cref="Config.Attributes.INestedGroupDrawerAttribute"/>) on a configuration group
    /// type without knowing the concrete generic type argument at the call site.
    /// </remarks>
    /// <typeparam name="T">The attribute type or interface to search for. Must be a reference type.</typeparam>
    /// <param name="type">The type whose custom attributes are inspected.</param>
    /// <returns>
    /// The first attribute instance that is assignable to <typeparamref name="T"/>,
    /// or <see langword="null"/> when no such attribute is present.
    /// </returns>
    public static T? GetDrawerAttribute<T>(this Type type) where T : class
    {
        foreach (var a in type.GetCustomAttributes(false))
        {
            if (a is T ca)
                return ca;
        }

        return null;
    }
}
