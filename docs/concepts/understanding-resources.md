---
description: >-
  Learn about DSC resources, how to discover them from PowerShell Gallery, understand resource
  naming conventions, and how WinGet Studio catalogs and manages resources.
ms.date: 11/04/2025
ms.topic: concept-article
title: Understanding DSC resources
---

# Understanding DSC resources

Desired State Configuration (DSC) resources are the building blocks of configuration files in
WinGet Studio. Understanding what resources are, where they come from, and how to discover them
is essential for creating effective configurations.

## What are DSC resources

A DSC resource represents a standardized interface for managing a specific component or setting
on a system. Resources can manage anything from files and registry keys to installed software
and system settings.

DSC resources are designed to be [idempotent][07], meaning they can be applied multiple times
without changing the result beyond the initial application. This ensures that configurations are
safe to run repeatedly.

### Resource characteristics

Each DSC resource typically provides:

- **Get operation**: Retrieves the current state of the managed component
- **Set operation**: Enforces the desired state of the component
- **Test operation**: Checks if the component is in the desired state
- **Export operation**: Generates configuration from current state (Microsoft DSC v3 only)

> [!NOTE]
> The Export operation is newly introduced in Microsoft DSC v3. It allows resources to enumerate
> all instances of a managed component and generate a configuration document representing the
> current state. This feature is not available in PowerShell DSC v2 resources.

### Resource examples

Common resource types include:

- **Package installers**: `Microsoft.WinGet.DSC/WinGetPackage` for installing software
- **File management**: `PSDscResources/File` for managing files and directories
- **Registry settings**: `Microsoft.Windows/Registry` for Windows registry keys
- **System settings**: `Microsoft.Windows.Settings/WindowsSettings` for Windows features
- **Environment variables**: `PSDesiredStateConfiguration/Environment` for environment settings

## PowerShell DSC v2 vs Microsoft DSC v3 resources

WinGet Studio supports both PowerShell DSC v2 and Microsoft DSC v3 resources.

### PowerShell DSC v2 resources

PowerShell DSC v2 resources are PowerShell-based resources that were part of the original
PowerShell Desired State Configuration platform.

**Characteristics:**

- Written in PowerShell (`.psm1` files or PowerShell classes)
- Distributed through PowerShell modules
- Use the `resource` keyword in 0.2.0 configuration format
- Available from PowerShell Gallery
- Syntax: PowerShell-based configuration

**Example v2 resource usage:**

```yaml
# 0.2.0 format
properties:
  resources:
    - resource: PSDesiredStateConfiguration/Environment
      settings:
        Name: MY_VAR
        Value: MyValue
        Ensure: Present
```

### Microsoft DSC v3 resources

Microsoft DSC v3 resources are part of the modern Microsoft Desired State Configuration platform
and can be written in any language.

**Characteristics:**

- Can be written in any language (PowerShell, Python, C#, etc.)
- Use JSON schemas for property definitions
- Use the `type` keyword in Microsoft DSC 3.x configuration format
- Command-based execution through resource manifests
- Syntax: JSON/YAML-based configuration

**Example v3 resource usage:**

```yaml
# Microsoft DSC 3.x format
resources:
  - name: Configure Environment Variable
    type: Microsoft.DSC/PowerShell
    properties:
      resources:
        - name: MY_VAR
          type: PSDesiredStateConfiguration/Environment
          properties:
            Name: MY_VAR
            Value: MyValue
            Ensure: Present
```

### Using PowerShell DSC v2 resources in Microsoft DSC 3.x

To use PowerShell DSC v2 resources in Microsoft DSC 3.x configurations, wrap them with adapter
resources:

- **`Microsoft.Windows/WindowsPowerShell`**: Uses Windows PowerShell 5.1
- **`Microsoft.DSC/PowerShell`**: Uses PowerShell 7+

## PowerShell Gallery as a resource source

The PowerShell Gallery is the primary public repository for PowerShell modules, including those
that contain DSC resources.

### How WinGet Studio discovers resources

WinGet Studio uses the `PowerShellGalleryModuleProvider` to discover and catalog DSC resources:

1. **Search for DSC modules**: Queries PowerShell Gallery for modules tagged with `dscresource`
1. **Extract resource names**: Reads module tags with the prefix `PSDscResource_`
1. **Download and analyze**: Downloads module packages to extract detailed resource information
1. **Parse resource definitions**: Analyzes `.psm1` files and PowerShell classes
1. **Build resource catalog**: Creates a searchable catalog of available resources

### Resource naming conventions

PowerShell Gallery uses specific tagging conventions for DSC resources:

#### Module tags

Modules containing DSC resources are tagged with:

- **`dscresource`**: Indicates the module contains DSC resources
- **`PSDscResource_<ResourceName>`**: Identifies specific resources in the module

**Example module tags:**

```text
Tags: DSC, DesiredStateConfiguration, dscresource,
      PSDscResource_Environment, PSDscResource_File
```

#### Resource naming format

DSC resources follow a naming convention:

```text
<ModuleName>/<ResourceName>
```

**Examples:**

- `PSDesiredStateConfiguration/Environment`
- `Microsoft.WinGet.DSC/WinGetPackage`
- `Microsoft.Windows.Settings/WindowsSettings`
- `PSDscResources/File`

### Finding resources on PowerShell Gallery

You can search for DSC resources directly on PowerShell Gallery:

#### Using the website

1. Visit [PowerShell Gallery][01]
1. Search for `dscresource` tag
1. Filter by "Modules"
1. Review module details and tags

#### Using PowerShell

**PowerShell 5.1 with PowerShellGet:**

```powershell
# Find modules with DSC resources
Find-Module -Tag dscresource

# Search for specific resource
Find-Module -Tag PSDscResource_WinGetPackage

# Get module details
Find-Module -Name Microsoft.WinGet.DSC | Select-Object -ExpandProperty Tags
```

**PowerShell 7+ with Microsoft.PowerShell.PSResourceGet:**

```powershell
# Find modules with DSC resources
Find-PSResource -Tag dscresource

# Search for specific resource
Find-PSResource -Tag PSDscResource_WinGetPackage

# Get module details
Find-PSResource -Name Microsoft.WinGet.DSC | Select-Object -Property Name, Version, Tags

# Install using PSResourceGet
Install-PSResource -Name Microsoft.WinGet.DSC
```

> [!TIP]
> PowerShell 7+ users should use `Find-PSResource` and `Install-PSResource` from the
> `Microsoft.PowerShell.PSResourceGet` module, which provides improved performance and features
> over the older `PowerShellGet` module. It's also installed by default when installing PowerShell
> 7.2+ or above.

#### Using WinGet Studio

WinGet Studio provides a built-in resource catalog:

1. Open WinGet Studio
1. Edit or validate a resource
1. Search for resources by name or module
1. View resource details and properties by clicking on the ℹ️ icon
   1. ⚠️ This step is currently not supported for script-based resources
1. Add resources to your configuration

## Module and resource versioning

### Module versions

PowerShell modules follow semantic versioning (SemVer):

```text
Major.Minor.Patch[-Prerelease]
```

**Examples:**

- `1.0.0` - Stable release
- `2.3.1` - Minor update with patch
- `3.0.0-preview` - Prerelease version

### Resource versions

Resource versions typically match their containing module version. When you reference a resource,
you can specify a module version to ensure consistency:

**Using PowerShellGet (PowerShell 5.1+):**

```powershell
# Install specific module version
Install-Module -Name Microsoft.WinGet.DSC -RequiredVersion 1.2.0

# Install latest stable version
Install-Module -Name Microsoft.WinGet.DSC

# Install latest (including prerelease)
Install-Module -Name Microsoft.WinGet.DSC -AllowPrerelease
```

**Using PSResourceGet (PowerShell 7+):**

```powershell
# Install specific module version
Install-PSResource -Name Microsoft.WinGet.DSC -Version 1.2.0

# Install latest stable version
Install-PSResource -Name Microsoft.WinGet.DSC

# Install latest (including prerelease)
Install-PSResource -Name Microsoft.WinGet.DSC -Prerelease
```

### Version compatibility

When working with resources:

- **PowerShell DSC v2 resources**: Check module compatibility with your PowerShell version
- **Microsoft DSC v3 resources**: Verify the resource manifest specifies supported DSC versions
- **Module dependencies**: Some modules depend on others; install all required dependencies

## How WinGet Studio catalogs resources

WinGet Studio maintains a local catalog of available resources to enable fast searching and
filtering.

### Catalog structure

The resource catalog includes:

- **Module information**: Name, version, source (PowerShell Gallery or Local)
- **Resource list**: All resources provided by each module
- **Resource properties**: Detailed property schemas for each resource
- **DSC version**: Whether the resource is v2 or v3

### Catalog sources

WinGet Studio pulls resources from two main sources:

#### PowerShell Gallery

- **Discovered resources**: Automatically found through PowerShell Gallery API
- **Public availability**: Available to all users
- **Update frequency**: Catalog refreshes periodically
- **Source identifier**: `PSGallery`

#### Local Microsoft DSC v3

- **Installed resources**: Microsoft DSC v3 resources installed on your system
- **Command-based**: Resources with `.dsc.resource.json` manifests
- **System PATH**: Discovered from executables in PATH
- **Source identifier**: `LocalDscV3`

### Refreshing the catalog

To update the resource catalog in WinGet Studio:

1. Open WinGet Studio
1. Navigate to Settings
1. Click "Clear cache" button under "Resources" section
1. Next time you browse a module or resource, the data will be reloaded

## Working with resource schemas

Each resource defines a schema that describes its configurable properties.

### Viewing resource schemas

**In WinGet Studio:**

1. Browse the resource catalog
1. Select a resource
1. Click the ℹ️ icon to view the resource properties
   1. ⚠️ This is currently not supported for script-based resources

**Using CLI:**

```powershell
# Get resource schema
wingetstudio dsc schema --resource Microsoft.WinGet.DSC/WinGetPackage
```

**Using DSC directly:**

```powershell
# For Microsoft DSC v3 resources
dsc resource schema --resource Microsoft.Windows/Registry
```

### Understanding property types

Resource properties can be:

- **String**: Text values (e.g., `Name: "MyApp"`)
- **Integer**: Numeric values (e.g., `Port: 8080`)
- **Boolean**: True/false values (e.g., `Ensure: Present`)
- **Array**: Lists of values (e.g., `Tags: [dev, test]`)
- **Object**: Nested properties (e.g., `Credential: { User: "admin" }`)

### Required vs optional properties

Schemas indicate which properties are:

- **Required**: Must be specified for the resource to work
- **Optional**: Can be omitted; resource uses defaults
- **Key**: Uniquely identify a resource instance

## Best practices for working with resources

### Choose the right resource

Select resources that:

- Match your requirements exactly
- Are actively maintained
- Have good documentation
- Support your target platforms

### Verify resource availability

Before using a resource:

**Using PowerShellGet:**

```powershell
# Check if module is installed
Get-Module -ListAvailable -Name Microsoft.WinGet.DSC

# Install if missing
Install-Module -Name Microsoft.WinGet.DSC -Scope CurrentUser
```

**Using PSResourceGet (PowerShell 7+):**

```powershell
# Check if module is installed
Get-PSResource -Name Microsoft.WinGet.DSC

# Install if missing
Install-PSResource -Name Microsoft.WinGet.DSC -Scope CurrentUser
```

### Use specific versions in production

For production configurations, specify exact module versions:

**Using PowerShellGet:**

```powershell
# Install specific version
Install-Module -Name Microsoft.WinGet.DSC -RequiredVersion 1.2.0 -Force
```

**Using PSResourceGet (PowerShell 7+):**

```powershell
# Install specific version
Install-PSResource -Name Microsoft.WinGet.DSC -Version 1.2.0 -TrustRepository
```

### Document resource dependencies

In your configuration, document required modules:

```yaml
# Required modules:
# - Microsoft.WinGet.DSC (v1.2.0+)
# - PSDesiredStateConfiguration (v2.0.5+)
#
# Install with PowerShellGet:
# Install-Module Microsoft.WinGet.DSC -Scope CurrentUser
# Install-Module PSDesiredStateConfiguration -Scope CurrentUser
#
# Or with PSResourceGet (PowerShell 7+):
# Install-PSResource Microsoft.WinGet.DSC -Scope CurrentUser
# Install-PSResource PSDesiredStateConfiguration -Scope CurrentUser
```

## Related content

- [WinGet Configuration versions][02]
- [Customize exported configurations][03]
- [Get started with WinGet Studio][04]
- [PowerShell Gallery][01]
- [DSC Resources overview][05]
- [Differences from PowerShell DSC][06]

<!-- Link reference definitions -->
[01]: https://www.powershellgallery.com
[02]: ./configuration-versions.md
[03]: ../how-to/customize-exported-configuration.md
[04]: ../get-started/index.md
[05]: https://learn.microsoft.com/powershell/dsc/concepts/resources/overview
[06]: https://learn.microsoft.com/en-us/powershell/dsc/overview?view=dsc-3.0#differences-from-powershell-dsc
[07]: https://learn.microsoft.com/en-us/powershell/dsc/overview/dscforengineers?view=dsc-1.1
