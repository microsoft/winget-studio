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

The Manage a Configuration File feature in WinGet Studio enables you to create and modify WinGet
Configuration files. This is also used to view, validate, test and apply WinGet Configuration
files in the GUI (Graphical User Interface). This tutorial walks you through using the tool to
work with WinGet 0.2.0 Configuration files and Microsoft Desired State Configuration files.

## Prerequisites

Before you begin, ensure you have:

- **WinGet Studio** installed from the [GitHub releases page][01]
- **Windows Package Manager (WinGet)** installed
- **PowerShell 7 or later** for working with Microsoft DSC 3.x resources
- **Administrator privileges** for certain resource operations

To install PowerShell 7 if needed:

```powershell
winget install Microsoft.PowerShell
```

## What you'll learn

In this tutorial, you'll learn how to:

- Navigate to the Manage a configuration file page
- View an existing WinGet Configuration file
- Validate, Test, and Apply a WinGet Configuration file

## Launching the Manage Configuration experience

When you launch WinGet Studio, the Home screen displays two main options:

![WinGet Studio home screen with Manage Configuration and Validate Resource buttons][02]

Select **Manage Configuration** to open the configuration management page.

![The Manage a Configuration file page with New configuration and Open configuration buttons][03]

The Configuration Management page provides tools to work with WinGet Configuration files. Once a
file has been created or opened you will be abe to edit, validate, test, apply, or save the file.

Start a new configuration by clicking on **New configuration**

![New Configuration][04]

WinGet Studio will generate a new WinGet Configuration File using Microsoft Desired State
Configuration. It will add a new "Module/Resource" to the left side of the editor, and it will
show the modifiable fields to the right side.

![New Configuration Template][05]

> [!NOTE]
> This first resource instance added to the configuration file is a placeholder. It will correctly
> validate in WinGet Studio, but it would fail if you attempted to run it in its current state.

If you are already familiar with the ![Validate a resource][06] experience in WinGet Studio, this
may look familiar, but there are extra fields related to how this instance of a resource is
included in the configuration file.

Expand the "Module/Resource" by clicking the arror to the right of the "Edit" button next to the "Module/Resource" on the left side of the editor. This will open up a visual view of the resource instance in the
configuration file.

![Expand Resource view on left side][07]

<!-- Link reference definitions -->
[01]: https://github.com/microsoft/winget-studio/releases
[02]: .././images/studio/0.100.302.0/First-Launch.png
[03]: .././images/studio/0.100.302.0/Manage-a-configuration-file.png
[04]: .././images/studio/0.100.302.0/Manage-New-Configuration.png
[05]: .././images/studio/0.100.302.0/Manage-New-Configuration-Template.png
[06]: ./validate-resources.md
[07]: .././images/studio/0.100.302.0/Manage-Configuration-Expand-Resource.png