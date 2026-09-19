using BatteryTool.Core;
using Microsoft.Win32;

namespace BatteryTool.Winform;

/// <summary>Per-user "run at startup" via the HKCU Run key (no admin rights needed).</summary>
internal static class AutoStart
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "BatteryTool";

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is string value &&
                string.Equals(value, BuildCommand(), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException or InvalidOperationException)
        {
            System.Diagnostics.Trace.WriteLine($"读取自启动状态失败：{ex.Message}");
            return false;
        }
    }

    public static bool SetEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (enable) key.SetValue(ValueName, BuildCommand());
            else if (key.GetValue(ValueName) is not null) key.DeleteValue(ValueName);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException or InvalidOperationException)
        {
            System.Diagnostics.Trace.WriteLine($"设置自启动失败：{ex.Message}");
            return false;
        }
    }

    // Startup display preferences are read by Form1, not embedded in the Run command.
    internal static string BuildCommand()
    {
        string executable = Environment.ProcessPath
            ?? throw new InvalidOperationException(I18n.ProcessPathUnknown);
        // Also support launching the framework-dependent assembly through dotnet.
        return string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase)
            ? $"\"{executable}\" \"{typeof(AutoStart).Assembly.Location}\""
            : $"\"{executable}\"";
    }
}
