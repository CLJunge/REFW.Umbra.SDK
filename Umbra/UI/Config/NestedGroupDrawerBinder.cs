using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config;

/// <summary>
/// Creates cached draw delegates for nested-group custom drawers.
/// </summary>
/// <remarks>
/// This type isolates nested-group drawer compatibility checks, expression compilation, instance
/// activation, and disposable tracking from <see cref="ConfigDrawerBuilder"/> so the builder can
/// remain focused on config-tree traversal and draw-node composition.
/// </remarks>
internal static class NestedGroupDrawerBinder
{
    private static readonly ConcurrentDictionary<NestedGroupDrawerFactoryKey, NestedGroupDrawerFactory> s_factories = new();

    /// <summary>
    /// Cache key for one nested-group drawer binding shape.
    /// </summary>
    /// <param name="DrawerType">The concrete nested-group drawer type being instantiated.</param>
    /// <param name="GroupType">The runtime nested settings-group type exposed by the property.</param>
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
    private sealed class NestedGroupDrawerFactory(bool isSupported, Action<object, object>? invoker)
    {
        internal bool IsSupported { get; } = isSupported;

        internal Action Bind(object drawerInstance, object nested)
        {
            if (invoker is null)
                throw new InvalidOperationException("Cannot bind an unsupported nested-group drawer factory.");

            return () => invoker(drawerInstance, nested);
        }
    }

    /// <summary>
    /// Creates the one-time draw delegate for a nested-group custom drawer and returns any
    /// disposable drawer instance that should be tracked by the caller.
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
        INestedGroupDrawerAttribute nestedDrawerAttr,
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
            Logger.Error(
                $"ConfigDrawer: nested group drawer '{drawerType.Name}' does not support group type '{groupType.FullName}'.");
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
    private static NestedGroupDrawerFactory CreateFactory(Type drawerType, Type groupType)
    {
        Type? genericIface = null;
        Type? supportedGroupType = null;
        foreach (var iface in drawerType.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() != typeof(INestedGroupDrawer<>))
                continue;

            var candidateGroupType = iface.GetGenericArguments()[0];
            if (!candidateGroupType.IsAssignableFrom(groupType))
                continue;

            genericIface = iface;
            supportedGroupType = candidateGroupType;
            break;
        }

        if (genericIface is null || supportedGroupType is null)
            return new NestedGroupDrawerFactory(false, null);

        var drawMethod = genericIface.GetMethod(nameof(INestedGroupDrawer<object>.Draw))!;
        var drawerParam = Expression.Parameter(typeof(object), "drawer");
        var groupParam = Expression.Parameter(typeof(object), "group");
        var callExpr = Expression.Call(
            Expression.Convert(drawerParam, genericIface),
            drawMethod,
            Expression.Convert(groupParam, supportedGroupType));
        var invoker = Expression.Lambda<Action<object, object>>(callExpr, drawerParam, groupParam).Compile();

        return new NestedGroupDrawerFactory(true, invoker);
    }
}
