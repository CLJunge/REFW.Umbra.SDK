using Umbra.SDK;
using Umbra.SDK.Config.UI;

namespace Umbra.SDK.UI.Panel;

/// <summary>
/// A <see cref="IPanelSection"/> that renders a typed configuration object as a settings
/// panel using <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Pass the config instance returned by <see cref="Config.SettingsStore{TConfig}.Load()"/>
/// so that parameter metadata is already populated at construction time.
/// </para>
/// <para>
/// The <paramref name="idScope"/> defaults to the config type name when not supplied.
/// <see cref="PluginPanel"/> pushes a top-level ImGui ID scope before calling
/// <see cref="Draw"/>; this sub-scope nests inside it, preventing widget ID collisions
/// when two config sections of the same type appear in the same panel.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type. Must have a public parameterless constructor.
/// </typeparam>
public sealed class ConfigSection<TConfig> : IPanelSection where TConfig : class, new()
{
    private readonly ConfigDrawer<TConfig> _drawer;
    private readonly int                   _order;
    private bool                           _disposed;

    /// <summary>
    /// Initialises a new config section wrapping a <see cref="ConfigDrawer{TConfig}"/>.
    /// </summary>
    /// <param name="config">
    /// A fully initialised configuration instance, ideally returned by
    /// <see cref="Config.SettingsStore{TConfig}.Load()"/>.
    /// </param>
    /// <param name="idScope">
    /// ImGui widget ID sub-scope for this section. Defaults to
    /// <c>typeof(<typeparamref name="TConfig"/>).Name</c> when not supplied.
    /// </param>
    public ConfigSection(TConfig config, string? idScope = null)
    {
        _order  = typeof(TConfig).GetDrawerAttribute<SectionOrderAttribute>()?.Order ?? int.MaxValue;
        _drawer = new ConfigDrawer<TConfig>(config, idScope ?? typeof(TConfig).Name);
    }

    /// <inheritdoc/>
    public int Order => _order;

    /// <inheritdoc/>
    public void Draw()
    {
        if (_disposed) return;
        _drawer.Draw();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _drawer.Dispose();
        GC.SuppressFinalize(this);
    }
}
