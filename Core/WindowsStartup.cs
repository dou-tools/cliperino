#if STORE
using Windows.ApplicationModel;
#else
using Microsoft.Win32;
#endif

namespace Cliperino.Core;

static class WindowsStartup
{
#if STORE
    const string TaskId = "CliperinoStartup";

    public static async Task<bool> IsEnabledAsync()
    {
        try
        {
            var task = await StartupTask.GetAsync(TaskId);
            return task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
        }
        catch (Exception ex)
        {
            Log.Write($"No se pudo leer el inicio con Windows: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> ApplyAsync(bool enabled)
    {
        try
        {
            var task = await StartupTask.GetAsync(TaskId);
            if (!enabled)
            {
                if (task.State == StartupTaskState.Enabled) task.Disable();
                return true;
            }
            var state = task.State == StartupTaskState.Disabled ? await task.RequestEnableAsync() : task.State;
            Log.Write($"Inicio con Windows: {state}");
            return state is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
        }
        catch (Exception ex)
        {
            Log.Write($"No se pudo actualizar el inicio con Windows: {ex.Message}");
            return false;
        }
    }
#else
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static Task<bool> IsEnabledAsync() => Task.FromResult(false);

    public static Task<bool> ApplyAsync(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKey);
            if (enabled) key.SetValue("Cliperino", $"\"{Environment.ProcessPath}\"");
            else key.DeleteValue("Cliperino", throwOnMissingValue: false);
        }
        catch (Exception ex)
        {
            Log.Write($"No se pudo actualizar el inicio con Windows: {ex.Message}");
        }
        return Task.FromResult(true);
    }
#endif
}
