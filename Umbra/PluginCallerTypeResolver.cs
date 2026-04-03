using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Umbra;

/// <summary>
/// Resolves the external caller type for convenience APIs that infer plugin identity from the call stack.
/// </summary>
/// <remarks>
/// This helper skips its own frame and the convenience API frame, then inspects the external caller that invoked that API. Umbra uses it for overloads such as <see cref="PluginBootstrapper.Load(Action)"/> so runtime helpers can keep their public surface small without duplicating stack-frame logic.
/// </remarks>
internal static class PluginCallerTypeResolver
{
    private const int _callerFrameIndex = 2;

    /// <summary>
    /// Resolves the declaring type of the external caller of the convenience runtime API.
    /// </summary>
    /// <param name="apiOwner">The runtime API type performing caller inference.</param>
    /// <param name="apiName">The runtime API member name performing caller inference.</param>
    /// <param name="fallbackSignature">The explicit overload signature callers should use when inference is unavailable.</param>
    /// <returns>The declaring type of the external caller.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="apiOwner"/>, <paramref name="apiName"/>, or <paramref name="fallbackSignature"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The calling method cannot be resolved or does not declare a plugin type.</exception>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Type ResolveCallingPluginType(Type apiOwner, string apiName, string fallbackSignature)
    {
        ArgumentNullException.ThrowIfNull(apiOwner);
        ArgumentNullException.ThrowIfNull(apiName);
        ArgumentNullException.ThrowIfNull(fallbackSignature);

        var callerMethod = new StackFrame(_callerFrameIndex, false).GetMethod()
            ?? throw new InvalidOperationException(
                $"Unable to resolve the calling method for {apiOwner.Name}.{apiName}.");

        return callerMethod.DeclaringType
            ?? throw new InvalidOperationException(
                $"Calling method '{callerMethod.Name}' does not declare a plugin type. " +
                $"Use {fallbackSignature} when caller inference is not available.");
    }
}
