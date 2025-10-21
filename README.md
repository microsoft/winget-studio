# WinGet Studio

This repository contains the source code for [WinGet Studio](https://github.com/microsoft/winget-studio).

[![Build Status](https://microsoft.visualstudio.com/Apps/_apis/build/status%2FApp%20Installer%2FWinGet-Studio%20-%20Dev?repoName=microsoft%2Fwinget-studio&branchName=main)](https://microsoft.visualstudio.com/Apps/_build/latest?definitionId=179787&repoName=microsoft%2Fwinget-studio&branchName=main)

## Overview

WinGet Studio is designed to help users create and modify WinGet Configuration files. It also includes the ability to use "get", "set", and "test" for any installed Desired State Configuration (DSC) resources.

## Installation

The **WinGet Studio** is available for download from the [winget-studio](https://github.com/microsoft/winget-studio/releases) repository.  To install the package, simply click the the MSIX file in your browser.  Once it has downloaded, click open.

## Building from source

### Apply the WinGet configuration file
To configure your machine for WinGet Studio, apply the WinGet configuration file containing the necessary settings for building the application.

```ps
git clone 'https://github.com/microsoft/winget-studio'
cd 'winget-studio'
winget configure '.\.config\configuration.winget'
```

### Build the application

#### Option 1: Visual Studio
Open [./src/WinGetStudio.sln](./src/WinGetStudio.sln) in Visual Studio and build.

#### Option 2: PowerShell Script
Open a PowerShell terminal and run the build script.

> [!TIP]
> Certificate signing requires admin privileges.  To sign, run the following as admin.

```ps
.\Build.ps1
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com. More
information is available in our [CONTRIBUTING.md](/CONTRIBUTING.md) file.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, please refer to the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Data/Telemetry

The WinGet Studio application is instrumented to collect usage and diagnostic (error) data and sends it to Microsoft to help improve the product.

If you build the application yourself the instrumentation will not be enabled and no data will be sent to Microsoft.

The WinGet Studio application respects machine wide privacy settings and users can opt-out on their device, as documented in the Microsoft Windows privacy statement [here](https://support.microsoft.com/help/4468236/diagnostics-feedback-and-privacy-in-windows-10-microsoft-privacy).

In short to opt-out, do one of the following:

**Windows 11**: Go to `Start`, then select `Settings` > `Privacy & security` > `Diagnostics & feedback` > `Diagnostic data` and unselect `Send optional diagnostic data`.

**Windows 10**: Go to `Start`, then select `Settings` > `Privacy` > `Diagnostics & feedback`, and select `Required diagnostic data`.

See the [privacy statement](/PRIVACY.md) for more details.
