using System.Text.Json;
using BatteryTool.Core;

namespace BatteryTool.Winform;

internal sealed class AppSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BatteryTool", "settings.json");

    public int IntervalSeconds { get; set; } = 30;
    public bool StartMinimized { get; set; }
    public string Language { get; set; } = "";

    /// <summary>
    /// Returns zh/en. On first launch (no saved language) this follows the OS UI language
    /// and persists the choice so later OS language changes do not override the user.
    /// </summary>
    public string ResolveLanguage()
    {
        if (Language is "zh" or "en") return Language;
        Language = I18n.DetectFromSystem();
        Save();
        return Language;
    }

    public static AppSettings Load()
    {
        try
        {
            var settings = File.Exists(FilePath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new()
                : new AppSettings();
            if (settings.IntervalSeconds is not (0 or 5 or 10 or 20 or 30 or 60 or 120))
                settings.IntervalSeconds = 30;
            return settings;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            System.Diagnostics.Trace.WriteLine($"读取设置失败：{ex.Message}");
            return new();
        }
    }

    public bool Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath + ".tmp", JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(FilePath + ".tmp", FilePath, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            System.Diagnostics.Trace.WriteLine($"保存设置失败：{ex.Message}");
            return false;
        }
    }
}
