# AB Launcher

Quick Actions Hub for AnkleBreaker Studio tools.

Lightweight WPF launcher (.exe) that reads `actions.json` to display configurable action buttons.

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
