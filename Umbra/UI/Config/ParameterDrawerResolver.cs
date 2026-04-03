using Umbra.Config;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves custom parameter drawers recorded in <see cref="ParameterMetadata"/>.
/// </summary>
/// <remarks>
/// This type isolates custom-drawer activation and error handling from <see cref="ControlFactory"/>. Full custom drawers declared through <see cref="ParameterMetadata.DrawerType"/> take priority over two-column custom drawers declared through <see cref="ParameterMetadata.TwoColumnDrawerType"/>.
/// </remarks>
internal static class ParameterDrawerResolver
{
    /// <summary>
    /// Attempts to create the highest-priority custom draw action for <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The parameter whose metadata may specify a custom drawer.</param>
    /// <param name="label">The resolved display label for the parameter row.</param>
    /// <param name="alignGroup">The alignment group used when a two-column custom drawer is resolved.</param>
    /// <returns>A tuple containing the draw action and disposable drawer resource when a custom drawer was resolved successfully; otherwise, <see langword="null"/>.</returns>
    /// <remarks>
    /// If instantiation of a recorded custom drawer type throws, the exception is logged and resolution falls through to the next supported drawer shape or to the built-in control path in <see cref="ControlFactory"/>.
    /// </remarks>
    internal static (Action draw, IDisposable? resource)? TryResolve(
        IParameter parameter,
        string label,
        LabelAlignmentGroup alignGroup)
    {
        var meta = parameter.Metadata;

        if (meta.DrawerType is { } customDrawerType)
        {
            try
            {
                var drawer = (IParameterDrawer)Activator.CreateInstance(customDrawerType)!;
                return (() => drawer.Draw(label, parameter), drawer);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"ConfigDrawer: failed to instantiate custom drawer '{customDrawerType.Name}'.");
            }
        }

        if (meta.TwoColumnDrawerType is { } twoColumnDrawerType)
        {
            try
            {
                var drawer = (ITwoColumnParameterDrawer)Activator.CreateInstance(twoColumnDrawerType)!;
                var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);
                return (() =>
                {
                    layout.Pre();
                    drawer.Draw(parameter);
                }, drawer);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"ConfigDrawer: failed to instantiate two-column custom drawer '{twoColumnDrawerType.Name}'.");
            }
        }

        return null;
    }
}
