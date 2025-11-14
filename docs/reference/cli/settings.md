---
title: "settings command"
description: >-
  Open the WinGet Studio settings file for editing
ms.topic: reference
ms.date: 11/04/2025
---

# settings command

The `settings` command opens the WinGet Studio settings file (`settings.json`) in your default
JSON editor. If no editor is configured, Windows prompts you to select an application.

## Syntax

```text
wingetstudio settings [options]
```

## Options

| Option           | Description                     |
|------------------|---------------------------------|
| `-?, -h, --help` | Show help and usage information |

## Settings file location

WinGet Studio stores settings in `settings.json`. The command automatically locates and opens
this file in your configured JSON editor.

## Available settings

The settings file supports the following configuration options:

### Theme

Controls the appearance of the WinGet Studio user interface.

```json
{
  "theme": "Dark"
}
```

Valid values:

- **Default**: Matches the current Windows theme
- **Light**: Light theme
- **Dark**: Dark theme

### Telemetry

Controls whether WinGet Studio writes ETW events that may be sent to Microsoft.

```json
{
  "telemetry": {
    "disable": true
  }
}
```

When `disable` is `true`, WinGet Studio prevents writing telemetry events.

For more information, see [WinGet Studio telemetry][01] and the [privacy statement][02].

## Examples

### Open settings file

```powershell
wingetstudio settings
```

This command opens `settings.json` in your default JSON editor.

### Example settings file

```json
{
  "theme": "Dark",
  "telemetry": {
    "disable": true
  }
}
```

This configuration sets the dark theme and disables telemetry.

## Alternative: DSC resource

You can also manage WinGet Studio settings using the Settings DSC resource:

```powershell
# Get current settings
wingetstudio dsc get --resource settings

# Set settings using DSC
wingetstudio dsc set --resource settings --input '{
  "settings": {
    "theme": "Dark",
    "telemetry": {
      "disable": true
    }
  }
}'
```

For more information, see the [Settings resource reference][03].

## See also

- [CLI reference][04]
- [Settings resource reference][03]
- [dsc command reference][05]

<!-- Link reference definitions -->
[01]: https://github.com/microsoft/winget-studio#datatelemetry
[02]: ../../PRIVACY.md
[03]: ../resources/settings.md
[04]: ./index.md
[05]: ./dsc.md
