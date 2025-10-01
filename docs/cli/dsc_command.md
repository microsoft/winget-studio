# dsc command

The **dsc** command is the entry point for using DSC (Desired State Coonfiguration) with WinGet Studio.

## Usage

`wingetstudio.exe dsc [<options>]`

## Resources
| Resource | Type | Description | <div style="white-space: nowrap;">get</div> | <div style="white-space: nowrap;">set</div> | <div style="white-space: nowrap;">test</div> | <div style="white-space: nowrap;">export</div> | <div style="white-space: nowrap;">schema</div> | <div style="white-space: nowrap;">manifest</div> | Link |
| ---- | ------ | ------------| ----- | ----- | ----- | ----- | ----- | ----- | ----- |
| `settings` | `Microsoft.WinGetStudio/Settings` | Manage the settings for WinGet Studio | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | [Details](dsc_resources/settings.md) |

## Arguments

The following subcommands are available:

| <div style="width:100px">Argument</div> | Description |
| --------------------------------------- | ------------|
| **get** | Get the resource state
| **set** | Set the resource state |
| **test** | Test the resource state |
| **export** | Get all state instances |
| **schema** |  Outputs schema of the resource |
| **manifest** |  Outputs manifest of the resource |
| **-?, -h, --help** |  Show help and usage information |