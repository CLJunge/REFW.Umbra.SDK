using REFrameworkNET;

namespace Umbra.Runtime;

/// <summary>
/// Implements <see cref="IManagedObjectBridge"/> by resolving native RE Engine objects through REFramework.NET.
/// </summary>
/// <remarks>
/// This is the production bridge used by <see cref="ManagedObjectResolver"/> inside the game process.
/// </remarks>
internal sealed class REFrameworkManagedObjectBridge : IManagedObjectBridge
{
    /// <inheritdoc/>
    public bool TryResolve<T>(ulong address, out T? value) where T : class
    {
        value = ManagedObject.ToManagedObject(address).TryAs<T>();
        return value is not null;
    }
}
