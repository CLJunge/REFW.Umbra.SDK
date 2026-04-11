namespace Umbra;

/// <summary>
/// Extends <see cref="IUmbraPlugin"/> with identity metadata that diagnostic tooling
/// and the <see cref="PluginHost{TPlugin}"/> status API use to describe loaded plugins.
/// </summary>
public partial interface IUmbraPlugin
{
    /// <summary>
    /// Gets the human-readable display name of this plugin.
    /// </summary>
    /// <value>A non-null display name. Implementations that derive from <see cref="UmbraPlugin"/>
    /// return the concrete type name by default.</value>
    string PluginName { get; }

    /// <summary>
    /// Gets the version string of this plugin, or <see langword="null"/> when no version is available.
    /// </summary>
    /// <value>A version string such as <c>"1.2.0"</c>, or <see langword="null"/>.</value>
    string? PluginVersion { get; }

    /// <summary>
    /// Gets the author of this plugin, or <see langword="null"/> when no author is declared.
    /// </summary>
    /// <value>An author name, or <see langword="null"/>.</value>
    string? PluginAuthor { get; }
}
