---
description: >-
  Learn how to discover, explore, and test DSC resources using the Validate Resource feature
  in WinGet Studio. This hands-on walkthrough covers resource operations and settings.
ms.date: 11/06/2025
ms.topic: tutorial
title: Validate and test DSC resources
---

# Validate and test DSC resources

The Validate Resource feature in WinGet Studio helps you discover DSC (Desired State Configuration)
resources, explore their properties, and test their operations before adding them to configurations.
This tutorial walks you through using the resource validator to work with both Microsoft DSC 3.0
and PowerShell DSC 2.0 resources.

## Prerequisites

Before you begin, ensure you have:

- **WinGet Studio** installed from the [GitHub releases page][31]
- **Windows Package Manager (WinGet)** installed
- **PowerShell 7 or later** for working with Microsoft DSC 3.x resources
- **Administrator privileges** for certain resource operations

To install PowerShell 7 if needed:

```powershell
winget install Microsoft.PowerShell
```

## What you'll learn

In this tutorial, you'll learn how to:

- Navigate to the Validate Resource page
- Search for and select DSC resources
- View resource schemas and properties
- Use the Get, Set, and Test operations
- Work with Microsoft DSC 3.0 resources
- Work with PowerShell DSC 2.0 resources
- Understand resource requirements and dependencies

## Understanding DSC resource operations

DSC resources provide standard methods for managing system components:

- **Get**: Retrieves the current state without making changes
- **Set**: Applies the desired state to the system
- **Test**: Checks if the system is in the desired state

Some Microsoft DSC 3.0 resources also support **Export**, which generates configurations from
the current system state. This feature is not yet implemented in WinGet Studio.

For more information about resource types, see [Understanding DSC resources][02].

## Launching the resource validator

When you launch WinGet Studio, the Home screen displays two main options:

![WinGet Studio home screen with Manage Configuration and Validate Resource buttons][01]

Select **Validate Resource** to open the resource validation page.

![The Validate a resource page with search field and operation buttons][33]

The Validate Resource page provides tools to discover resources, explore their settings, and test
their operations.

## Discovering DSC resource versions

DSC resources come in three versions:

- **[PowerShell DSC 1.1][03]**: Original PowerShell DSC platform
- **[PowerShell DSC 2.0][04]**: Enhanced PowerShell DSC with class-based resources
- **[Microsoft DSC 3.0][05]**: Modern cross-platform DSC with command-based resources

This tutorial focuses on Microsoft DSC 3.0 and PowerShell DSC 2.0 resources, as these are the
most commonly used with WinGet Configuration files.

## Searching for resources

WinGet Studio searches both locally installed DSC resources and resources available in the
PowerShell Gallery.

### Search for Microsoft DSC 3.0 resources

<!-- markdownlint-disable MD044 -->

Type "winget" in the **Search DSC resources** field. WinGet Studio displays matching resources:

![Search results displaying WinGet-related DSC resources with version numbers and source indicators][06]

<!-- markdownlint-enable MD044 -->

The search results display:

- **Left side**: Resource name and version number
- **Right side**: Source indicator (`LocalDscV3` for local Microsoft DSC 3.0 resources)

For resources from PowerShell Gallery, the source shows `PSGallery`.

### Select a resource

Select **Microsoft.WinGet.DSC/WinGetPackage** from the dropdown list.

## Viewing resource information

After selecting a resource, click the **ℹ️** icon to the right of the resource input field.

![Resource input field with the info icon highlighted][07]

WinGet Studio displays the settings exposed by the resource.

![Resource settings information panel displaying properties and their details][08]

### Understanding resource schemas

Microsoft DSC 3.0 resources use JSON schemas to define properties and default values. Click
**View JSON schema** under the **Code** column to see the schema definition.

![JSON schema viewer showing resource property definitions and types][34]

The schema helps you understand property data types, valid values, and requirements.

> [!NOTE]
> Microsoft DSC 3.0 uses the underscore character `_` to denote standard properties handled by
> `dsc.exe` directly. For example, the `_exist` property indicates whether a resource instance
> should exist (installed) or not.

### Exploring property definitions

Scroll through the schema to find specific properties. For example, the `installMode` property
shows it's an enumeration with specific valid values:

![The installMode property definition with enumeration values][10]

### Switching between views

Click **View** to return to the settings display.

![View menu button in the resource information panel][11]

Select **Summary view** to see the simplified settings overview.

![Summary view option selected in the view dropdown menu][12]

## Working with resource settings

### Copy default settings

Click **Copy as YAML** to copy the default properties and values to your clipboard. A notification
appears confirming the copy operation.

### Paste and configure settings

Click in the **Settings** editor and paste the YAML (Ctrl+V). The editor displays all available
settings with their default values.

![Settings editor populated with default YAML properties and values][13]

> [!TIP]
> Refer to the schema when configuring settings to understand what each property controls and what
> values are valid.

### Configure required properties

For the `Microsoft.WinGet.DSC/WinGetPackage` resource, the most important properties are:

- **id**: The WinGet package identifier
- **source**: The WinGet source where the package is available

Remove unnecessary settings and configure the required ones:

```yaml
id: Microsoft.AppInstaller
source: winget
```

> [!NOTE]
> YAML is sensitive to spaces. Include a space after the colon for each setting.
![Configured settings showing id and source properties for Microsoft.AppInstaller][15]
> [!NOTE]
> The default value `useLatest: "false"` exposes a bug in the current DSC resource version.
> Boolean values should not be quoted as strings. This issue is tracked in
> [WinGet-cli issue #5833][14].

## Testing resource operations

### Using the Get operation

The **Get** operation retrieves the current state without making changes. Many resources require
specific settings to return results. For `Microsoft.WinGet.DSC/WinGetPackage`, the `id` property
is required.

Click **Get** to retrieve the current state.

![Get button highlighted in the operation toolbar][16]

WinGet Studio displays a progress indicator while the operation runs. When complete, the results
show the current state:

![Get operation results displaying current package state with version and properties][17]

The results include additional properties beyond what you specified, providing a complete picture
of the resource's current state.

### Using the Test operation

The **Test** operation checks if the system is in the desired state. DSC may perform a synthetic
test by calling **Get** and comparing the results against your desired state configuration.

Click **Test** to check the desired state.

![Test button highlighted in the operation toolbar][18]

When the operation completes, a message in the top-right corner indicates whether the system is in
the desired state:

![Success notification indicating machine is in the desired state][19]

To see what happens when the system is not in the desired state, add an incorrect value to the
settings:

```yaml
id: Microsoft.AppInstaller
source: winget
version: "0"
```

> [!NOTE]
> The `version` property expects a string value, so it should be enclosed in quotes.

Run **Test** again to see the "not in desired state" result:

![Warning notification indicating machine is not in the desired state][20]

### Using the Set operation

The **Set** operation applies the desired state to the system.

> [!WARNING]
> The Set operation modifies your system. Ensure your settings are correct before applying them
> to avoid unintended changes.

#### Testing Set with a different resource

To demonstrate the Set operation safely, use the `Microsoft.WinGet/UserSettingsFile`
resource instead:

1. Search for and select **Microsoft.WinGet/UserSettingsFile**
1. Click the **ℹ️** button to view resource information
1. Review the JSON schema to understand the required `settings` property
1. Configure the settings property in the editor
1. Click **Get** to see current WinGet settings

The Get operation returns your current WinGet configuration:

![Current WinGet settings displayed with visual preferences and schema reference][21]

#### Modifying WinGet settings

To experiment with WinGet settings, try configuring the progress bar and Sixel support:

```yaml
settings:
  visual:
    progressBar: rainbow
    enableSixels: true
  $schema: https://aka.ms/winget-settings.schema.json
```

Valid `progressBar` values include:

- `accent`
- `rainbow`
- `retro`
- `sixel`
- `disabled`

For more settings options, see the [WinGet Settings Schema][22].

The `action` property in the resource determines whether to:

- **Partial**: Set only the specified values
- **Full**: Perform a complete overwrite with your settings

After configuring your desired settings, click **Set** to apply them.

![Set button highlighted in the operation toolbar][23]

To verify the changes, run a WinGet command that displays visual elements:

```powershell
winget show GitHub.GitHubDesktop
```

This command shows the modified progress bar and, if supported, a Sixel icon:

![WinGet show command displaying rainbow progress bar and Sixel icon][24]

## Working with PowerShell DSC 2.0 resources

PowerShell DSC 2.0 resources are implemented in PowerShell modules and are available from the
PowerShell Gallery. WinGet Studio can show their settings even if the module isn't installed, but
you must install the module to execute Get, Set, or Test operations.

### Checking for installed modules

Verify if the `Microsoft.Windows.Settings` module is installed:

```powershell
Get-DSCResource -Module Microsoft.Windows.Settings
```

![PowerShell console output listing available DSC resources in the module][25]

If the module isn't installed, you can install it using PowerShellGet or PSResourceGet:

```powershell
# Using PowerShellGet (PowerShell 5.1+)
Install-Module Microsoft.Windows.Settings -Scope CurrentUser
# Using PSResourceGet (PowerShell 7+)
Install-PSResource Microsoft.Windows.Settings -Scope CurrentUser
```

### Searching for PowerShell DSC resources

Type "windowssettings" in the resource search field. The results show PowerShell DSC resources
from the PowerShell Gallery:

![Search results showing WindowsSettings resource from PSGallery][26]

The source indicator shows `PSGallery` instead of `LocalDscV3`.

### Viewing PowerShell DSC resource properties

Click the **ℹ️** button to view resource information. PowerShell DSC 2.0 resources display
differently than Microsoft DSC 3.0 resources:

![Resource information panel for PowerShell DSC showing extracted code snippets][27]

Instead of JSON schema links, the **Code** column shows extracted PowerShell code from the
resource definition.

Switch to **Code View** to see the PowerShell code that defines the resource properties:

![Code View displaying the PowerShell DSC resource class definition with properties][28]

The code view helps identify:

- Property names and types
- Default values
- Required properties (marked with `[Key]`)
- Properties that should not be set

For the `WindowsSettings` resource, the `$SID` property is marked as a key property that should
not be manually set.

### Testing PowerShell DSC resources

Click **Get** to retrieve the current Windows settings. The results include:

- `SystemColorMode`
- `AppColorMode`
- `TaskbarAlignment`
- `DeveloperMode` state

The `SID` property returns as a blank value, as expected.

Copy the results to the settings editor and remove the `SID` line before making changes.

> [!NOTE]
> The `DeveloperMode` property requires elevation to modify. If you attempt to change this setting
> without administrator privileges, an exception occurs:
>
> ![Error message indicating access denied due to insufficient privileges][29]
>
> The synthetic test behavior in DSC may not show this error if you're not actually changing the
> value, which can be misleading.

### Applying PowerShell DSC settings

You can modify other properties without requiring elevation. For example, change the
`AppColorMode` to `Dark`:

```yaml
SystemColorMode: Dark
AppColorMode: Dark
TaskbarAlignment: Left
```

Click **Set** to apply the changes. Some changes may require restarting applications to take full
effect.

![Windows color mode changed to dark theme in WinGet Studio][30]

## Next steps

Now that you understand how to validate and test DSC resources:

- [Create your first configuration][04] using the resources you've explored
- [Understand DSC resource concepts][02] in more depth
- [Learn about configuration versions][32] and format differences
- [Customize exported configurations][05] with parameters and dependencies

## Troubleshooting

### Module not found errors

If you encounter "module not found" errors when testing PowerShell DSC resources:

1. Verify the module is installed:

   ```powershell
   # Using PowerShellGet (PowerShell 5.1+)
   Get-Module -ListAvailable -Name <ModuleName>
   
   # Using PSResourceGet (PowerShell 7+)
   Get-PSResource -Name <ModuleName>
   ```

1. Install the module if missing:

   ```powershell
   # Using PowerShellGet (PowerShell 5.1+)
   Install-Module <ModuleName> -Scope CurrentUser
   
   # Using PSResourceGet (PowerShell 7+)
   Install-PSResource <ModuleName> -Scope CurrentUser
   ```

### Permission denied errors

Some resources require elevated permissions. If you encounter access denied errors:

1. Launch WinGet Studio as administrator
1. Or use the CLI with elevated privileges

### Resource not found errors

If WinGet Studio cannot find a resource you're searching for:

1. Navigate to **Settings** > **Resources**
1. Click **Clear cache** to refresh the resource catalog
1. Search for the resource again

For more information about resource discovery, see [Understanding DSC resources][02].

## Related content

- [Get started with WinGet Studio][04]
- [Understanding DSC resources][02]
- [WinGet Configuration versions][32]
- [Customize exported configurations][05]
- [WinGet Configuration documentation][09]

<!-- Link reference definitions -->
[01]: .././images/studio/0.100.302.0/First-Launch.png
[02]: ./concepts/understanding-resources.md
[03]: https://learn.microsoft.com/powershell/dsc/overview?view=dsc-1.1
[04]: ./get-started/index.md
[05]: ./how-to/customize-exported-configuration.md
[06]: .././images/studio/0.100.302.0/Validate-search-winget.png
[07]: .././images/studio/0.100.302.0/Validate-resource-info.png
[08]: .././images/studio/0.100.302.0/Validate-winget-package-info.png
[09]: https://learn.microsoft.com/windows/package-manager/configuration/
[10]: .././images/studio/0.100.302.0/Validate-winget-package-schema-enumeration.png
[11]: .././images/studio/0.100.302.0/Validate-winget-package-resource-info.png
[12]: .././images/studio/0.100.302.0/Validate-winget-package-resource-summary.png
[13]: .././images/studio/0.100.302.0/Validate-winget-package-resource-settings.png
[14]: https://github.com/microsoft/winget-cli/issues/5833
[15]: .././images/studio/0.100.302.0/Validate-winget-package-AppInstaller.png
[16]: .././images/studio/0.100.302.0/Validate-a-resource-Get.png
[17]: .././images/studio/0.100.302.0/Validate-winget-package-Get-AppInstaller.png
[18]: .././images/studio/0.100.302.0/Validate-a-resource-Test.png
[19]: .././images/studio/0.100.302.0/Validate-a-winget-package-Test.png
[20]: .././images/studio/0.100.302.0/Validate-a-winget-package-Test-invalid.png
[21]: .././images/studio/0.100.302.0/Validate-winget-settings-Get.png
[22]: https://aka.ms/winget-settings.schema.json
[23]: .././images/studio/0.100.302.0/Validate-a-resource-Set.png
[24]: .././images/WinGet/winget-show-GitHubDesktop.png
[25]: .././images/PowerShell/Get-DscResource-Microsoft.Windows.Settings.png
[26]: .././images/studio/0.100.302.0/Validate-WindowsSettings-v2.png
[27]: .././images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-info.png
[28]: .././images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-code.png
[29]: .././images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-DeveloperMode.png
[30]: .././images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-AppColorMode.png
[31]: https://github.com/microsoft/winget-studio/releases
[32]: ./concepts/configuration-versions.md
[33]: ./../images/studio/0.100.302.0/Validate-a-resource.png
[34]: ./../images/studio/0.100.302.0/Validate-winget-package-schema.png
