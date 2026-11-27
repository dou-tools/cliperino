using System.Runtime.InteropServices;
using Cliperino.Core;

namespace Cliperino.Capture;

public static class ClipStorage
{
    public const long BytesPerGigabyte = 1_000_000_000;
    const string ClipPattern = "Cliperino_*.mp4";
    const uint FO_DELETE = 3;
    const ushort FOF_SILENT = 0x4, FOF_NOCONFIRMATION = 0x10, FOF_ALLOWUNDO = 0x40, FOF_NOERRORUI = 0x400;
    static readonly EnumerationOptions Options = new() { IgnoreInaccessible = true };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        public string pFrom;
        public string? pTo;
        public ushort fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string? lpszProgressTitle;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)] static extern int SHFileOperation(ref SHFILEOPSTRUCT operation);

    public static long UsedBytes(string folder) => Clips(folder).Sum(f => f.Length);

    public static void Trim(string folder, int maxGigabytes, string keep)
    {
        try
        {
            TrimOldest(folder, maxGigabytes, keep);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Write($"No se pudo revisar el espacio de los clips: {ex.Message}");
        }
    }

    static void TrimOldest(string folder, int maxGigabytes, string keep)
    {
        var clips = Clips(folder).OrderBy(f => f.LastWriteTimeUtc).ToList();
        long total = clips.Sum(f => f.Length);
        long limit = maxGigabytes * BytesPerGigabyte;
        int removed = 0;
        long freed = 0;
        foreach (var clip in clips)
        {
            if (total <= limit) break;
            if (string.Equals(clip.FullName, Path.GetFullPath(keep), StringComparison.OrdinalIgnoreCase)) continue;
            if (!SendToRecycleBin(clip.FullName))
            {
                Log.Write($"No se pudo mandar a la papelera {clip.FullName}.");
                continue;
            }
            total -= clip.Length;
            freed += clip.Length;
            removed++;
        }
        if (removed > 0) Log.Write($"Se mandaron a la papelera {removed} clips viejos ({freed / (double)BytesPerGigabyte:0.0} GB) para no pasar de {maxGigabytes} GB.");
    }

    static IEnumerable<FileInfo> Clips(string folder)
    {
        var root = new DirectoryInfo(folder);
        if (!root.Exists) return [];
        return root.EnumerateFiles(ClipPattern, Options)
            .Concat(root.EnumerateDirectories("*", Options).SelectMany(d => d.EnumerateFiles(ClipPattern, Options)));
    }

    static bool SendToRecycleBin(string path)
    {
        var operation = new SHFILEOPSTRUCT
        {
            wFunc = FO_DELETE,
            pFrom = path + '\0',
            fFlags = FOF_SILENT | FOF_NOCONFIRMATION | FOF_ALLOWUNDO | FOF_NOERRORUI,
        };
        return SHFileOperation(ref operation) == 0 && !operation.fAnyOperationsAborted && !File.Exists(path);
    }
}
