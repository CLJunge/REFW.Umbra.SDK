namespace Umbra.Config;

/// <summary>
/// Defines the import/export and file-path operations required by Umbra's built-in config transfer UI.
/// </summary>
/// <remarks>
/// This non-generic contract exists so transfer-capable UI surfaces can work with a loaded settings
/// store without requiring the UI type itself to carry the store's generic constraints.
/// </remarks>
public interface IConfigTransferStore
{
    /// <summary>
    /// Gets the configured main settings file path for the store.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Gets whether the store has completed load successfully.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets whether the store has been disposed.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Exports the current registered parameter values to a versioned config exchange document.
    /// </summary>
    /// <param name="filePath">The destination file path.</param>
    void Export(string filePath);

    /// <summary>
    /// Imports compatible values from a config exchange document or legacy flat settings file.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="options">Optional import finalization settings.</param>
    /// <returns>A structured report describing the import outcome.</returns>
    SettingsImportReport Import(string filePath, SettingsImportOptions? options = null);
}
