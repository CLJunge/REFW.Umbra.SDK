using Umbra.Config;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves custom parameter drawers recorded in <see cref="ParameterMetadata"/>.
/// </summary>
/// <remarks>
/// This type isolates custom-drawer activation and error handling from <see cref="ControlFactory"/>
/// so the factory can remain focused on choosing the appropriate control-building strategy.
/// </remarks>
internal static class ParameterDrawerResolver
{
    /// <summary>
    /// Attempts to create the highest-priority custom draw action for <paramref name="parameter"/>.
    /// </summary>
    /// <param name="parameter">The parameter whose metadata may specify a custom drawer.</param>
    /// <param name="label">The resolved display label for the parameter row.</param>
    /// <param name="alignGroup">The alignment group for two-column-aware controls.</param>
    /// <returns>
    /// A tuple containing the draw action and disposable resource when a custom drawer was resolved;
    /// otherwise <see langword="null"/>.
    /// </returns>
    internal static (Action draw, IDisposable? resource)? TryResolve(
        IParameter parameter,
        string label,
        LabelAlignmentGroup alignGroup)
    {
        var meta = parameter.Metadata;

        // 1. Full custom drawer — explicit opt-in, highest priority.
        if (meta.CustomDrawerType is { } customDrawerType)
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

        // 2. Two-column custom drawer — factory owns layout; drawer renders widget only.
        if (meta.TwoColumnCustomDrawerType is { } twoColumnDrawerType)
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
