namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the schema version used by a root configuration type for import/export compatibility checks.
/// </summary>
/// <remarks>
/// Umbra uses this attribute only for versioned config exchange documents. It does not change normal runtime key registration or the default `config.json` persistence format. When the attribute is absent, Umbra treats the configuration schema version as `1`.
/// </remarks>
/// <param name="version">The positive schema version number for the annotated configuration type.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraConfigVersionAttribute(int version) : Attribute
{
    /// <summary>
    /// Gets the declared schema version.
    /// </summary>
    /// <value>A positive integer representing the configuration schema version.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the declared version is less than `1`.
    /// </exception>
    public int Version { get; } = version >= 1
        ? version
        : throw new ArgumentOutOfRangeException(nameof(version), version, "Config schema version must be greater than or equal to 1.");
}
