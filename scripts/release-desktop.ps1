param(
    [string]$ProjectPath = "C:\Users\PC\Desktop\maba\MabaControlCenter\MabaControlCenter.csproj",
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$Version = "",
    [string]$PublishRoot = "C:\Users\PC\Desktop\maba\MabaControlCenter\publish",
    [string]$UpdateArtifactsDirectory = "C:\Users\PC\Desktop\maba\artifacts\desktop-updates\stable",
    [string]$InstallerArtifactsDirectory = "C:\Users\PC\Desktop\maba\artifacts\desktop-installer\stable",
    [string]$InstallerScriptPath = "C:\Users\PC\Desktop\maba\installer\MabaControlCenter.iss",
    [string]$SignScriptPath = "C:\Users\PC\Desktop\maba\scripts\sign-maba-release.ps1"
)

$ErrorActionPreference = "Stop"

function Get-ProjectVersion {
    param([string]$CsprojPath)

    [xml]$xml = Get-Content $CsprojPath
    $versionNode = $xml.Project.PropertyGroup.Version | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($versionNode)) {
        throw "No <Version> found in $CsprojPath"
    }

    return $versionNode
}

function Find-InnoCompiler {
    $candidates = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

function Test-SigningConfigured {
    $hasCertPath = -not [string]::IsNullOrWhiteSpace($env:MABA_CODESIGN_CERT_PATH) -and (Test-Path $env:MABA_CODESIGN_CERT_PATH)
    $hasThumbprint = -not [string]::IsNullOrWhiteSpace($env:MABA_CODESIGN_CERT_THUMBPRINT)
    return ($hasCertPath -or $hasThumbprint)
}

function Write-Manifest {
    param(
        [string]$ManifestPath,
        [string]$VersionValue,
        [string]$PackageName,
        [string]$Notes
    )

    $manifest = [ordered]@{
        version = $VersionValue
        packageUri = $PackageName
        notes = $Notes
        publishedAt = (Get-Date).ToUniversalTime().ToString("o")
    }

    $manifest | ConvertTo-Json | Set-Content -Path $ManifestPath -Encoding UTF8
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Get-ProjectVersion -CsprojPath $ProjectPath
}

$publishDirectory = Join-Path $PublishRoot "$Runtime-$Version"
$updatePackageName = "MabaControlCenter-$Version-$Runtime.zip"
$updatePackagePath = Join-Path $UpdateArtifactsDirectory $updatePackageName
$updateManifestPath = Join-Path $UpdateArtifactsDirectory "manifest.json"
$installerName = "MabaControlCenter-$Version-Setup.exe"
$installerPath = Join-Path $InstallerArtifactsDirectory $installerName
$installerManifestPath = Join-Path $InstallerArtifactsDirectory "manifest.json"
$appExePath = Join-Path $publishDirectory "MabaControlCenter.exe"
$updaterExePath = Join-Path $publishDirectory "Updater\MabaUpdater.exe"

New-Item -ItemType Directory -Force -Path $publishDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $UpdateArtifactsDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $InstallerArtifactsDirectory | Out-Null

dotnet publish $ProjectPath -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=false -o $publishDirectory

$signingConfigured = Test-SigningConfigured
$signedArtifacts = @()

if ($signingConfigured) {
    if (-not (Test-Path $SignScriptPath)) {
        throw "Signing script not found at $SignScriptPath"
    }

    powershell -ExecutionPolicy Bypass -File $SignScriptPath -Files @($appExePath, $updaterExePath)
    $signedArtifacts += $appExePath, $updaterExePath
}

if (Test-Path $updatePackagePath) {
    Remove-Item $updatePackagePath -Force
}

Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $updatePackagePath -Force

Write-Manifest `
    -ManifestPath $updateManifestPath `
    -VersionValue $Version `
    -PackageName $updatePackageName `
    -Notes "Desktop app release $Version"

$innoCompiler = Find-InnoCompiler
if (-not $innoCompiler) {
    throw "Inno Setup 6 compiler was not found. Install Inno Setup 6 to build the installer."
}

& $innoCompiler `
    "/DMyAppVersion=$Version" `
    "/DSourcePublishDir=$publishDirectory" `
    "/DOutputInstallerDir=$InstallerArtifactsDirectory" `
    $InstallerScriptPath | Out-Host

if (-not (Test-Path $installerPath)) {
    throw "Installer output was not created at $installerPath"
}

if ($signingConfigured) {
    powershell -ExecutionPolicy Bypass -File $SignScriptPath -Files @($installerPath)
    $signedArtifacts += $installerPath
}

Write-Manifest `
    -ManifestPath $installerManifestPath `
    -VersionValue $Version `
    -PackageName $installerName `
    -Notes "Installer release $Version"

$result = [ordered]@{
    version = $Version
    configuration = $Configuration
    runtime = $Runtime
    publishDirectory = $publishDirectory
    updatePackagePath = $updatePackagePath
    updateManifestPath = $updateManifestPath
    installerPath = $installerPath
    installerManifestPath = $installerManifestPath
    signingConfigured = $signingConfigured
    signedArtifacts = $signedArtifacts
}

$result | ConvertTo-Json -Depth 4 | Out-Host

Write-Host ""
Write-Host "Final artifacts:" -ForegroundColor Cyan
Write-Host "  Publish directory : $publishDirectory"
Write-Host "  Update package    : $updatePackagePath"
Write-Host "  Update manifest   : $updateManifestPath"
Write-Host "  Installer         : $installerPath"
Write-Host "  Installer manifest: $installerManifestPath"
if ($signingConfigured) {
    Write-Host "  Signing           : enabled" -ForegroundColor Green
}
else {
    Write-Host "  Signing           : skipped (no certificate configured)" -ForegroundColor Yellow
}
