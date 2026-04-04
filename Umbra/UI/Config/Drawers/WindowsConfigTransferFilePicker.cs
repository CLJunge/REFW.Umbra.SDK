using System.Runtime.InteropServices;
using System.Text;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Uses the native Windows common file dialogs for config transfer path selection.
/// </summary>
internal sealed class WindowsConfigTransferFilePicker : IConfigTransferFilePicker
{
    private const int MaxPathLength = 1024;
    private const string Filter = "JSON Files (*.json)\0*.json\0All Files (*.*)\0*.*\0\0";
    private const int OfnPathMustExist = 0x00000800;
    private const int OfnFileMustExist = 0x00001000;
    private const int OfnOverwritePrompt = 0x00000002;
    private const int OfnNoChangeDir = 0x00000008;

    public bool TryPickImportPath(string? currentPath, out string? selectedPath)
        => TryPickPath(currentPath, "Choose Config File for Import", forExport: false, out selectedPath);

    public bool TryPickExportPath(string? currentPath, out string? selectedPath)
        => TryPickPath(currentPath, "Choose Config File Destination", forExport: true, out selectedPath);

    private static bool TryPickPath(string? currentPath, string title, bool forExport, out string? selectedPath)
    {
        selectedPath = null;
        if (!OperatingSystem.IsWindows())
            return false;

        var buffer = new StringBuilder(MaxPathLength);
        SeedInitialPath(buffer, currentPath);
        var initialDirectory = GetExistingDirectory(currentPath);
        var dialog = new OpenFileName
        {
            lStructSize = Marshal.SizeOf<OpenFileName>(),
            lpstrFilter = Filter,
            lpstrFile = buffer,
            nMaxFile = buffer.Capacity,
            lpstrInitialDir = initialDirectory,
            lpstrTitle = title,
            Flags = OfnNoChangeDir | OfnPathMustExist,
            lpstrDefExt = "json"
        };

        if (!forExport)
            dialog.Flags |= OfnFileMustExist;
        else
            dialog.Flags |= OfnOverwritePrompt;

        var success = forExport
            ? GetSaveFileName(dialog)
            : GetOpenFileName(dialog);
        if (!success)
            return false;

        selectedPath = dialog.lpstrFile.ToString();
        return !string.IsNullOrWhiteSpace(selectedPath);
    }

    private static void SeedInitialPath(StringBuilder buffer, string? currentPath)
    {
        if (string.IsNullOrWhiteSpace(currentPath))
            return;

        buffer.Append(currentPath);
    }

    private static string? GetExistingDirectory(string? currentPath)
    {
        if (string.IsNullOrWhiteSpace(currentPath))
            return null;

        if (Directory.Exists(currentPath))
            return currentPath;

        var directoryPath = Path.GetDirectoryName(currentPath);
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            return null;

        return directoryPath;
    }

    [DllImport("comdlg32.dll", EntryPoint = "GetOpenFileNameW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName openFileName);

    [DllImport("comdlg32.dll", EntryPoint = "GetSaveFileNameW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSaveFileName([In, Out] OpenFileName openFileName);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class OpenFileName
    {
        public int lStructSize;
        public nint hwndOwner;
        public nint hInstance;
        public string? lpstrFilter;
        public string? lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public StringBuilder lpstrFile = null!;
        public int nMaxFile;
        public StringBuilder? lpstrFileTitle;
        public int nMaxFileTitle;
        public string? lpstrInitialDir;
        public string? lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string? lpstrDefExt;
        public nint lCustData;
        public nint lpfnHook;
        public string? lpTemplateName;
        public nint pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }
}
