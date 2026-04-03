using System.Text.Json;
using System.Text.Json.Serialization;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Reads and writes registered settings values to the JSON file used by a <see cref="SettingsStore{TConfig}"/>.
/// </summary>
/// <remarks>
/// This helper serializes only the registered parameter map, leaving store lifecycle and recovery policy orchestration to <see cref="SettingsStorePersistenceCoordinator{TConfig}"/>.
/// </remarks>
internal static class SettingsPersistence
{
    /// <summary>
    /// Describes the outcome of a settings-file load attempt.
    /// </summary>
    internal enum LoadResult
    {
        /// <summary>
        /// The settings file was read successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The settings file was not present when the read was attempted.
        /// </summary>
        /// <remarks>
        /// Callers treat this the same as a successful load with no persisted values and can write fresh defaults.
        /// </remarks>
        MissingFile,

        /// <summary>
        /// The settings file was unreadable, but it was moved aside so defaults can be rewritten safely.
        /// </summary>
        RecoveredToDefaults,

        /// <summary>
        /// The settings file was unreadable and could not be moved aside, so the original file was left untouched.
        /// </summary>
        Failed
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes every persisted registered parameter value and overwrites the destination JSON file.
    /// </summary>
    /// <param name="filePath">The absolute or relative destination file path.</param>
    /// <param name="parameters">The registered parameter map keyed by persisted setting name.</param>
    /// <remarks>
    /// Delegate-valued parameters are skipped because they do not represent persisted state. If the parent directory of <paramref name="filePath"/> does not exist yet, it is created automatically.
    /// </remarks>
    internal static void Save(string filePath, IReadOnlyDictionary<string, IParameter> parameters)
    {
        try
        {
            EnsureParentDirectoryExists(filePath);

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
    /// Reads the specified JSON file and applies matching persisted values to the registered parameter map.
    /// </summary>
    /// <param name="filePath">The absolute or relative source file path.</param>
    /// <param name="parameters">The registered parameter map keyed by persisted setting name.</param>
    /// <returns>The outcome of the load attempt.</returns>
    /// <remarks>
    /// Matching values are applied through <see cref="IParameter.SetValueWithoutNotify(object?)"/>, so loading does not raise <see cref="IParameter.ValueChanged"/> and does not run metadata-based validation.
    /// </remarks>
    internal static LoadResult Load(string filePath, IReadOnlyDictionary<string, IParameter> parameters)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonOptions);
            if (dict == null) return LoadResult.Success;

            var applied = 0;
            foreach (var (key, element) in dict)
            {
                if (!parameters.TryGetValue(key, out var param)) continue;
                ParameterJsonReader.Apply(param, element);
                applied++;
            }

            Logger.Info($"SettingsPersistence: loaded {applied} of {dict.Count} key(s) from '{filePath}'.");
            return LoadResult.Success;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            Logger.Info($"SettingsPersistence: settings file '{filePath}' not found (race condition or external deletion); using defaults.");
            return LoadResult.MissingFile;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"SettingsPersistence: failed to load settings from '{filePath}'.");
            return TryBackupUnreadableSettingsFile(filePath, out var backupPath)
                ? LogRecoveredToDefaults(filePath, backupPath)
                : LoadResult.Failed;
        }
    }

    /// <summary>
    /// Attempts to move an unreadable settings file aside to a timestamped backup path in the same directory.
    /// </summary>
    /// <param name="filePath">The unreadable settings file path.</param>
    /// <param name="backupPath">Receives the generated backup path when the move succeeds.</param>
    /// <returns><see langword="true"/> when the file was moved successfully; otherwise <see langword="false"/>.</returns>
    private static bool TryBackupUnreadableSettingsFile(string filePath, out string backupPath)
    {
        backupPath = string.Empty;

        try
        {
            backupPath = GetUnreadableSettingsBackupPath(filePath);
            File.Move(filePath, backupPath);
            return true;
        }
        catch (Exception backupEx)
        {
            Logger.Exception(backupEx,
                $"SettingsPersistence: failed to back up unreadable settings file '{filePath}' before rewriting defaults.");
            backupPath = string.Empty;
            return false;
        }
    }

    /// <summary>
    /// Logs a recovery message after an unreadable settings file has been moved aside.
    /// </summary>
    /// <param name="filePath">The original unreadable settings file path.</param>
    /// <param name="backupPath">The backup path that now holds the unreadable file.</param>
    /// <returns><see cref="LoadResult.RecoveredToDefaults"/>.</returns>
    private static LoadResult LogRecoveredToDefaults(string filePath, string backupPath)
    {
        Logger.Warning(
            $"SettingsPersistence: moved unreadable settings file '{filePath}' to '{backupPath}'. Defaults will be rewritten.");
        return LoadResult.RecoveredToDefaults;
    }

    /// <summary>
    /// Generates a unique backup path for an unreadable settings file.
    /// </summary>
    /// <param name="filePath">The original unreadable settings file path.</param>
    /// <returns>A non-existent backup path in the same directory.</returns>
    private static string GetUnreadableSettingsBackupPath(string filePath)
    {
        var directoryPath = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        for (var attempt = 0; attempt < 1000; attempt++)
        {
            var suffix = attempt == 0 ? string.Empty : $"-{attempt}";
            var candidate = Path.Combine(
                directoryPath,
                $"{fileNameWithoutExtension}.invalid-{timestamp}{suffix}{extension}");

            if (!File.Exists(candidate))
                return candidate;
        }

        return Path.Combine(
            directoryPath,
            $"{fileNameWithoutExtension}.invalid-{timestamp}-{Guid.NewGuid():N}{extension}");
    }

    /// <summary>
    /// Ensures that the parent directory of <paramref name="filePath"/> exists before a save.
    /// </summary>
    /// <param name="filePath">The destination file path whose containing directory should exist.</param>
    private static void EnsureParentDirectoryExists(string filePath)
    {
        var directoryPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (string.IsNullOrEmpty(directoryPath))
            return;

        Directory.CreateDirectory(directoryPath);
    }
}
