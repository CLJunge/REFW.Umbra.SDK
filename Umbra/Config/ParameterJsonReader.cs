using System.Text.Json;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Provides internal utilities for deserializing <see cref="JsonElement"/> values
/// into the concrete types expected by <see cref="IParameter"/> instances.
/// </summary>
internal static class ParameterJsonReader
{
    /// <summary>
    /// Reads the value from <paramref name="element"/> and silently applies it to
    /// <paramref name="param"/> without raising <see cref="IParameter.ValueChanged"/>.
    /// </summary>
    /// <remarks>
    /// When <see cref="ConvertElement"/> returns <see langword="null"/> for a non-null JSON element
    /// (e.g. an unrecognised enum string), the assignment is skipped entirely so the parameter
    /// retains its default value rather than being overwritten with <see langword="null"/>.
    /// </remarks>
    /// <param name="param">The parameter to receive the deserialized value.</param>
    /// <param name="element">
    /// The <see cref="JsonElement"/> containing the persisted value to restore.
    /// </param>
    internal static void Apply(IParameter param, JsonElement element)
    {
        var value = ConvertElement(element, param.ValueType);
        // A null result from ConvertElement for a non-null element means the stored value
        // was unrecognised (e.g. a stale/renamed enum member). Skip the assignment so the
        // parameter keeps its in-memory default instead of being set to null.
        if (value is null && element.ValueKind != JsonValueKind.Null) return;
        param.SetValueWithoutNotify(value);
    }

    /// <summary>
    /// Converts a <see cref="JsonElement"/> to an object of <paramref name="targetType"/>,
    /// dispatching to the appropriate typed converter based on the element's
    /// <see cref="JsonValueKind"/>.
    /// </summary>
    /// <param name="element">The JSON element to convert.</param>
    /// <param name="targetType">The CLR type the element should be converted to.</param>
    /// <returns>
    /// The converted value, or <see langword="null"/> if the element is
    /// <see cref="JsonValueKind.Null"/> or the kind is not supported.
    /// </returns>
    private static object? ConvertElement(JsonElement element, Type targetType)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Number => ConvertNumber(element, targetType),
            JsonValueKind.True or JsonValueKind.False
                when targetType == typeof(bool) || targetType == typeof(bool?)
                => element.GetBoolean(),
            JsonValueKind.String => ConvertString(element, targetType),
            _ => null
        };
    }

    /// <summary>
    /// Converts a numeric <see cref="JsonElement"/> to the specified numeric CLR type.
    /// Supports <see cref="int"/>, <see cref="long"/>, <see cref="float"/>, <see cref="double"/>,
    /// <see cref="uint"/>, <see cref="short"/>, <see cref="byte"/>, and their nullable counterparts.
    /// Falls back to <see cref="double"/> for any unrecognised numeric type.
    /// </summary>
    /// <param name="element">The numeric JSON element to convert.</param>
    /// <param name="t">The target CLR numeric type.</param>
    /// <returns>The converted numeric value as an <see cref="object"/>.</returns>
    private static object? ConvertNumber(JsonElement element, Type t)
    {
        if (t == typeof(int) || t == typeof(int?)) return element.GetInt32();
        if (t == typeof(float) || t == typeof(float?)) return element.GetSingle();
        if (t == typeof(double) || t == typeof(double?)) return element.GetDouble();
        if (t == typeof(long) || t == typeof(long?)) return element.GetInt64();
        if (t == typeof(uint) || t == typeof(uint?)) return element.GetUInt32();
        if (t == typeof(short) || t == typeof(short?)) return (short)element.GetInt32();
        if (t == typeof(byte) || t == typeof(byte?)) return element.GetByte();
        return element.GetDouble();
    }

    /// <summary>
    /// Converts a string <see cref="JsonElement"/> to the specified CLR type.
    /// Supports both <see cref="Enum"/> and nullable-enum target types via
    /// <see cref="Enum.TryParse(Type, string, bool, out object)"/> and falls back to the raw
    /// <see cref="string"/> value for all other types.
    /// </summary>
    /// <param name="element">The string JSON element to convert.</param>
    /// <param name="t">The target CLR type.</param>
    /// <returns>
    /// An <see cref="Enum"/> value if <paramref name="t"/> is an enum type (or nullable enum type)
    /// and the string matches a defined member name (case-insensitive); <see langword="null"/>
    /// when the string does not match any member; otherwise the raw <see cref="string"/> value.
    /// </returns>
    private static object? ConvertString(JsonElement element, Type t)
    {
        var raw = element.GetString();
        var enumType = Nullable.GetUnderlyingType(t) ?? t;
        if (enumType.IsEnum)
        {
            if (Enum.TryParse(enumType, raw, ignoreCase: true, out var parsed))
                return parsed;

            Logger.Warning($"ParameterJsonReader: unrecognised enum value '{raw}' for '{enumType.Name}', keeping default.");
            return null;
        }
        return raw;
    }
}
