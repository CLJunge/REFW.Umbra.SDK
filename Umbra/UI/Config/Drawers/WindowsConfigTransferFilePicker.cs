using System.Runtime.InteropServices;
namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Uses the native Windows common file dialogs for config transfer path selection.
/// </summary>
internal sealed partial class WindowsConfigTransferFilePicker : IConfigTransferFilePicker
{
    private const int MaxPathLength = 1024;
    private const string Filter = "JSON Files (*.json)\0*.json\0All Files (*.*)\0*.*\0\0";
    private const int OfnPathMustExist = 0x00000800;
    private const int OfnFileMustExist = 0x00001000;
    private const int OfnOverwritePrompt = 0x00000002;
    private const int OfnNoChangeDir = 0x00000008;

    public bool TryPickImportPath(string? currentPath, string? fallbackDirectory, out string? selectedPath)
        => TryPickPath(currentPath, fallbackDirectory, "Choose Config File for Import", forExport: false, out selectedPath);

    public bool TryPickExportPath(string? currentPath, string? fallbackDirectory, out string? selectedPath)
        => TryPickPath(currentPath, fallbackDirectory, "Choose Config File Destination", forExport: true, out selectedPath);

    private static bool TryPickPath(string? currentPath, string? fallbackDirectory, string title, bool forExport, out string? selectedPath)
    {
        selectedPath = null;
        if (!OperatingSystem.IsWindows())
            return false;

        var initialDirectory = GetInitialDirectory(currentPath, fallbackDirectory);
        var fileBuffer = AllocatePathBuffer(currentPath);
        var filterPointer = Marshal.StringToHGlobalUni(Filter);
        var initialDirectoryPointer = initialDirectory is null ? nint.Zero : Marshal.StringToHGlobalUni(initialDirectory);
        var titlePointer = Marshal.StringToHGlobalUni(title);
        var defaultExtensionPointer = Marshal.StringToHGlobalUni("json");

        try
        {
            var dialog = new OpenFileName
            {
                lStructSize = Marshal.SizeOf<OpenFileName>(),
                lpstrFilter = filterPointer,
                lpstrFile = fileBuffer,
                nMaxFile = MaxPathLength,
                lpstrInitialDir = initialDirectoryPointer,
                lpstrTitle = titlePointer,
                Flags = OfnNoChangeDir | OfnPathMustExist,
                lpstrDefExt = defaultExtensionPointer
            };

            if (!forExport)
                dialog.Flags |= OfnFileMustExist;
            else
                dialog.Flags |= OfnOverwritePrompt;

            var success = forExport
                ? GetSaveFileName(ref dialog)
                : GetOpenFileName(ref dialog);
            if (!success)
                return false;

            selectedPath = Marshal.PtrToStringUni(dialog.lpstrFile);
            return !string.IsNullOrWhiteSpace(selectedPath);
        }
        finally
        {
            Marshal.FreeHGlobal(fileBuffer);
            Marshal.FreeHGlobal(filterPointer);
            if (initialDirectoryPointer != nint.Zero)
                Marshal.FreeHGlobal(initialDirectoryPointer);

            Marshal.FreeHGlobal(titlePointer);
            Marshal.FreeHGlobal(defaultExtensionPointer);
        }
    }

    private static nint AllocatePathBuffer(string? currentPath)
    {
        var bufferPointer = Marshal.AllocHGlobal(MaxPathLength * sizeof(char));
        var characters = new char[MaxPathLength];
        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            var copyLength = Math.Min(currentPath.Length, MaxPathLength - 1);
            currentPath.CopyTo(0, characters, 0, copyLength);
        }

        Marshal.Copy(characters, 0, bufferPointer, characters.Length);
        return bufferPointer;
    }

    private static string? GetInitialDirectory(string? currentPath, string? fallbackDirectory)
    {
        if (string.IsNullOrWhiteSpace(currentPath))
            return GetExistingDirectory(fallbackDirectory);

        if (Directory.Exists(currentPath))
            return currentPath;

        var directoryPath = Path.GetDirectoryName(currentPath);
        if (!string.IsNullOrWhiteSpace(directoryPath) && Directory.Exists(directoryPath))
            return directoryPath;

        return GetExistingDirectory(fallbackDirectory);
    }

    private static string? GetExistingDirectory(string? directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            return null;

        return directoryPath;
    }

    [LibraryImport("comdlg32.dll", EntryPoint = "GetOpenFileNameW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetOpenFileName(ref OpenFileName openFileName);

    [LibraryImport("comdlg32.dll", EntryPoint = "GetSaveFileNameW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSaveFileName(ref OpenFileName openFileName);

    [StructLayout(LayoutKind.Sequential)]
    private struct OpenFileName
    {
        public int lStructSize;
        public nint hwndOwner;
        public nint hInstance;
        public nint lpstrFilter;
        public nint lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public nint lpstrFile;
        public int nMaxFile;
        public nint lpstrFileTitle;
        public int nMaxFileTitle;
        public nint lpstrInitialDir;
        public nint lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public nint lpstrDefExt;
        public nint lCustData;
        public nint lpfnHook;
        public nint lpTemplateName;
        public nint pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }
}
