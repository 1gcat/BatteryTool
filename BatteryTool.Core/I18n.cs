using System.Globalization;

namespace BatteryTool.Core;

public enum AppLanguage { Zh, En }

/// <summary>
/// UI and device-facing strings. First launch follows the OS UI language;
/// afterwards the saved choice in settings.json wins.
/// </summary>
public static class I18n
{
    public static AppLanguage Current { get; private set; } = AppLanguage.Zh;

    public static bool IsZh => Current == AppLanguage.Zh;

    public static string Code => IsZh ? "zh" : "en";

    public static void Set(string? code)
    {
        Current = IsChineseCode(code) ? AppLanguage.Zh : AppLanguage.En;
        var culture = IsZh ? new CultureInfo("zh-CN") : new CultureInfo("en-US");
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public static string DetectFromSystem()
    {
        for (var culture = CultureInfo.CurrentUICulture;
             culture is not null && !string.IsNullOrEmpty(culture.Name);
             culture = culture.Parent)
        {
            if (culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase))
                return "zh";
            if (culture.Equals(CultureInfo.InvariantCulture)) break;
        }
        return "en";
    }

    public static bool IsChineseCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) &&
        (code.Equals("zh", StringComparison.OrdinalIgnoreCase) ||
         code.StartsWith("zh-", StringComparison.OrdinalIgnoreCase));

    public static string T(string zh, string en) => IsZh ? zh : en;

    public static string DisplayDeviceName(string englishName)
    {
        if (!IsZh) return englishName;
        if (englishName == "ROG Mouse (OMNI Receiver)") return "ROG 鼠标（OMNI 接收器）";
        return englishName.Replace(" (USB)", "（USB）", StringComparison.Ordinal);
    }

    public static string FormTitle => T("BatteryTool — 外设电量", "BatteryTool — Peripheral Battery");
    public static string ColumnDevice => T("设备", "Device");
    public static string ColumnBattery => T("电量", "Battery");
    public static string ColumnStatus => T("状态", "Status");
    public static string ColumnUpdated => T("上次成功读取", "Last success");
    public static string Refresh => T("立即刷新", "Refresh");
    public static string AutoRefresh => T("自动刷新：", "Auto-refresh:");
    public static string DetailHint => T("选择设备可查看连接状态和故障说明。", "Select a device to see connection status and details.");
    public static string StartMinimized => T("启动时最小化到托盘", "Minimize to tray at startup");
    public static string AutoStart => T("开机自动启动（当前用户登录时）", "Start with Windows (current user)");
    public static string TrayLegend => T(
        "托盘：左键盘 / 右鼠标 · 绿 >50% / 黄 ≤50% / 红 ≤20% · 灰：旧数据 · ?：未知 · 蓝：充电",
        "Tray: left keyboard / right mouse · green >50% / yellow ≤50% / red ≤20% · gray: stale · ?: unknown · blue: charging");
    public static string MinimizeToTray => T("最小化到托盘", "Minimize to tray");
    public static string StatusReady => T("就绪", "Ready");
    public static string StatusReading => T("正在读取电量…", "Reading battery…");
    public static string MenuShow => T("显示窗口", "Show window");
    public static string MenuRefresh => T("立即刷新", "Refresh now");
    public static string MenuExit => T("退出", "Exit");
    public static string AutostartFailed => T("无法修改开机自启动配置，请检查当前用户的注册表权限。",
        "Could not change startup settings. Check registry permission for the current user.");
    public static string SettingsSaveFailed => T("无法保存设置；本次更改仍然有效，但重启后可能丢失。",
        "Could not save settings. The change applies now but may be lost after restart.");
    public static string Unknown => T("未知", "n/a");
    public static string StateCharging => T("充电中", "Charging");
    public static string StateLow => T("电量低", "Low");
    public static string StateOk => T("正常", "OK");
    public static string StateNoResponse => T("无响应（可能休眠）", "No response (asleep?)");
    public static string StateUnsupported => T("不支持当前模式", "Unsupported mode");
    public static string StateDisconnected => T("未连接", "Disconnected");
    public static string StateError => T("读取失败", "Read failed");
    public static string StaleSuffix => T("（旧数据）", " (stale)");
    public static string CherryMx3 => T("Cherry MX 3.0S 无线版", "Cherry MX 3.0S Wireless");
    public static string CherryMx2 => "Cherry MX 2.0S";
    public static string CherryKeyboard => T("Cherry 键盘", "Cherry keyboard");
    public static string RogMouse => T("ROG 鼠标", "ROG mouse");
    public static string CherryNotFound => T("未检测到 Cherry 设备。请连接 MX 2.0S 或 MX 3.0S 无线接收器。",
        "No Cherry device found. Connect an MX 2.0S or MX 3.0S wireless receiver.");
    public static string RogNotFound => T("未检测到支持的 ROG 鼠标通道。请连接 USB 接收器或数据线；暂不支持蓝牙。",
        "No supported ROG mouse channel found. Connect a USB receiver or cable; Bluetooth is not supported.");
    public static string MouseAsleep => T("接收器在线但鼠标未响应（FF AA）：鼠标可能已休眠或关机，唤醒后刷新。",
        "Receiver is online but the mouse did not answer (FF AA). It may be asleep or off; wake it and refresh.");
    public static string NoValidBattery => T("未收到有效电量：设备可能休眠、关闭或被其他软件占用。唤醒后刷新；不将无效的 0 值当作 0%。",
        "No valid battery reading. The device may be asleep, off, or in use by another app. Wake it and refresh; a raw 0 is not treated as 0%.");
    public static string DeviceDisconnected => T("设备已断开；显示上次成功读取的电量。",
        "Device disconnected; showing the last successful reading.");
    public static string MockKeyboard => T("Mock 键盘（演示数据）", "Mock keyboard (demo data)");
    public static string MockMouse => T("Mock 鼠标（演示数据）", "Mock mouse (demo data)");
    public static string MockKeyboardDetail => T("MOCK：模拟 Cherry 键盘，用于预览界面效果。",
        "MOCK: simulated Cherry keyboard for previewing the UI.");
    public static string MockMouseSleep => T("MOCK：模拟鼠标休眠（FF AA），托盘应显示旧数据（灰色）。",
        "MOCK: simulated mouse sleep (FF AA); the tray should show stale (gray) data.");
    public static string MockMouseDetail => T("MOCK：模拟 ROG 鼠标，用于预览界面效果。",
        "MOCK: simulated ROG mouse for previewing the UI.");
    public static string ProcessPathUnknown => T("无法确定程序路径。", "Could not determine the process path.");

    public static object[] IntervalItems => IsZh
        ? ["5 秒", "10 秒", "20 秒", "30 秒", "60 秒", "120 秒", "关闭"]
        : ["5 s", "10 s", "20 s", "30 s", "60 s", "120 s", "Off"];

    public static string StatusDone(int ready, DateTime time) => T(
        $"检测完成 {time:HH:mm:ss} · 成功读取 {ready} 台；选择设备查看详情",
        $"Updated {time:HH:mm:ss} · {ready} device(s) read. Select a device for details.");

    public static string StatusReadFailed(string message) => T($"读取失败：{message}", $"Read failed: {message}");

    public static string ReadFailedDetail(string message) => T(
        $"读取失败：{message}。可尝试退出其他外设管理软件后刷新。",
        $"Read failed: {message}. Close other peripheral software and refresh.");

    public static string CherryUnsupportedChannel(int productId) => T(
        $"检测到 Cherry PID {productId:X4}，但未找到已支持的 01AC 或 00EA / Col04 通道，未发送命令。",
        $"Cherry PID {productId:X4} was found, but no supported 01AC or 00EA / Col04 channel is present. No command was sent.");

    public static string TrayText(string keyboard, string mouse) => T(
        $"左·键盘 {keyboard} / 右·鼠标 {mouse}",
        $"KB {keyboard} / MS {mouse}");

    public static string DescribePercent(int percent, bool stale, bool charging)
    {
        string suffix = stale ? T("（旧）", " stale") : charging ? T("（充电）", " chg") : "";
        return $"{percent}%{suffix}";
    }

    public static string JoinDetail(string descriptor, string detail) =>
        IsZh ? $"{descriptor}；{detail}" : $"{descriptor}; {detail}";
}
