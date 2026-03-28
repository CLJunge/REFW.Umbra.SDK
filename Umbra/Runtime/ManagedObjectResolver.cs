using REFrameworkNET;

namespace Umbra.Runtime;

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
    /// This is a convenience wrapper over <see cref="TryResolve{T}(ulong, out T?)"/> for call
    /// sites that prefer a nullable return value over an explicit success flag.
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
        => TryResolve(address, out T? value) ? value : null;

    /// <summary>
    /// Attempts to resolve the native game object at <paramref name="address"/> to a
    /// strongly-typed managed reference.
    /// </summary>
    /// <remarks>
    /// Internally calls <see cref="ManagedObject.ToManagedObject"/> to wrap the raw address and
    /// then invokes <c>TryAs&lt;T&gt;</c> to perform the type cast.
    /// <para>
    /// The method returns <see langword="false"/> in three distinct cases:
    /// <list type="bullet">
    ///   <item><description>
    ///     <paramref name="address"/> is zero — returned immediately without entering the
    ///     exception-handling path.
    ///   </description></item>
    ///   <item><description>
    ///     <c>TryAs&lt;T&gt;</c> returns <see langword="null"/> when the runtime type of the object
    ///     at <paramref name="address"/> is not compatible with <typeparamref name="T"/>.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ManagedObject.ToManagedObject"/> throws — for example when
    ///     <paramref name="address"/> is otherwise invalid — and the exception is swallowed so
    ///     the game-facing call site can stay on a simple failure path.
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This API is useful when callers need to distinguish success from failure without relying on
    /// a nullable return value alone.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The managed type to cast the resolved object to. Must be a reference type.
    /// </typeparam>
    /// <param name="address">The native memory address of the game object to resolve.</param>
    /// <param name="value">
    /// Receives the resolved instance cast to <typeparamref name="T"/> when the method returns
    /// <see langword="true"/>; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the object was resolved and cast successfully; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryResolve<T>(ulong address, out T? value) where T : class
    {
        if (address == 0)
        {
            value = null;
            return false;
        }

        try
        {
            value = ManagedObject.ToManagedObject(address).TryAs<T>();
            return value is not null;
        }
        catch
        {
            value = null;
            return false;
        }
    }
}
