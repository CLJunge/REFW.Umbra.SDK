using Umbra.Config;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the persisted parameters required by Umbra's config transfer control.
/// </summary>
/// <remarks>
/// Implement this contract on a nested config group when the group should be rendered by
/// <see cref="ConfigTransferDrawer"/>. The control edits the path parameters and invokes the
/// action parameters, but it does not own the import or export business logic.
/// </remarks>
public interface IConfigTransferGroup
{
    /// <summary>
    /// Gets the source path used by the import action.
    /// </summary>
    Parameter<string> ImportPath { get; }

    /// <summary>
    /// Gets the destination path used by the export action.
    /// </summary>
    Parameter<string> ExportPath { get; }

    /// <summary>
    /// Gets the action invoked to import config data.
    /// </summary>
    Parameter<Action> ImportConfig { get; }

    /// <summary>
    /// Gets the action invoked to export config data.
    /// </summary>
    Parameter<Action> ExportConfig { get; }
}
