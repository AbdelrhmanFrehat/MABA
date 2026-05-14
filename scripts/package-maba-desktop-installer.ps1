param(
    [string]$ProjectPath = "C:\Users\PC\Desktop\maba\MabaControlCenter\MabaControlCenter.csproj",
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$Version = "",
    [string]$InstallerScriptPath = "C:\Users\PC\Desktop\maba\installer\MabaControlCenter.iss",
    [string]$InstallerArtifactsDirectory = "C:\Users\PC\Desktop\maba\artifacts\desktop-installer\stable"
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

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Get-ProjectVersion -CsprojPath $ProjectPath
}

$publishDirectory = Join-Path (Split-Path $ProjectPath -Parent) "publish\$Runtime-$Version"
New-Item -ItemType Directory -Force -Path $publishDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $InstallerArtifactsDirectory | Out-Null

dotnet publish $ProjectPath -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=false -o $publishDirectory

$innoCompiler = Find-InnoCompiler
$result = [ordered]@{
    version = $Version
    publishDirectory = $publishDirectory
    installerScript = $InstallerScriptPath
    installerOutputDirectory = $InstallerArtifactsDirectory
    installerCompilerFound = [bool]$innoCompiler
    installerPath = $null
    note = $null
}

if ($innoCompiler) {
    & $innoCompiler `
        "/DMyAppVersion=$Version" `
        "/DSourcePublishDir=$publishDirectory" `
        "/DOutputInstallerDir=$InstallerArtifactsDirectory" `
        $InstallerScriptPath | Out-Host

    $result.installerPath = Join-Path $InstallerArtifactsDirectory "MabaControlCenter-$Version-Setup.exe"
}
else {
    $result.note = "Inno Setup compiler was not found on this machine. Publish output is ready; install Inno Setup 6 to build the installer executable."
}

$result | ConvertTo-Json
