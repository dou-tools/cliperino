namespace Cliperino.Core;

static class AppPaths
{
    static readonly string Local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    static readonly string Roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    static readonly (string Cache, string State)? Package = PackageFolders();

    public static readonly string Data = Package?.Cache ?? Path.Combine(Local, "Cliperino");
    public static readonly string Buffer = Path.Combine(Data, "buffer");
    public static readonly string Temp = Path.Combine(Data, "temp");
    public static readonly string Logs = Path.Combine(Data, "logs");
    public static readonly string FfmpegDir = Path.Combine(Data, "ffmpeg");
    public static readonly string SettingsFile = Path.Combine(Package?.State ?? Path.Combine(Roaming, "Cliperino"), "settings.json");
    public static readonly string? PreviousSettingsFile = Package != null ? Path.Combine(Roaming, "Cliperino", "settings.json") : null;
    public static readonly string DefaultClips =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Cliperino");

    static (string Cache, string State)? PackageFolders()
    {
#if STORE
        try
        {
            var folders = Windows.Storage.ApplicationData.Current;
            return (folders.LocalCacheFolder.Path, folders.LocalFolder.Path);
        }
        catch (Exception)
        {
            return null;
        }
#else
        return null;
#endif
    }
}
