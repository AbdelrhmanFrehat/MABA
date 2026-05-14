param(
    [Parameter(Mandatory = $true)]
    [string[]]$Files,
    [string]$TimestampUrl = "http://timestamp.digicert.com",
    [string]$CertPath = $env:MABA_CODESIGN_CERT_PATH,
    [string]$CertPassword = $env:MABA_CODESIGN_CERT_PASSWORD,
    [string]$CertThumbprint = $env:MABA_CODESIGN_CERT_THUMBPRINT
)

$ErrorActionPreference = "Stop"

function Find-SignTool {
    $candidates = @(
        "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe",
        "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

$signTool = Find-SignTool
if (-not $signTool) {
    throw "signtool.exe was not found. Install the Windows SDK signing tools first."
}

if ($CertPath -and (Test-Path $CertPath)) {
    foreach ($file in $Files) {
        & $signTool sign /fd SHA256 /tr $TimestampUrl /td SHA256 /f $CertPath /p $CertPassword $file
        if ($LASTEXITCODE -ne 0) {
            throw "Signing failed for $file"
        }
    }
    return
}

if ($CertThumbprint) {
    foreach ($file in $Files) {
        & $signTool sign /fd SHA256 /tr $TimestampUrl /td SHA256 /sha1 $CertThumbprint $file
        if ($LASTEXITCODE -ne 0) {
            throw "Signing failed for $file"
        }
    }
    return
}

throw "No signing certificate was configured. Set MABA_CODESIGN_CERT_PATH (+ password) or MABA_CODESIGN_CERT_THUMBPRINT."
