namespace Umbra.Runtime;

/// <summary>
/// Resolves native RE Engine object addresses into managed typed references.
/// </summary>
/// <remarks>
/// This internal seam isolates <see cref="ManagedObjectResolver"/> from the concrete
/// REFramework.NET runtime API so unit tests can substitute deterministic behavior without
/// requiring the game host to be active.
/// </remarks>
internal interface IManagedObjectBridge
{
    /// <summary>
    /// Attempts to resolve the native object at <paramref name="address"/> to a managed instance
    /// of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The requested managed reference type.</typeparam>
    /// <param name="address">The native object address to resolve.</param>
    /// <param name="value">
    /// Receives the resolved managed instance when the lookup succeeds; otherwise
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the object was resolved and cast successfully; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool TryResolve<T>(ulong address, out T? value) where T : class;
}
