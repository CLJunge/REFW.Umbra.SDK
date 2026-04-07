using Umbra.Config.Attributes;

namespace Umbra.Config;

/// <summary>
/// Owns config exchange import/export orchestration for one <see cref="ConfigStore{TConfig}"/>
/// parameter map.
/// </summary>
/// <remarks>
/// This type encapsulates schema resolution from <typeparamref name="TConfig"/> and keeps the
/// store-level import/export flow aligned with <see cref="ConfigExchangePersistence"/>.
/// </remarks>
internal sealed class ConfigStoreTransferCoordinator<TConfig>
    where TConfig : class, new()
{
    private readonly IReadOnlyDictionary<string, IParameter> _parameters;
    private readonly string _schemaId;
    private readonly int _schemaVersion;

    /// <summary>
    /// Initializes a new transfer coordinator for the supplied registered parameter map.
    /// </summary>
    /// <param name="parameters">The registered parameter map shared by the owning config store.</param>
    internal ConfigStoreTransferCoordinator(IReadOnlyDictionary<string, IParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        _parameters = parameters;
        _schemaId = typeof(TConfig).FullName ?? typeof(TConfig).Name;
        _schemaVersion = typeof(TConfig).GetCustomAttributes(typeof(UmbraConfigVersionAttribute), inherit: true)
            is [UmbraConfigVersionAttribute attribute, ..]
                ? attribute.Version
                : 1;
    }

    /// <summary>
    /// Writes the current registered parameter values to a versioned exchange document.
    /// </summary>
    /// <param name="filePath">The destination file path.</param>
    internal void Export(string filePath)
        => ConfigExchangePersistence.Export(filePath, _parameters, _schemaId, _schemaVersion);

    /// <summary>
    /// Imports compatible values and optionally persists the accepted final state.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="options">The import finalization options.</param>
    /// <param name="save">The callback used to persist the final state when required.</param>
    /// <returns>A structured report describing the import outcome.</returns>
    internal ConfigImportReport Import(string filePath, ConfigImportOptions options, Action save)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(save);

        var report = ConfigExchangePersistence.Import(filePath, _parameters, _schemaId, _schemaVersion);
        if (!report.Success || !options.SaveAfterImport || report.AppliedCount == 0)
            return report;

        save();
        report.Saved = true;
        return report;
    }
}
