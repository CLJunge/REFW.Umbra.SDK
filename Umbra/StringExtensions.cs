using System.Text;

namespace Umbra;

/// <summary>
/// Provides extension methods for working with strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts the first character of the string to lowercase, returning a camelCase form.
    /// If the string is <see langword="null"/>, empty, or already starts with a lowercase letter,
    /// it is returned unchanged.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>
    /// A new string with the first character converted to lowercase,
    /// or the original string if it is <see langword="null"/>, empty, or already starts with
    /// a lowercase character.
    /// </returns>
    public static string? ToCamelCase(this string? value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
            return value;

        return string.Create(value.Length, value, static (span, source) =>
        {
            source.AsSpan().CopyTo(span);
            span[0] = char.ToLowerInvariant(span[0]);
        });
    }

    /// <summary>
    /// Converts a PascalCase or camelCase identifier into a space-separated display name
    /// by inserting a space before each uppercase letter that follows a lowercase letter.
    /// </summary>
    /// <param name="name">The raw identifier to convert (e.g. <c>"FieldOfView"</c>).</param>
    /// <returns>The human-readable display name (e.g. <c>"Field Of View"</c>).</returns>
    public static string ToDisplayName(this string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }
}
