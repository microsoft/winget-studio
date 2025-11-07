---
description: >-
  Get started with WinGet Studio to create, manage, and deploy configuration files for Windows
  system setup using Desired State Configuration resources.
ms.date: 11/04/2025
title: Get started with WinGet Studio
---

# Get started with WinGet Studio

WinGet Studio is an experimental tool that simplifies creating and managing WinGet Configuration
files. This guide walks you through installing WinGet Studio, creating your first configuration,
and deploying it to a system.

## Prerequisites

Before you begin, ensure you have:

- **Windows 10 version 1809 or later**
- **Windows Package Manager (WinGet)** installed
- **PowerShell 7 or later** (for Microsoft DSC 3.x configurations)
- **Administrator privileges** (for certain operations)

### Install PowerShell 7

If you don't have PowerShell 7 installed:

```powershell
winget install Microsoft.PowerShell
```

Verify the installation:

```powershell
pwsh --version
```

### Install WinGet Studio

Download and install WinGet Studio from the [GitHub releases page][01]:

1. Download the latest `.msix` file
1. Double-click the file to install
1. Follow the installation prompts
1. Launch WinGet Studio from the Start menu

## Creating your first configuration

### Option 1: Using the GUI

1. **Launch WinGet Studio**
   - Open WinGet Studio from the Start menu
   - The main window displays the configuration editor

1. **Create a new configuration**
   - Click "New nonfiguration"
   - WinGet Studio creates a new configuration with a placeholder resource

1. **Add resources from the catalog**
   - Click "Add Resource" in the toolbar
   - Browse or search the resource catalog
   - Select a resource (e.g., `Microsoft.WinGet.DSC/WinGetPackage`)
   - Click "Add to Configuration"

1. **Configure resource properties**
   - Select the added resource in the configuration
   - Fill in required properties in the properties panel
   - Example for WinGetPackage:
     - **id**: `Git.Git`
     - **source**: `winget`

1. **Add more resources**
   - Repeat the process to add additional resources
   - Configure dependencies if needed
   - Set security contexts (e.g., `elevated`) where required

1. **Test the configuration**
   - Click "Test" in the toolbar
   - Review the test results
   - Fix any errors or warnings

1. **Save the configuration**
   - Click "Save", or "Save as" if you want to create a new file
   - Choose a location and filename (e.g., `dev-setup.dsc.yaml`)
   - WinGet Studio exports in Microsoft DSC 3.x format by default

### Option 2: Using the CLI

Create a configuration using the WinGet Studio CLI:

```powershell
# Create a new configuration file
$config = @"
`$schema:
  https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
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
"@

# Save to file
$config | Out-File -FilePath "dev-setup.dsc.yaml" -Encoding utf8
```

## Working with the resource catalog

WinGet Studio maintains a catalog of available DSC resources.

### Browsing resources

1. **Open the resource catalog**
   - In WinGet Studio, click "Resources" in the navigation
   - The catalog displays available resources

1. **Search for resources**
   - Use the search box to filter by name
   - Filter by module or source
   - View resource details and properties

1. **Refresh the catalog**
   - Click "Settings" > "Clear resources cache" > Clear cache
   - WinGet Studio updates the catalog from PowerShell Gallery and local DSC v3 resources

### Understanding resource sources

Resources come from two main sources:

- **PowerShell Gallery**: Public repository of PowerShell modules with DSC resources
- **Local Microsoft DSC v3**: Resources installed on your local system

## Testing and validating configurations

### Test before applying

Always test configurations before applying them:

**Using the GUI:**

1. Open your configuration in WinGet Studio
1. Click "Test" in the toolbar
1. Review the test results for each resource
1. Check for resources not in desired state

**Using the CLI:**

```powershell
# Test with wingetstudio CLI
wingetstudio dsc test --file dev-setup.dsc.yaml

# Or use DSC directly
dsc config test --file dev-setup.dsc.yaml
```

### Validate configuration structure

Verify your configuration is well-formed:

**Using WinGet Studio:**

1. Open your configuration
1. Click "Validate" in the toolbar
1. Review any schema or syntax errors

**Using CLI:**

```powershell
dsc config validate --file dev-setup.dsc.yaml
```

## Applying configurations

Once you've tested your configuration, apply it to enforce the desired state.

### Apply with WinGet

```powershell
winget configure --file dev-setup.dsc.yaml
```

### Apply with DSC

```powershell
dsc config set --file dev-setup.dsc.yaml
```

### Monitor progress

- WinGet and DSC provide progress output during application
- Review success/failure messages for each resource
- Check logs if resources fail

## Exporting configurations

### Export using CLI

```powershell
# Export configuration state
wingetstudio dsc export --resource Microsoft.WinGet.DSC/WinGetPackage
```

## Opening existing configurations

### Open in WinGet Studio GUI

1. Click "Open configuration"
1. Browse to your configuration file
1. Select the file (`.winget`, `.dsc.yaml`, or `.dsc.yml`)
1. Click "Open"

WinGet Studio detects whether it's 0.2.0 or Microsoft DSC 3.x format automatically.

## Managing dependencies

Resources can depend on other resources to ensure correct execution order.

### Add dependencies in GUI

1. Select a resource in edit mode
1. Open the "Dependencies" panel
1. Select resources that must run first
1. WinGet Studio adds `dependsOn` references

### Add dependencies manually

```yaml
resources:
  - name: Install PowerShell 7
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: PowerShell
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

## Example: Complete workflow

Here's a complete example workflow:

```powershell
# Step 1: Create configuration
$config = @"
`$schema:
  https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2023/08/config/document.json
metadata:
  description: Basic development environment
resources:
  - name: Install Git
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: Git
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: Git.Git
            source: winget
  - name: Install VS Code
    type: Microsoft.Windows/WindowsPowerShell
    properties:
      resources:
        - name: VSCode
          type: Microsoft.WinGet.DSC/WinGetPackage
          properties:
            id: Microsoft.VisualStudioCode
            source: winget
"@

# Step 2: Save configuration
$config | Out-File -FilePath "dev-setup.dsc.yaml" -Encoding utf8

# Step 3: Install required modules
Install-Module Microsoft.WinGet.DSC -Scope CurrentUser -Force

# Step 4: Test configuration
dsc config test --file dev-setup.dsc.yaml

# Step 5: Apply configuration
winget configure --file dev-setup.dsc.yaml

# Step 6: Verify
dsc config get --file dev-setup.dsc.yaml
```

## Next steps

Now that you've created your first configuration:

- [Customize exported configurations][02] to add parameters and variables
- [Understand DSC resources][03] to learn about available resources
- [Learn about configuration versions][04] for format differences
- Explore the WinGet Studio CLI for automation scenarios

## Troubleshooting

### Module not found errors

If you see "module not found" errors:

```powershell
# Install missing modules
Install-Module Microsoft.WinGet.DSC -Scope CurrentUser
Install-Module PSDesiredStateConfiguration -Scope CurrentUser
```

### Permission denied errors

Some resources require elevated permissions:

1. Run WinGet Studio as administrator
1. Or use the CLI with elevated privileges:

```powershell
Start-Process pwsh -Verb RunAs -ArgumentList "-Command",
  "dsc config set --file dev-setup.dsc.yaml"
```

### Configuration validation errors

If validation fails:

1. Check YAML syntax (indentation, colons, hyphens)
1. Verify resource names match available resources
1. Ensure required properties are specified
1. Check schema URL is correct

## Related content

- [WinGet Configuration versions][04]
- [Understanding DSC resources][03]
- [Customize exported configurations][02]
- [WinGet Configuration documentation][05]

<!-- Link reference definitions -->
[01]: https://github.com/microsoft/winget-studio/releases
[02]: ../how-to/customize-exported-configuration.md
[03]: ../concepts/understanding-resources.md
[04]: ../concepts/configuration-versions.md
[05]: https://learn.microsoft.com/en-us/windows/package-manager/configuration/
