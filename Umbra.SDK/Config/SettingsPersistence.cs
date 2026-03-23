using System.Text.Json;
using System.Text.Json.Serialization;
using Umbra.SDK.Logging;

namespace Umbra.SDK.Config;

/// <summary>
/// Handles reading and writing of plugin settings to and from a JSON file on disk.
/// </summary>
internal static class SettingsPersistence
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes the current value of every registered parameter and writes the result
    /// to the specified JSON file, overwriting any existing content.
    /// </summary>
    /// <param name="filePath">The absolute or relative path of the destination JSON file.</param>
    /// <param name="parameters">
    /// A read-only dictionary of all registered parameters, keyed by their unique setting key.
    /// </param>
    internal static void Save(string filePath, IReadOnlyDictionary<string, IParameter> parameters)
    {
        try
        {
            var dict = new Dictionary<string, object?>();
            foreach (var param in parameters.Values)
            {
                // Action-backed button parameters are never persisted — delegates are not
                // JSON-serializable and carry no meaningful state to save or restore.
                if (typeof(Delegate).IsAssignableFrom(param.ValueType)) continue;
                dict[param.Key] = param.GetValue();
            }

            File.WriteAllText(filePath, JsonSerializer.Serialize(dict, _jsonOptions));
            Logger.Info($"SettingsPersistence: saved {dict.Count} parameter(s) to '{filePath}'.");
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"SettingsPersistence: failed to save settings to '{filePath}'.");
        }
    }

    /// <summary>
    /// Reads the specified JSON file and applies the persisted values to the matching registered parameters.
    /// Parameters whose keys are not present in the file are left at their current (default) values.
    /// </summary>
    /// <param name="filePath">The absolute or relative path of the source JSON file.</param>
    /// <param name="parameters">
    /// A read-only dictionary of all registered parameters, keyed by their unique setting key.
    /// </param>
    /// <remarks>
    /// Values are applied via <see cref="IParameter.SetValueWithoutNotify"/>; no
    /// <see cref="IParameter.ValueChanged"/> events are raised during load.
    /// </remarks>
    internal static void Load(string filePath, IReadOnlyDictionary<string, IParameter> parameters)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonOptions);
            if (dict == null) return;

            var applied = 0;
            foreach (var (key, element) in dict)
            {
                if (!parameters.TryGetValue(key, out var param)) continue;
                ParameterJsonReader.Apply(param, element);
                applied++;
            }

            Logger.Info($"SettingsPersistence: loaded {applied} of {dict.Count} key(s) from '{filePath}'.");
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"SettingsPersistence: failed to load settings from '{filePath}'.");
        }
    }
}
