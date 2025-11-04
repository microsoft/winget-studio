---
title: "Settings resource"
description: >-
  DSC resource for managing WinGet Studio settings
ms.topic: reference
ms.date: 11/04/2025
---

# Settings resource

The Settings resource manages WinGet Studio application settings including theme and telemetry
preferences. This resource is available through the WinGet Studio CLI.

## Type

`Microsoft.WinGetStudio/Settings`

## Supported operations

| Operation | Supported | Description                                |
|-----------|-----------|--------------------------------------------|
| get       | ✅         | Retrieve current settings                  |
| set       | ✅         | Configure settings to desired state        |
| test      | ✅         | Test whether settings match desired state  |
| export    | ✅         | Export current settings (identical to get) |
| schema    | ✅         | Display JSON schema for the resource       |
| manifest  | ✅         | Display resource manifest                  |

## Properties

### settings

The settings object contains WinGet Studio configuration properties.

| Property              | Type    | Required | Description                          |
|-----------------------|---------|----------|--------------------------------------|
| **theme**             | string  | No       | UI theme: Default, Light, or Dark    |
| **telemetry.disable** | boolean | No       | When true, disables telemetry events |

## Examples

### Get operation

Retrieve the current WinGet Studio settings:

```powershell
wingetstudio dsc get --resource settings
```

Output:

```json
{
  "settings": {
    "theme": "Default",
    "telemetry": {
      "disable": false
    }
  }
}
```

### Set operation

Configure WinGet Studio with desired settings:

```powershell
wingetstudio dsc set --resource settings --input '{
  "settings": {
    "theme": "Dark",
    "telemetry": {
      "disable": true
    }
  }
}'
```

Output:

```json
{
  "settings": {
    "theme": "Dark",
    "telemetry": {
      "disable": true
    }
  }
}
```

The output shows the settings after applying the desired state.

### Test operation

Test whether current settings match desired state:

```powershell
wingetstudio dsc test --resource settings --input '{
  "settings": {
    "theme": "Dark",
    "telemetry": {
      "disable": false
    }
  }
}'
```

Output:

```json
{
  "settings": {
    "theme": "Dark",
    "telemetry": {
      "disable": true
    }
  },
  "_inDesiredState": false
}
```

The `_inDesiredState` property indicates whether the current state matches the input. In this
example, it's `false` because the current telemetry setting differs from the desired state.

### Export operation

Export current settings (output is identical to get operation):

```powershell
wingetstudio dsc export --resource settings
```

Output:

```json
{
  "settings": {
    "theme": "Default",
    "telemetry": {
      "disable": false
    }
  }
}
```

### Schema operation

Display the JSON schema for the Settings resource:

```powershell
wingetstudio dsc schema --resource settings
```

Returns the complete JSON schema defining the resource's properties, types, and constraints.

### Manifest operation

Display the resource manifest:

```powershell
wingetstudio dsc manifest --resource settings
```

Returns the DSC resource manifest including version, kind, adapter, and supported operations.

## Configuration file usage

You can use the Settings resource in WinGet Configuration files:

### Microsoft DSC 3.x format

<!-- markdownlint-disable MD013 -->

```yaml
# configuration.dsc.yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
resources:
  - name: Configure WinGet Studio
    type: Microsoft.WinGetStudio/Settings
    properties:
      settings:
        theme: Dark
        telemetry:
          disable: true
```

<!-- markdownlint-enable MD013 -->

Apply the configuration:

```powershell
dsc config set --path configuration.dsc.yaml
```

## Theme options

The **theme** property controls the WinGet Studio UI appearance:

- **Default**: Automatically matches the current Windows theme
- **Light**: Light theme with bright background
- **Dark**: Dark theme with dark background

## Telemetry control

The **telemetry.disable** property controls ETW event logging:

- **false** (default): WinGet Studio writes telemetry events
- **true**: Disables all telemetry event writing

For more information about data collection, see:

- [WinGet Studio telemetry][01]
- [Privacy statement][02]

## See also

- [dsc command reference][03]
- [settings command reference][04]
- [CLI reference][05]
- [Get started with WinGet Studio][06]

<!-- Link reference definitions -->
[01]: https://github.com/microsoft/winget-studio#datatelemetry
[02]: ../../../PRIVACY.md
[03]: ../cli/dsc.md
[04]: ../cli/settings.md
[05]: ../cli/index.md
[06]: ../../get-started/index.md
