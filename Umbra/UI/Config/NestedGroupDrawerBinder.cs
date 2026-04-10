using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves and caches the delegates used to invoke custom nested-group drawers.
/// </summary>
/// <remarks>
/// This binder isolates nested-drawer compatibility checks, instance activation, expression compilation, and disposable tracking from the higher-level configuration drawer builder.
/// </remarks>
internal static class NestedDrawerBinder
{
    private static readonly ConcurrentDictionary<NestedGroupDrawerFactoryKey, NestedDrawerFactory> s_factories = new();

    /// <summary>
    /// Cache key for one nested-group drawer binding shape.
    /// </summary>
    /// <param name="DrawerType">The concrete nested-group drawer type being instantiated.</param>
    /// <param name="GroupType">The runtime nested config-group type exposed by the property.</param>
    private readonly record struct NestedGroupDrawerFactoryKey(Type DrawerType, Type GroupType);

    /// <summary>
    /// Cached result of resolving and compiling the draw invoker for one drawer/group type pair.
    /// </summary>
    /// <remarks>
    /// The expensive interface scan and expression compilation happen once per unique pair and are
    /// reused by all subsequent config-drawer builds for the same shape. Per-node work is then
    /// reduced to creating the drawer instance and binding the cached invoker to that instance and
    /// nested group object.
    /// </remarks>
    private sealed class NestedDrawerFactory(bool isSupported, Action<object, object>? invoker)
    {
        internal bool IsSupported { get; } = isSupported;
        private int _unsupportedLogged;

        internal bool TryMarkUnsupportedLogged() => Interlocked.Exchange(ref _unsupportedLogged, 1) == 0;

        internal Action Bind(object drawerInstance, object nested)
        {
            if (invoker is null)
                throw new InvalidOperationException("Cannot bind an unsupported nested-group drawer factory.");

            return () => invoker(drawerInstance, nested);
        }
    }

    /// <summary>
    /// Creates the one-time draw delegate for a custom nested-group drawer and returns any disposable drawer instance.
    /// </summary>
    /// <param name="nestedDrawerAttr">The resolved nested-group drawer attribute.</param>
    /// <param name="groupType">The runtime type of the nested group.</param>
    /// <param name="nestedGroup">The live nested group instance that will be passed into the drawer.</param>
    /// <param name="disposable">Receives the drawer instance when it implements <see cref="IDisposable"/>.</param>
    /// <returns>
    /// A draw delegate bound to a cached per-type invoker, or <see langword="null"/> when the
    /// drawer type does not support <paramref name="groupType"/>.
    /// </returns>
    internal static Action? BuildDrawAction(
        INestedDrawerAttribute nestedDrawerAttr,
        Type groupType,
        object nestedGroup,
        out IDisposable? disposable)
    {
        disposable = null;
        var drawerType = nestedDrawerAttr.DrawerType;
        var factory = s_factories.GetOrAdd(
            new NestedGroupDrawerFactoryKey(drawerType, groupType),
            static key => CreateFactory(key.DrawerType, key.GroupType));

        if (!factory.IsSupported)
        {
            if (factory.TryMarkUnsupportedLogged())
            {
                Logger.Error(
                    $"ConfigDrawer: nested group drawer '{drawerType.Name}' does not support group type '{groupType.FullName}'.");
            }

            return null;
        }

        var drawerInstance = Activator.CreateInstance(drawerType)!;

        if (drawerInstance is IDisposable trackedDisposable)
            disposable = trackedDisposable;

        return factory.Bind(drawerInstance, nestedGroup);
    }

    /// <summary>
    /// Resolves and compiles the cached invoker used by nested-group drawers for a specific
    /// drawer/group type pair.
    /// </summary>
    /// <param name="drawerType">The concrete drawer type to inspect.</param>
    /// <param name="groupType">The runtime nested-group type exposed by the property.</param>
    /// <returns>
    /// A cached factory describing whether the drawer supports <paramref name="groupType"/> and,
    /// when supported, the precompiled invoker used to bind concrete instances.
    /// </returns>
    private static NestedDrawerFactory CreateFactory(Type drawerType, Type groupType)
    {
        Type? genericIface = null;
        Type? supportedGroupType = null;
        foreach (var iface in drawerType.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() != typeof(INestedDrawer<>))
                continue;

            var candidateGroupType = iface.GetGenericArguments()[0];
            if (!candidateGroupType.IsAssignableFrom(groupType))
                continue;

            genericIface = iface;
            supportedGroupType = candidateGroupType;
            break;
        }

        if (genericIface is null || supportedGroupType is null)
            return new NestedDrawerFactory(false, null);

        var drawMethod = genericIface.GetMethod(nameof(INestedDrawer<object>.Draw))!;
        var drawerParam = Expression.Parameter(typeof(object), "drawer");
        var groupParam = Expression.Parameter(typeof(object), "group");
        var callExpr = Expression.Call(
            Expression.Convert(drawerParam, genericIface),
            drawMethod,
            Expression.Convert(groupParam, supportedGroupType));
        var invoker = Expression.Lambda<Action<object, object>>(callExpr, drawerParam, groupParam).Compile();

        return new NestedDrawerFactory(true, invoker);
    }
}
