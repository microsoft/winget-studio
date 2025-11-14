---
description: >-
  Learn how to run, modify, and create WinGet Configuration files using the Manage
  a configuration file feature in WinGet Studio. This hands-on walkthrough covers
  working with WinGet Configuration files.
ms.date: 11/10/2025
ms.topic: tutorial
title: Manage WinGet Configuration Files
---

# Manage WinGet Configuration Files

The **Manage a Configuration File** feature in WinGet Studio enables you to create and modify
WinGet Configuration files. You can also use it to view, validate, test, and apply WinGet
Configuration files in the graphical user interface (GUI). This tutorial walks you through using
the tool to work with WinGet 0.2.0 Configuration files and Microsoft Desired State Configuration
(DSC) files.

## Prerequisites

Before you begin, ensure you have:

- **WinGet Studio** installed from the [GitHub releases page][01]
- **Windows Package Manager (WinGet)** installed
- **PowerShell 7 or later** for working with Microsoft DSC 3.x resources
- **Administrator privileges** for certain resource operations
- **Familiarity with DSC and PowerShell DSC 2.0** - Complete the
  [resource validation guide][06] first if you're new to these concepts. The validation guide also
  installs the required DSC resources and tools needed to complete this guide.

## What you'll learn

In this tutorial, you learn how to:

- Navigate to the Manage a configuration file page
- Create a new WinGet Configuration file
  - Add a resource to a configuration
  - Specify dependencies for sequencing in a configuration
  - Specify that a resource requires elevation
- View an existing WinGet Configuration file
- Validate, test, and apply a WinGet Configuration file

## Launching the Manage Configuration experience

When you launch WinGet Studio, the Home screen displays two main options:

![WinGet Studio home screen with Manage Configuration and Validate Resource buttons][02]

Select **Manage Configuration** to open the configuration management page.

![The Manage a Configuration file page with New configuration and Open configuration buttons][03]

The Configuration Management page provides tools to work with WinGet Configuration files. Once
you create or open a file, you can edit, validate, test, apply, or save it.

In this step-by-step example, you author a new WinGet configuration file to ensure a package is
installed, and modify a setting for that package requiring elevation. The WinGet package
"Microsoft.AppInstaller" from the community repository is used (it should already be installed on
your system).

## New Configuration

Start a new configuration by selecting **New configuration**.

![New Configuration][04]

WinGet Studio generates a new WinGet Configuration File using DSC. It adds a new "Module/Resource"
to the left side of the editor and shows the modifiable fields on the right side.

![New Configuration Template][05]

> [!NOTE]
> This first resource instance added to the configuration file is a placeholder. It correctly
> validates in WinGet Studio, but it would fail if you attempted to run it in its current state.

If you're already familiar with the ![Validate a resource][06] experience in WinGet Studio, this
interface might look familiar. However, there are extra fields related to how this instance of a
resource is included in the configuration file.

Expand the "Module/Resource" by selecting the arrow to the right of the **Edit** button next to
the "Module/Resource" on the left side of the editor. This action opens a visual view of the
resource instance in the configuration file.

![Expand Resource view on left side][07]

## Modifying a Resource Instance

In the **Resource type** field, enter `winget`. WinGet Studio performs a search to find matching
resources. This should look familiar to the "Validate a resource" experience. Available resources
appear on the left side of the dropdown menu, and their types and DSC versions appear on the right
side.

Select the **Microsoft.WinGet/Package** resource from the dropdown menu.

![Entering WinGet in the Resource type field on the right side][08]

The **Resource name** field is used to uniquely identify an instance of a resource being used in a
WinGet Configuration file. This value is used to map dependencies to guarantee sequencing in your
configuration. Short descriptive values for name are often the best.

Since WinGet is installed via the Microsoft.AppInstaller package, update the **Resource name** to
`Install WinGet`. The **Description** field is an optional field where you can provide a more
verbose explanation of what this resource instance does. Update the **Description** to
`Ensure WinGet is installed`.

![Resource name "install WinGet" description "Ensure WinGet is installed"][09]

Select the **ℹ️** button to the right of **Microsoft.WinGet/Package**.

![Information button to the right of the resource][10]

The resource properties are displayed.

![Microsoft.WinGet/Package Resource properties][11]

Select **Copy as YAML**.

![Copy as YAML highlighted in the resource properties display][12]

Paste the results in the editor below the description field as shown. This method of copying the
properties includes default values for each setting.

![YAML pasted into properties under Description field][13]

In this example, you only need two properties. Modify the properties so that only the **id** and
**source** are displayed as shown:

```YAML
id: Microsoft.AppInstaller
source: winget
```

![Microsoft.AppInstaller package specified from the WinGet source][14]

Now, select the **Update** button on the lower right corner of WinGet Studio.

![Update the resource instance in the configuration file][15]

Notice the visual resource display on the left has been updated and a notification appears on the
top right stating "Configuration unit updated".

![Visual display on the left has been updated][16]

## Validate a WinGet Configuration File

The next step is to validate the configuration. When you select the **Validate** button for the
configuration file, WinGet Studio checks for syntax correctness.

![Validate the configuration button highlighted][17]

If the configuration file is valid, WinGet Studio notifies you that the "Configuration code is
valid".

![Configuration code is valid][18]

## Test a WinGet Configuration File

Next, select the **Test** button next to the **Validate** button.

![Test button next to Validate Button][19]

This action executes the "Test" method on the entire configuration file. No changes are made to
your system when you run "Test". If your system is in the desired state (which it should be for
this example), WinGet Studio informs you "Machine is in the desired state".

![Machine is in the desired state][20]

Given your machine is already in the desired state as shown in the previous step, DSC doesn't
perform any action to change anything on your system when you apply the configuration. DSC always
performs "Test" before "Set".

## Apply a WinGet Configuration File

For completeness, select the **Apply** button next to the **Test** button.

![Apply is highlighted next to Test][21]

A warning appears any time you run a configuration file. If you agree, select the **Apply** button
below the warning.

![WinGet Configuration Terms / Warning][22]

WinGet Studio displays progress while the configuration is running.

![Applying the Configuration][23]

Once the configuration completes, the results are visible.

![Configuration applied][24]

Select **Done** to return to the Manage a configuration file experience.

## View the Configuration file as YAML

Select the **Code** button to toggle between the graphical resource view and the code view to see
the raw YAML configuration file.

![Code Button][25]

Notice the graphical representation of the configuration on the left side of the editor now
displays the raw YAML configuration file.

![Code Button][26]

Select the **Code** button again to return to the visual representation.

Now is a good time to save the configuration file. Since this configuration file was built using
the **New configuration** button, the **Save** button has been disabled. Use the **Save As** button
to save the file. Consider using "tutorial.WinGet" as the file name, and choosing your desktop is
convenient if you don't have another preferred location to save the file.

![Save As Button][27]

WinGet Studio informs you once the file has been saved.

![Configuration saved successfully][28]

## Add a Resource Instance to a Configuration

The configuration you've been building up to this point only contains a single resource instance.
Next, you add another resource to this configuration. The new resource has a dependency on the
existing resource in the configuration, and it requires elevation (administrator privilege) to run.

Select the **Add resource** button.

![Add Resource Button][29]

Follow the same process as before. In the **Resource type**, search for `winget`. Select the
**Microsoft.WinGet/AdminSettings** resource. Use `Configure WinGet` for the **Resource name**.
Use `Ensure WinGet Local Manifests are Enabled` for the **Description**. Use the following YAML in
the editor below the Description:

```YAML
settings:
  LocalManifestFiles: true
```

> [!IMPORTANT]
> YAML is whitespace sensitive. For the Microsoft.WinGet/AdminSettings properties, you need two
> spaces on the second line before "LocalManifestFiles".

Once you've entered everything, select the **Update** button. WinGet Studio should look like the
following image.

![WinGet Admin Settings Configured in WinGet Studio][30]

Now, select **Elevated** from the **Security Context** dropdown.

![Selecting Elevated from the Security Context][31]

Select the **Update** button to update the resource settings. Notice the shield icon in the visual
view for the resource on the left side of the editor.

![Elevated Resource with a Shield][32]

The next step is to specify the dependency for this resource instance. It wouldn't make sense to
configure the WinGet Administrator Settings if WinGet wasn't installed on your system.

> [!NOTE]
> Another common scenario for resource dependencies is adding an extension or a workload to an
> IDE. You would need to ensure the IDE was installed on the machine before attempting to
> configure it.

Select the **Dependencies** button, and check the box next to **Install WinGet**.

![Set Install WinGet as a dependency][33]

Select the **Update** button to save your changes. Then expand the visual view of the resource on
the left side of the editor. The location of the expander is indicated by the red arrow in the
following image. You see the Dependencies now includes **Install WinGet**.

![Dependencies include Install WinGet][34]

<!-- Link reference definitions -->
[01]: https://github.com/microsoft/winget-studio/releases
[02]: .././images/studio/0.100.302.0/First-Launch.png
[03]: .././images/studio/0.100.302.0/Manage-a-configuration-file.png
[04]: .././images/studio/0.100.302.0/Manage-New-Configuration.png
[05]: .././images/studio/0.100.302.0/Manage-New-Configuration-Template.png
[06]: ./validate-resources.md
[07]: .././images/studio/0.100.302.0/Manage-Configuration-Expand-Resource.png
[08]: .././images/studio/0.100.302.0/Manage-Configuration-winget.png
[09]: .././images/studio/0.100.302.0/Manage-Configuration-resource-name-description.png
[10]: .././images/studio/0.100.302.0/Manage-Configuration-resource-info.png
[11]: .././images/studio/0.100.302.0/Manage-Configuration-winget-info.png
[12]: .././images/studio/0.100.302.0/Manage-Configuration-resource-YAML.png
[13]: .././images/studio/0.100.302.0/Manage-Configuration-resource-properties.png
[14]: .././images/studio/0.100.302.0/Manage-Configuration-AppInstaller.png
[15]: .././images/studio/0.100.302.0/Manage-Configuration-Update-AppInstaller.png
[16]: .././images/studio/0.100.302.0/Manage-Configuration-AppInstaller-updated.png
[17]: .././images/studio/0.100.302.0/Manage-Configuration-Validate.png
[18]: .././images/studio/0.100.302.0/Manage-Configuration-Validated.png
[19]: .././images/studio/0.100.302.0/Manage-Configuration-Test.png
[20]: .././images/studio/0.100.302.0/Manage-Configuration-Tested.png
[21]: .././images/studio/0.100.302.0/Manage-Configuration-Apply.png
[22]: .././images/studio/0.100.302.0/Manage-Configuration-Apply-Terms.png
[23]: .././images/studio/0.100.302.0/Manage-Configuration-Applying.png
[24]: .././images/studio/0.100.302.0/Manage-Configuration-Applied.png
[25]: .././images/studio/0.100.302.0/Manage-Configuration-Code.png
[26]: .././images/studio/0.100.302.0/Manage-Configuration-Code-View.png
[27]: .././images/studio/0.100.302.0/Manage-Configuration-SaveAs.png
[28]: .././images/studio/0.100.302.0/Manage-Configuration-Saved.png
[29]: .././images/studio/0.100.302.0/Manage-Configuration-Add-Resource.png
[30]: .././images/studio/0.100.302.0/Manage-Configuration-WinGet-Admin.png
[31]: .././images/studio/0.100.302.0/Manage-Configuration-Security-Context.png
[32]: .././images/studio/0.100.302.0/Manage-Configuration-Elevated.png
[33]: .././images/studio/0.100.302.0/Manage-Configuration-Dependency.png
[34]: .././images/studio/0.100.302.0/Manage-Configuration-View-Dependency.png
