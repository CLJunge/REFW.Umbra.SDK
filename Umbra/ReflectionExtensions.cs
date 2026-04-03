using System.Reflection;

namespace Umbra;

/// <summary>
/// Provides reflection helpers used by Umbra's attribute-driven configuration and UI discovery code.
/// </summary>
internal static class ReflectionExtensions
{
    /// <summary>
    /// Returns the first custom attribute on <paramref name="member"/> whose runtime type is a closed generic constructed from <paramref name="genericType"/>.
    /// </summary>
    /// <param name="member">The member whose custom attributes are inspected.</param>
    /// <param name="genericType">The open generic type definition to match.</param>
    /// <returns>The first matching attribute instance, or <see langword="null"/> if no match is found.</returns>
    internal static Attribute? GetCustomGenericAttribute(this MemberInfo member, Type genericType)
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
    /// Returns the first custom attribute on <paramref name="property"/> that is assignable to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Umbra uses this helper to locate interface-typed property attributes such as <see cref="Config.Attributes.IDrawerAttribute"/> without knowing the concrete generic attribute type at the call site.
    /// </remarks>
    /// <typeparam name="T">The attribute type or attribute interface to search for.</typeparam>
    /// <param name="property">The property whose custom attributes are inspected.</param>
    /// <returns>The first matching attribute instance, or <see langword="null"/> if no such attribute is present.</returns>
    internal static T? GetDrawerAttribute<T>(this PropertyInfo property) where T : class
    {
        foreach (var a in property.GetCustomAttributes(false))
        {
            if (a is T ca)
                return ca;
        }

        return null;
    }

    /// <summary>
    /// Returns the first custom attribute on <paramref name="type"/> that is assignable to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Umbra uses this helper to locate interface-typed class-level attributes such as <see cref="Config.Attributes.INestedDrawerAttribute"/> without knowing the concrete generic attribute type at the call site.
    /// </remarks>
    /// <typeparam name="T">The attribute type or attribute interface to search for.</typeparam>
    /// <param name="type">The type whose custom attributes are inspected.</param>
    /// <returns>The first matching attribute instance, or <see langword="null"/> if no such attribute is present.</returns>
    internal static T? GetDrawerAttribute<T>(this Type type) where T : class
    {
        foreach (var a in type.GetCustomAttributes(false))
        {
            if (a is T ca)
                return ca;
        }

        return null;
    }
}
