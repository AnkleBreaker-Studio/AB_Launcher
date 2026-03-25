# AB Launcher — Quick Actions Hub

> **Lightweight WPF launcher (.exe) that reads `actions.json` to display configurable action buttons.** Run scripts, open URLs, launch tools — all from one clean interface. Supports bat, ps1, exe, shell, and url actions with optional admin elevation. By [AnkleBreaker Studio](https://github.com/AnkleBreaker-Studio).

## Usage

1. Clone or download this repo
2. Place all files in the same folder
3. Run `AB-Launcher.exe`

## Files

| File | Role |
|---|---|
| `AB-Launcher.exe` | Compiled launcher (WPF, .NET Framework) |
| `AB-Launcher.cs` | Source code |
| `actions.json` | Action configuration (buttons, commands, types) |
| `Full-Restart-Claude.bat` | Action: kill Claude + restart CoworkVMService + relaunch |
| `Clear-Claude-Cache.bat` | Action: kill Claude, purge cache/logs, restart |

## Adding Actions

Edit `actions.json` to add new buttons. Supported types: `bat`, `ps1`, `exe`, `shell`, `url`.

```json
{
  "name": "My Action",
  "icon": "🚀",
  "command": "my-script.bat",
  "type": "bat",
  "admin": false,
  "description": "Does something cool"
}
```

## Build

```
csc /target:winexe /r:PresentationFramework.dll /r:PresentationCore.dll /r:WindowsBase.dll /r:System.Web.Extensions.dll AB-Launcher.cs
```

## About AnkleBreaker Studio

We build AI-powered developer tools and open-source Unity packages. Check out our flagship project: [Unity MCP](https://github.com/AnkleBreaker-Studio/unity-mcp-server) — 268 tools for AI-assisted game development.

[![Sponsor](https://img.shields.io/badge/Sponsor-AnkleBreaker%20Studio-red?logo=github)](https://github.com/sponsors/AnkleBreaker-Studio)
