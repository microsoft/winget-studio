---
description: >-
  Step-by-step guide to migrate your WinGet Configuration file from the 0.2.0 schema to the
  Microsoft DSC 3.x format, including schema updates, syntax changes, and module requirements.
ms.date: 11/04/2025
ms.topic: how-to-article
title: Migrate from 0.2.0 to Microsoft DSC 3.x
---

# Migrate from 0.2.0 to Microsoft DSC 3.x

This guide walks you through updating your WinGet Configuration file from the 0.2.0 schema to the
Microsoft DSC 3.x format. The Microsoft DSC 3.x format provides enhanced capabilities and better
integration with modern configuration management practices.

## Prerequisites

Before migrating your configuration file, ensure you have:

- PowerShell 7 or later installed
- WinGet Studio (for visual assistance with migration)
- Your existing 0.2.0 configuration file
- Understanding of the differences between formats (see [Configuration versions][01])

## Migration overview

The migration process involves five main steps:

1. Update the schema declaration
1. Convert the configuration structure
1. Update resource definitions
1. Handle directives and metadata
1. Test the migrated configuration

## Step 1: Update the schema declaration

### Before (0.2.0)

```yaml
# yaml-language-server: $schema=https://aka.ms/configuration-dsc-schema/0.2
properties:
  configurationVersion: 0.2.0
  resources:
    # resources here
```

### After (Microsoft DSC 3.x)

<!-- markdownlint-disable MD013 -->

```yaml
# yaml-language-server: $schema=https://aka.ms/dsc/schemas/v3/bundled/config/document.vscode.json
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
resources:
  # resources here
```

<!-- markdownlint-enable MD013 -->

**Changes:**

- Replace the YAML language server comment with the Microsoft DSC 3.x schema
- Add the `$schema` property to the root of the document
- Remove the `properties` wrapper
- Remove the `configurationVersion` property (not used in DSC 3.x)

## Step 2: Convert the configuration structure

### Root-level changes

The 0.2.0 format wraps everything in a `properties` object, while Microsoft DSC 3.x uses a flat
root structure.

#### Before (0.2.0)

```yaml
properties:
  resources:
    - resource: Microsoft.WinGet.DSC/WinGetPackage
      # resource configuration
  configurationVersion: 0.2.0
```

#### After (Microsoft DSC 3.x)

<!-- markdownlint-disable MD013 -->

```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
resources:
  - name: Install Package
    type: Microsoft.WinGet.DSC/WinGetPackage
    properties:
      # resource properties
```

<!-- markdownlint-enable MD013 -->

**Changes:**

- Remove the `properties` wrapper
- Move `resources` to the root level
- Each resource now requires a `name` property

### Handling assertions (optional)

In 0.2.0, the `assertions` section was used for resources that should be validated but not
enforced. In Microsoft DSC 3.x, assertions are typically not needed for WinGet Configuration files.
Most resources can validate and enforce state together.

If you have specific validation-only requirements, you can use `DSC/AssertionGroup` to
wrap resources that should only be tested:

#### Before (0.2.0)

```yaml
properties:
  assertions:
    - resource: Microsoft.Windows/Registry
      directives:
        description: Verify registry setting
      settings:
        keyPath: HKCU\Software\MyApp
        valueName: Setting
        valueData: Enabled
```

#### After (Microsoft DSC 3.x)

<!-- markdownlint-disable MD013 -->

```yaml
resources:
  - name: Registry Setting Assertion
    type: Microsoft.DSC/AssertionGroup
    properties:
      $schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
      resources:
        - name: Verify registry setting
          type: Microsoft.Windows/Registry
          properties:
            keyPath: HKCU\Software\MyApp
            valueName: Setting
            valueData: Enabled
```

<!-- markdownlint-enable MD013 -->

> [!NOTE]
> For most WinGet Configuration scenarios, you won't need assertions or `AssertionGroup`. Simply
> use regular resources which will both validate and apply the desired state.

## Step 3: Update resource definitions

### Resource keyword changes

The most significant change is replacing the `resource` keyword with `type` and adding a `name`.

#### Before (0.2.0) - Resource definition

```yaml
- resource: Microsoft.WinGet.DSC/WinGetPackage
  id: vsPackage
  directives:
    description: Install Visual Studio 2022
    securityContext: elevated
  settings:
    id: Microsoft.VisualStudio.2022.Community
    source: winget
```

#### After (Microsoft DSC 3.x) - Resource definition

```yaml
- name: Install Visual Studio 2022
  type: Microsoft.WinGet.DSC/WinGetPackage
  properties:
    id: Microsoft.VisualStudio.2022.Community
    source: winget
```

**Changes:**

- Replace `resource:` with `type:`
- Replace `id:` with `name:` (the name is descriptive, not an identifier)
- Replace `settings:` with `properties:`
- Move directives to appropriate locations (see Step 4)

### Nested PowerShell DSC v2 resources

For PowerShell DSC v2 resources, you need to wrap them in the `Microsoft.Windows/WindowsPowerShell`
adapter.

#### Before (0.2.0) - Nested resource

```yaml
- resource: Microsoft.Windows.Settings/WindowsSettings
  directives:
    description: Enable Developer Mode
  settings:
    DeveloperMode: true
```

#### After (Microsoft DSC 3.x) - Nested resource

```yaml
- name: Developer Mode Configuration
  type: Microsoft.Windows/WindowsPowerShell
  properties:
    resources:
      - name: Enable Developer Mode
        type: Microsoft.Windows.Settings/WindowsSettings
        properties:
          DeveloperMode: true
```

**Changes:**

- Wrap PowerShell DSC v2 resources in the `Microsoft.Windows/WindowsPowerShell` adapter
- The adapter's `properties` contains a `resources` array
- Each nested resource follows the same structure with `name`, `type`, and `properties`

> [!TIP]
> You can directly reference PowerShell DSC v2 resources by their type name without wrapping them
> in an adapter (e.g., `type: Microsoft.Windows.Settings/WindowsSettings`). However, this approach
> has a performance cost: DSC must determine which adapter to use and build up the adapter cache
> each time. For better performance, especially in production, explicitly wrap PowerShell DSC v2
> resources in the appropriate adapter (`Microsoft.Windows/WindowsPowerShell` or
> `Microsoft.DSC/PowerShell`).

## Step 4: Handle directives and metadata

Directives in 0.2.0 become metadata or configuration properties in Microsoft DSC 3.x.

### Description directive

#### Before (0.2.0) - Description

```yaml
- resource: Microsoft.WinGet.DSC/WinGetPackage
  directives:
    description: Install development tools
  settings:
    # resource settings
```

#### After (Microsoft DSC 3.x) - Description

```yaml
- name: Install development tools
  type: Microsoft.WinGet.DSC/WinGetPackage
  properties:
    # resource properties
```

The `description` directive becomes the resource `name` in Microsoft DSC 3.x.

### Security context directive

### Before (0.2.0)

```yaml
directives:
  securityContext: elevated
```

### After (Microsoft DSC 3.x)

Add metadata at the root level to specify elevation requirements:

<!-- markdownlint-disable MD013 -->

```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
metadata:
  Microsoft.DSC:
    securityContext: elevated
resources:
  # resources here
```

<!-- markdownlint-enable MD013 -->

### Allow prerelease directive

#### Before (0.2.0) - Prerelease

```yaml
directives:
  allowPrerelease: true
```

#### After (Microsoft DSC 3.x) - Prerelease

This directive doesn't have a direct equivalent in Microsoft DSC 3.x. The resource itself should
handle prerelease versions through its properties if supported.

### Module directive

#### Before (0.2.0) - Module

```yaml
directives:
  module: MyModule
```

#### After (Microsoft DSC 3.x) - Module

In Microsoft DSC 3.x, modules must be explicitly installed before running the configuration. Add
installation resources at the beginning of your configuration:

```yaml
resources:
  - name: Install Required Module
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: Install MyModule
          type: PSDesiredStateConfiguration/PSModule
          properties:
            Name: MyModule
            Ensure: Present
  # other resources that use MyModule
```

## Step 5: Complete migration example

Here's a complete example showing a 0.2.0 configuration and its Microsoft DSC 3.x equivalent.

### Original 0.2.0 configuration

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
    - resource: Microsoft.WinGet.DSC/WinGetPackage
      id: gitPackage
      directives:
        description: Install Git
        dependsOn:
          - vsPackage
      settings:
        id: Git.Git
        source: winget
  configurationVersion: 0.2.0
```

### Migrated Microsoft DSC 3.x configuration

<!-- markdownlint-disable MD013 -->

```yaml
# yaml-language-server:
# $schema=https://aka.ms/dsc/schemas/v3/bundled/config/document.vscode.json
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
metadata:
  Microsoft.DSC:
    securityContext: elevated
  Microsoft.WinGet.Studio:
    version: 1.0.0
    description: Development environment setup
resources:
  - name: Developer Mode Configuration
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: Enable Developer Mode
          type: Microsoft.Windows.Settings/WindowsSettings
          properties:
            DeveloperMode: true
  - name: Install Visual Studio 2022
    type: Microsoft.WinGet.DSC/WinGetPackage
    properties:
      id: Microsoft.VisualStudio.2022.Community
      source: winget
  - name: Install Git
    type: Microsoft.WinGet.DSC/WinGetPackage
    dependsOn:
      - "[resourceId('Microsoft.WinGet.DSC/WinGetPackage', 'Install Visual Studio 2022')]"
    properties:
      id: Git.Git
      source: winget
```

<!-- markdownlint-enable MD013 -->

## Testing your migrated configuration

After migrating your configuration, test it thoroughly:

### Using WinGet CLI

```powershell
# Validate the configuration syntax
winget configure validate --file path\to\your-config.dsc.yaml

# Test what would change (dry run)
winget configure --file path\to\your-config.dsc.yaml --accept-configuration-agreements

# Apply the configuration
winget configure --file path\to\your-config.dsc.yaml
```

### Using WinGet Studio

1. Open the migrated configuration file in WinGet Studio
1. Use the **Validate** feature to check for syntax errors
1. Use the **Test** feature to see what would change without applying
1. Use the **Apply** feature to execute the configuration

### Using WinGet Studio CLI

```powershell
# Validate the configuration syntax
wingetstudio dsc validate --file path\to\your-config.dsc.yaml

# Test the configuration (dry run)
wingetstudio dsc test --file path\to\your-config.dsc.yaml

# Apply the configuration
wingetstudio dsc set --file path\to\your-config.dsc.yaml

# Get the current state of resources in the configuration
wingetstudio dsc get --file path\to\your-config.dsc.yaml
```

## Common migration issues

### Issue: Module not found

**Symptom:** Error indicating a PowerShell DSC module cannot be found.

**Solution:** Ensure all required PowerShell modules are installed before running the configuration.
Add module installation resources at the beginning of your configuration:

```yaml
- name: Install Required Modules
  type: Microsoft.Windows/WindowsPowerShell
  properties:
    resources:
      - name: Install Module
        type: PSDesiredStateConfiguration/PSModule
        properties:
          Name: ModuleName
          Ensure: Present
```

### Issue: Syntax validation errors

**Symptom:** The configuration file fails schema validation.

**Solution:** Use a schema-aware editor like VS Code with the YAML extension. Ensure you're using
the correct schema URL and that all required properties (`name`, `type`, `properties`) are present
for each resource.

### Issue: Dependencies not working

**Symptom:** Resources execute in the wrong order.

**Solution:** Use the `dependsOn` property with proper `resourceId()` syntax:

```yaml
dependsOn:
  - "[resourceId('ResourceType', 'ResourceName')]"
```

### Issue: Security context not applied

**Symptom:** Resources that require elevation fail.

**Solution:** Add the `securityContext` metadata at the root level:

```yaml
metadata:
  Microsoft.DSC:
    securityContext: elevated
```

## Additional resources

- [Configuration versions][01] - Detailed comparison of configuration formats
- [Understanding resources][02] - Learn about resource types and discovery
- [Customize exported configuration][03] - Advanced configuration techniques

<!-- Link reference definitions -->
[01]: ../concepts/configuration-versions.md
[02]: ../concepts/understanding-resources.md
[03]: customize-exported-configuration.md
