namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Deterministic file-picker test double for <see cref="ConfigTransferDrawer"/> tests.
/// </summary>
internal sealed class TestConfigTransferFilePicker : IConfigTransferFilePicker
{
    public int ImportPickCallCount { get; private set; }

    public int ExportPickCallCount { get; private set; }

    public string? LastImportCurrentPath { get; private set; }

    public string? LastImportFallbackDirectory { get; private set; }

    public string? LastExportCurrentPath { get; private set; }

    public string? LastExportFallbackDirectory { get; private set; }

    public Queue<(bool Success, string? SelectedPath)> ImportResults { get; } = new();

    public Queue<(bool Success, string? SelectedPath)> ExportResults { get; } = new();

    public bool TryPickImportPath(string? currentPath, string? fallbackDirectory, out string? selectedPath)
    {
        ImportPickCallCount++;
        LastImportCurrentPath = currentPath;
        LastImportFallbackDirectory = fallbackDirectory;
        if (ImportResults.Count == 0)
        {
            selectedPath = null;
            return false;
        }

        var result = ImportResults.Dequeue();
        selectedPath = result.SelectedPath;
        return result.Success;
    }

    public bool TryPickExportPath(string? currentPath, string? fallbackDirectory, out string? selectedPath)
    {
        ExportPickCallCount++;
        LastExportCurrentPath = currentPath;
        LastExportFallbackDirectory = fallbackDirectory;
        if (ExportResults.Count == 0)
        {
            selectedPath = null;
            return false;
        }

        var result = ExportResults.Dequeue();
        selectedPath = result.SelectedPath;
        return result.Success;
    }
}
