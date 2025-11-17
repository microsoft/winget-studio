# WinGet Studio

This repository contains the source code for [WinGet Studio (Experimental)][winget-studio-main].

> [!WARNING]  
> This software should be considered experimental.

[![Build Status][build-badge]][build-link]

## Overview

Building WinGet Configuration files (Configuration as Code with Microsoft (DSC) Desired State
Configuration) is a complex undertaking for folks who are not already familiar with the technology
stack. WinGet Studio is an experiment to see how we can help make it easier to author configuration
files and build or test DSC resources.

WinGet Studio is designed to help users create and modify WinGet Configuration files. It also
includes the ability to use "get", "set", and "test" for any **installed** Desired State
Configuration (DSC) resources.

## Documentation

Learn more about WinGet Studio and how to use it:

- [Overview][docs-overview] - Introduction to WinGet Studio's features and capabilities
- [Getting Started][docs-getting-started] - Quick start guide for new users
- [Working with Resources][docs-validate-resources] - Step-by-step guide to work with DSC resources
- [Working with Configuration Files][docs-manage-configurations] - Step-by-step guide to work with WinGet Configuration Files
- [Configuration Versions][docs-config-versions] - Understanding 0.2.0 vs Microsoft DSC 3.x formats
- [Migrate to DSC 3.x][docs-migration] - Step-by-step guide to upgrade your configurations
- [Understanding Resources][docs-resources] - Learn about DSC resources and PowerShell Gallery
- [Customize Configuration][docs-customize] - Advanced configuration customization techniques
- [CLI Reference - DSC Commands][docs-cli-dsc] - Command-line interface for DSC operations
- [CLI Reference - Settings Commands][docs-cli-settings] - Command-line interface for settings
- [Changelog][docs-changelog] - Release notes and version history

## Installation

**WinGet Studio** is available for download from the
[WinGet Studio releases][winget-studio-releases] repository. To install the package, simply click
the link and download the MSIX file using your browser. Once it has downloaded, click to open the
package and follow the prompts to install it.

## Building from source

### Apply the WinGet configuration file

To configure your machine for WinGet Studio, apply the WinGet configuration file containing the
necessary settings for building the application.

```powershell
git clone 'https://github.com/microsoft/winget-studio'
cd 'winget-studio'
winget configure '.\.config\configuration.winget'
```

### Build the application

#### Option 1: Visual Studio

Open [./src/WinGetStudio.sln][solution-file] in Visual Studio and build.

#### Option 2: PowerShell Script

Open a PowerShell terminal and run the build script.

> [!TIP]
> Certificate signing requires admin privileges.  To sign, run the following as admin.

```powershell
.\Build.ps1
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit the [Microsoft CLA site][ms-cla]. More
information is available in our [CONTRIBUTING.md][contributing-file] file.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the
instructions provided by the bot. You will only need to do this once across all repos using our
CLA.

This project has adopted the [Microsoft Open Source Code of Conduct][ms-code-of-conduct]. For more
information, please refer to the [Code of Conduct FAQ][code-of-conduct-faq] or contact
[opencode@microsoft.com][opencode-email] with any additional questions or comments.

## Privacy Statement

The application logs basic diagnostic data (telemetry). For more privacy information and what we collect, see our [WinGet Studio Data and Privacy documentation][privacy-file].

<!-- Link reference definitions -->
[winget-studio-main]: https://github.com/microsoft/winget-studio
[winget-studio-releases]: https://github.com/microsoft/winget-studio/releases
[build-badge]: https://microsoft.visualstudio.com/Apps/_apis/build/status%2FApp%20Installer%2FWinGet-Studio%20-%20Dev?repoName=microsoft%2Fwinget-studio&branchName=main
[build-link]: https://microsoft.visualstudio.com/Apps/_build/latest?definitionId=179787&repoName=microsoft%2Fwinget-studio&branchName=main
[solution-file]: ./src/WinGetStudio.sln
[ms-cla]: https://cla.opensource.microsoft.com
[contributing-file]: /CONTRIBUTING.md
[ms-code-of-conduct]: https://opensource.microsoft.com/codeofconduct/
[code-of-conduct-faq]: https://opensource.microsoft.com/codeofconduct/faq/
[opencode-email]: mailto:opencode@microsoft.com
[privacy-file]: /PRIVACY.md
[docs-overview]: /docs/overview.md
[docs-getting-started]: /docs/get-started/index.md
[docs-validate-resources]: /docs/get-started/validate-resources.md
[docs-manage-configurations]: /docs/get-started/manage-configurations.md
[docs-config-versions]: /docs/concepts/configuration-versions.md
[docs-migration]: /docs/how-to/migrate-configuration-to-dsc3.md
[docs-resources]: /docs/concepts/understanding-resources.md
[docs-customize]: /docs/how-to/customize-exported-configuration.md
[docs-cli-dsc]: /docs/reference/cli/dsc.md
[docs-cli-settings]: /docs/reference/cli/settings.md
[docs-changelog]: /docs/changelog.md
