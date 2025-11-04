---
description: >-
  Learn about the differences between WinGet Configuration 0.2.0 and Microsoft DSC 3.x formats,
  including schema differences, PowerShell module dependencies, and migration considerations.
ms.date: 11/04/2025
ms.topic: concept-article
title: WinGet Configuration versions
---

# WinGet Configuration versions

WinGet Configuration files come in two major formats: the 0.2.0 format and the Microsoft DSC 3.x
format.
Understanding the differences between these formats is essential for creating, maintaining, and
migrating configuration files in WinGet Studio.

## Configuration format overview

### WinGet Configuration 0.2.0

The 0.2.0 format is the original WinGet Configuration format that integrates with PowerShell
Desired State Configuration (PSDSC) version 2. This format uses a specific schema and relies on
WinGet to automatically discover and load PowerShell DSC modules.

**Key characteristics:**

- Uses schema: `https://aka.ms/configuration-dsc-schema/0.2`
- Configuration version: `0.2.0`
- PowerShell modules are automatically discovered by WinGet
- Uses PSDSC v2 resources
- Resources are defined with the `resource` keyword
- Supports directives for resource behavior

**Example configuration:**

```yaml
# yaml-language-server: $schema=https://aka.ms/configuration-dsc-schema/0.2
properties:
  resources:
    - resource: Microsoft.Windows.Settings/WindowsSettings
      directives:
        description: Enable Developer Mode
        allowPrerelease: true
        securityContext: elevated
      settings:
        DeveloperMode: true
    - resource: Microsoft.WinGet.DSC/WinGetPackage
      id: vsPackage
      directives:
        description: Install Visual Studio 2022
        securityContext: elevated
      settings:
        id: Microsoft.VisualStudio.2022.Community
        source: winget
  configurationVersion: 0.2.0
```

### Microsoft DSC 3.x format

The Microsoft DSC 3.x format is the modern configuration format based on the new Microsoft
Desired State Configuration v3 platform. This format provides more flexibility and requires
explicit PowerShell 7 and module installation.

**Key characteristics:**

- Uses schema:
  `https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json`
- No explicit configuration version property
- Requires PowerShell 7 or later
- PowerShell modules must be explicitly installed
- Resources are defined with the `type` keyword
- Supports configuration parameters and variables
- Enhanced metadata and expression functions

**Example configuration:**

<!-- markdownlint-disable MD013 -->

```yaml
# yaml-language-server:
# $schema=https://aka.ms/dsc/schemas/v3/bundled/config/document.vscode.json
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
metadata:
  Microsoft.WinGet.Studio:
    version: 0.1.0
resources:
  - name: Enable Developer Mode
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: Developer Mode Setting
          type: Microsoft.Windows.Settings/WindowsSettings
          properties:
            DeveloperMode: true
  - name: Install Visual Studio
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: VS 2022 Community
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: Microsoft.VisualStudio.2022.Community
            source: winget
```

<!-- markdownlint-enable MD013 -->

## Key differences

### Schema and structure

| Aspect                    | 0.2.0 Format                                  | Microsoft DSC 3.x Format                                                                     |
|---------------------------|-----------------------------------------------|----------------------------------------------------------------------------------------------|
| **Schema URL**            | `https://aka.ms/configuration-dsc-schema/0.2` | `https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json` |
| **Root structure**        | `properties` wrapper with `resources` array   | Direct `resources` array                                                                     |
| **Resource identifier**   | `resource` keyword                            | `type` keyword                                                                               |
| **Configuration version** | `configurationVersion: 0.2.0`                 | Not specified                                                                                |
| **Resource directives**   | `directives` object                           | Not used (metadata instead)                                                                  |

### PowerShell module dependencies

**0.2.0 format:**

- WinGet automatically discovers and loads PowerShell DSC modules
- No manual module installation required
- Relies on Windows PowerShell (5.1) or PowerShell 7
- Modules must be available in the PowerShell module path

**Microsoft DSC 3.x format:**

- Requires PowerShell 7 or later
- PowerShell modules must be explicitly installed before running the configuration
- Uses adapter resources like `Microsoft.Windows/WindowsPowerShell` or `Microsoft.DSC/PowerShell`
- Provides more control over module versions and sources

### Resource adapters

In the Microsoft DSC 3.x format, PowerShell DSC resources are accessed through adapter resources:

- **`Microsoft.Windows/WindowsPowerShell`**: Invokes PowerShell DSC v2 script-based or class-based
  resources in Windows PowerShell 5.1
- **`Microsoft.DSC/PowerShell`**: Invokes PowerShell DSC v2 class-based resources in PowerShell 7+

These adapters enable the use of classic PowerShell DSC v2 resources within the new Microsoft
DSC 3.x framework.

## How WinGet Studio handles both formats

WinGet Studio supports both configuration formats through its internal `DSCVersion` enumeration:

- **`DSCVersion.V2`**: Represents 0.2.0 format configurations
- **`DSCVersion.V3`**: Represents Microsoft DSC 3.x format configurations

When you work with configuration files in WinGet Studio:

1. **Opening files**: WinGet Studio detects the format based on the schema and structure
1. **Editing**: The UI adapts to the configuration format being used
1. **Exporting**: By default, WinGet Studio exports configurations in Microsoft DSC 3.x format
1. **Resource catalog**: Resources are tagged with their DSC version for compatibility

## Migration considerations

### Migrating from 0.2.0 to Microsoft DSC 3.x

When migrating from 0.2.0 to Microsoft DSC 3.x format, consider the following:

1. **Schema update**: Change the schema URL to the Microsoft DSC 3.x schema
1. **Structure changes**: Remove the `properties` wrapper and use direct `resources` array
1. **Resource keyword**: Replace `resource` with `type` for each resource instance
1. **Adapter resources**: Wrap PowerShell DSC v2 resources in adapter resources
1. **Directives**: Convert directives to metadata or resource properties
1. **Module dependencies**: Ensure PowerShell 7 is installed and required modules are
   available

**Before (0.2.0):**

```yaml
# yaml-language-server: $schema=https://aka.ms/configuration-dsc-schema/0.2
properties:
  resources:
    - resource: Microsoft.WinGet.DSC/WinGetPackage
      directives:
        description: Install Git
      settings:
        id: Git.Git
        source: winget
  configurationVersion: 0.2.0
```

**After (Microsoft DSC 3.x):**

<!-- markdownlint-disable MD013 -->

```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
metadata:
  description: Install Git
resources:
  - name: Install Git
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: Git Package
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: Git.Git
            source: winget
```

<!-- markdownlint-enable MD013 -->

### PowerShell module requirements

For Microsoft DSC 3.x configurations, you need to ensure that:

1. **PowerShell 7 or later is installed**:

   ```powershell
   winget install Microsoft.PowerShell
   ```

1. **Required modules are installed**:

   ```powershell
   Install-PSResource -Name Microsoft.WinGet.DSC -Scope CurrentUser
   Install-PSResource -Name Microsoft.Windows.Settings -Scope CurrentUser
   ```

1. **Modules are up to date**:

   ```powershell
   Update-PSResource -Name Microsoft.WinGet.DSC
   ```

> [!NOTE]
> Automated module installation is currently under development in WinGet Studio, which will make
> it easier to ensure all required modules are available when running Microsoft DSC 3.x
> configurations on different devices.

## Best practices

### Choosing a format

- **Use 0.2.0** when:
  - Working with existing 0.2.0 configurations
  - Need automatic module discovery
  - Target systems may not have PowerShell 7

- **Use Microsoft DSC 3.x** when:
  - Creating new configurations
  - Need advanced features like parameters and variables
  - Want explicit control over module versions
  - Working with cross-platform scenarios

### Format consistency

When working with configuration files:

1. Stick to one format per configuration file
1. Use consistent naming conventions
1. Document any format-specific requirements
1. Test configurations on target systems before deployment

## Related content

- [Understanding DSC resources][01]
- [Customizing exported configurations][02]
- [WinGet Configuration documentation][03]
- [Microsoft DSC v3 overview][04]

<!-- Link reference definitions -->
[01]: ./understanding-resources.md
[02]: ../how-to/customize-exported-configuration.md
[03]: https://learn.microsoft.com/en-us/windows/package-manager/configuration/
[04]: https://learn.microsoft.com/powershell/dsc/overview
