using Microsoft.Win32;

namespace AroundTheWorldClocks.Services;

/// <summary>
/// Starts the widget at Windows sign-in through the current user's Run registry key
/// (HKCU\Software\Microsoft\Windows\CurrentVersion\Run). No admin rights needed.
/// The registry is the source of truth, so the setting also reflects changes made in Task Manager > Startup apps.
/// </summary>
public static class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "AroundTheWorldClocks";

    private static string ExePath => Environment.ProcessPath
        ?? throw new InvalidOperationException("Cannot determine the executable path.");

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is string command
                && string.Equals(command.Trim('"'), ExePath, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (enabled)
            key.SetValue(ValueName, $"\"{ExePath}\"");
        else
            key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
