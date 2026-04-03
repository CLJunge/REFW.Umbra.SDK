using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Coordinates load, save, and unreadable-file recovery behavior for one <see cref="SettingsStore{TConfig}"/> instance.
/// </summary>
/// <remarks>
/// This type isolates persistence orchestration from <see cref="SettingsStore{TConfig}"/> so the store can remain focused on public lifecycle and parameter ownership. It also owns save suppression after unrecoverable load failures and the one-time blocked-save warning policy.
/// </remarks>
internal sealed class SettingsStorePersistenceCoordinator<TConfig>
    where TConfig : class, new()
{
    private readonly string _filePath;
    private readonly Dictionary<string, IParameter> _parameters;
    private bool _saveBlocked;
    private bool _saveBlockedWarningLogged;

    /// <summary>
    /// Initializes a new persistence coordinator for the supplied settings file and parameter map.
    /// </summary>
    /// <param name="filePath">The JSON file path used for persistence.</param>
    /// <param name="parameters">The shared registered-parameter map owned by the store.</param>
    internal SettingsStorePersistenceCoordinator(string filePath, Dictionary<string, IParameter> parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(parameters);

        _filePath = filePath;
        _parameters = parameters;
    }

    /// <summary>
    /// Persists the current registered parameter values unless saves have been blocked by a prior unrecoverable load failure.
    /// </summary>
    internal void Save()
    {
        if (_saveBlocked)
        {
            WarnSaveBlockedOnce();
            return;
        }

        SettingsPersistence.Save(_filePath, _parameters);
    }

    /// <summary>
    /// Creates a fresh registered configuration instance, then applies persisted values or the configured recovery policy.
    /// </summary>
    /// <param name="createRegisteredDefaults">The callback that creates a new <typeparamref name="TConfig"/> instance and repopulates the shared parameter map with declared defaults.</param>
    /// <returns>The configuration instance for the current store session.</returns>
    internal TConfig Load(Func<TConfig> createRegisteredDefaults)
    {
        ArgumentNullException.ThrowIfNull(createRegisteredDefaults);

        var instance = createRegisteredDefaults();
        Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: discovered {_parameters.Count} parameter(s).");

        if (!File.Exists(_filePath))
        {
            Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: no existing config file found at '{_filePath}', saving defaults.");
            Save();
            return instance;
        }

        var loadResult = SettingsPersistence.Load(_filePath, _parameters);
        if (loadResult == SettingsPersistence.LoadResult.MissingFile)
        {
            Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: settings file '{_filePath}' vanished before it could be read; saving defaults.");
            Save();
            return instance;
        }

        if (loadResult == SettingsPersistence.LoadResult.RecoveredToDefaults)
        {
            Logger.Warning(
                $"SettingsStore<{typeof(TConfig).Name}>: existing config was unreadable; rewriting defaults to '{_filePath}'.");

            instance = createRegisteredDefaults();
            Save();
            return instance;
        }

        if (loadResult == SettingsPersistence.LoadResult.Failed)
        {
            instance = createRegisteredDefaults();
            _saveBlocked = true;
            Logger.Warning(
                $"SettingsStore<{typeof(TConfig).Name}>: preserving unreadable config at '{_filePath}'. " +
                "Saves are suppressed for this store instance because the file could not be backed up safely.");
        }

        return instance;
    }

    /// <summary>
    /// Logs a warning once when saves are being suppressed after an unrecoverable load failure.
    /// </summary>
    private void WarnSaveBlockedOnce()
    {
        if (_saveBlockedWarningLogged)
            return;

        _saveBlockedWarningLogged = true;
        Logger.Warning(
            $"SettingsStore<{typeof(TConfig).Name}>: Save() ignored because the original config file at '{_filePath}' " +
            "was unreadable and could not be backed up during Load().");
    }
}
