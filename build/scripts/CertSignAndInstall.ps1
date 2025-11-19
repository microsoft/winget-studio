function Invoke-SignPackage([string]$Path) {
    if (-not($Path)) {
        Write-Information 'Path parameter cannot be empty' -InformationAction Continue
        return
    }

    if (-not(Test-Path $Path -PathType Leaf)) {
        Write-Information "$Path is not a valid file" -InformationAction Continue
        return
    }

    $certName = 'Microsoft.WinGetStudio'
    $cert = Get-ChildItem 'Cert:\CurrentUser\My' | Where-Object { $_.FriendlyName -match $certName } | Select-Object -First 1

    if ($cert) {
        $expiration = $cert.NotAfter
        $now = Get-Date
        if ( $expiration -lt $now) {
            Write-Information "Test certificate for $($cert.Thumbprint)...Expired ($expiration). Replacing it with a new one." -InformationAction Continue
            Remove-Item $cert
            $cert = $Null
        }
    }

    if (-not($cert)) {
        Write-Information 'No certificate found. Creating a new certificate for signing.' -InformationAction Continue
        $cert = & New-SelfSignedCertificate -Type Custom -Subject 'CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US' -KeyUsage DigitalSignature -FriendlyName $certName -CertStoreLocation 'Cert:\CurrentUser\My' -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
    }

    SignTool sign /fd SHA256 /sha1 $($cert.Thumbprint) $Path

    if (-not(Test-Path Cert:\LocalMachine\TrustedPeople\$($cert.Thumbprint))) {
        Export-Certificate -Cert $cert -FilePath "$($PSScriptRoot)\Microsoft.WinGetStudio.cer" -Type CERT
        Import-Certificate -FilePath "$($PSScriptRoot)\Microsoft.WinGetStudio.cer" -CertStoreLocation Cert:\LocalMachine\TrustedPeople
        Remove-Item -Path "$($PSScriptRoot)\Microsoft.WinGetStudio.cer"
        (Get-ChildItem Cert:\LocalMachine\TrustedPeople\$($cert.Thumbprint)).FriendlyName = $certName
    }
}

function Remove-WinGetStudioCertificates {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Scope = 'Function',
        Justification = 'This function is a wrapper which removes multiple certificates matching a pattern.')]

    [CmdletBinding(SupportsShouldProcess = $true)]
    param()

    # Remove certificates from CurrentUser\My
    $userCerts = Get-ChildItem 'Cert:\CurrentUser\My' | Where-Object { $_.FriendlyName -match 'Microsoft.WinGetStudio' }
    foreach ($cert in $userCerts) {
        $target = "Cert:\CurrentUser\My\$($cert.Thumbprint)"
        if ($PSCmdlet.ShouldProcess($target, 'Remove certificate')) {
            Remove-Item -Path $cert.PSPath -ErrorAction SilentlyContinue
        }
    }

    # Remove certificates from LocalMachine\TrustedPeople
    $localCerts = Get-ChildItem 'Cert:\LocalMachine\TrustedPeople' | Where-Object { $_.FriendlyName -match 'Microsoft.WinGetStudio' }
    foreach ($cert in $localCerts) {
        $target = "Cert:\LocalMachine\TrustedPeople\$($cert.Thumbprint)"
        if ($PSCmdlet.ShouldProcess($target, 'Remove certificate')) {
            Remove-Item -Path $cert.PSPath -ErrorAction SilentlyContinue
        }
    }
}