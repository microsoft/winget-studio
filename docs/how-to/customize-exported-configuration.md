---
description: >-
  Learn how to customize configuration files exported from WinGet Studio, including
  understanding the structure, adding parameters, modifying dependencies, and best practices.
ms.date: 11/04/2025
ms.topic: how-to-article
title: Customize exported configuration files
---

# Customize exported configuration files

WinGet Studio makes it easy to create and export configuration files for system setup and
management. Once you export a configuration, you might want to customize it for specific needs,
such as adding parameters, adjusting dependencies, or modifying resource properties. This guide
shows you how to understand and customize exported configuration files effectively.

## Understanding exported configurations

When you export a configuration from WinGet Studio, it generates a Microsoft DSC 3.x format file
in YAML.
The exported file includes:

- **Schema declaration**: Points to the Microsoft DSC 3.x schema
- **Metadata**: Information about the configuration (e.g., created by WinGet Studio)
- **Resources**: The list of resource instances you configured in WinGet Studio

### Example exported configuration

<!-- markdownlint-disable MD013 -->

```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
metadata:
  Microsoft.WinGet.Studio:
    version: 0.1.0
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

## Common customizations

### Adding parameters

Parameters make your configuration reusable across different environments. You can add
parameters to customize values without modifying the configuration logic.

**Add a parameters section:**

<!-- markdownlint-disable MD013 -->

```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
parameters:
  packageId:
    type: string
    defaultValue: Git.Git
    metadata:
      description: The WinGet package identifier to install
  packageSource:
    type: string
    defaultValue: winget
    allowedValues:
      - winget
      - msstore
resources:
  - name: Install Package
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: WinGet Package
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: "[parameters('packageId')]"
            source: "[parameters('packageSource')]"
```

<!-- markdownlint-enable MD013 -->

**Using parameters:**

Create a `parameters.json` file:

```json
{
  "parameters": {
    "packageId": "Microsoft.VisualStudioCode",
    "packageSource": "winget"
  }
}
```

Apply with parameters:

```powershell
dsc config set --file configuration.dsc.yaml --parameters-file parameters.json
```

### Modifying dependencies

Dependencies ensure resources are processed in the correct order. You can add or modify the
`dependsOn` property to control execution sequence.

**Add dependencies:**

```yaml
resources:
  - name: Install PowerShell 7
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: PowerShell Package
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: Microsoft.PowerShell
            source: winget
  
  - name: Install PowerShell Module
    type: Microsoft.Windows/WindowsPowerShell
    dependsOn:
      - "[resourceId('Microsoft.Windows/WindowsPowerShell', 'Install PowerShell 7')]"
    properties:
      resources:
        - name: Az Module
          type: PSDesiredStateConfiguration/PSModule
          properties:
            Name: Az
            Ensure: Present
```

The `resourceId()` function creates a unique identifier for the dependency reference.

### Adding metadata

Metadata helps document your configuration and can be used by tools for additional context.

**Add custom metadata:**

```yaml
metadata:
  Microsoft.WinGet.Studio:
    version: 0.1.0
  author: Your Name
  description: Development environment setup
  created: 2025-11-04
  tags:
    - development
    - setup
    - automation
```

### Modifying resource properties

You can adjust resource properties to fine-tune behavior.

**Example - Adding retry logic:**

```yaml
resources:
  - name: Install Package
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: Git Package
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: Git.Git
            source: winget
            # Add version specification
            version: 2.42.0
```

### Using variables

Variables help avoid repetition and make configurations more maintainable.

**Add variables section:**

```yaml
variables:
  commonSource: winget
  packageVersion: 1.0.0
  installPath: C:\Tools
resources:
  - name: Install Tool
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: Tool Package
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: SomeTool
            source: "[variables('commonSource')]"
```

### Conditional resource inclusion

Use parameters to conditionally include resources.

**Example - Optional features:**

```yaml
parameters:
  installGit:
    type: bool
    defaultValue: true
  installVSCode:
    type: bool
    defaultValue: false
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
            _exist: "[parameters('installGit')]"
```

## Best practices for customization

### Document your changes

Add comments to explain customizations:

<!-- markdownlint-disable MD013 -->

```yaml
# Development tools configuration
# Author: Team Name
# Last updated: 2025-11-04
# Purpose: Setup development environment with Git, VS Code, and PowerShell tools
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
```

<!-- markdownlint-enable MD013 -->

### Use descriptive names

Choose clear, descriptive names for resources:

```yaml
# ❌ Avoid generic names
- name: Package 1
  type: Microsoft.Windows/WindowsPowerShell

# ✅ Use descriptive names
- name: Install Git for Version Control
  type: Microsoft.Windows/WindowsPowerShell
```

### Validate after customization

Always validate your configuration after making changes:

```powershell
# Validate syntax and structure
dsc config test --file configuration.dsc.yaml

# Check for errors
dsc config validate --file configuration.dsc.yaml
```

### Version control your configurations

Store configurations in version control to track changes:

```powershell
git add configuration.dsc.yaml parameters.json
git commit -m "Add PowerShell 7 dependency to module installation"
```

### Test in non-production first

Test customized configurations in a safe environment before production:

```powershell
# Test in a virtual machine or dev environment
dsc config test --file configuration.dsc.yaml --parameters-file dev-params.json

# Verify with whatif (when supported)
dsc config set --file configuration.dsc.yaml --what-if
```

### Modularize complex configurations

Break down large configurations into smaller, focused files:

```yaml
# base-tools.dsc.yaml - Core development tools
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

<!-- markdownlint-disable MD013 -->

```yaml
# dev-environment.dsc.yaml - Main configuration
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
resources:
  - name: Base Tools
    type: Microsoft.DSC/Include
    properties:
      configurationFile: ./base-tools.dsc.yaml
  - name: IDE Setup
    type: Microsoft.DSC/Include
    properties:
      configurationFile: ./ide-tools.dsc.yaml
```

<!-- markdownlint-enable MD013 -->

## Handling module dependencies

> [!NOTE]
> Automated module installation is currently under development in WinGet Studio, which will make
> it easier to ensure all required PowerShell modules are available when running configurations
> on different devices.

Until automated module management is available, document required modules:

<!-- markdownlint-disable MD013 -->

```yaml
# Required PowerShell Modules:
# Install-Module Microsoft.WinGet.DSC -Scope CurrentUser
# Install-Module PSDesiredStateConfiguration -Scope CurrentUser
# Install-Module Microsoft.Windows.Settings -Scope CurrentUser
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
```

<!-- markdownlint-enable MD013 -->

### Creating a module installation configuration

You can create a configuration that installs required modules first:

```yaml
resources:
  - name: Install Required Modules
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: WinGet DSC Module
          type: PSDesiredStateConfiguration/PSModule
          properties:
            Name: Microsoft.WinGet.DSC
            Ensure: Present
        - name: Windows Settings Module
          type: PSDesiredStateConfiguration/PSModule
          properties:
            Name: Microsoft.Windows.Settings
            Ensure: Present
  
  - name: Configure System
    type: Microsoft.Windows/WindowsPowerShell
    dependsOn:
      - "[resourceId('Microsoft.Windows/WindowsPowerShell',
         'Install Required Modules')]"
    properties:
      resources:
        # Your configuration resources here
```

## Troubleshooting customizations

### Common issues

#### Configuration fails after adding parameters

Check that parameter references use the correct syntax:

```yaml
# ❌ Incorrect
properties:
  id: $packageId

# ✅ Correct
properties:
  id: "[parameters('packageId')]"
```

#### Dependencies not working

Ensure `resourceId()` references match exact resource names and types:

```yaml
# Resource definition
- name: Install PowerShell
  type: Microsoft.Windows/WindowsPowerShell

# ❌ Incorrect dependency reference
dependsOn:
  - Install PowerShell

# ✅ Correct dependency reference
dependsOn:
  - "[resourceId('Microsoft.Windows/WindowsPowerShell', 'Install PowerShell')]"
```

#### YAML syntax errors

Validate YAML structure with proper indentation:

```powershell
# Use a YAML validator
dsc config test --file configuration.dsc.yaml 2>&1 | Select-String "error"
```

### Getting help

If you encounter issues:

1. Check the [Microsoft DSC 3.x documentation][01] for syntax and function references
1. Validate your YAML syntax with an online YAML validator
1. Test individual resources before combining them
1. Review WinGet Studio logs for detailed error messages

## Related content

- [WinGet Configuration versions][02]
- [Understanding DSC resources][03]
- [Get started with WinGet Studio][04]
- [DSC configuration functions reference][05]

<!-- Link reference definitions -->
[01]: https://learn.microsoft.com/powershell/dsc/overview
[02]: ../concepts/configuration-versions.md
[03]: ../concepts/understanding-resources.md
[04]: ../get-started/index.md
[05]: https://learn.microsoft.com/powershell/dsc/reference/schemas/config/functions/overview
