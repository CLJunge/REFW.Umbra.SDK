namespace Umbra;

/// <summary>
/// Provides string helpers used by Umbra naming and configuration key generation code.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Converts the first character of <paramref name="value"/> to lowercase to produce a camel-cased identifier.
    /// </summary>
    /// <param name="value">The identifier to convert.</param>
    /// <returns>The original value when it is <see langword="null"/>, empty, or already starts with a lowercase character; otherwise, a new string whose first character has been lowercased.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    internal static string? ToCamelCase(this string? value)
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
    /// Converts an identifier into a space-separated display label by inserting spaces before eligible uppercase transitions.
    /// </summary>
    /// <remarks>
    /// Uppercase characters that follow another uppercase character, or one of the separator characters recognized by <see cref="IsWordSeparator(char)"/>, do not trigger an inserted space.
    /// </remarks>
    /// <param name="name">The identifier to convert.</param>
    /// <returns>A display label built from <paramref name="name"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    internal static string ToDisplayName(this string name)
    {
        var insertedSpaces = 0;
        for (var i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]) && !IsWordSeparator(name[i - 1]))
                insertedSpaces++;
        }

        if (insertedSpaces == 0)
            return name;

        return string.Create(name.Length + insertedSpaces, name, static (span, source) =>
        {
            var writeIndex = 0;
            span[writeIndex++] = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                var current = source[i];
                if (char.IsUpper(current) && !char.IsUpper(source[i - 1]) && !IsWordSeparator(source[i - 1]))
                    span[writeIndex++] = ' ';
                span[writeIndex++] = current;
            }
        });
    }

    private static bool IsWordSeparator(char c) => c is '_' or '-' or '.' or '@';
}
