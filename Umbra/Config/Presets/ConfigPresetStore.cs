using System.Text.Json;
using Umbra.Logging;
using Umbra.UI.Toast;

namespace Umbra.Config.Presets;

/// <summary>
/// Manages named config presets for a loaded <see cref="ConfigStore{TConfig}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Presets are persisted as JSON files in the same directory as the main config file, using
/// the naming convention <c>config-preset-{name}.json</c>. Each preset file contains a flat
/// dictionary of parameter keys to values, mirroring the format used by
/// <see cref="ConfigPersistence"/>.
/// </para>
/// <para>
/// Delegate-typed parameters are excluded from presets because they do not represent
/// persisted state.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration class managed by the store.</typeparam>
public sealed class ConfigPresetStore<TConfig>
    where TConfig : class, new()
{
    private const string PresetPrefix = "config-preset-";
    private const string PresetExtension = ".json";

    private readonly IReadOnlyDictionary<string, IParameter> _parameters;
    private readonly string _presetDirectory;

    /// <summary>
    /// Initializes a new preset store that operates on the specified loaded config store.
    /// </summary>
    /// <param name="store">
    /// A loaded <see cref="ConfigStore{TConfig}"/>. Must be loaded and not disposed.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="store"/> has not been loaded.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="store"/> has been disposed.</exception>
    public ConfigPresetStore(ConfigStore<TConfig> store)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (!store.IsLoaded)
            throw new InvalidOperationException("The config store must be loaded before creating a preset store.");

        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store), "The config store has been disposed.");

        var target = (IConfigStoreCopyTarget<TConfig>)store;
        _parameters = target.Parameters;

        var configDir = Path.GetDirectoryName(Path.GetFullPath(store.FilePath));
        _presetDirectory = configDir ?? ".";
    }

    /// <summary>
    /// Saves the current parameter values as a named preset.
    /// </summary>
    /// <param name="name">
    /// The preset name. Must not be <see langword="null"/>, empty, or whitespace, and must
    /// not contain invalid file-name characters.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or contains
    /// invalid file-name characters.
    /// </exception>
    public void Save(string name)
    {
        ValidateName(name);

        var dict = new Dictionary<string, object?>();
        foreach (var kvp in _parameters)
        {
            var param = kvp.Value;
            if (typeof(Delegate).IsAssignableFrom(param.ValueType)) continue;
            dict[param.Key] = param.GetValue();
        }

        var filePath = GetPresetFilePath(name);
        try
        {
            Directory.CreateDirectory(_presetDirectory);
            File.WriteAllText(filePath, JsonSerializer.Serialize(dict, ConfigPersistence.JsonOptions));
            Logger.Info($"ConfigPresetStore: saved preset '{name}' ({dict.Count} parameter(s)) to '{filePath}'.");
            ToastQueue.Push($"Preset saved: {name}", ToastLevel.Success);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to save preset '{name}' to '{filePath}'.");
            ToastQueue.Push($"Failed to save preset: {name}", ToastLevel.Error);
        }
    }

    /// <summary>
    /// Loads a named preset and applies its values to the registered parameters.
    /// </summary>
    /// <param name="name">The preset name to load.</param>
    /// <returns>
    /// <see langword="true"/> if the preset was loaded and applied successfully;
    /// <see langword="false"/> if the preset file does not exist or could not be read.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or contains
    /// invalid file-name characters.
    /// </exception>
    public bool Load(string name)
    {
        ValidateName(name);

        var filePath = GetPresetFilePath(name);
        if (!File.Exists(filePath))
        {
            Logger.Info($"ConfigPresetStore: preset file '{filePath}' not found.");
            ToastQueue.Push($"Preset not found: {name}", ToastLevel.Warning);
            return false;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, ConfigPersistence.JsonOptions);
            if (dict == null)
            {
                Logger.Info($"ConfigPresetStore: preset '{name}' deserialized to null.");
                return false;
            }

            var applied = 0;
            foreach (var (key, element) in dict)
            {
                if (!_parameters.TryGetValue(key, out var param)) continue;

                if (!ParameterJsonReader.TryConvert(element, param.ValueType, out var value, out var reason))
                {
                    Logger.Warning($"ConfigPresetStore: skipping key '{key}' — {reason}");
                    continue;
                }

                param.SetValue(value);
                applied++;
            }

            Logger.Info($"ConfigPresetStore: loaded preset '{name}' ({applied} of {dict.Count} key(s)) from '{filePath}'.");
            ToastQueue.Push($"Preset loaded: {name}", ToastLevel.Success);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to load preset '{name}' from '{filePath}'.");
            ToastQueue.Push($"Failed to load preset: {name}", ToastLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// Returns the names of all saved presets.
    /// </summary>
    /// <returns>A list of preset names found in the preset directory.</returns>
    public List<string> List()
    {
        var result = new List<string>();

        if (!Directory.Exists(_presetDirectory))
            return result;

        var pattern = PresetPrefix + "*" + PresetExtension;
        string[] files;
        try
        {
            files = Directory.GetFiles(_presetDirectory, pattern);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to enumerate presets in '{_presetDirectory}'.");
            return result;
        }

        for (int i = 0; i < files.Length; i++)
        {
            var fileName = Path.GetFileNameWithoutExtension(files[i]);
            if (fileName.StartsWith(PresetPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var name = fileName.Substring(PresetPrefix.Length);
                if (name.Length > 0)
                    result.Add(name);
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// Deletes a named preset file.
    /// </summary>
    /// <param name="name">The preset name to delete.</param>
    /// <returns>
    /// <see langword="true"/> if the preset file was deleted;
    /// <see langword="false"/> if the preset file did not exist.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or contains
    /// invalid file-name characters.
    /// </exception>
    public bool Delete(string name)
    {
        ValidateName(name);

        var filePath = GetPresetFilePath(name);
        if (!File.Exists(filePath))
        {
            Logger.Info($"ConfigPresetStore: preset '{name}' not found at '{filePath}'.");
            return false;
        }

        try
        {
            File.Delete(filePath);
            Logger.Info($"ConfigPresetStore: deleted preset '{name}' at '{filePath}'.");
            ToastQueue.Push($"Preset deleted: {name}", ToastLevel.Info);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to delete preset '{name}' at '{filePath}'.");
            ToastQueue.Push($"Failed to delete preset: {name}", ToastLevel.Error);
            return false;
        }
    }

    private string GetPresetFilePath(string name) =>
        Path.Combine(_presetDirectory, PresetPrefix + name + PresetExtension);

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name cannot be null, empty, or whitespace.", nameof(name));

        var invalidChars = Path.GetInvalidFileNameChars();
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            for (int j = 0; j < invalidChars.Length; j++)
            {
                if (c == invalidChars[j])
                    throw new ArgumentException($"Preset name contains invalid file-name character '{c}'.", nameof(name));
            }
        }
    }
}
