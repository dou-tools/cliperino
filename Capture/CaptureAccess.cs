using Cliperino.Core;
#if STORE
using Windows.Graphics.Capture;
#endif

namespace Cliperino.Capture;

static class CaptureAccess
{
    public static async Task AllowBorderlessAsync()
    {
#if STORE
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 20348)) return;
        try
        {
            var status = await GraphicsCaptureAccess.RequestAccessAsync(GraphicsCaptureAccessKind.Borderless);
            Log.Write($"Captura sin borde: {status}");
        }
        catch (Exception ex)
        {
            Log.Write($"No se pudo pedir la captura sin borde: {ex.Message}");
        }
#else
        await Task.CompletedTask;
#endif
    }
}
