---
description: >-
  Walk through WinGet Studio, an experimental tool for authoring and managing WinGet Configuration
  files using Desired State Configuration resources.
ms.date: 11/04/2025
ms.topic: overview
title: WinGet Studio overview
---

# Walkthrough
## First Launch
When you launch WinGet Studio the Home screen is displayed. The two prominent choices presented on
the main screen are "Manage Configuration" and "Validate Resource". In this walkthrough, we'll 
start with the basics.

![Local Image][01]

## Working with DSC Resources

Select "Validate Resource", and you will be taken to the "Validate a resource" page.

![Local Image][02]

This page is designed to help you find DSC (Desired State Configuration) resources, and allows you
to explore their settings. The standard methods on all DSC resources are "get", "set", and "test".
Some of the new Microsoft DSC resources also support "export", but that has not been implemented in
WinGet Studio yet.

There are three versions of DSC resources.
* [PowerShell DSC 1.1][03]
* [PowerShell DSC 2.0][04]
* [Microsoft DSC 3.0][05]

## Search for a DSC Resource

In this walkthrough we will focus on Microsoft DSC 3.0 resources and PowerShell DSC 2.0 resources.
The first resource we will look at is the WinGet Package resource. This is a Microsoft DSC 3.0
resource. In the "Search DSC resources.." field, go ahead and type "winget". WinGet Studio will
perform a search for locally installed DSC resources as well as resources available in the
PowerShell gallery.

On the left side of the dropdown list, a subset of the available resources are displayed along with their version numbers. On the right side is an indication of "LocalDscV3" for locally
installed Microsoft DSC 3.0 resources, and to the right of that is the version of the resource. In
the image below all resources with "winget" are displayed. In this case, they are all DSC 3.0
resources.

> [!TIP]
> If you do not see any resources in the dropdown list after you've entered "winget", then the
> configuration system hasn't be provisioned on your device. One of the easiest ways to make sure
> you have all the prerequisites is to run WinGet Configuration Export. This may take a couple of > minutes.
>
> `winget configure export --all -o config.winget`
>
> You may need to enable the configuration engine. WinGet will prompt with instructions on how to
> enable configuration if it's necessary. Once the export is completed, all the necessary software
> will be available on your device. The last step is to use the "refresh" (circular arrow) to
> load the locally available DSC resources into WinGet Studio.

![Local Image][06]

## Get Information on a Resource

Go ahead and select "Microsoft.WinGet/Package". Now that you have a resource selected, click on the "i" in the circle to the right of the resource input field.

![Local Image][07]

WinGet Studio is now displaying the "settings" exposed for the resource.

![Local Image][08]

Since the Microsoft.WinGet/Package resource is a Microsoft DSC 3.0 resource, a JSON schema defines
the properties and any default values if those are specified. Go ahead and click on one of the instances of "View JSON schema" under the "Code" column.

![Local Image][09]

The schema for the resource is displayed. This is helpful when you are trying to understand the
data types for the properties.

> [!NOTE]
> Microsoft Desired State Configuration 3.0 uses the underscore character "_" to denote standard
> properties handled by DSC.exe directly. In this example, the "_exist" property is one such
> standard property. In the context of Microsoft.WinGet/Package, this value is used to indicate if
> an instance of a WinGet Package is installed or not.

Scroll down so you can see the "installMode" property. Note it is an enumeration, and the valid
values are specified. 

![Local Image][10]

Switch back to the settings view by clicking on "View".

![Local Image][11]

Then select "Summary view".

![Local Image][12]

You're now back at the original view for displaying settings for the resource. Select "Copy as
YAML" to load the default properties and their values into your clipboard. You will see an
indication on the top right hand side of the Validate a resource page letting you know the YAML
has been copied to the clipboard.

Click in the editor under "Settings" and paste the YAML from your clipboard [Ctrl]+[v]. All available settings (from the Microsoft.WinGet/Package resource) and default values have been inserted.

![Local Image][13]

This is the point where having some knowledge about the resource (and what it configures) becomes
important. You can always refer back to the schema to get help and context on what the settings or
properties represent.

For the Microsoft.WinGet/Package resource, the most important fields are the "id:" which
represents the WinGet package identifier, and the "source:" which represents the WinGet source
the package is available in.

> [!NOTE]
> The "useLatest: "false"" is exposing a bug in the current version of the DSC resource. A boolean
> value should not be in quotes as a string data type. This bug is being tracked with [Inaccurate default for 'useLatest' in DSC v3 schema][14].

Remove the settings other than "id:" and "source:". Specify "Microsoft.AppInstaller" for the "id:"
and specify "winget" for the "source:".

> [!NOTE]
> YAML is sensitive to spaces. Be sure to include a space after the colon for each setting.

![Local Image][15]

You're now ready to explore the behavior of the WinGet Package resource with WinGet Studio. Before clicking those exciting buttons, a basic understanding of what DSC.exe is doing deeper in the
stack will help here.

## DSC Basics

The "Get" method of a DSC resource is used to get the current state. This operation will not make
any changes to the state of the device. It's primary use in DSC.exe is to help with a synthetic
test that will be explained later. Many resources will require one or more settings to be present
in order to return the results. In the case of the Microsoft.WinGet/Package resource, a package
identifier "id:" is necessary.

> [!WARNING]
> The "Set" method will attempt to change the state of your device. If you have modified the
> settings, the results could make you sad if you make a mistake or have something unintentional
> in your settings.

The "Set" method of a DSC resource is used to apply the desired state. An optimization in DSC.exe
will run "Test" to check and see if the system is already in the desired state, and if it is in the
desired state, no operation will be performed.

The "Test" method is designed to check if the system is in the desired state. In some cases DSC.exe will perform a synthetic test. This is done by calling the "Get" method, and performing a
comparison against the desired state. This is an optimization done for DSC resource authors to
simplify their implementation, but in some cases a custom behavior may need to be implemented. This is often the case when DSC is acting on a collection of objects managed by resource, or where
the structure of the settings is complex.

## Getting the Current State

OK, enough of the basics. Go ahead and click "Get".

![Local Image][16]

WinGet Studio will render a progress indicator just below the title bar to let you know it's working. When it's done you should see something similar to the following image. The package
"version:" and "useLatest:" might vary.

![Local Image][17]

The settings you provided WinGet via the Microsoft.WinGet/Package resource gave it enough information know you're looking to see if the Microsoft.AppInstaller package is installed (it
should be since that's how WinGet is installed). Additional properties were returned. These
additional properties represent a more complete picture of the state the device is currently
in.

## Testing against the Desired State
Next, click "Test".

![Local Image][18]

You should see the progress indicator, and when it's done, the box in the top right corner will
tell you if the device is in the desired state. In this case, you've already called "Get" and
since the App Installer is installed on your device, you will see the message "Machine is in the
desired state".

![Local Image][19]

For the sake of completeness, add "version: "0"" (the version is a string so it should be in
quotation marks) to the settings. Run "Test" again to see what it looks like when the device is
not in the desired state.

![Local Image][20]

## Setting the Desired State

Given the nature of "Set", rather than installing a new package on your machine (and having no
way to know what's not installed on your device), it's time to try a different resource out.

> [!WARNING]
> You could use the "_exist:" setting to ensure a package is uninstalled using "Set" for the
> Microsoft.WinGet/Package resource by setting the value to "false". I suggest caution since
> uninstalling the Microsoft.AppInstaller package could lead to the device ending up in a bad
> state.

In the resource input field, search for the "Microsoft.WinGet/UserSettingsFile" and select that
resource. If you take a look at the information for the resource (using the circled "i" button)
and then "View" the JSON schema for the resource, you might notice "settings:" is a required
property (currently line 6 shows required properties). 

Ensure the only properties you have specified under "Settings" is an empty "settings:" property in the editor. Then click the "Get" button.

> [!TIP]
> If you copy the YAML under the results and save it somewhere, you can easily restore those
> settings using "Set" when you're finished with this walk through. You can also open the
> WinGet Settings file by running `winget settings`. It's a JSON file, and I have Visual
> Studio Code as my default JSON editor to make it easier to work with.

Most users haven't customized their WinGet settings, but I happen to prefer the rainbow progress
bar in WinGet as well as having Sixels (icons rendered as images in the terminal) enabled. The
image below is the result on my machine. On your machine, the settings will include anything
you have configured with WinGet, or it could be a pair of empty curly braces ("settings {}") for
the settings property under "Results".


![Local Image][21]

Try changing a couple of your WinGet settings, I'd suggest trying out the rainbow progress bar
setting and enabling Sixels. I've included the sample "Settings" in YAML below. You can
copy and paste these settings into the editor under "Settings".

```YAML
settings:
  visual:
    progressBar: rainbow
    enableSixels: true
  $schema: https://aka.ms/winget-settings.schema.json
```

The other progressBar values you could try are:
* accent
* retro
* sixel
* disabled

> [!TIP]
> You can open up the raw [WinGet Settings Schema][22] to see what other WinGet settings you might
> want. 

> [!IMPORTANT]
> The "action:" property in the Microsoft.WinGet/UserSettingsFile resource determines
> how the WinGet Settings file is to be modified. The default behavior is "Partial" if you
> do not specify anything. Think of this like merging your settings under "Settings" with
> what ever is in the settings file. It's just going to change the values you provided.
> If you use "action: Full" you will be erasing all settings with the exception of those
> you specified.

Once you've specified the settings you want, it's time to press the "Set" button.

![Local Image][23]

To see the results of the WinGet settings you've modified, run `winget show GitHub.GitHubDesktop`
as an example of a package with an icon rendered as a Sixel (and you should see the modified
progress bar).

When you're done, you can restore your WinGet settings by either using "Set" with the results
captured earlier when you called "Get", or if you like your new settings, you can just continue
with the walk through.

![Local Image][24]

## Working with PowerShell DSC 2.0 Resources

Both of the DSC resources you've explored up till this point are DSC v3 resources. Next, you'll
explore some PowerShell DSC 2.0 resources. These resources are most often implemented in PowerShell
modules. Earlier versions (prior to 1.11) of WinGet only supported PowerShell DSC resources, but
now it's possible to mix and match (with caveats) both versions of resources in a WinGet
Configuration file.

WinGet Studio can search the PowerShell gallery for these resources, and it can show you the
settings (even if the module isn't installed). This could lead to some confusion when you want to
"Get", "Set", or "Test" these resources. If they haven't been installed on your machine, you will
not be able to use them.

If you've ever run `winget configure export --all -o export.winget` there is a good chance you
already have the Microsoft.Windows.Settings module installed. You can check by launching
PowerShell 7 and running `Get-DSCResource -Module Microsoft.Windows.Settings`. If you don't have
PowerShell 7 installed, you can run `winget install Microsoft.PowerShell` to get the latest version
available in the WinGet Community Repository.

![Local Image][25]

Once you've confirmed you have the Microsoft.Windows.Settings PowerShell DSC 2.0 resource
installed search for "windowssettings" in the resource input field. The first PowerShell DSC 2.0
resource you'll explore is Microsoft.Windows.Settings/WindowsSettings. Just like the DSC 3.0
resources were displayed, the name and version are displayed. This time, however the source on the
right is "PSGallery" (there is no indication of whether the resource is a 2.0 resource or a 1.1
resource currently in WinGet Studio).

![Local Image][26]

Click the circled "i" button to get information about the resource. This time you should notice
a big difference under the "Code" column. Instead of links to the schema, you'll see extracted
code from the resource.

![Local Image][27]

If you switch the "View" to "Code View", you'll see the PowerShell code used to define the
resources properties. This again is done to help identify properties used in the resource as
well as default values, and required properties. The WindowsSettings resource in the
Microsoft.Windows.Settings module requires a "(Key)" property "$SID" and as the comment indicates
it's a value that should not be set.

![Local Image][28]

If you have any properties under "Settings" in the editor, clear them out at this point. Then go
ahead and press the "Get" button. The results will indicate the settings for "SystemColorMode:",
"DeveloperMode", "TaskbarAlignment", and "AppColorMode" as they are currently configured on your
machine. You may notice the "SID:" setting is returned as a blank value. Copy all the results over
to settings and remove the line with "SID:" since there was a comment in the code stating it
should not be set.

> [!NOTE]
> The "DeveloperMode:" setting requires elevation to modify. The exception thrown when the resource
> isn't executed with elevation is visible in the "Code View" for the resource. This is what the
> error would look like if you tried to modify the setting using the "Validate a resource page" in
> WinGet Studio.
> ![Local Image][29]
> Remember the synthetic test provided by DSC.exe could also lead you to believe elevation isn't
> required because you can run set on the resource as long as you don't attempt to change the 
> value.

You can change the other settings without requiring elevation. Below is an image when I changed the
"AppColorMode:" to "Dark" while WinGet Studio's Theme was set to "Default". The change wasn't
fully applied in this case, but restarting WinGet Studio did result in the change being fully set.

![Local Image][30]

<!-- Link reference definitions -->
[01]: ../images/studio/0.100.302.0/First-Launch.png
[02]: ../images/studio/0.100.302.0/Validate-a-resource.png
[03]: https://learn.microsoft.com/powershell/dsc/overview?view=dsc-1.1
[04]: https://learn.microsoft.com/powershell/dsc/overview?view=dsc-2.0
[05]: https://learn.microsoft.com/en-us/powershell/dsc/overview?view=dsc-3.0
[06]: ../images/studio/0.100.302.0/Validate-search-winget.png
[07]: ../images/studio/0.100.302.0/Validate-resource-info.png
[08]: ../images/studio/0.100.302.0/Validate-winget-package-info.png
[09]: ../images/studio/0.100.302.0/Validate-winget-package-schema.png
[10]: ../images/studio/0.100.302.0/Validate-winget-package-schema-enumeration.png
[11]: ../images/studio/0.100.302.0/Validate-winget-package-resource-info.png
[12]: ../images/studio/0.100.302.0/Validate-winget-package-resource-summary.png
[12]: ../images/studio/0.100.302.0/Validate-winget-package-copy-as-YAML.png
[13]: ../images/studio/0.100.302.0/Validate-winget-package-resource-settings.png
[14]: https://github.com/microsoft/winget-cli/issues/5833
[15]: ../images/studio/0.100.302.0/Validate-winget-package-AppInstaller.png
[16]: ../images/studio/0.100.302.0/Validate-a-resource-Get.png
[17]: ../images/studio/0.100.302.0/Validate-winget-package-Get-AppInstaller.png
[18]: ../images/studio/0.100.302.0/Validate-a-resource-Test.png
[19]: ../images/studio/0.100.302.0/Validate-a-winget-package-Test.png
[20]: ../images/studio/0.100.302.0/Validate-a-winget-package-Test-invalid.png
[21]: ../images/studio/0.100.302.0/Validate-winget-settings-Get.png
[22]: https://aka.ms/winget-settings.schema.json
[23]: ../images/studio/0.100.302.0/Validate-a-resource-Set.png
[24]: ../images/WinGet/winget-show-GitHubDesktop.png
[25]: ../images/PowerShell/Get-DscResource-Microsoft.Windows.Settings.png
[26]: ../images/studio/0.100.302.0/Validate-WindowsSettings-v2.png
[27]: ../images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-info.png
[28]: ../images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-code.png
[29]: ../images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-DeveloperMode.png
[30]: ../images/studio/0.100.302.0/Validate-Microsoft.Windows.Settings-AppColorMode.png