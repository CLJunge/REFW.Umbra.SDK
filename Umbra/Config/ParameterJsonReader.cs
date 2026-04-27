using System.Numerics;
using System.Text.Json;
using Umbra.Input;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Converts persisted JSON values into objects that can be applied to registered <see cref="IParameter"/> instances.
/// </summary>
/// <remarks>
/// <see cref="ConfigPersistence"/> uses this helper while loading a config file. Values are applied through <see cref="IParameter.SetValueWithoutNotify(object?)"/> so persisted-state restoration does not trigger change notifications.
/// </remarks>
internal static class ParameterJsonReader
{
    /// <summary>
    /// Converts <paramref name="element"/> to the value type expected by <paramref name="param"/> and applies it silently.
    /// </summary>
    /// <param name="param">The registered parameter that should receive the converted value.</param>
    /// <param name="element">The persisted JSON value to restore.</param>
    /// <remarks>
    /// When <see cref="ConvertElement"/> returns <see langword="null"/> for a non-null JSON element, the assignment is skipped so the parameter keeps its current in-memory value. This preserves declared defaults for stale or unrecognized persisted values such as renamed enum members.
    /// </remarks>
    internal static void Apply(IParameter param, JsonElement element)
    {
        var value = ConvertElement(element, param.ValueType);
        if (value is null && element.ValueKind != JsonValueKind.Null) return;
        param.SetValueWithoutNotify(value);
    }

    /// <summary>
    /// Attempts to convert <paramref name="element"/> to a value assignable to <paramref name="targetType"/>.
    /// </summary>
    /// <param name="element">The JSON element to convert.</param>
    /// <param name="targetType">The destination CLR type.</param>
    /// <param name="value">Receives the converted value when conversion succeeds.</param>
    /// <param name="failureReason">Receives a human-readable conversion failure reason when conversion fails.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    internal static bool TryConvert(
        JsonElement element,
        Type targetType,
        out object? value,
        out string? failureReason)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        value = null;
        failureReason = null;

        if (element.ValueKind == JsonValueKind.Null)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
            {
                failureReason = $"Null is not valid for non-nullable value type '{targetType.FullName ?? targetType.Name}'.";
                return false;
            }

            return true;
        }

        try
        {
            value = ConvertElement(element, targetType);
        }
        catch (Exception ex)
        {
            failureReason = ex.Message;
            value = null;
            return false;
        }

        if (value is not null)
            return true;

        failureReason = $"JSON value kind '{element.ValueKind}' is not compatible with '{targetType.FullName ?? targetType.Name}'.";
        return false;
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
            JsonValueKind.Object => ConvertObject(element, targetType),
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Keeps the type checks more readable and maintainable in a single block.")]
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

    /// <summary>
    /// Converts a JSON object element to the specified CLR type.
    /// Supports <see cref="Vector4"/> and <see cref="HotkeyBinding"/>.
    /// </summary>
    /// <param name="element">The JSON object element to convert.</param>
    /// <param name="t">The target CLR type.</param>
    /// <returns>The converted value, or <see langword="null"/> if the type is not supported.</returns>
    private static object? ConvertObject(JsonElement element, Type t)
    {
        return t == typeof(Vector4) || t == typeof(Vector4?)
            ? ConvertVector4(element)
            : t == typeof(HotkeyBinding) || t == typeof(HotkeyBinding?) ? ConvertHotkeyBinding(element) : (object?)null;
    }

    /// <summary>
    /// Reads a <see cref="Vector4"/> from a JSON object with <c>X</c>, <c>Y</c>, <c>Z</c>, <c>W</c> properties.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Keeps the property checks more readable and maintainable in a single block.")]
    private static Vector4? ConvertVector4(JsonElement element)
    {
        if (!element.TryGetProperty("X", out var xProp) && !element.TryGetProperty("x", out xProp))
            return null;
        if (!element.TryGetProperty("Y", out var yProp) && !element.TryGetProperty("y", out yProp))
            return null;
        if (!element.TryGetProperty("Z", out var zProp) && !element.TryGetProperty("z", out zProp))
            return null;
        if (!element.TryGetProperty("W", out var wProp) && !element.TryGetProperty("w", out wProp))
            return null;

        return new Vector4(xProp.GetSingle(), yProp.GetSingle(), zProp.GetSingle(), wProp.GetSingle());
    }

    /// <summary>
    /// Reads a <see cref="HotkeyBinding"/> from a JSON object with <c>Key</c>, <c>Ctrl</c>, <c>Shift</c>, <c>Alt</c> properties.
    /// </summary>
    private static HotkeyBinding? ConvertHotkeyBinding(JsonElement element)
    {
        if (!element.TryGetProperty("Key", out var keyProp) && !element.TryGetProperty("key", out keyProp))
            return null;

        var key = keyProp.GetInt32();
        var ctrl = TryGetBool(element, "Ctrl", "ctrl");
        var shift = TryGetBool(element, "Shift", "shift");
        var alt = TryGetBool(element, "Alt", "alt");

        return new HotkeyBinding(key, ctrl, shift, alt);
    }

    /// <summary>
    /// Tries to read a boolean property by Pascal-case or camelCase name, defaulting to <see langword="false"/>.
    /// </summary>
    private static bool TryGetBool(JsonElement element, string pascalName, string camelName)
        => (element.TryGetProperty(pascalName, out var prop) || element.TryGetProperty(camelName, out prop)) && prop.ValueKind == JsonValueKind.True;
}
