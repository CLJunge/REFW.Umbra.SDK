namespace Umbra.Config;

/// <summary>
/// Controls how <see cref="SettingsStore{TConfig}.Import(string, SettingsImportOptions?)"/> finalizes a successful config import.
/// </summary>
public sealed class SettingsImportOptions
{
    /// <summary>
    /// Gets a reusable default options instance.
    /// </summary>
    public static SettingsImportOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the store should save the accepted imported state to its configured runtime file after import completes.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/>, Umbra saves at most once after all compatible values have been applied.
    /// </remarks>
    public bool SaveAfterImport { get; init; } = true;
}
