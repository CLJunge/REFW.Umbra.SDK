using Umbra.Config;

namespace Umbra.UI.Panel;

/// <summary>
/// Owns a <see cref="PluginPanel"/> together with the <see cref="ConfigStore{TConfig}"/> and loaded
/// <typeparamref name="TConfig"/> instance that back it, providing single-call lifecycle management.
/// </summary>
/// <remarks>
/// <para>
/// Instances are created exclusively through <see cref="PluginPanelFactory.Create{TConfig}"/>. The
/// factory creates the config store, loads the config, and wires the panel — this type then owns the
/// full disposal sequence so the caller only needs to hold one reference.
/// </para>
/// <para>
/// <see cref="Dispose"/> sequences: panel dispose (which flushes the internal
/// <see cref="ConfigSaveController{TConfig}"/>), then an explicit <see cref="ConfigStore{TConfig}.Save"/>
/// (defensive), then <see cref="ConfigStore{TConfig}.Dispose"/>. After disposal, <see cref="Draw"/>
/// becomes a silent no-op.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration type rendered by the panel's config section.</typeparam>
public sealed class ManagedPluginPanel<TConfig> : IDisposable
    where TConfig : class, new()
{
    private bool _disposed;

    internal ManagedPluginPanel(PluginPanel panel, TConfig config, ConfigStore<TConfig> store)
    {
        Panel = panel;
        Config = config;
        Store = store;
    }

    /// <summary>
    /// Gets the underlying <see cref="PluginPanel"/> for advanced operations such as
    /// <see cref="PluginPanel.Add(IPanelSection)"/>.
    /// </summary>
    public PluginPanel Panel { get; }

    /// <summary>
    /// Gets the loaded configuration instance rendered by the panel's config section.
    /// </summary>
    public TConfig Config { get; }

    /// <summary>
    /// Gets the <see cref="ConfigStore{TConfig}"/> that persists the config instance.
    /// </summary>
    public ConfigStore<TConfig> Store { get; }

    /// <summary>
    /// Renders all panel sections for the current ImGui frame.
    /// </summary>
    /// <remarks>
    /// Forwards to <see cref="PluginPanel.Draw"/>. After <see cref="Dispose"/>, this method
    /// becomes a silent no-op.
    /// </remarks>
    public void Draw() => Panel.Draw();

    /// <summary>
    /// Disposes the panel, saves the config store, and disposes the store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Disposal sequence: <see cref="PluginPanel.Dispose"/> (flushes the internal save controller),
    /// <see cref="ConfigStore{TConfig}.Save"/> (defensive final persist), then
    /// <see cref="ConfigStore{TConfig}.Dispose"/> (releases listeners).
    /// </para>
    /// <para>
    /// Repeated calls after the first one do nothing.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Panel.Dispose();
        Store.Save();
        Store.Dispose();

        GC.SuppressFinalize(this);
    }
}
