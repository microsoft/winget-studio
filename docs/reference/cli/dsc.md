---
title: "dsc command"
description: >-
  Execute Microsoft DSC v3 resource operations from the command line
ms.topic: reference
ms.date: 11/04/2025
---

# dsc command

The `dsc` command provides access to Microsoft DSC (Desired State Configuration) resource
operations. Use this command to get, set, test, and export resource states from the command
line.

## Syntax

```text
wingetstudio dsc <operation> [options]
```

## Operations

The `dsc` command supports the following resource operations:

| Operation  | Description                                 |
|------------|---------------------------------------------|
| `get`      | Retrieve the current state of a resource    |
| `set`      | Configure a resource to match desired state |
| `test`     | Test whether a resource is in desired state |
| `export`   | Export all instances of a resource          |
| `schema`   | Display the JSON schema for a resource      |
| `manifest` | Display the resource manifest               |

## Options

| Option           | Description                     |
|------------------|---------------------------------|
| `-?, -h, --help` | Show help and usage information |

## Available resources

The following DSC resources are provided by WinGet Studio:

| Resource   | Type                              | Operations                               | Reference      |
|------------|-----------------------------------|------------------------------------------|----------------|
| `settings` | `Microsoft.WinGetStudio/Settings` | get, set, test, export, schema, manifest | [Settings][01] |

## Examples

### Get resource state

Retrieve the current state of the Settings resource:

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

### Set resource state

Configure the Settings resource with desired state:

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

### Test resource state

Test whether the Settings resource is in desired state:

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

The `_inDesiredState` property indicates whether the current state matches the desired state.

### Export resource configuration

Export all instances of the Settings resource:

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

### Display resource schema

View the JSON schema for the Settings resource:

```powershell
wingetstudio dsc schema --resource settings
```

Returns the complete JSON schema defining the resource's properties and structure.

### Display resource manifest

View the resource manifest:

```powershell
wingetstudio dsc manifest --resource settings
```

Returns the DSC resource manifest including supported operations and adapter information.

## See also

- [CLI reference][02]
- [Settings resource reference][01]
- [Understanding DSC resources][03]
- [Get started with WinGet Studio][04]

<!-- Link reference definitions -->
[01]: ../resources/settings.md
[02]: ./index.md
[03]: ../../concepts/understanding-resources.md
[04]: ../../get-started/index.md
