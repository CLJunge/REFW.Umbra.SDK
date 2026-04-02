using System.Linq.Expressions;
using System.Reflection;

namespace Umbra.UI.Config;

/// <summary>
/// Creates cached boxed property getters used during config draw-tree metadata construction.
/// </summary>
/// <remarks>
/// This type isolates expression compilation and reflection fallback behavior from
/// <see cref="TypeDrawMetadata"/> so metadata caching can remain focused on immutable data shape
/// and cache access.
/// </remarks>
internal static class PropertyGetterFactory
{
    /// <summary>
    /// Builds the cached boxed getter used during config-object traversal for a reflected property.
    /// </summary>
    /// <remarks>
    /// Expression compilation is performed once per cached property. If the expression path cannot be
    /// compiled, the returned delegate falls back to <see cref="PropertyInfo.GetValue(object?)"/> so
    /// config UI construction remains functional.
    /// </remarks>
    /// <param name="property">The reflected property whose getter should be cached.</param>
    /// <returns>A delegate that reads the property's current value from a boxed owner instance.</returns>
    internal static Func<object, object?> Create(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        try
        {
            var ownerParameter = Expression.Parameter(typeof(object), "owner");
            var typedOwner = Expression.Convert(ownerParameter, property.DeclaringType!);
            var propertyAccess = Expression.Property(typedOwner, property);
            var boxedValue = Expression.Convert(propertyAccess, typeof(object));

            return Expression.Lambda<Func<object, object?>>(boxedValue, ownerParameter).Compile();
        }
        catch
        {
            return property.GetValue;
        }
    }
}
