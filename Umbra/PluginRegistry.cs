namespace Umbra;

/// <summary>
/// Tracks all active <see cref="IPluginStatusProvider"/> instances so diagnostic tooling can
/// enumerate loaded plugins and their current status without holding direct references to
/// each <see cref="PluginHost{TPlugin}"/>.
/// </summary>
/// <remarks>
/// <see cref="PluginHost{TPlugin}"/> registers itself on construction and deregisters on unload.
/// All public methods are thread-safe.
/// </remarks>
public static class PluginRegistry
{
    private static readonly Lock _sync = new();
    private static readonly List<IPluginStatusProvider> _providers = [];

    /// <summary>
    /// Gets the number of currently registered providers.
    /// </summary>
    public static int Count
    {
        get
        {
            lock (_sync)
                return _providers.Count;
        }
    }

    /// <summary>
    /// Registers a status provider so it appears in subsequent <see cref="GetAll"/> calls.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
    public static void Register(IPluginStatusProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_sync)
            _providers.Add(provider);
    }

    /// <summary>
    /// Removes a previously registered provider.
    /// </summary>
    /// <param name="provider">The provider to remove.</param>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
    public static void Deregister(IPluginStatusProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_sync)
        {
            for (var i = _providers.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_providers[i], provider))
                {
                    _providers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Appends a <see cref="PluginStatus"/> snapshot for every registered provider to the caller-supplied list.
    /// </summary>
    /// <param name="destination">The list to which status snapshots are appended. The caller owns this list and may reuse it across calls to avoid allocation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <see langword="null"/>.</exception>
    public static void GetAll(IList<PluginStatus> destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        lock (_sync)
        {
            for (var i = 0; i < _providers.Count; i++)
                destination.Add(_providers[i].GetStatus());
        }
    }

    /// <summary>
    /// Removes all registered providers.
    /// </summary>
    /// <remarks>
    /// This method exists for unit tests that need deterministic isolation between runs.
    /// </remarks>
    internal static void Reset()
    {
        lock (_sync)
            _providers.Clear();
    }
}
