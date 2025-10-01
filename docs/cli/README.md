# Winget Studio CLI
The Winget Studio command line interface (CLI) is a companion to the Winget Studio application.

You can run `wingetstudio --help` to see a list of available commands.

```
PS C:\> wingetstudio --help
Description:
  WinGetStudio command line interface - v0.0.0.0

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

## Commands
| Command | Description | Link |
| ------- | ----------- | ---- |
| `dsc` | DSC v3 resource commands | [Details](dsc_command.md) |
| `settings` | Opens the settings file | [Details](settings_command.md) |

## Options
| Option | Description |
| ------ | ----------- |
| `-?, -h, --help` | Show help and usage information |
| `--version` | Show version information |
| `--logs` | Opens the logs folder |