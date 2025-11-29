param(
    [string]$Platform = 'x64',
    [string]$Configuration = 'Debug',
    [string]$Version,
    [string]$BuildStep = 'all',
    [bool]$IsRelease = $false,
    [string]$OutputDir,
    [switch]$IsAzurePipelineBuild = $false,
    [switch]$Help = $false
)

$StartTime = Get-Date

if ($Help) {
    Write-Information @'
Copyright (c) Microsoft Corporation and Contributors.
Licensed under the MIT License.

Syntax:
      Build.cmd [options]

Description:
      Builds WinGet Studio.

Options:

  -Platform <platform>
      Only build the selected platform(s)
      Example: -Platform x64
      Example: -Platform "x86,x64,arm64"

  -Configuration <configuration>
      Only build the selected configuration(s)
      Example: -Configuration Release
      Example: -Configuration "Debug,Release"

  -Help
      Display this usage message.
'@ -InformationAction Continue
    exit
}

function Write-InformationColored {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Message,

        [Parameter(Mandatory = $false)]
        [ConsoleColor]$ForegroundColor = 'White' # Default color
    )

    $originalForegroundColor = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    Write-Information $Message -InformationAction Continue
    $host.UI.RawUI.ForegroundColor = $originalForegroundColor
}

$env:Build_RootDirectory = (Split-Path $MyInvocation.MyCommand.Path)
$env:Build_Platform = $Platform.ToLower()
$env:Build_Configuration = $Configuration.ToLower()
$env:msix_version = build\Scripts\CreateBuildInfo.ps1 -Version $Version -IsAzurePipelineBuild $IsAzurePipelineBuild

$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')

$msbuildPath = &"${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe

if ([string]::IsNullOrEmpty($OutputDir)) {
    $OutputDir = $env:Build_RootDirectory
}

$ErrorActionPreference = 'Stop'

# Install NuGet Cred Provider
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
# Download the cred provider script to a temp file and execute it directly to avoid Invoke-Expression
$credProviderUrl = 'https://aka.ms/install-artifacts-credprovider.ps1'
$tempScript = Join-Path $env:TEMP ([System.Guid]::NewGuid().ToString() + '.ps1')
Invoke-WebRequest -Uri $credProviderUrl -UseBasicParsing -OutFile $tempScript
try {
    & $tempScript -AddNetfx
} finally {
    Remove-Item $tempScript -ErrorAction SilentlyContinue -Force
}

. build\Scripts\CertSignAndInstall.ps1

try {
    if (($BuildStep -ieq 'all') -or ($BuildStep -ieq 'msix')) {
        # Load current (dev) appxmanifest
        [Reflection.Assembly]::LoadWithPartialName('System.Xml.Linq')
        $appxmanifestPath = (Join-Path $env:Build_RootDirectory 'src\WinGetStudio\Package.appxmanifest')
        $appxmanifest = [System.Xml.Linq.XDocument]::Load($appxmanifestPath)

        # Define the xml namespaces
        $xIdentity = [System.Xml.Linq.XName]::Get('{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Identity');
        $xProperties = [System.Xml.Linq.XName]::Get('{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Properties');
        $xDisplayName = [System.Xml.Linq.XName]::Get('{http://schemas.microsoft.com/appx/manifest/foundation/windows10}DisplayName');
        $xApplications = [System.Xml.Linq.XName]::Get('{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Applications');
        $xApplication = [System.Xml.Linq.XName]::Get('{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Application');
        $uapVisualElements = [System.Xml.Linq.XName]::Get('{http://schemas.microsoft.com/appx/manifest/uap/windows10}VisualElements');

        # Cache current (dev) values
        $devVersion = $appxmanifest.Root.Element($xIdentity).Attribute('Version').Value
        $devPackageName = $appxmanifest.Root.Element($xIdentity).Attribute('Name').Value
        $devPackageDisplayName = $appxmanifest.Root.Element($xProperties).Element($xDisplayName).Value
        $devAppDisplayNameResource = $appxmanifest.Root.Element($xApplications).Element($xApplication).Element($uapVisualElements).Attribute('DisplayName').Value

        # For dev build, use the cached values
        $buildRing = 'Dev'
        $packageName = $devPackageName
        $packageDisplayName = $devPackageDisplayName
        $appDisplayNameResource = $devAppDisplayNameResource

        # For release build, use the new values
        if ($IsRelease) {
            $buildRing = 'Stable'
            $packageName = 'Microsoft.Windows.WinGetStudio'
            $packageDisplayName = 'WinGet Studio (Experimental)'
            $appDisplayNameResource = 'ms-resource:AppDisplayNameStable'
        }

        try {
            # Update the appxmanifest
            $appxmanifest.Root.Element($xIdentity).Attribute('Version').Value = $env:msix_version
            $appxmanifest.Root.Element($xIdentity).Attribute('Name').Value = $packageName
            $appxmanifest.Root.Element($xProperties).Element($xDisplayName).Value = $packageDisplayName
            $appxmanifest.Root.Element($xApplications).Element($xApplication).Element($uapVisualElements).Attribute('DisplayName').Value = $appDisplayNameResource
            $appxmanifest.Save($appxmanifestPath)

            foreach ($platform in $env:Build_Platform.Split(',')) {
                foreach ($configuration in $env:Build_Configuration.Split(',')) {
                    $appxPackageDir = (Join-Path $OutputDir "AppxPackages\$configuration")
                    $msbuildArgs = @(
                        ('src/WinGetStudio.sln'),
                        ('/p:Platform=' + $platform),
                        ('/p:Configuration=' + $configuration),
                        ('/restore'),
                        ("/binaryLogger:WinGetStudio.$platform.$configuration.binlog"),
                        ("/p:AppxPackageOutput=$appxPackageDir\WinGetStudio-$platform.msix"),
                        ('/p:AppxPackageSigningEnabled=false'),
                        ('/p:GenerateAppxPackageOnBuild=true'),
                        ("/p:BuildRing=$buildRing")
                    )

                    & $msbuildPath $msbuildArgs
                    if (-not($IsAzurePipelineBuild) -and $isAdmin) {
                        Invoke-SignPackage "$appxPackageDir\WinGetStudio-$platform.msix"
                    }
                }
            }
        } finally {
            # Revert the appxmanifest to dev values
            $appxmanifest.Root.Element($xIdentity).Attribute('Version').Value = $devVersion
            $appxmanifest.Root.Element($xIdentity).Attribute('Name').Value = $devPackageName
            $appxmanifest.Root.Element($xProperties).Element($xDisplayName).Value = $devPackageDisplayName
            $appxmanifest.Root.Element($xApplications).Element($xApplication).Element($uapVisualElements).Attribute('DisplayName').Value = $devAppDisplayNameResource
            $appxmanifest.Save($appxmanifestPath)
        }
    }

    if (($BuildStep -ieq 'all') -or ($BuildStep -ieq 'msixbundle')) {
        foreach ($configuration in $env:Build_Configuration.Split(',')) {
            $appxPackageDir = (Join-Path $OutputDir "AppxPackages\$configuration")
            $appxBundlePath = (Join-Path $OutputDir ("AppxBundles\$configuration\WinGetStudio_" + $env:msix_version + '_8wekyb3d8bbwe.msixbundle'))
            .\build\scripts\Create-AppxBundle.ps1 -InputPath $appxPackageDir -ProjectName WinGetStudio -BundleVersion ([version]$env:msix_version) -OutputPath $appxBundlePath
            if (-not($IsAzurePipelineBuild) -and $isAdmin) {
                Invoke-SignPackage $appxBundlePath
            }
        }
    }
} catch {
    $formatString = "`n{0}`n`n{1}`n`n"
    $fields = $_, $_.ScriptStackTrace
    Write-InformationColored -Message ($formatString -f $fields) -ForegroundColor RED
    exit 1
}

$TotalTime = (Get-Date) - $StartTime
$TotalMinutes = [math]::Floor($TotalTime.TotalMinutes)
$TotalSeconds = [math]::Ceiling($TotalTime.TotalSeconds) - ($totalMinutes * 60)

if (-not($isAdmin)) {
    Write-InformationColored -Message @'

WARNING: Cert signing requires admin privileges.  To sign, run the following in an elevated Developer Command Prompt.
'@ -ForegroundColor GREEN
    foreach ($platform in $env:Build_Platform.Split(',')) {
        foreach ($configuration in $env:Build_Configuration.Split(',')) {
            $appxPackageFile = (Join-Path $OutputDir "AppxPackages\$configuration\WinGetStudio-$platform.msix")
            Write-InformationColored -Message @"
powershell -command "& { . build\scripts\CertSignAndInstall.ps1; Invoke-SignPackage $appxPackageFile }"
"@ -ForegroundColor GREEN
        }
    }
}

Write-InformationColored -Message @"

Total Running Time:
$TotalMinutes minutes and $TotalSeconds seconds
"@ -ForegroundColor CYAN