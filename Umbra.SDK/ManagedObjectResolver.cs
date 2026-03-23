using REFrameworkNET;

namespace Umbra.SDK;

/// <summary>
/// Provides utilities for resolving native game object addresses into managed
/// typed references via the <see cref="ManagedObject"/> REFramework.NET API.
/// </summary>
public static class ManagedObjectResolver
{
    /// <summary>
    /// Resolves the native game object at <paramref name="address"/> and assigns it
    /// to <paramref name="field"/> as a strongly-typed managed reference.
    /// </summary>
    /// <remarks>
    /// Internally calls <see cref="ManagedObject.ToManagedObject"/> to wrap the raw
    /// address and then invokes <c>TryAs&lt;T&gt;</c> to perform the type cast.
    /// If either step throws — for example when the address is invalid or the
    /// runtime type is incompatible with <typeparamref name="T"/> — the exception is
    /// silently swallowed and <paramref name="field"/> is set to <see langword="null"/>.
    /// </remarks>
    /// <typeparam name="T">
    /// The managed type to cast the resolved object to. Must be a reference type.
    /// </typeparam>
    /// <param name="field">
    /// The field to populate. Set to the resolved typed instance on success, or
    /// <see langword="null"/> on failure.
    /// </param>
    /// <param name="address">
    /// The native memory address of the game object to resolve.
    /// </param>
    public static void SetManagerObject<T>(ref T? field, ulong address) where T : class
    {
        try
        {
            field = ManagedObject.ToManagedObject(address).TryAs<T>();
        }
        catch
        {
            field = null;
        }

    }
}
