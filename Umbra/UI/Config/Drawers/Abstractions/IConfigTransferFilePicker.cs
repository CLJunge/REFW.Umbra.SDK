namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the native file-picking operations used by <see cref="ConfigTransferDrawer"/>.
/// </summary>
/// <remarks>
/// The transfer drawer uses this seam only to let the user choose a path. Import and export logic remain owned by the configured action callbacks.
/// </remarks>
internal interface IConfigTransferFilePicker
{
    /// <summary>
    /// Tries to pick an existing file path for import.
    /// </summary>
    /// <param name="currentPath">The current path shown by the control, if any.</param>
    /// <param name="selectedPath">Receives the selected path when the user completes the picker.</param>
    /// <returns><see langword="true"/> when a path was selected; otherwise, <see langword="false"/>.</returns>
    bool TryPickImportPath(string? currentPath, out string? selectedPath);

    /// <summary>
    /// Tries to pick a destination file path for export.
    /// </summary>
    /// <param name="currentPath">The current path shown by the control, if any.</param>
    /// <param name="selectedPath">Receives the selected path when the user completes the picker.</param>
    /// <returns><see langword="true"/> when a path was selected; otherwise, <see langword="false"/>.</returns>
    bool TryPickExportPath(string? currentPath, out string? selectedPath);
}
