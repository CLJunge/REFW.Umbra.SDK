using REFrameworkNET;

namespace Umbra.Runtime;

/// <summary>
/// Resolves native RE Engine object addresses through the REFramework.NET managed host.
/// </summary>
/// <remarks>
/// This is the production bridge used by <see cref="ManagedObjectResolver"/> at runtime inside the
/// game process. Test code can replace it with a deterministic in-memory bridge.
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
