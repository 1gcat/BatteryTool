<p align="right"><strong>English</strong> · <a href="./README.zh-CN.md">简体中文</a></p>

<p align="center">
  <img src="docs/logo.png" width="144" alt="BatteryTool">
</p>

# BatteryTool

Windows tray utility for **Cherry keyboard** and **ROG mouse** battery levels. It only sends HID battery queries — no lighting, DPI, pairing, or power-profile writes.

<!-- If the GitHub path is not 1gcat/BatteryTool, replace that slug in the badge / star-history URLs. -->
[![License: GPL v3](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4.svg)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D6.svg)](https://github.com/1gcat/BatteryTool)
[![Stars](https://img.shields.io/github/stars/1gcat/BatteryTool?style=flat)](https://github.com/1gcat/BatteryTool/stargazers)
[![Issues](https://img.shields.io/github/issues/1gcat/BatteryTool)](https://github.com/1gcat/BatteryTool/issues)
[![Last commit](https://img.shields.io/github/last-commit/1gcat/BatteryTool)](https://github.com/1gcat/BatteryTool/commits)

## Demo

<p align="center">
  <img src="docs/ui.png" width="720" alt="BatteryTool demo">
</p>

## Tray icon

One icon, two devices: **left = keyboard**, **right = mouse**. Green &gt;50%, yellow ≤50%, red ≤20%, gray = stale, `?` = unknown, blue bolt = charging.

| Normal | Charging | Low |
| :---: | :---: | :---: |
| <img src="docs/tray/normal.png" width="64" alt="Normal"> | <img src="docs/tray/charging.png" width="64" alt="Charging"> | <img src="docs/tray/low.png" width="64" alt="Low"> |
| 87% / 66% | charging | ≤20% |

| Stale | Unknown | Keyboard only |
| :---: | :---: | :---: |
| <img src="docs/tray/stale.png" width="64" alt="Stale"> | <img src="docs/tray/unknown.png" width="64" alt="Unknown"> | <img src="docs/tray/mixed.png" width="64" alt="Keyboard only"> |
| last good reading | disconnected | mouse missing |

## Features

- Auto-refresh, start minimized, per-user Windows logon autostart
- Keeps the last good reading when a device is asleep or briefly missing
- Chinese / English UI; first launch follows the OS language

## Supported devices

### Cherry keyboards

VID `046A`, Col04 channel.

- Cherry MX 2.0S (`01AC`)
- Cherry MX 3.0S Wireless (`00EA`)

### ROG mice

VID `0B05`. USB receiver or cable; Bluetooth is not supported.

- ROG Mouse (OMNI Receiver)
- ROG Harpe II
- ROG Harpe II Ace (USB)
- ROG Harpe Ace Aim Lab, Harpe Ace Aim Lab (USB), Harpe Ace Extreme (USB), Harpe Ace Mini (USB)
- ROG Chakram X, Chakram X (USB), Chakram, Chakram (USB)
- ROG Gladius III Wireless, Gladius III (USB), Gladius III Aimpoint, Gladius III Aimpoint (USB), Gladius III EVA-02, Gladius III EVA-02 (USB), Gladius II Wireless
- ROG Keris Aimpoint, Keris Aimpoint (USB), Keris II Ace (USB), Keris Wireless, Keris (USB), Keris EVA, Keris EVA (USB), Keris II Origin (USB), Keris II Origin KJP (USB)
- ROG Spatha X, Spatha X (USB)
- ROG Pugio II, Pugio II (USB)
- ROG Strix Impact II Wireless, Strix Impact II (USB), Strix Carry

## Download

Windows **x64** only (no 32-bit). Get the [latest release](https://github.com/1gcat/BatteryTool/releases/latest); checksums are in `SHA256SUMS.txt`.

| File | Variant | Notes |
| --- | --- | --- |
| `BatteryTool-{ver}-win-x64-framework.zip` | Framework-dependent | Smallest. Requires [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (Windows x64). |
| `BatteryTool-{ver}-win-x64-self-contained.zip` | Self-contained | Includes the runtime. Unzip and run; no extra .NET install. |
| `BatteryTool-{ver}-win-x64.exe` | Single-file | Self-contained one-file build. Download and run. |

## Getting started

### Requirements

- Windows 10 or later, 64-bit
- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build from source
- Framework-dependent releases also need the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build

```bash
git clone https://github.com/1gcat/BatteryTool.git
cd BatteryTool
dotnet build BatteryTool.slnx -c Release
dotnet run --project BatteryTool.Winform -c Release
```

Protocol smoke checks:

```bash
dotnet run --project BatteryTool.Checks -c Release
```

## Usage

| Action | |
| --- | --- |
| Refresh | Toolbar button, tray menu, or the auto-refresh interval |
| Hide | Minimize, or **Minimize to tray** |
| Restore | Double-click the tray icon, or **Show window** |
| Language | `中文` / `English` combo on the main window |
| Demo (no hardware) | `BatteryTool.Winform --mock` |

Closing the window exits the app. Minimizing keeps monitoring in the tray.

## Configuration

Settings are stored at `%LOCALAPPDATA%\BatteryTool\settings.json` (refresh interval, start minimized, language). Autostart uses the current-user `Run` registry key and does not require administrator rights.

## Acknowledgments

https://github.com/zcmk123/cherry-battery-state  
https://github.com/seerge/g-helper

## License

[GPL-3.0](LICENSE)

## Star History

[![Star History Chart](https://api.star-history.com/chart?repos=1gcat/BatteryTool&type=Date)](https://star-history.com/#1gcat/BatteryTool&Date)
