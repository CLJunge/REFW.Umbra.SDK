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
    private const string PresetExtension = ".json";
    private const long CacheCheckIntervalMs = 1000;

    private readonly IReadOnlyDictionary<string, IParameter> _parameters;
    private readonly string _presetDirectory;
    private readonly string _presetFilePrefix;
    private readonly ConfigToastOptions? _toast;
    private List<string>? _cachedNames;
    private DateTime _lastDirectoryWriteTimeUtc;
    private long _lastCheckTicks;

    /// <summary>
    /// Gets the directory where preset files are stored.
    /// </summary>
    public string PresetDirectory => _presetDirectory;

    /// <summary>
    /// Gets the file-name prefix prepended to preset names when generating preset file names.
    /// </summary>
    public string PresetFilePrefix => _presetFilePrefix;

    /// <summary>
    /// Initializes a new preset store that operates on the specified loaded config store.
    /// </summary>
    /// <param name="store">
    /// A loaded <see cref="ConfigStore{TConfig}"/>. Must be loaded and not disposed.
    /// </param>
    /// <remarks>
    /// Toast notifications are disabled when using this constructor. Supply a
    /// <see cref="ConfigPresetOptions"/> instance with a non-<see langword="null"/>
    /// <see cref="ConfigPresetOptions.Toast"/> to enable them.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="store"/> has not been loaded.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="store"/> has been disposed.</exception>
    public ConfigPresetStore(ConfigStore<TConfig> store)
        : this(store, ConfigPresetOptions.DefaultPresetFilePrefix, null, toast: null)
    {
    }

    /// <summary>
    /// Initializes a new preset store that operates on the specified loaded config store,
    /// using the supplied <see cref="ConfigPresetOptions"/>.
    /// </summary>
    /// <param name="store">
    /// A loaded <see cref="ConfigStore{TConfig}"/>. Must be loaded and not disposed.
    /// </param>
    /// <param name="options">The preset-store options that control file prefix, directory, and toast behavior.</param>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="store"/> has not been loaded.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="store"/> has been disposed.</exception>
    public ConfigPresetStore(ConfigStore<TConfig> store, ConfigPresetOptions options)
        : this(store,
              options is not null ? options.PresetFilePrefix : throw new ArgumentNullException(nameof(options)),
              options.PresetDirectory,
              options.Toast)
    {
    }

    private ConfigPresetStore(ConfigStore<TConfig> store, string presetFilePrefix, string? presetDirectory, ConfigToastOptions? toast)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (!store.IsLoaded)
            throw new InvalidOperationException("The config store must be loaded before creating a preset store.");

        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store), "The config store has been disposed.");

        var target = (IConfigStoreCopyTarget<TConfig>)store;
        _parameters = target.Parameters;

        _presetFilePrefix = presetFilePrefix;
        _toast = toast;

        if (presetDirectory is not null)
        {
            _presetDirectory = presetDirectory;
        }
        else
        {
            var configDir = Path.GetDirectoryName(Path.GetFullPath(store.FilePath));
            _presetDirectory = configDir ?? ".";
        }
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
            InvalidateCache();
            Logger.Info($"ConfigPresetStore: saved preset '{name}' ({dict.Count} parameter(s)) to '{filePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Preset saved: {name}", ToastLevel.Success, _toast.Duration);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to save preset '{name}' to '{filePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Failed to save preset: {name}", ToastLevel.Error, _toast.Duration);
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
            if (_toast is not null)
                ToastQueue.Push($"Preset not found: {name}", ToastLevel.Warning, _toast.Duration);
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
            if (_toast is not null)
                ToastQueue.Push($"Preset loaded: {name}", ToastLevel.Success, _toast.Duration);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to load preset '{name}' from '{filePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Failed to load preset: {name}", ToastLevel.Error, _toast.Duration);
            return false;
        }
    }

    /// <summary>
    /// Returns the names of all saved presets.
    /// </summary>
    /// <remarks>
    /// Results are cached internally. The cache is invalidated automatically after
    /// <see cref="Save"/>, <see cref="Delete"/>, and <see cref="ImportPreset"/>
    /// operations. External filesystem changes are detected via a throttled
    /// directory-timestamp check (at most once per second).
    /// </remarks>
    /// <returns>A list of preset names found in the preset directory.</returns>
    public List<string> List()
    {
        if (_cachedNames is not null)
        {
            var now = Environment.TickCount64;
            if (now - _lastCheckTicks < CacheCheckIntervalMs)
                return _cachedNames;

            _lastCheckTicks = now;

            if (!Directory.Exists(_presetDirectory))
            {
                _cachedNames = [];
                return _cachedNames;
            }

            try
            {
                var currentWriteTime = Directory.GetLastWriteTimeUtc(_presetDirectory);
                if (currentWriteTime == _lastDirectoryWriteTimeUtc)
                    return _cachedNames;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"ConfigPresetStore: failed to check directory timestamp for '{_presetDirectory}'.");
                return _cachedNames;
            }
        }

        _cachedNames = ScanPresetNames();
        _lastCheckTicks = Environment.TickCount64;

        try
        {
            if (Directory.Exists(_presetDirectory))
                _lastDirectoryWriteTimeUtc = Directory.GetLastWriteTimeUtc(_presetDirectory);
        }
        catch
        {
            // Best-effort; timestamp will be re-read on next check.
        }

        return _cachedNames;
    }

    /// <summary>
    /// Invalidates the cached preset name list so the next <see cref="List"/> call
    /// performs a fresh directory scan.
    /// </summary>
    private void InvalidateCache() => _cachedNames = null;

    private List<string> ScanPresetNames()
    {
        var result = new List<string>();

        if (!Directory.Exists(_presetDirectory))
            return result;

        var pattern = _presetFilePrefix + "*" + PresetExtension;
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

        for (var i = 0; i < files.Length; i++)
        {
            var fileName = Path.GetFileNameWithoutExtension(files[i]);
            if (fileName.StartsWith(_presetFilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var name = fileName[_presetFilePrefix.Length..];
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
            InvalidateCache();
            Logger.Info($"ConfigPresetStore: deleted preset '{name}' at '{filePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Preset deleted: {name}", ToastLevel.Info, _toast.Duration);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to delete preset '{name}' at '{filePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Failed to delete preset: {name}", ToastLevel.Error, _toast.Duration);
            return false;
        }
    }

    /// <summary>
    /// Exports a named preset to an external file path.
    /// </summary>
    /// <param name="name">The preset name to export.</param>
    /// <param name="destinationFilePath">The destination file path.</param>
    /// <returns>
    /// <see langword="true"/> if the preset was copied successfully;
    /// <see langword="false"/> if the preset file does not exist or the copy failed.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or contains
    /// invalid file-name characters, or <paramref name="destinationFilePath"/> is
    /// <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public bool ExportPreset(string name, string destinationFilePath)
    {
        ValidateName(name);
        if (string.IsNullOrWhiteSpace(destinationFilePath))
            throw new ArgumentException("Destination file path cannot be null, empty, or whitespace.", nameof(destinationFilePath));

        var sourceFilePath = GetPresetFilePath(name);
        if (!File.Exists(sourceFilePath))
        {
            Logger.Info($"ConfigPresetStore: cannot export preset '{name}' — source file '{sourceFilePath}' not found.");
            if (_toast is not null)
                ToastQueue.Push($"Preset not found: {name}", ToastLevel.Warning, _toast.Duration);
            return false;
        }

        try
        {
            var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
            Logger.Info($"ConfigPresetStore: exported preset '{name}' to '{destinationFilePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Preset exported: {name}", ToastLevel.Success, _toast.Duration);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to export preset '{name}' to '{destinationFilePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Failed to export preset: {name}", ToastLevel.Error, _toast.Duration);
            return false;
        }
    }

    /// <summary>
    /// Imports a preset from an external file into the preset directory.
    /// </summary>
    /// <remarks>
    /// The preset name is derived from the source file name by stripping the configured
    /// file prefix (if present) and the <c>.json</c> extension. The imported file is copied
    /// into the preset directory using the standard naming convention.
    /// </remarks>
    /// <param name="sourceFilePath">The path to the external preset file to import.</param>
    /// <returns>
    /// The derived preset name when the import succeeds; <see langword="null"/> when the
    /// source file does not exist or the copy fails.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="sourceFilePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public string? ImportPreset(string sourceFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new ArgumentException("Source file path cannot be null, empty, or whitespace.", nameof(sourceFilePath));

        if (!File.Exists(sourceFilePath))
        {
            Logger.Info($"ConfigPresetStore: cannot import preset — source file '{sourceFilePath}' not found.");
            if (_toast is not null)
                ToastQueue.Push("Preset import failed: file not found", ToastLevel.Warning, _toast.Duration);
            return null;
        }

        var name = DerivePresetName(sourceFilePath);
        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.Warning($"ConfigPresetStore: cannot derive preset name from '{sourceFilePath}'.");
            if (_toast is not null)
                ToastQueue.Push("Preset import failed: invalid file name", ToastLevel.Error, _toast.Duration);
            return null;
        }

        var destinationFilePath = GetPresetFilePath(name);
        try
        {
            Directory.CreateDirectory(_presetDirectory);
            File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
            InvalidateCache();
            Logger.Info($"ConfigPresetStore: imported preset '{name}' from '{sourceFilePath}' to '{destinationFilePath}'.");
            if (_toast is not null)
                ToastQueue.Push($"Preset imported: {name}", ToastLevel.Success, _toast.Duration);
            return name;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigPresetStore: failed to import preset from '{sourceFilePath}' to '{destinationFilePath}'.");
            if (_toast is not null)
                ToastQueue.Push("Failed to import preset", ToastLevel.Error, _toast.Duration);
            return null;
        }
    }

    /// <summary>
    /// Derives a preset name from a source file path by stripping the configured prefix and extension.
    /// </summary>
    internal string DerivePresetName(string sourceFilePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        if (fileName.StartsWith(_presetFilePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var stripped = fileName[_presetFilePrefix.Length..];
            if (stripped.Length > 0)
                return stripped;
        }

        return fileName;
    }

    private string GetPresetFilePath(string name) =>
        Path.Combine(_presetDirectory, _presetFilePrefix + name + PresetExtension);

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name cannot be null, empty, or whitespace.", nameof(name));

        var invalidChars = Path.GetInvalidFileNameChars();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            for (var j = 0; j < invalidChars.Length; j++)
            {
                if (c == invalidChars[j])
                    throw new ArgumentException($"Preset name contains invalid file-name character '{c}'.", nameof(name));
            }
        }
    }
}
