# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

function Invoke-SignPackage([string]$Path)
{
    if (-not($Path))
    {
        Write-Information "Path parameter cannot be empty"
        return
    }

    if (-not(Test-Path $Path -PathType Leaf))
    {
        Write-Information "File not found at path: $Path"
        return
    }

    $certName = "Microsoft.WinGetStudio"
    $cert = Get-ChildItem 'Cert:\CurrentUser\My' | Where-Object { $_.FriendlyName -match $certName } | Select-Object -First 1

    if ($cert)
    {
        $expiration = $cert.NotAfter
        $now = Get-Date
        if ( $expiration -lt $now)
        {
            Write-Information "Test certificate for $($cert.Thumbprint)...Expired ($expiration). Replacing it with a new one."
            Remove-Item $cert
            $cert = $Null
        }
    }

    if (-not($cert))
    {
        Write-Information "No certificate found. Creating a new certificate for signing."
        $cert = & New-SelfSignedCertificate -Type Custom -Subject "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US" -KeyUsage DigitalSignature -FriendlyName $certName -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    }

    $signToolPath = Get-SignToolPath

    & $signToolPath sign /fd SHA256 /sha1 $($cert.Thumbprint) $Path

    if (-not(Test-Path Cert:\LocalMachine\TrustedPeople\$($cert.Thumbprint)))
    {
        Export-Certificate -Cert $cert -FilePath "$($PSScriptRoot)\Microsoft.WinGetStudio.cer" -Type CERT
        Import-Certificate -FilePath "$($PSScriptRoot)\Microsoft.WinGetStudio.cer" -CertStoreLocation Cert:\LocalMachine\TrustedPeople    
        Remove-Item -Path "$($PSScriptRoot)\Microsoft.WinGetStudio.cer"
        (Get-ChildItem Cert:\LocalMachine\TrustedPeople\$($cert.Thumbprint)).FriendlyName = $certName
    }
}

function Get-SignToolPath
{
    <#
    .SYNOPSIS
        Locates SignTool.exe in the Windows SDK installation.

    .DESCRIPTION
        The Get-SignToolPath function searches for SignTool.exe in the Windows SDK
        installation directory. It automatically detects the latest SDK version and
        returns the path to SignTool.exe.

    .EXAMPLE
        $signToolPath = Get-SignToolPath
        & $signToolPath sign /fd SHA256 /sha1 $thumbprint $package

    .OUTPUTS
        System.String
        Returns the full path to SignTool.exe

    .NOTES
        Requires Windows SDK to be installed.
        Searches x64 and x86 directories for SignTool.exe.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    process
    {
        $sdkBasePath = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"

        if (-not (Test-Path $sdkBasePath))
        {
            throw "Windows SDK not found at '$sdkBasePath'. Please install the Windows SDK."
        }

        Write-Verbose "Searching for SignTool.exe in Windows SDK..."

        # Find all SDK versions and sort to get the latest
        $sdkVersions = Get-ChildItem -Path $sdkBasePath -Directory | 
            Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
            Sort-Object { [version]$_.Name } -Descending

        foreach ($sdkVersion in $sdkVersions)
        {
            # Check x64 first, then x86
            $paths = @(
                (Join-Path $sdkVersion.FullName "x64\SignTool.exe"),
                (Join-Path $sdkVersion.FullName "x86\SignTool.exe")
            )

            foreach ($path in $paths)
            {
                if (Test-Path $path -PathType Leaf)
                {
                    Write-Verbose "Found SignTool.exe at: $path"
                    return $path
                }
            }
        }

        throw "Could not find SignTool.exe in any Windows SDK version. Please install the Windows SDK."
    }
}

function Remove-WinGetStudioCertificates()
{
    Get-ChildItem 'Cert:\CurrentUser\My' | Where-Object { $_.FriendlyName -match 'Microsoft.WinGetStudio' } | Remove-Item
    Get-ChildItem 'Cert:\LocalMachine\TrustedPeople' | Where-Object { $_.FriendlyName -match 'Microsoft.WinGetStudio' } | Remove-Item
}

function New-BuildInfo
{
    <#
    .SYNOPSIS
        Creates a build version string in MSIX-compatible format.

    .DESCRIPTION
        The New-BuildInfo function generates a version string formatted for MSIX packages.
        The version format is M.NPP.E.B where:
        - M = Major version (max <= 65535)
        - N = Minor version (max <= 654)
        - P = Patch version (max <= 99)
        - E = Elapsed days since epoch (Jan 1, 2025) (max <= 65535)
        - B = Build number (max <= 65535)

        For local builds, the Build number is computed as HHMM (hour and minute in UTC).
        For Azure Pipeline builds, the Build number is set to 0.

    .PARAMETER Version
        The base version string to parse. Can be in various formats:
        - M
        - M.N
        - M.N.E
        - M.N.E.B
        - M.NPP (where NPP is concatenated Minor and Patch)

    .PARAMETER IsAzurePipelineBuild
        Indicates whether this is an Azure Pipeline build. When true, sets Build number to 0.
        When false, Build number is computed from current UTC time as HHMM.

    .EXAMPLE
        New-BuildInfo -Version "1.0"
        Creates a build version for version 1.0 with computed elapsed days and build time.

    .EXAMPLE
        New-BuildInfo -Version "1.299.365.1234" -IsAzurePipelineBuild $true
        Creates a build version 1.299.365.0 for an Azure Pipeline build.

    .OUTPUTS
        System.String
        Returns a version string in the format M.NPP.E.B

    .NOTES
        The epoch date is January 1, 2025.
        All date calculations use Universal Time (UTC).
        Default values: Major=0, Minor=1, Patch=99 (for local builds)
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(
            Position = 0,
            ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "Base version string to parse (e.g., '1.0', '1.299.365')"
        )]
        [AllowEmptyString()]
        [string]$Version = '',

        [Parameter(
            Position = 1,
            HelpMessage = "Indicates if this is an Azure Pipeline build"
        )]
        [bool]$IsAzurePipelineBuild = $false
    )

    begin
    {
        Write-Verbose "Starting build info creation"
        
        # Define epoch date
        $epoch = (Get-Date -Year 2025 -Month 1 -Day 1).ToUniversalTime()
        Write-Verbose "Epoch date: $epoch"
    }

    process
    {
        # Initialize default version components
        $Major = "0"
        $Minor = "1"
        $Patch = "99"  # default to 99 for local builds
        $Elapsed = $null
        $Build = $null

        if (-not [string]::IsNullOrWhiteSpace($Version))
        {
            $versionSplit = $Version.Split(".")
            Write-Verbose "Parsing version string: $Version (split into $($versionSplit.Length) parts)"

            if ($versionSplit.Length -gt 3)
            {
                $Build = $versionSplit[3]
                Write-Verbose "Build from version string: $Build"
            }

            if ($versionSplit.Length -gt 2)
            {
                $Elapsed = $versionSplit[2]
                Write-Verbose "Elapsed from version string: $Elapsed"
            }

            if ($versionSplit.Length -gt 1)
            {
                if ($versionSplit[1].Length -gt 2)
                {
                    $Minor = $versionSplit[1].Substring(0, $versionSplit[1].Length - 2)
                    $Patch = $versionSplit[1].Substring($versionSplit[1].Length - 2, 2)
                    Write-Verbose "Minor and Patch from concatenated string: Minor=$Minor, Patch=$Patch"
                }
                else
                {
                    $Minor = $versionSplit[1]
                    Write-Verbose "Minor from version string: $Minor"
                }
            }

            if ($versionSplit.Length -gt 0)
            {
                $Major = $versionSplit[0]
                Write-Verbose "Major from version string: $Major"
            }
        }

        $now = (Get-Date).ToUniversalTime()
        if ([string]::IsNullOrWhiteSpace($Elapsed))
        {
            $Elapsed = (New-TimeSpan -Start $epoch -End $now).Days
            Write-Verbose "Computed Elapsed days since epoch: $Elapsed"
        }

        if ([string]::IsNullOrWhiteSpace($Build))
        {
            if ($IsAzurePipelineBuild)
            {
                $Build = "0"
                Write-Verbose "Azure Pipeline build - Build number set to: $Build"
            }
            else
            {
                $version_h = $now.Hour
                $version_m = $now.Minute
                $Build = ($version_h * 100 + $version_m).ToString()
                Write-Verbose "Local build - Build number computed from time (${version_h}${version_m}): $Build"
            }
        }

        $version_dotquad = [int[]]($Major, ($Minor + $Patch), $Elapsed, $Build)
        $resultVersion = $version_dotquad -join "."

        Write-Verbose "Final version components: Major=$Major, Minor+Patch=$($Minor + $Patch), Elapsed=$Elapsed, Build=$Build"
        Write-Verbose "Generated version string: $resultVersion"

        return $resultVersion
    }

    end
    {
        Write-Verbose "Build info creation completed"
    }
}

function Test-IsAdmin
{
    return ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')
}

function Get-MSBuildPath
{
    $msbuildPath = &"${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
    if (-not($msbuildPath))
    {
        throw "MSBuild.exe not found. Please ensure that Visual Studio with MSBuild component is installed."
    }
    return $msbuildPath
}

function Get-XmlElement
{
    <#
    .SYNOPSIS
        Loads an APPX manifest XML file and returns parsed elements.

    .DESCRIPTION
        The Get-XmlElement function loads an APPX manifest file using System.Xml.Linq
        and returns a PSCustomObject containing the XDocument and all relevant XName
        objects for common APPX manifest elements.

    .PARAMETER Path
        The full path to the APPX manifest file to load.

    .EXAMPLE
        $manifest = Get-XmlElement -Path "C:\src\Package.appxmanifest"
        $identity = $manifest.Document.Root.Element($manifest.Identity)

    .OUTPUTS
        PSCustomObject
        Returns an object with the following properties:
        - Document: The loaded XDocument
        - Identity: XName for Identity element
        - Properties: XName for Properties element
        - DisplayName: XName for DisplayName element
        - Applications: XName for Applications element
        - Application: XName for Application element
        - VisualElements: XName for VisualElements element
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param (
        [Parameter(
            Mandatory = $true,
            Position = 0,
            ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "Path to the APPX manifest file"
        )]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({
                if (Test-Path $_ -PathType Leaf)
                {
                    $true
                }
                else
                {
                    throw "The specified path '$_' does not exist or is not a file."
                }
            })]
        [string]$Path
    )

    process
    {
        Write-Verbose "Loading XML from path: $Path"

        [void][Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq")

        try
        {
            $appxmanifest = [System.Xml.Linq.XDocument]::Load($Path)
            Write-Verbose "Successfully loaded XML document"
        }
        catch
        {
            throw "Failed to load XML document from '$Path': $_"
        }

        $namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10"
        $uapNamespace = "http://schemas.microsoft.com/appx/manifest/uap/windows10"
        $result = [PSCustomObject]@{
            Document       = $appxmanifest
            Identity       = [System.Xml.Linq.XName]::Get("{$namespace}Identity")
            Properties     = [System.Xml.Linq.XName]::Get("{$namespace}Properties")
            DisplayName    = [System.Xml.Linq.XName]::Get("{$namespace}DisplayName")
            Applications   = [System.Xml.Linq.XName]::Get("{$namespace}Applications")
            Application    = [System.Xml.Linq.XName]::Get("{$namespace}Application")
            VisualElements = [System.Xml.Linq.XName]::Get("{$uapNamespace}VisualElements")
        }

        return $result
    }
}

function Restore-NuGet
{
    [CmdletBinding()]
    param (
        [string]$SolutionPath,
        [switch]$UseInternal
    )

    $nugetConfig = Join-Path (Split-Path $SolutionPath -Parent) "nuget.public.config"

    if ($UseInternal)
    {
        Write-Verbose "Using internal NuGet configuration"
        $nugetConfig = Join-Path (Split-Path $SolutionPath -Parent) "nuget.config"
    }

    $dotNetPath = (Get-Command "dotnet" -CommandType Application -ErrorAction SilentlyContinue).Source

    if (!$dotNetPath)
    {
        throw "dotnet CLI not found. Please ensure that .NET SDK is installed and 'dotnet' is in the system PATH."
    }

    & dotnet restore $SolutionPath --configfile $nugetConfig
}

function Invoke-MSBuildPackage
{
    <#
    .SYNOPSIS
        Builds an MSIX package using MSBuild for a specific platform and configuration.

    .DESCRIPTION
        The Invoke-MSBuildPackage function invokes MSBuild to build a WinGet Studio MSIX package
        for the specified platform and configuration. It handles MSBuild arguments, output paths,
        and optional package signing for local builds.

    .PARAMETER SolutionPath
        Path to the Visual Studio solution file to build.

    .PARAMETER Platform
        The platform architecture to build (e.g., x64, x86, arm64).

    .PARAMETER Configuration
        The build configuration (e.g., Debug, Release).

    .PARAMETER OutputDirectory
        The output directory for the built MSIX package.

    .PARAMETER BuildRing
        The build ring identifier (e.g., Dev, Stable).

    .PARAMETER MSBuildPath
        Path to the MSBuild.exe executable.

    .EXAMPLE
        Invoke-MSBuildPackage -SolutionPath "src/WinGetStudio.sln" -Platform "x64" -Configuration "Debug" -OutputDirectory "C:\output\AppxPackages\Debug" -BuildRing "Dev" -MSBuildPath $msbuildPath

    .EXAMPLE
        Invoke-MSBuildPackage -SolutionPath "src/WinGetStudio.sln" -Platform "arm64" -Configuration "Release" -OutputDirectory "C:\output\AppxPackages\Release" -BuildRing "Stable" -MSBuildPath $msbuildPath

    .OUTPUTS
        None. Invokes MSBuild and optionally signs the package.
    #>
    [CmdletBinding()]
    param(
        [Parameter(
            Mandatory = $true,
            Position = 0,
            HelpMessage = "Path to the Visual Studio solution file"
        )]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({
                if (Test-Path $_ -PathType Leaf) { $true }
                else { throw "Solution file '$_' does not exist." }
            })]
        [string]$SolutionPath,

        [Parameter(
            Mandatory = $true,
            Position = 1,
            HelpMessage = "Platform architecture (x64, x86, arm64)"
        )]
        [ValidateSet("x64", "x86", "arm64")]
        [string]$Platform,

        [Parameter(
            Mandatory = $true,
            Position = 2,
            HelpMessage = "Build configuration (Debug, Release)"
        )]
        [ValidateSet("Debug", "Release")]
        [string]$Configuration,

        [Parameter(
            Mandatory = $true,
            HelpMessage = "Output directory for the MSIX package"
        )]
        [ValidateNotNullOrEmpty()]
        [string]$OutputDirectory,

        [Parameter(
            Mandatory = $true,
            HelpMessage = "Build ring identifier (Dev, Stable, etc.)"
        )]
        [ValidateNotNullOrEmpty()]
        [string]$BuildRing,

        [Parameter(
            Mandatory = $true,
            HelpMessage = "Path to MSBuild.exe"
        )]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({
                if (Test-Path $_ -PathType Leaf) { $true }
                else { throw "MSBuild.exe not found at '$_'." }
            })]
        [string]$MSBuildPath
    )

    begin
    {
        Write-Verbose "Starting MSBuild package build"
        Write-Verbose "Platform: $Platform, Configuration: $Configuration"
        Write-Verbose "Output Directory: $OutputDirectory"
        Write-Verbose "Build Ring: $BuildRing"
    }

    process
    {
        if (-not (Test-Path $OutputDirectory))
        {
            Write-Verbose "Creating output directory: $OutputDirectory"
            New-Item -Path $OutputDirectory -ItemType Directory -Force | Out-Null
        }

        $packagePath = Join-Path $OutputDirectory "WinGetStudio-$Platform.msix"
        Write-Verbose "Package output path: $packagePath"

        $msbuildArgs = @(
            $SolutionPath,
            "/p:Platform=$Platform",
            "/p:Configuration=$Configuration",
            "/p:RestorePackages=false"
            "/binaryLogger:WinGetStudio.$Platform.$Configuration.binlog",
            "/p:AppxPackageOutput=$packagePath",
            "/p:AppxPackageSigningEnabled=false",
            # "/p:GenerateAppxPackageOnBuild=true",
            "/p:BuildRing=$BuildRing"
        )

        Write-Verbose "MSBuild arguments: $($msbuildArgs -join ' ')"

        try
        {
            Write-Verbose "Building $Platform-$Configuration package..."
            & dotnet build $msbuildArgs

            if ($LASTEXITCODE -ne 0)
            {
                throw "MSBuild failed with exit code $LASTEXITCODE"
            }

            Write-Verbose "MSBuild completed successfully"
        }
        catch
        {
            Write-Error "Failed to build package: $_"
            throw
        }

        if (-not $env:TF_BUILD -and (Test-IsAdmin))
        {
            Write-Verbose "Signing package (local admin build)"
            try
            {
                Invoke-SignPackage -Path $packagePath
                Write-Verbose "Successfully signed: $packagePath"
            }
            catch
            {
                Write-Warning "Failed to sign package: $_"
            }
        }
        elseif (-not $env:TF_BUILD -and -not (Test-IsAdmin))
        {
            Write-Warning "Package signing skipped (administrator privileges required)"
        }
        elseif ($env:TF_BUILD)
        {
            Write-Verbose "Package signing skipped (Azure Pipeline build)"
        }
    }

    end
    {
        Write-Verbose "MSBuild package build completed"
    }
}

function New-AppxBundle
{
    <#
  .SYNOPSIS
    Creates an MSIX/APPX bundle from individual platform packages.

  .DESCRIPTION
    The New-AppxBundle function creates an MSIX or APPX bundle by combining multiple
    platform-specific packages (x64, x86, arm64) into a single bundle file. It automatically
    locates MakeAppx.exe from the Windows SDK and generates the required bundle mapping file.

  .PARAMETER ProjectName
    Base name for input .appx/.msix files. Used to filter and identify package files.

  .PARAMETER BundleVersion
    The version number for the bundle in System.Version format (e.g., 1.0.0.0).

  .PARAMETER InputPath
    Path to the directory containing the .appx/.msix files to bundle.

  .PARAMETER OutputPath
    Full path where the bundle file will be created (including filename and extension).

  .PARAMETER MakeAppxPath
    Optional path to MakeAppx.exe. If not specified, the function will automatically
    search for the latest version in the Windows SDK installation.

  .EXAMPLE
    New-AppxBundle -ProjectName "WinGetStudio" -BundleVersion "1.0.0.0" -InputPath "C:\build\packages" -OutputPath "C:\build\WinGetStudio_1.0.0.0.msixbundle"

  .OUTPUTS
    None. Creates an MSIX/APPX bundle file at the specified output path.

  .NOTES
    Requires Windows SDK to be installed for MakeAppx.exe.
    The function searches for the latest SDK version automatically if MakeAppxPath is not specified.
  #>
    [CmdletBinding()]
    param(
        [Parameter(
            Mandatory = $true,
            Position = 0,
            HelpMessage = "Base name for input .appx/.msix files"
        )]
        [ValidateNotNullOrEmpty()]
        [string]$ProjectName,

        [Parameter(
            Mandatory = $true,
            Position = 1,
            HelpMessage = "Appx Bundle Version"
        )]
        [ValidateNotNull()]
        [version]$BundleVersion,

        [Parameter(
            Mandatory = $true,
            Position = 2,
            HelpMessage = "Path under which to locate appx/msix files"
        )]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({
                if (Test-Path $_ -PathType Container) { $true }
                else { throw "Input path '$_' does not exist or is not a directory." }
            })]
        [string]$InputPath,

        [Parameter(
            Mandatory = $true,
            Position = 3,
            HelpMessage = "Output path for the bundle file"
        )]
        [ValidateNotNullOrEmpty()]
        [string]$OutputPath,

        [Parameter(
            HelpMessage = "Path to makeappx.exe (auto-detected if not specified)"
        )]
        [ValidateScript({
                if ([string]::IsNullOrWhiteSpace($_) -or (Test-Path $_ -PathType Leaf)) { $true }
                else { throw "MakeAppx.exe not found at '$_'." }
            })]
        [string]$MakeAppxPath
    )

    begin
    {
        Write-Verbose "Starting APPX bundle creation"
        Write-Verbose "Project Name: $ProjectName"
        Write-Verbose "Bundle Version: $BundleVersion"
        Write-Verbose "Input Path: $InputPath"
        Write-Verbose "Output Path: $OutputPath"

        # Function to find MakeAppx.exe in Windows SDK
        function Find-MakeAppxPath
        {
            [CmdletBinding()]
            [OutputType([string])]
            param()

            $sdkBasePath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
      
            if (-not (Test-Path $sdkBasePath))
            {
                throw "Windows SDK not found at '$sdkBasePath'. Please install the Windows SDK or specify -MakeAppxPath."
            }

            Write-Verbose "Searching for MakeAppx.exe in Windows SDK..."

            # Find all SDK versions and sort to get the latest
            $sdkVersions = Get-ChildItem -Path $sdkBasePath -Directory | 
            Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
            Sort-Object { [version]$_.Name } -Descending

            foreach ($sdkVersion in $sdkVersions)
            {
                # Check x64 first, then x86
                $paths = @(
                    (Join-Path $sdkVersion.FullName "x64\MakeAppx.exe"),
                    (Join-Path $sdkVersion.FullName "x86\MakeAppx.exe")
                )

                foreach ($path in $paths)
                {
                    if (Test-Path $path -PathType Leaf)
                    {
                        Write-Verbose "Found MakeAppx.exe at: $path"
                        return $path
                    }
                }
            }

            throw "Could not find MakeAppx.exe in any Windows SDK version. Please install the Windows SDK or specify -MakeAppxPath."
        }

        # Function to create bundle mapping file
        function New-AppxBundleMapping
        {
            [CmdletBinding()]
            [OutputType([System.IO.FileInfo])]
            param(
                [Parameter(Mandatory = $true)]
                [string]$InputPath,

                [Parameter(Mandatory = $true)]
                [string]$ProjectName
            )

            Write-Verbose "Creating bundle mapping file"
            Write-Verbose "Searching for packages matching '*$ProjectName*' in: $InputPath"

            $lines = @("[Files]")
            $packages = Get-ChildItem -Path $InputPath -Recurse -Filter "*$ProjectName*" -Include *.appx, *.msix

            if ($packages.Count -eq 0)
            {
                throw "No .appx or .msix files found matching '*$ProjectName*' in '$InputPath'"
            }

            Write-Verbose "Found $($packages.Count) package(s) to bundle:"
            foreach ($package in $packages)
            {
                $lines += ("`"{0}`" `"{1}`"" -f ($package.FullName, $package.Name))
                Write-Verbose "  - $($package.Name)"
            }

            $mappingFile = New-TemporaryFile
            $lines | Out-File -Encoding ASCII $mappingFile
            Write-Verbose "Bundle mapping file created at: $($mappingFile.FullName)"

            return $mappingFile
        }
    }

    process
    {
        try
        {
            if ([string]::IsNullOrWhiteSpace($MakeAppxPath))
            {
                $MakeAppxPath = Find-MakeAppxPath
            }
            else
            {
                Write-Verbose "Using specified MakeAppx.exe path: $MakeAppxPath"
            }

            if (-not (Test-Path $MakeAppxPath -PathType Leaf))
            {
                throw "MakeAppx.exe not found at '$MakeAppxPath'"
            }

            $mappingFile = New-AppxBundleMapping -InputPath $InputPath -ProjectName $ProjectName

            $outputDir = Split-Path $OutputPath -Parent
            if (-not [string]::IsNullOrWhiteSpace($outputDir) -and -not (Test-Path $outputDir))
            {
                Write-Verbose "Creating output directory: $outputDir"
                New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
            }

            Write-Information "Creating bundle: $OutputPath" -InformationAction Continue
            Write-Verbose "Executing: $MakeAppxPath bundle /v /bv $($BundleVersion.ToString()) /f $($mappingFile.FullName) /p $OutputPath"

            & $MakeAppxPath bundle /v /bv $BundleVersion.ToString() /f $mappingFile.FullName /p $OutputPath

            if ($LASTEXITCODE -ne 0)
            {
                throw "MakeAppx.exe failed with exit code $LASTEXITCODE"
            }

            Write-Information "Successfully created bundle: $OutputPath" -InformationAction Continue

            if (Test-Path $mappingFile.FullName)
            {
                Remove-Item $mappingFile.FullName -Force -ErrorAction SilentlyContinue
                Write-Verbose "Cleaned up temporary mapping file"
            }
        }
        catch
        {
            Write-Error "Failed to create bundle: $_"
            throw
        }
    }

    end
    {
        Write-Verbose "Bundle creation completed"
    }
}