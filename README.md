# Around The World Clocks

A small, borderless desktop widget for Windows that shows analog (or digital) clocks for multiple cities at a glance.

> **Note:** This project is a playground for experimenting with [Claude Code](https://claude.com/claude-code). Most of the code was written together with Claude, and the repo is mainly used to try out features, workflows and ideas. Expect things to change and to break now and then.

## Features

- Clocks for any number of cities, each with its own IANA time zone (defaults: Amsterdam, London, New York, Tokyo)
- Eight themes: Mica Glass, Swiss Railway, Roman Classic, Squircle Sky, Mid-century, Compact Strip, Inset Digital and Retro Flip
- Horizontal or vertical layout, adjustable clock size
- 12- or 24-hour format, optional second hand
- Always-on-top mode
- Start with Windows (per-user, no admin rights needed)
- Remembers its position and settings between runs

Right-click the widget for quick access to Settings, theme, second hand, always-on-top and Exit.

## Requirements

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build from source (the installer is self-contained and does not need .NET on the target PC)

## Build and run

```powershell
dotnet run --project src/AroundTheWorldClocks
```

## Build the installer

```powershell
./build-installer.ps1
```

This publishes a self-contained, single-file `win-x64` exe and packages it with WiX Toolset v5 into `artifacts/AroundTheWorldClocks-<version>.msi`. The version comes from `<Version>` in `src/AroundTheWorldClocks/AroundTheWorldClocks.csproj`; bump it for every release so installed copies get upgraded.

## Settings

Settings are stored as JSON in `%APPDATA%\AroundTheWorldClocks\settings.json`. Delete the file to reset to defaults.

## Project structure

```
src/AroundTheWorldClocks/
  Controls/      ClockFace control
  Models/        Settings, city clocks and themes
  Rendering/     Analog dial renderers per theme
  Services/      Settings persistence and auto-start
  Themes/        XAML resource dictionary per theme
  ViewModels/    MVVM view models
  Views/         Widget and Settings windows
installer/       WiX MSI project
```
