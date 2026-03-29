using System.Text.Json;
using System.Text.Json.Serialization;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Handles reading and writing of plugin settings to and from a JSON file on disk.
/// </summary>
internal static class SettingsPersistence
{
    /// <summary>
    /// Describes the outcome of a settings-file load attempt.
    /// </summary>
    internal enum LoadResult
    {
        /// <summary>The settings file was read successfully.</summary>
        Success,

        /// <summary>
        /// The settings file was not present at read time (including TOCTOU races where the file
        /// disappears between an existence check and the actual read). The caller should treat
        /// this identically to <see cref="Success"/> with no persisted values and save fresh defaults.
        /// </summary>
        MissingFile,

        /// <summary>
        /// The settings file could not be read, but the unreadable file was moved aside to a
        /// backup path so defaults can be written safely.
        /// </summary>
        RecoveredToDefaults,

        /// <summary>
        /// The settings file could not be read and could not be moved aside, so the original file
        /// was left untouched.
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
    /// Serializes the current value of every registered parameter and writes the result
    /// to the specified JSON file, overwriting any existing content.
    /// </summary>
    /// <param name="filePath">The absolute or relative path of the destination JSON file.</param>
    /// <param name="parameters">
    /// A read-only dictionary of all registered parameters, keyed by their unique setting key.
    /// </param>
    /// <remarks>
    /// If the parent directory of <paramref name="filePath"/> does not yet exist, it is created
    /// automatically before the file is written. This allows first-run saves to succeed when the
    /// caller supplies a new plugin-specific data directory path.
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
    /// Reads the specified JSON file and applies the persisted values to the matching registered parameters.
    /// Parameters whose keys are not present in the file are left at their current (default) values.
    /// </summary>
    /// <param name="filePath">The absolute or relative path of the source JSON file.</param>
    /// <param name="parameters">
    /// A read-only dictionary of all registered parameters, keyed by their unique setting key.
    /// </param>
    /// <remarks>
    /// Values are applied via <see cref="IParameter.SetValueWithoutNotify"/>; no
    /// <see cref="IParameter.ValueChanged"/> events are raised during load, and metadata-based
    /// validation is intentionally bypassed while restoring persisted values.
    /// If the file does not exist (including a TOCTOU race where it is deleted between the caller's
    /// existence check and this read), <see cref="LoadResult.MissingFile"/> is returned so callers
    /// can write fresh defaults without suppressing future saves.
    /// If the file exists but cannot be deserialized, Umbra attempts to move it aside to a timestamped
    /// <c>.invalid-*.json</c> backup in the same directory so callers can safely rewrite defaults.
    /// </remarks>
    /// <returns>
    /// <see cref="LoadResult.Success"/> when the file was read successfully;
    /// <see cref="LoadResult.MissingFile"/> when the file was not found (treat as "no saved settings");
    /// <see cref="LoadResult.RecoveredToDefaults"/> when the unreadable file was backed up and the
    /// caller can rewrite defaults safely; otherwise <see cref="LoadResult.Failed"/>.
    /// </returns>
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
            // Treat a missing file as "no saved settings" — return MissingFile so the caller
            // writes fresh defaults rather than suppressing saves for the rest of the session.
            // This handles TOCTOU races where the file is deleted between the File.Exists guard
            // in SettingsStore.Load() and the ReadAllText call above.
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
