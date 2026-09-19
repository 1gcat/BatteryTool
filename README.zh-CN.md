<p align="right"><a href="./README.md">English</a> · <strong>简体中文</strong></p>

<p align="center">
  <img src="docs/logo.png" width="144" alt="BatteryTool">
</p>

# BatteryTool

在系统托盘显示 **Cherry 键盘** 和 **ROG 鼠标** 电量的 Windows 小工具。只发送 HID 电量查询，不会改灯效、DPI、配对或电源策略。

<!-- 若 GitHub 仓库路径不是 1gcat/BatteryTool，请同步替换徽标和 star 走势图中的仓库名。 -->
[![License: GPL v3](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4.svg)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D6.svg)](https://github.com/1gcat/BatteryTool)
[![Stars](https://img.shields.io/github/stars/1gcat/BatteryTool?style=flat)](https://github.com/1gcat/BatteryTool/stargazers)
[![Issues](https://img.shields.io/github/issues/1gcat/BatteryTool)](https://github.com/1gcat/BatteryTool/issues)
[![Last commit](https://img.shields.io/github/last-commit/1gcat/BatteryTool)](https://github.com/1gcat/BatteryTool/commits)

## 演示

<p align="center">
  <img src="docs/ui.png" width="720" alt="BatteryTool 演示">
</p>

## 托盘图标

一枚图标同时显示两台设备：**左键盘 / 右鼠标**。绿 &gt;50%，黄 ≤50%，红 ≤20%，灰 = 旧数据，`?` = 未知，蓝色闪电 = 充电。

| 正常 | 充电中 | 电量低 |
| :---: | :---: | :---: |
| <img src="docs/tray/normal.png" width="64" alt="正常"> | <img src="docs/tray/charging.png" width="64" alt="充电中"> | <img src="docs/tray/low.png" width="64" alt="电量低"> |
| 87% / 66% | 充电 | ≤20% |

| 旧数据 | 未知 | 仅键盘 |
| :---: | :---: | :---: |
| <img src="docs/tray/stale.png" width="64" alt="旧数据"> | <img src="docs/tray/unknown.png" width="64" alt="未知"> | <img src="docs/tray/mixed.png" width="64" alt="仅键盘"> |
| 上次成功读数 | 未连接 | 鼠标缺失 |

## 功能

- 自动刷新、启动时最小化、当前用户开机自启
- 设备休眠或短暂消失时保留上次成功读数
- 中 / 英界面；初次启动跟随系统语言

## 支持的设备

### Cherry 键盘

VID `046A`，Col04 通道。

- Cherry MX 2.0S（`01AC`）
- Cherry MX 3.0S 无线版（`00EA`）

### ROG 鼠标

VID `0B05`。USB 接收器或数据线；不支持蓝牙。

- ROG 鼠标（OMNI 接收器）
- ROG Harpe II
- ROG Harpe II Ace（USB）
- ROG Harpe Ace Aim Lab、Harpe Ace Aim Lab（USB）、Harpe Ace Extreme（USB）、Harpe Ace Mini（USB）
- ROG Chakram X、Chakram X（USB）、Chakram、Chakram（USB）
- ROG Gladius III Wireless、Gladius III（USB）、Gladius III Aimpoint、Gladius III Aimpoint（USB）、Gladius III EVA-02、Gladius III EVA-02（USB）、Gladius II Wireless
- ROG Keris Aimpoint、Keris Aimpoint（USB）、Keris II Ace（USB）、Keris Wireless、Keris（USB）、Keris EVA、Keris EVA（USB）、Keris II Origin（USB）、Keris II Origin KJP（USB）
- ROG Spatha X、Spatha X（USB）
- ROG Pugio II、Pugio II（USB）
- ROG Strix Impact II Wireless、Strix Impact II（USB）、Strix Carry

## 下载

仅 **Windows x64**（不提供 32 位）。从 [最新 Release](https://github.com/1gcat/BatteryTool/releases/latest) 下载，校验和见 `SHA256SUMS.txt`。

| 文件 | 版本 | 说明 |
| --- | --- | --- |
| `BatteryTool-{ver}-win-x64-framework.zip` | 依赖框架 | 体积最小。需安装 [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)（Windows x64）。 |
| `BatteryTool-{ver}-win-x64-self-contained.zip` | 独立发布 | 自带运行时。解压即可运行，不必另装 .NET。 |
| `BatteryTool-{ver}-win-x64.exe` | 单个文件 | 独立的单文件构建。下载后直接运行。 |

## 开始使用

### 环境

- Windows 10 或更高版本，64 位
- 从源码编译需要 [.NET 10 SDK](https://dotnet.microsoft.com/download)
- 依赖框架版还需 [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

### 编译

```bash
git clone https://github.com/1gcat/BatteryTool.git
cd BatteryTool
dotnet build BatteryTool.slnx -c Release
dotnet run --project BatteryTool.Winform -c Release
```

协议冒烟检查：

```bash
dotnet run --project BatteryTool.Checks -c Release
```

## 用法

| 操作 | |
| --- | --- |
| 刷新 | 窗口按钮、托盘菜单，或自动刷新间隔 |
| 隐藏 | 最小化，或点 **最小化到托盘** |
| 恢复 | 双击托盘图标，或 **显示窗口** |
| 语言 | 主窗口的 `中文` / `English` 下拉框 |
| 无硬件演示 | `BatteryTool.Winform --mock` |

关闭窗口会退出程序；最小化后继续在托盘监测。

## 配置

设置保存在 `%LOCALAPPDATA%\BatteryTool\settings.json`（刷新间隔、启动最小化、语言）。开机自启写入当前用户的 `Run` 注册表项，不需要管理员权限。

## 致谢

https://github.com/zcmk123/cherry-battery-state  
https://github.com/seerge/g-helper

## 许可证

[GPL-3.0](LICENSE)

## Star 走势

[![Star History Chart](https://api.star-history.com/chart?repos=1gcat/BatteryTool&type=Date)](https://star-history.com/#1gcat/BatteryTool&Date)
