---
title: "Glossary: WinGet Studio"
description: >-
  A glossary of terms for WinGet Studio and Desired State Configuration
ms.topic: glossary
ms.date: 11/04/2025
---

# Glossary: WinGet Studio

WinGet Studio and Desired State Configuration (DSC) use several terms that might have different
definitions elsewhere. This document lists the terms, their meanings, and shows how they're
formatted in the documentation.

<!-- markdownlint-disable MD028 MD036 MD024 -->

## Configuration terms

### WinGet Configuration file

A YAML or JSON file that defines the desired state of a Windows system using DSC resources.
WinGet Configuration files can be applied using Windows Package Manager or DSC commands.

#### Guidelines

- **First mention:** WinGet Configuration file
- **Subsequent mentions:** configuration file or configuration
- Format file extensions as code: `.winget`, `.dsc.yaml`, `.dsc.config.yaml`

#### Examples

> WinGet Configuration files define system setup using DSC resources.

> Save your configuration with the `.dsc.yaml` extension.

### Configuration version

The format version of a WinGet Configuration file.

- **0.2.0 format**: Original WinGet Configuration format using PowerShell DSC v2
- **Microsoft DSC 3.x format**: Modern format using Microsoft DSC v3 platform

#### Guidelines

- **First mention:** Configuration version or configuration format
- **Subsequent mentions:** version or format
- Always specify the version number when referring to a specific format

#### Examples

> WinGet Studio supports both 0.2.0 and Microsoft DSC 3.x configuration formats.

> Export your configuration in Microsoft DSC 3.x format for new projects.

## Resource terms

### DSC resource

A standardized interface for managing a specific component or setting on a system. Resources
define properties that can be configured and operations (get, set, test) for managing state.

#### Guidelines

- **First mention:** DSC resource or Desired State Configuration resource
- **Subsequent mentions:** resource
- Format resource type names as code

#### Examples

> The `Microsoft.WinGet.DSC/WinGetPackage` DSC resource manages software packages.

> Add resources to your configuration to define desired state.

### Resource type

The fully qualified identifier for a resource, following the format
`<ModuleName>/<ResourceName>`.

#### Guidelines

- **First mention:** resource type or resource type name
- **Subsequent mentions:** type
- Always format as code

#### Examples

> The resource type `PSDesiredStateConfiguration/Environment` manages environment variables.

> Specify the correct type when adding resources to configurations.

### Resource instance

A specific configuration of a resource within a configuration file. Each instance has a unique
name and defines the desired state for one component.

#### Guidelines

- **First mention:** resource instance
- **Subsequent mentions:** instance
- Use "instance" when distinguishing from the resource type

#### Examples

> Each resource instance in the configuration has a unique name.

> Configure the properties for this instance in the property editor.

### Resource properties

The configurable settings for a resource instance. Properties define what should be managed and
how the resource should behave.

#### Guidelines

- **First mention:** resource properties
- **Subsequent mentions:** properties
- Format property names as bold text
- Format property values as code

#### Examples

> Set the **id** property to `Git.Git` for the WinGetPackage resource.

> Resource properties are validated against the resource schema.

### Resource catalog

The searchable database of available DSC resources in WinGet Studio, sourced from PowerShell
Gallery and local DSC v3 installations.

#### Guidelines

- **First mention:** resource catalog
- **Subsequent mentions:** catalog

#### Examples

> Browse the resource catalog to find available DSC resources.

> Refresh the catalog to discover newly published resources.

## Module terms

### PowerShell module

A package containing PowerShell cmdlets, functions, and potentially DSC resources. Modules are
distributed through PowerShell Gallery.

#### Guidelines

- **First mention:** PowerShell module
- **Subsequent mentions:** module
- Format module names as code

#### Examples

> Install the `Microsoft.WinGet.DSC` PowerShell module to use WinGet resources.

> Modules from PowerShell Gallery provide DSC resources.

### Module provider

A component in WinGet Studio that discovers and catalogs resources from a specific source.

- **PowerShell Gallery provider**: Discovers resources from PowerShell Gallery
- **Local DSC v3 provider**: Discovers command-based DSC v3 resources on the system

#### Guidelines

- **First mention:** module provider
- **Subsequent mentions:** provider
- Capitalize specific provider names

#### Examples

> The PowerShell Gallery provider searches for modules tagged with `dscresource`.

> Module providers refresh the resource catalog periodically.

## Dependency terms

### Resource dependency

A relationship between resources where one resource must be successfully processed before another.
Dependencies ensure correct execution order.

#### Guidelines

- **First mention:** resource dependency
- **Subsequent mentions:** dependency
- Use singular "dependency" or plural "dependencies" as appropriate

#### Examples

> Define resource dependencies using the `dependsOn` property.

> Dependencies ensure PowerShell 7 is installed before installing modules.

### DependsOn property

The configuration property that specifies which resources must complete before the current resource
is processed.

#### Guidelines

- **First mention:** `dependsOn` property
- **Subsequent mentions:** `dependsOn` or dependency reference
- Always format as code

#### Examples

> Use the `dependsOn` property to create execution order.

> Reference dependencies with the `resourceId()` function.

## Adapter resource terms

### Adapter resource

A special type of DSC resource that enables using resources from different platforms or versions.
Adapter resources wrap other resources to provide compatibility.

#### Guidelines

- **First mention:** adapter resource
- **Subsequent mentions:** adapter
- Format adapter type names as code

#### Examples

> The `Microsoft.Windows/WindowsPowerShell` adapter resource enables PowerShell DSC v2 resources
> in Microsoft DSC 3.x configurations.

> Use adapter resources to wrap PowerShell-based resources.

### Nested resource

A resource instance defined within an adapter resource's properties. Nested resources are managed
by the adapter.

#### Guidelines

- **First mention:** nested resource or nested resource instance
- **Subsequent mentions:** nested resource
- Use "nested" to distinguish from top-level resources

#### Examples

> Define nested resources in the adapter's `resources` property.

> Each nested resource is processed by the adapter.

## WinGet Studio terms

### Configuration editor

The main interface in WinGet Studio for creating and modifying configuration files.

#### Guidelines

- **First mention:** configuration editor
- **Subsequent mentions:** editor

#### Examples

> Open your configuration in the configuration editor.

> The editor provides visual editing and validation.

### Configuration unit

A term used in WinGet and WinGet Studio to refer to a resource instance within a configuration.

#### Guidelines

- **First mention:** configuration unit or resource instance
- **Subsequent mentions:** unit
- Note that "unit" and "resource instance" are synonymous in WinGet Studio context

#### Examples

> Each configuration unit defines one component to manage.

> Add units to your configuration from the resource catalog.

### Security context

The privilege level required to execute a resource or configuration.

- **Current**: Run with current user privileges
- **Elevated**: Run with administrator privileges

#### Guidelines

- **First mention:** security context
- **Subsequent mentions:** context or privileges
- Capitalize specific context values

#### Examples

> Some resources require Elevated security context.

> Set the security context based on resource requirements.

## Operation terms

### Get operation

Retrieves the current state of a resource instance without making changes.

#### Guidelines

- **First mention:** get operation or **Get** operation
- **Subsequent mentions:** get or **Get**
- Use bold formatting when emphasizing the operation name

#### Examples

> Use the **Get** operation to query current state.

> The get operation returns property values from the system.

### Set operation

Enforces the desired state for a resource instance, making changes if necessary.

#### Guidelines

- **First mention:** set operation or **Set** operation
- **Subsequent mentions:** set or **Set**
- Use bold formatting when emphasizing the operation name

#### Examples

> The **Set** operation modifies the system to match desired state.

> Run a set operation to apply your configuration.

### Test operation

Compares the current state to the desired state without making changes, indicating whether the
resource is in the desired state.

#### Guidelines

- **First mention:** test operation or **Test** operation
- **Subsequent mentions:** test or **Test**
- Use bold formatting when emphasizing the operation name

#### Examples

> The **Test** operation validates state without making changes.

> Use test operations before applying configurations.

### Export operation

Retrieves the current state of all instances of a resource type on the system.

#### Guidelines

- **First mention:** export operation or **Export** operation
- **Subsequent mentions:** export or **Export**
- Use bold formatting when emphasizing the operation name

#### Examples

> The **Export** operation generates configuration from current system state.

> Export resources to create baseline configurations.

## Schema terms

### Resource schema

The JSON schema that describes a resource's properties, including their types, constraints, and
whether they're required or optional.

#### Guidelines

- **First mention:** resource schema
- **Subsequent mentions:** schema
- Specify "resource schema" when context isn't clear

#### Examples

> View the resource schema to understand available properties.

> WinGet Studio validates property values against the resource schema.

### Configuration schema

The JSON schema that defines the structure and validation rules for configuration files.

#### Guidelines

- **First mention:** configuration schema or DSC configuration schema
- **Subsequent mentions:** schema or configuration schema
- Use the full URL when showing example configurations

#### Examples

> Configuration files specify their schema in the `$schema` property.

> WinGet Studio validates configurations against the DSC configuration schema.

## General terms

### Desired State Configuration (DSC)

Microsoft's declarative configuration platform for managing system state. PowerShell DSC v2 is the
classic Windows PowerShell version, while Microsoft DSC v3 is the modern, cross-platform version.

#### Guidelines

- **First mention:** Desired State Configuration (DSC) or DSC platform
- **Subsequent mentions:** DSC
- Specify "PowerShell DSC v2" or "Microsoft DSC v3" when the distinction matters

#### Examples

> Microsoft's Desired State Configuration (DSC) platform provides declarative system management.

> WinGet Studio creates configurations compatible with Microsoft DSC v3.

### PowerShell Gallery

The public repository for PowerShell modules, scripts, and DSC resources.

#### Guidelines

- **First mention:** PowerShell Gallery
- **Subsequent mentions:** PowerShell Gallery or the Gallery
- Always capitalize both words

#### Examples

> Resources are discovered from PowerShell Gallery.

> Search PowerShell Gallery for modules containing DSC resources.

### Windows Package Manager (WinGet)

Microsoft's package manager for Windows, supporting software installation and system configuration.

#### Guidelines

- **First mention:** Windows Package Manager (WinGet)
- **Subsequent mentions:** WinGet or Windows Package Manager
- Use WinGet for brevity, Windows Package Manager for formality

#### Examples

> Windows Package Manager (WinGet) applies configuration files to systems.

> Use WinGet to install software and configure Windows.

## See also

- [WinGet Configuration versions][01]
- [Understanding DSC resources][02]
- [Customize exported configurations][03]
- [Get started with WinGet Studio][04]

<!-- Link reference definitions -->
[01]: ./concepts/configuration-versions.md
[02]: ./concepts/understanding-resources.md
[03]: ./how-to/customize-exported-configuration.md
[04]: ./get-started/index.md
