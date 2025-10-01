# Settings resource
Manage the settings for WinGet Studio

## 📄 Get
```shell
PS C:\> wingetstudio dsc get --resource settings
{"settings":{"theme":"Default","telemetry":{"disable":false}}}
```

## 🖨️ Export
ℹ️ Settings resource Get and Export operation output states are identical.
```shell
PS C:\> wingetstudio dsc export --resource settings
{"settings":{"theme":"Default","telemetry":{"disable":false}}}
```

## 📝 Set
```shell
PS C:\> wingetstudio dsc set --resource settings --input '{"settings":{"theme":"Dark","telemetry":{"disable": true}}}'
{"settings":{"theme":"Dark","telemetry":{"disable":true}}}
["settings"]
```

## 🧪 Test
```shell
PS C:\> wingetstudio dsc test --resource settings --input '{"settings":{"theme":"Dark","telemetry":{"disable": false}}}'
{"settings":{"theme":"Dark","telemetry":{"disable":true}},"_inDesiredState":false}
["settings"]
```