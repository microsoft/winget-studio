---
description: >-
  Learn about WinGet Studio, an experimental tool for authoring and managing WinGet Configuration
  files using Desired State Configuration resources.
ms.date: 11/04/2025
ms.topic: overview
title: WinGet Studio overview
---

# WinGet Studio overview

WinGet Studio is an experimental tool designed to simplify the creation and management of WinGet
Configuration files. Building configuration files for Windows Package Manager (WinGet) with
Desired State Configuration (DSC) resources can be complex for users unfamiliar with the
technology stack. WinGet Studio addresses this challenge by providing both a graphical interface
and a command-line interface for working with configurations.

## What is WinGet Studio

WinGet Studio helps users create, modify, and deploy WinGet Configuration files that define the
desired state of a Windows system. It integrates with the DSC platform to provide a comprehensive
solution for configuration as code.

### Key capabilities

- **Visual configuration authoring**: Create configurations using a graphical interface
- **Resource catalog**: Browse and search available DSC resources
- **Resource testing**: Test individual resources with get, set, and test operations
- **Configuration validation**: Verify configurations before deployment
- **Export and import**: Work with both 0.2.0 and Microsoft DSC 3.x format configurations
- **CLI support**: Automate configuration tasks with command-line tools

## WinGet Configuration files

WinGet Configuration files define the desired state of a system using DSC resources. These files
can specify:

- Software packages to install
- System settings to configure
- Files and registry keys to manage
- Environment variables to set
- Services to enable or disable

WinGet Studio supports both configuration formats:

- **0.2.0 format**: The original WinGet Configuration format
- **Microsoft DSC 3.x format**: The modern Microsoft DSC platform format

For more information, see [WinGet Configuration versions][01].

## DSC resource exploration

A key feature of WinGet Studio is its resource catalog, which helps you discover and understand
available DSC resources.

### Resource discovery

WinGet Studio automatically discovers resources from:

- **PowerShell Gallery**: Public repository of PowerShell modules
- **Local system**: Installed Microsoft DSC v3 resources

### Resource management

The tool provides capabilities to:

- Browse available resources by name or module
- View resource properties and schemas
- Test resource operations (get, set, test)
- Generate sample configurations for resources
- Refresh the catalog to discover new resources

For more information, see [Understanding DSC resources][02].

## When to use WinGet Studio

### Use WinGet Studio when

- **Learning DSC**: You're new to Desired State Configuration
- **Visual authoring**: You prefer a graphical interface for creating configurations
- **Resource discovery**: You need to find and understand available resources
- **Rapid prototyping**: You want to quickly build and test configurations
- **Complex configurations**: You're managing multiple resources with dependencies
- **Resource testing**: You need to test resources before including them in configurations

### Use raw WinGet/DSC when

- **Automation scripts**: You're building fully automated deployment pipelines
- **Simple configurations**: You have straightforward, well-understood configurations
- **CI/CD integration**: You're integrating with build and deployment systems
- **Advanced features**: You need low-level control over DSC operations
- **Performance**: You're running configurations at scale

WinGet Studio and raw tools complement each other—use WinGet Studio for authoring and testing,
then deploy with WinGet or DSC commands in production.

## Application architecture

WinGet Studio consists of two main components:

### GUI application

The graphical interface provides:

- **Configuration editor**: Visual editing of configuration files
- **Resource catalog**: Searchable database of available resources
- **Property editor**: Form-based editing of resource properties
- **Validation engine**: Real-time syntax and schema validation
- **Test runner**: Execute test operations on configurations

### CLI application

The command-line interface (`wingetstudio`) provides:

- **DSC operations**: Execute `get`, `set`, `test`, and `export` operations
- **Resource management**: Query and test individual resources
- **Settings management**: Configure WinGet Studio preferences
- **Manifest generation**: Create DSC resource manifests
- **Automation support**: Script configuration tasks

For more information, see the [CLI reference][11].

## Integration with DSC platform

WinGet Studio integrates with the DSC platform in several ways:

### PowerShell DSC v2 support

- Uses PowerShell DSC v2 resources through adapter resources
- Supports class-based and MOF-based PowerShell resources
- Compatible with existing PowerShell Gallery modules
- Works with Windows PowerShell 5.1 and PowerShell 7+

### Microsoft DSC v3 support

- Exports configurations in Microsoft DSC 3.x format by default
- Discovers command-based Microsoft DSC v3 resources
- Uses JSON schemas for resource validation
- Supports cross-platform DSC capabilities

### WinGet integration

- Creates WinGet-compatible configuration files
- Supports `winget configure` command execution
- Integrates with WinGet package management
- Follows WinGet Configuration schema standards

## Features in development

> [!NOTE]
> WinGet Studio is experimental software under active development.

Planned and in-progress features include:

- **Automated module management**: Automatic installation of required PowerShell modules
- **Configuration templates**: Pre-built templates for common scenarios
- **Version control integration**: Git integration for configuration management
- **Remote resource testing**: Test resources on remote systems
- **Enhanced validation**: Advanced validation rules and best practice checks
- **Import from existing systems**: Generate configurations from current system state

## Getting started

To get started with WinGet Studio:

1. [Install WinGet Studio][03] from GitHub releases
1. [Create your first configuration][04] using the GUI or CLI
1. [Explore the resource catalog][02] to discover available resources
1. [Customize and export][05] configurations for deployment

## System requirements

- **Operating System**: Windows 10 version 1809 or later
- **Windows Package Manager**: Latest version of WinGet
- **PowerShell**: PowerShell 7+ recommended for Microsoft DSC 3.x features
- **.NET**: .NET 8 runtime (included with MSIX installer)

## Telemetry and privacy

WinGet Studio is instrumented to collect usage and diagnostic data to help improve the product.
Telemetry is only enabled in official builds distributed by Microsoft.

If you build the application yourself, instrumentation is not enabled and no data is sent to
Microsoft.

To opt-out of telemetry:

1. Open WinGet Studio Settings
1. Enable "Disable Telemetry"
1. Restart the application

For more information, see the [privacy statement][06].

## Contributing

WinGet Studio is an open-source project that welcomes contributions:

- Report issues and feature requests on [GitHub][07]
- Submit pull requests for bug fixes and improvements
- Provide feedback on experimental features
- Share configuration examples and best practices

For contribution guidelines, see [CONTRIBUTING.md][08].

## Related content

- [Get started with WinGet Studio][04]
- [WinGet Configuration versions][01]
- [Understanding DSC resources][02]
- [Customize exported configurations][05]
- [WinGet Configuration documentation][09]
- [DSC platform overview][10]

<!-- Link reference definitions -->
[01]: ./concepts/configuration-versions.md
[02]: ./concepts/understanding-resources.md
[03]: https://github.com/microsoft/winget-studio/releases
[04]: ./get-started/index.md
[05]: ./how-to/customize-exported-configuration.md
[06]: https://github.com/microsoft/winget-studio/blob/main/PRIVACY.md
[07]: https://github.com/microsoft/winget-studio/issues
[08]: https://github.com/microsoft/winget-studio/blob/main/CONTRIBUTING.md
[09]: https://learn.microsoft.com/en-us/windows/package-manager/configuration/
[10]: https://learn.microsoft.com/powershell/dsc/overview
[11]: ./reference/cli/index.md
