using static Cliperino.Core.Lang;
using System.Security.Cryptography;
namespace Cliperino.Core;

public static class Downloader
{
    const int BufferSize = 1 << 16;

    public static HttpClient CreateClient(TimeSpan timeout)
    {
        var http = new HttpClient { Timeout = timeout };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Cliperino");
        return http;
    }

    public static async Task DownloadAsync(
        string url, string sha256, string path, string label, IProgress<(string Text, int? Percent)> progress, CancellationToken cancellation)
    {
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var http = CreateClient(Timeout.InfiniteTimeSpan);
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            var total = response.Content.Headers.ContentLength;
            await using var source = await response.Content.ReadAsStreamAsync(cancellation);
            await using var target = File.Create(path);
            var buffer = new byte[BufferSize];
            long done = 0;
            int read, lastMark = -1;
            while ((read = await source.ReadAsync(buffer, cancellation)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), cancellation);
                hash.AppendData(buffer, 0, read);
                done += read;
                int mark = total > 0 ? (int)(done * 100 / total.Value) : (int)(done >> 20);
                if (mark == lastMark) continue;
                lastMark = mark;
                progress.Report(total > 0
                    ? (T($"{label}… {done >> 20} de {total.Value >> 20} MB", $"{label}… {done >> 20} of {total.Value >> 20} MB"), mark)
                    : ($"{label}… {mark} MB", null));
            }
            if (!Convert.ToHexString(hash.GetHashAndReset()).Equals(sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(T("El archivo descargado no coincide con el original, así que no se usó.", "The downloaded file doesn't match the original, so it wasn't used."));
        }
        catch
        {
            File.Delete(path);
            throw;
        }
    }
}
