using System.Linq.Expressions;
using System.Reflection;

namespace Umbra.SDK.UI.Panel;

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
    /// Set to the instantiated drawer cast to <see cref="IDisposable"/> so the caller can
    /// track its lifetime, or <see langword="null"/> when the drawer does not hold
    /// disposable resources beyond the default interface implementation.
    /// </param>
    /// <returns>
    /// A compiled <see cref="Action"/> that invokes <c>drawer.Draw(context)</c> with no
    /// per-frame reflection cost.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="stateType"/> is not decorated with
    /// <see cref="LiveSectionDrawerAttribute{TDrawer}"/>, or when the declared drawer type
    /// does not implement <see cref="ILiveSectionDrawer{T}"/>.
    /// </exception>
    internal static Action Resolve(Type stateType, object context, out IDisposable? disposable)
    {
        var attr = stateType.GetDrawerAttribute<ILiveSectionDrawerAttribute>();
        if (attr is null)
            throw new InvalidOperationException(
                $"Live state type '{stateType.Name}' is not decorated with [LiveSectionDrawer<TDrawer>]. " +
                $"Apply the attribute to the state class to declare its drawer.");

        var drawerInstance = Activator.CreateInstance(attr.DrawerType)!;

        Type? genericIface = null;
        foreach (var iface in attr.DrawerType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ILiveSectionDrawer<>))
            { genericIface = iface; break; }
        }

        if (genericIface is null)
            throw new InvalidOperationException(
                $"Drawer type '{attr.DrawerType.Name}' does not implement ILiveSectionDrawer<T>.");

        disposable = drawerInstance as IDisposable;

        var drawMethod = genericIface.GetMethod("Draw")!;
        var stateParam = genericIface.GetGenericArguments()[0];
        var callExpr   = Expression.Call(
            Expression.Convert(Expression.Constant(drawerInstance), genericIface),
            drawMethod,
            Expression.Convert(Expression.Constant(context), stateParam));

        return Expression.Lambda<Action>(callExpr).Compile();
    }
}
