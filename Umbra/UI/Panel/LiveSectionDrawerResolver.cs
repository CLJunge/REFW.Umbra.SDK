using System.Linq.Expressions;

namespace Umbra.UI.Panel;

/// <summary>
/// Reads the <see cref="LiveSectionDrawerAttribute{TDrawer}"/> from a live state type,
/// instantiates the declared <see cref="ILiveSectionDrawer{T}"/>, and compiles a
/// zero-overhead <see cref="Action"/> delegate that invokes <c>Draw(T)</c> each frame.
/// </summary>
/// <remarks>
/// The compilation pass runs once at <see cref="LiveSection{T}"/> construction time.
/// Per-frame cost is a single delegate invocation with no reflection overhead.
/// </remarks>
internal static class LiveSectionDrawerResolver
{
    /// <summary>
    /// Resolves the drawer declared on <paramref name="stateType"/> and compiles a draw
    /// delegate bound to <paramref name="context"/>.
    /// </summary>
    /// <param name="stateType">
    /// The live state <see cref="Type"/> decorated with
    /// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
    /// </param>
    /// <param name="context">
    /// The live state instance that will be passed to the drawer on every
    /// <see cref="Action"/> invocation.
    /// </param>
    /// <param name="disposable">
    /// Set to the instantiated drawer cast to <see cref="IDisposable"/>. Because
    /// <see cref="ILiveSectionDrawer{T}"/> extends <see cref="IDisposable"/>, this is always
    /// non-<see langword="null"/> when the method returns successfully.
    /// </param>
    /// <returns>
    /// A compiled <see cref="Action"/> that invokes <c>drawer.Draw(context)</c> with no
    /// per-frame reflection cost.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="stateType"/> is not decorated with
    /// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>, when the declared drawer type
    /// does not implement <see cref="ILiveSectionDrawer{T}"/> with a generic argument
    /// compatible with <paramref name="stateType"/>, or when the drawer cannot be
    /// instantiated.
    /// </exception>
    internal static Action Resolve(Type stateType, object context, out IDisposable disposable)
    {
        var attr = stateType.GetDrawerAttribute<ILiveSectionDrawerAttribute>() ?? throw new InvalidOperationException(
                $"Live state type '{stateType.Name}' is not decorated with [LiveSectionDrawer<TDrawer>]. " +
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
            if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(ILiveSectionDrawer<>))
                continue;

            var tState = iface.GetGenericArguments()[0];
            if (!tState.IsAssignableFrom(stateType))
                continue;

            genericIface = iface;
            break;
        }

        if (genericIface is null)
            throw new InvalidOperationException(
                $"Drawer type '{attr.DrawerType.Name}' does not implement ILiveSectionDrawer<T> " +
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
