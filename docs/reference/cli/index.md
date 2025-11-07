---
title: "WinGet Studio CLI reference"
description: >-
  Command-line interface reference for WinGet Studio
ms.topic: reference
ms.date: 11/04/2025
---

# WinGet Studio CLI reference

The WinGet Studio command-line interface (CLI) provides access to DSC v3 resource operations and
configuration management directly from the command line. The CLI is a companion to the WinGet
Studio graphical application.

## Getting help

Display available commands and options:

```powershell
wingetstudio --help
```

Output:

```text
Description:
  WinGetStudio command line interface

Usage:
  WinGetStudio [command] [options]

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information
  --logs          Opens the logs folder

Commands:
  dsc       DSC v3 resource commands
  settings  Opens the settings file
```

## Global options

The following options are available for all commands:

| Option           | Description                     |
|------------------|---------------------------------|
| `-?, -h, --help` | Show help and usage information |
| `--version`      | Show version information        |
| `--logs`         | Opens the logs folder           |

## Commands

| Command    | Description                        | Reference              |
|------------|------------------------------------|------------------------|
| `dsc`      | Execute DSC v3 resource operations | [dsc command][01]      |
| `settings` | Open the settings file for editing | [settings command][02] |

## Examples

### Display version

```powershell
wingetstudio --version
```

### Open logs folder

```powershell
wingetstudio --logs
```

### Get resource state

```powershell
wingetstudio dsc get --resource settings
```

### Export resource configuration

```powershell
wingetstudio dsc export --resource settings
```

## See also

- [dsc command reference][01]
- [settings command reference][02]
- [Settings resource reference][03]
- [Get started with WinGet Studio][04]

<!-- Link reference definitions -->
[01]: ./dsc.md
[02]: ./settings.md
[03]: ../resources/settings.md
[04]: ../../get-started/index.md
