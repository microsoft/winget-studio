# Winget Studio Settings

You can configure Winget Studio by editing the `settings.json` file or from user interface. Running `wingetstudio settings` will open the file in the default json editor; if no editor is configured, Windows will prompt for you to select an editor, and Notepad is sensible option if you have no other preference.

## Theme

The `theme` settings control the appearance of the Winget Studio user interface.

### name

```json
    "theme":"Dark",
```

The `theme.name` setting controls the overall appearance of the Winget Studio user interface. The default value is `Default`. Other valid values are `Light` and `Dark`. The `Default` option will match the current Windows theme.

## Telemetry

The `telemetry` settings control whether Winget Studio writes ETW events that may be sent to Microsoft on a default installation of Windows.

See [details on telemetry](https://github.com/microsoft/winget-studio#datatelemetry), and our [primary privacy statement](../../PRIVACY.md).

### disable

```json
    "telemetry": {
        "disable": true
    },
```

If set to true, the `telemetry.disable` setting will prevent any event from being written by the program.