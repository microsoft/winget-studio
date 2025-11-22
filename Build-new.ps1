#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
using module ./build.helpers.psm1

<#
    .SYNOPSIS
        Build WinGet Studio MSIX packages and bundles.

    .DESCRIPTION
        This script builds WinGet Studio MSIX packages and bundles for the specified platform(s)
        and configuration(s). It can also sign the packages if run with administrator privileges.

    .PARAMETER Platform
        The platform(s) to build.

        Valid values are:

        - x64: Build for 64-bit architecture.
        - x86: Build for 32-bit architecture.
        - arm64: Build for ARM 64-bit architecture.

    .PARAMETER Configuration
        The configuration(s) to build. Valid values are "Debug" and "Release".

    .PARAMETER Version
        The version to set for the MSIX packages.

    .PARAMETER BuildStep
        The build step to execute.

        Valid values are:

        - all: Build both MSIX packages and bundles.
        - msix: Build only MSIX packages.
        - msixbundle: Build only MSIX bundles.

    .PARAMETER IsRelease
        Indicates whether this is a release build. If set, the package will be built as a stable release.

    .PARAMETER OutputDir
        The output directory for the built packages and bundles. If not specified, defaults to the script's directory.
#>
[CmdletBinding()]
param (
    [ValidateSet("x64", "x86", "arm64")]
    [string]$Platform = "x64",
    [ValidateSet("Debug", "Release")]
    [string[]]$Configuration = "Debug",
    [string]$Version,
    [ValidateSet("all", "msix", "msixbundle")]
    [string]$BuildStep = "all",
    [switch]$IsRelease = $false,
    [string]$OutputDir
)

$env:Build_RootDirectory = (Split-Path $MyInvocation.MyCommand.Path)
$env:Build_Platform = $Platform
$env:Build_Configuration = $Configuration
$env:msix_version = New-BuildInfo -Version $Version -IsAzurePipelineBuild ([bool]$env:TF_BUILD)
$msBuildPath = Get-MSBuildPath

if ([string]::IsNullOrEmpty($OutputDir))
{
    $OutputDir = $env:Build_RootDirectory
}

if (($BuildStep -ieq "all") -Or ($BuildStep -ieq "msix"))
{
    $appxManifestPath = (Join-Path $env:Build_RootDirectory 'src' 'WinGetStudio' 'Package.appxmanifest')
    $xmlElements = Get-XmlElement -Path $appxManifestPath

    $appxmanifest = [System.Xml.Linq.XDocument]::Load($appxManifestPath)

    # Cache current (dev) values
    $devVersion = $appxManifest.Root.Element($xmlElements.Identity).Attribute("Version").Value
    $devPackageName = $appxManifest.Root.Element($xmlElements.Identity).Attribute("Name").Value
    $devPackageDisplayName = $appxManifest.Root.Element($xmlElements.Properties).Element($xmlElements.DisplayName).Value
    $devAppDisplayNameResource = $appxManifest.Root.Element($xmlElements.Applications).Element($xmlElements.Application).Element($xmlElements.VisualElements).Attribute("DisplayName").Value

    # For dev build, use the cached values
    $buildRing = "Dev"
    $packageName = $devPackageName
    $packageDisplayName = $devPackageDisplayName
    $appDisplayNameResource = $devAppDisplayNameResource

    # For release build, use the new values
    if ($IsRelease)
    {
        $buildRing = "Stable"
        $packageName = "Microsoft.Windows.WinGetStudio"
        $packageDisplayName = "WinGet Studio (Experimental)"
        $appDisplayNameResource = "ms-resource:AppDisplayNameStable"
    }

    try {
        # Update the appxmanifest
        $appxManifest.Root.Element($xmlElements.Identity).Attribute("Version").Value = $env:msix_version
        $appxManifest.Root.Element($xmlElements.Identity).Attribute("Name").Value = $packageName
        $appxManifest.Root.Element($xmlElements.Properties).Element($xmlElements.DisplayName).Value = $packageDisplayName
        $appxManifest.Root.Element($xmlElements.Applications).Element($xmlElements.Application).Element($xmlElements.VisualElements).Attribute("DisplayName").Value = $appDisplayNameResource
        $appxManifest.Save($appxmanifestPath)

        $solutionPath = Join-Path $env:Build_RootDirectory "src" "WinGetStudio.sln"
        Restore-Nuget -SolutionPath $solutionPath -UseInternal:([bool]$env:TF_BUILD)

        foreach ($platform in $env:Build_Platform)
        {
            foreach ($configuration in $env:Build_Configuration)
            {
                $appxPackageDir = (Join-Path $OutputDir "AppxPackages\$configuration")
                Invoke-MSBuildPackage `
                    -MsBuildPath $msbuildPath `
                    -SolutionPath $solutionPath `
                    -Platform $platform.ToLower() `
                    -Configuration $configuration.ToLower() `
                    -OutputDir $appxPackageDir `
                    -BuildRing $buildRing
            }
        }
    } 

    finally {
        # Revert the appxmanifest to dev values
        $appxManifest.Root.Element($xmlElements.Identity).Attribute("Version").Value = $devVersion
        $appxManifest.Root.Element($xmlElements.Identity).Attribute("Name").Value = $devPackageName
        $appxManifest.Root.Element($xmlElements.Properties).Element($xmlElements.DisplayName).Value = $devPackageDisplayName
        $appxManifest.Root.Element($xmlElements.Applications).Element($xmlElements.Application).Element($xmlElements.VisualElements).Attribute("DisplayName").Value = $devAppDisplayNameResource
        $appxManifest.Save($appxmanifestPath) 
    }
}

if (($BuildStep -ieq "all") -Or ($BuildStep -ieq "msixbundle"))
{
    foreach ($configuration in $env:Build_Configuration)
    {
        $appxPackageDir = (Join-Path $OutputDir "AppxPackages\$configuration")
        $appxBundlePath = (Join-Path $OutputDir ("AppxBundles\$configuration\WinGetStudio_" + $env:msix_version + "_8wekyb3d8bbwe.msixbundle"))

        New-AppxBundle -InputPath $appxPackageDir -ProjectName WinGetStudio -BundleVersion $env:msix_version -OutputPath $appxBundlePath
        if (-not ($env:TF_BUILD) -and (Test-IsAdmin))
        {
            Invoke-SignPackage -AppxBundlePath $appxBundlePath
        }
    }
}