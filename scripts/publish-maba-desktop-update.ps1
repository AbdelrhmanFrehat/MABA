param(
    [string]$ProjectPath = "C:\Users\PC\Desktop\maba\MabaControlCenter\MabaControlCenter.csproj",
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$StagingDirectory = "C:\Users\PC\Desktop\maba\artifacts\desktop-updates\stable",
    [string]$Version = ""
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

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Get-ProjectVersion -CsprojPath $ProjectPath
}

$publishDirectory = Join-Path (Split-Path $ProjectPath -Parent) "publish\$Runtime-$Version"
$packageName = "MabaControlCenter-$Version-$Runtime.zip"
$packagePath = Join-Path $StagingDirectory $packageName
$manifestPath = Join-Path $StagingDirectory "manifest.json"

New-Item -ItemType Directory -Force -Path $publishDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $StagingDirectory | Out-Null

dotnet publish $ProjectPath -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=false -o $publishDirectory

if (Test-Path $packagePath) {
    Remove-Item $packagePath -Force
}

Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $packagePath -Force

$manifest = [ordered]@{
    version = $Version
    packageUri = $packageName
    notes = "Desktop app release $Version"
    publishedAt = (Get-Date).ToUniversalTime().ToString("o")
}

$manifest | ConvertTo-Json | Set-Content -Path $manifestPath -Encoding UTF8

Write-Host "Publish directory: $publishDirectory"
Write-Host "Package: $packagePath"
Write-Host "Manifest: $manifestPath"
