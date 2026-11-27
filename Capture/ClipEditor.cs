using static Cliperino.Core.Lang;
using System.Globalization;
using System.Text.RegularExpressions;
using Cliperino.Core;
using Cliperino.Ffmpeg;

namespace Cliperino.Capture;

public sealed record ClipEdit(double Start, double End, bool RemoveAudio, bool RemoveMicrophone, bool Exact, bool Replace);

public sealed record ClipInfo(int AudioTracks, VideoCodec Codec, double Fps)
{
    const int SeparateTracks = 3;

    public bool HasAudio => AudioTracks > 0;
    public bool SeparateMicrophone => AudioTracks >= SeparateTracks;
}

public static class ClipEditor
{
    const double MinimumDuration = 0.1;
    const int ReplaceAttempts = 10;
    static readonly TimeSpan ExportTimeout = TimeSpan.FromMinutes(15);
    static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(10);
    static readonly TimeSpan ReplaceRetryDelay = TimeSpan.FromMilliseconds(300);

    public static async Task<ClipInfo> ProbeAsync(string ffmpeg, string path)
    {
        var (_, output) = await FfmpegProcess.RunAsync(ffmpeg, $"-hide_banner -nostdin -i \"{path}\"", ProbeTimeout);
        var fps = Regex.Match(output, @"Video:.*?([\d.]+) fps");
        return new ClipInfo(
            Regex.Matches(output, @"Stream #\d+:\d+\S*: Audio:").Count,
            output.Contains("Video: hevc") ? VideoCodec.Hevc : VideoCodec.H264,
            fps.Success && double.TryParse(fps.Groups[1].Value, CultureInfo.InvariantCulture, out var value) ? value : 60);
    }

    public static async Task<string> ExportAsync(string ffmpeg, string input, ClipEdit edit, ClipInfo info, string exactEncoderArgs)
    {
        var duration = edit.End - edit.Start;
        if (duration < MinimumDuration) throw new InvalidOperationException(T("El recorte es demasiado corto.", "The trim is too short."));

        var directory = Path.GetDirectoryName(input)!;
        var name = Path.GetFileNameWithoutExtension(input);
        var target = edit.Replace
            ? Path.Combine(directory, $"{name}.editando.mp4")
            : ClipSaver.UniquePath(Path.Combine(directory, T($"{name}_editado.mp4", $"{name}_edited.mp4")));
        var video = edit.Exact ? exactEncoderArgs : "-c:v copy";
        var tracks = edit.RemoveMicrophone && info.SeparateMicrophone ? "-map 0:a:1 -disposition:a:0 default" : "-map 0:a?";
        var audio = edit.RemoveAudio ? "-an" : edit.Exact ? "-c:a aac -b:a 192k" : "-c:a copy";
        var args = $"-hide_banner -loglevel error -nostdin -y -ss {Seconds(edit.Start)} -i \"{input}\" -t {Seconds(duration)} " +
                   $"-map 0:v {tracks} {video} {ClipSaver.Mp4Tag(info.Codec)}{audio} -avoid_negative_ts make_zero -movflags +faststart \"{target}\"";

        var (code, log) = await FfmpegProcess.RunAsync(ffmpeg, args, ExportTimeout);
        if (code != 0 || !File.Exists(target))
        {
            TryDelete(target);
            var reason = log.Split('\n', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim();
            throw new InvalidOperationException(reason ?? T($"ffmpeg terminó con código {code}.", $"ffmpeg exited with code {code}."));
        }
        if (!edit.Replace) return target;

        try
        {
            await ReplaceAsync(target, input);
        }
        catch
        {
            TryDelete(target);
            throw;
        }
        return input;
    }

    static async Task ReplaceAsync(string source, string destination)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                File.Move(source, destination, overwrite: true);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException && attempt < ReplaceAttempts)
            {
                await Task.Delay(ReplaceRetryDelay);
            }
        }
    }

    static string Seconds(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
