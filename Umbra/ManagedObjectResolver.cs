using REFrameworkNET;

namespace Umbra;

/// <summary>
/// Provides utilities for resolving native game object addresses into managed
/// typed references via the <see cref="ManagedObject"/> REFramework.NET API.
/// </summary>
public static class ManagedObjectResolver
{
    /// <summary>
    /// Resolves the native game object at <paramref name="address"/> to a
    /// strongly-typed managed reference, or <see langword="null"/> on failure.
    /// </summary>
    /// <remarks>
    /// Internally calls <see cref="ManagedObject.ToManagedObject"/> to wrap the raw
    /// address and then invokes <c>TryAs&lt;T&gt;</c> to perform the type cast.
    /// <para>
    /// <see langword="null"/> is returned in three distinct cases:
    /// <list type="bullet">
    ///   <item><description>
    ///     <paramref name="address"/> is zero — returned immediately without entering
    ///     the try/catch block.
    ///   </description></item>
    ///   <item><description>
    ///     <c>TryAs&lt;T&gt;</c> returns <see langword="null"/> when the runtime type of
    ///     the object at <paramref name="address"/> is not compatible with
    ///     <typeparamref name="T"/> (no exception is raised).
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ManagedObject.ToManagedObject"/> throws — for example when
    ///     <paramref name="address"/> is otherwise invalid — and the exception
    ///     is silently swallowed.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The managed type to cast the resolved object to. Must be a reference type.
    /// </typeparam>
    /// <param name="address">
    /// The native memory address of the game object to resolve.
    /// </param>
    /// <returns>
    /// The resolved instance cast to <typeparamref name="T"/>, or
    /// <see langword="null"/> if the address is invalid or the object's runtime type
    /// is incompatible with <typeparamref name="T"/>.
    /// </returns>
    public static T? Resolve<T>(ulong address) where T : class
    {
        if (address == 0) return null;

        try
        {
            return ManagedObject.ToManagedObject(address).TryAs<T>();
        }
        catch
        {
            return null;
        }
    }
}
