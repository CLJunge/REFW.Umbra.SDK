namespace Umbra.Runtime;

/// <summary>
/// Defines the low-level bridge used to resolve native RE Engine object addresses into managed references.
/// </summary>
/// <remarks>
/// <see cref="ManagedObjectResolver"/> uses this internal seam to isolate its public control flow from the concrete REFramework.NET runtime API. Tests can replace the production bridge with a deterministic implementation.
/// </remarks>
internal interface IManagedObjectBridge
{
    /// <summary>
    /// Attempts to resolve the native object at <paramref name="address"/> to a managed instance compatible with <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The requested managed reference type.</typeparam>
    /// <param name="address">The native object address to resolve.</param>
    /// <param name="value">When this method returns <see langword="true"/>, contains the resolved managed instance; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the object was resolved and cast successfully; otherwise, <see langword="false"/>.</returns>
    bool TryResolve<T>(ulong address, out T? value) where T : class;
}
