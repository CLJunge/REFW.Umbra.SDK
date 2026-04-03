using System.Linq.Expressions;

namespace Umbra.UI.LiveState;

/// <summary>
/// Resolves the drawer declared on a live-state type and compiles the delegate used to invoke it each frame.
/// </summary>
/// <remarks>
/// This resolution pass runs once when <see cref="LiveStateSection{T}"/> is constructed. The returned delegate captures the instantiated drawer and bound context so per-frame rendering avoids reflection.
/// </remarks>
internal static class LiveStateSectionDrawerResolver
{
    /// <summary>
    /// Resolves the drawer declared on <paramref name="stateType"/> and binds it to <paramref name="context"/>.
    /// </summary>
    /// <param name="stateType">The live-state type decorated with <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>.</param>
    /// <param name="context">The live-state instance passed to the resolved drawer on every invocation.</param>
    /// <param name="disposable">When this method returns, contains the instantiated drawer as an <see cref="IDisposable"/>. This parameter is treated as uninitialized.</param>
    /// <returns>A compiled <see cref="Action"/> that invokes <c>Draw</c> on the resolved drawer for <paramref name="context"/>.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="stateType"/> is not decorated with <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>, the declared drawer type does not implement a compatible <see cref="ILiveStateSectionDrawer{T}"/>, or the drawer cannot be instantiated.</exception>
    internal static Action Resolve(Type stateType, object context, out IDisposable disposable)
    {
        var attr = stateType.GetDrawerAttribute<ILiveStateSectionDrawerAttribute>() ?? throw new InvalidOperationException(
                $"Live state type '{stateType.Name}' is not decorated with [LiveStateSectionDrawer<TDrawer>]. " +
                $"Apply the attribute to the state class to declare its drawer.");

        object drawerInstance;
        try
        {
            drawerInstance = Activator.CreateInstance(attr.DrawerType)
                ?? throw new InvalidOperationException(
                    $"Activator.CreateInstance returned null for drawer type '{attr.DrawerType.FullName}'.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to instantiate drawer type '{attr.DrawerType.FullName}' for live state type '{stateType.FullName}'. " +
                $"Ensure the drawer has a public parameterless constructor.", ex);
        }

        Type? genericIface = null;
        foreach (var iface in attr.DrawerType.GetInterfaces())
        {
            if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(ILiveStateSectionDrawer<>))
                continue;

            var tState = iface.GetGenericArguments()[0];
            if (!tState.IsAssignableFrom(stateType))
                continue;

            genericIface = iface;
            break;
        }

        if (genericIface is null)
            throw new InvalidOperationException(
                $"Drawer type '{attr.DrawerType.Name}' does not implement ILiveStateSectionDrawer<T> " +
                $"with a generic argument compatible with '{stateType.Name}'.");

        disposable = (IDisposable)drawerInstance;

        var drawMethod = genericIface.GetMethod("Draw")!;
        var stateParam = genericIface.GetGenericArguments()[0];
        var callExpr = Expression.Call(
            Expression.Convert(Expression.Constant(drawerInstance), genericIface),
            drawMethod,
            Expression.Convert(Expression.Constant(context), stateParam));

        return Expression.Lambda<Action>(callExpr).Compile();
    }
}
