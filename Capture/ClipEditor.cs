using static Cliperino.Core.Lang;
using System.Globalization;
using System.Text.RegularExpressions;
using Cliperino.Core;
using Cliperino.Ffmpeg;

namespace Cliperino.Capture;

public sealed record ClipEdit(double Start, double End, bool RemoveAudio, bool RemoveMicrophone, bool Exact, bool Replace, int? MaxMegabytes);

public sealed record ClipInfo(int AudioTracks, VideoCodec Codec, double Fps, Size Size)
{
    const int SeparateTracks = 3;

    public bool HasAudio => AudioTracks > 0;
    public bool SeparateMicrophone => AudioTracks >= SeparateTracks;
}

public static class ClipEditor
{
    const double MinimumDuration = 0.1;
    const int ReplaceAttempts = 10;
    const long BytesPerMegabyte = 1_000_000;
    const double SizeMargin = 0.95;
    const int CompressAttempts = 3;
    const int CompressedAudioKbps = 96;
    const int MinimumVideoKbps = 150;
    const int MaxCompressedFps = 60;
    const int LowBitrateFps = 30;
    const int LowBitrateKbps = 3000;
    const double MinimumBitsPerPixel = 0.04;
    static readonly int[] Heights = [2160, 1440, 1080, 720, 480, 360];
    static readonly TimeSpan ExportTimeout = TimeSpan.FromMinutes(15);
    static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(10);
    static readonly TimeSpan ReplaceRetryDelay = TimeSpan.FromMilliseconds(300);

    public static async Task<ClipInfo> ProbeAsync(string ffmpeg, string path)
    {
        var (_, output) = await FfmpegProcess.RunAsync(ffmpeg, $"-hide_banner -nostdin -i \"{path}\"", ProbeTimeout);
        var fps = Regex.Match(output, @"Video:.*?([\d.]+) fps");
        var size = Regex.Match(output, @"Video:.*?, (\d{2,5})x(\d{2,5})");
        return new ClipInfo(
            Regex.Matches(output, @"Stream #\d+:\d+\S*: Audio:").Count,
            output.Contains("Video: hevc") ? VideoCodec.Hevc : VideoCodec.H264,
            fps.Success && double.TryParse(fps.Groups[1].Value, CultureInfo.InvariantCulture, out var value) ? value : 60,
            size.Success ? new Size(int.Parse(size.Groups[1].Value), int.Parse(size.Groups[2].Value)) : new Size(1920, 1080));
    }

    public static async Task<string> ExportAsync(
        string ffmpeg, string input, ClipEdit edit, ClipInfo info, string exactEncoderArgs, Func<int, string> sizedEncoderArgs)
    {
        var duration = edit.End - edit.Start;
        if (duration < MinimumDuration) throw new InvalidOperationException(T("El recorte es demasiado corto.", "The trim is too short."));

        var directory = Path.GetDirectoryName(input)!;
        var name = Path.GetFileNameWithoutExtension(input);
        var fileName = edit.MaxMegabytes is { } mb ? $"{name}_{mb}MB.mp4" : T($"{name}_editado.mp4", $"{name}_edited.mp4");
        var target = edit.Replace
            ? Path.Combine(directory, $"{name}.editando.mp4")
            : ClipSaver.UniquePath(Path.Combine(directory, fileName));
        var head = $"-hide_banner -loglevel error -nostdin -y -ss {Seconds(edit.Start)} -i \"{input}\" -t {Seconds(duration)} -map 0:v";
        var tail = $"-avoid_negative_ts make_zero -movflags +faststart \"{target}\"";

        if (edit.MaxMegabytes is { } megabytes)
        {
            await CompressAsync(ffmpeg, head, tail, target, edit, info, duration, megabytes, sizedEncoderArgs);
        }
        else
        {
            var video = edit.Exact ? exactEncoderArgs : "-c:v copy";
            var tracks = edit.RemoveMicrophone && info.SeparateMicrophone ? "-map 0:a:1 -disposition:a:0 default" : "-map 0:a?";
            var audio = edit.RemoveAudio ? "-an" : edit.Exact ? "-c:a aac -b:a 192k" : "-c:a copy";
            await RunAsync(ffmpeg, $"{head} {tracks} {video} {ClipSaver.Mp4Tag(info.Codec)}{audio} {tail}", target);
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

    static async Task CompressAsync(string ffmpeg, string head, string tail, string target,
        ClipEdit edit, ClipInfo info, double duration, int megabytes, Func<int, string> sizedEncoderArgs)
    {
        long limit = megabytes * BytesPerMegabyte;
        int audioKbps = edit.RemoveAudio || !info.HasAudio ? 0 : CompressedAudioKbps;
        var audio = audioKbps == 0 ? "-an" : $"-map 0:a:{(edit.RemoveMicrophone && info.SeparateMicrophone ? 1 : 0)} -c:a aac -b:a {audioKbps}k -ac 2";
        int videoKbps = (int)(limit * 8 * SizeMargin / 1000 / duration) - audioKbps;
        var (fps, height) = Fit(info, videoKbps);
        var filters = new List<string>();
        if (height < info.Size.Height) filters.Add($"scale=-2:{height}:flags=lanczos");
        if (fps < info.Fps - 0.5) filters.Add($"fps={fps}");
        filters.Add("format=nv12");
        for (int attempt = 1; ; attempt++)
        {
            if (videoKbps < MinimumVideoKbps)
            {
                TryDelete(target);
                throw new InvalidOperationException(T($"El recorte es muy largo para {megabytes} MB, acortalo un poco.", $"The trim is too long for {megabytes} MB, make it a bit shorter."));
            }

            await RunAsync(ffmpeg, $"{head} -vf \"{string.Join(',', filters)}\" {sizedEncoderArgs(videoKbps)} -g {fps * 2} {audio} {tail}", target);

            long size = new FileInfo(target).Length;
            Log.Write($"Clip comprimido a {videoKbps} kbps, {height}p {fps} fps: {size / 1024} KB de {limit / 1024} KB");
            if (size <= limit) return;
            if (attempt == CompressAttempts)
            {
                TryDelete(target);
                throw new InvalidOperationException(T($"No se pudo dejar en {megabytes} MB, probá con un recorte más corto.", $"Couldn't get it under {megabytes} MB, try a shorter trim."));
            }
            videoKbps = (int)(videoKbps * limit * SizeMargin / size);
        }
    }

    static (int Fps, int Height) Fit(ClipInfo info, int videoKbps)
    {
        int fps = (int)Math.Round(Math.Min(info.Fps, MaxCompressedFps));
        if (videoKbps < LowBitrateKbps) fps = Math.Min(fps, LowBitrateFps);
        double aspect = info.Size.Width / (double)info.Size.Height;
        var heights = Heights.Where(h => h < info.Size.Height).Prepend(info.Size.Height).ToList();
        foreach (var height in heights)
        {
            if (videoKbps * 1000.0 / (height * aspect * height * fps) >= MinimumBitsPerPixel) return (fps, height);
        }
        return (fps, heights[^1]);
    }

    static async Task RunAsync(string ffmpeg, string args, string target)
    {
        var (code, log) = await FfmpegProcess.RunAsync(ffmpeg, args, ExportTimeout);
        if (code == 0 && File.Exists(target)) return;
        TryDelete(target);
        var reason = log.Split('\n', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim();
        throw new InvalidOperationException(reason ?? T($"ffmpeg terminó con código {code}.", $"ffmpeg exited with code {code}."));
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
