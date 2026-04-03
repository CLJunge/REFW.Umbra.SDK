using System.Linq.Expressions;
using System.Reflection;

namespace Umbra.UI.Config;

/// <summary>
/// Builds the cached boxed property getters used while traversing configuration objects.
/// </summary>
/// <remarks>
/// Expression compilation is attempted once per property. When compilation fails, the returned delegate falls back to <see cref="PropertyInfo.GetValue(object?)"/> so configuration drawer construction remains functional.
/// </remarks>
internal static class PropertyGetterFactory
{
    /// <summary>
    /// Creates the cached boxed getter for <paramref name="property"/>.
    /// </summary>
    /// <param name="property">The reflected property whose value should be read from boxed owner instances.</param>
    /// <returns>A delegate that returns the property's current value.</returns>
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
