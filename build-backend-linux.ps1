# Build script for MABA Backend API - Linux x64
# This script builds and publishes the .NET API for Linux deployment

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building MABA Backend API for Linux x64" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# Set paths
$apiPath = Join-Path $PSScriptRoot "API"
$projectPath = Join-Path $apiPath "Maba.Api\Maba.Api.csproj"
$outputPath = Join-Path $PSScriptRoot "publish\backend-linux"

# Check if project exists
if (-not (Test-Path $projectPath)) {
    Write-Host "Error: Project file not found at $projectPath" -ForegroundColor Red
    exit 1
}

Write-Host "`nCleaning previous build..." -ForegroundColor Yellow
dotnet clean $projectPath --configuration Release

Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $projectPath

Write-Host "`nBuilding project..." -ForegroundColor Yellow
dotnet build $projectPath --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nPublishing project for Linux x64 deployment..." -ForegroundColor Yellow
# Create output directory if it doesn't exist
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

# Restore with Linux x64 runtime identifier
Write-Host "Restoring with Linux x64 runtime identifier..." -ForegroundColor Yellow
dotnet restore $projectPath --runtime linux-x64

dotnet publish $projectPath `
    --configuration Release `
    --output $outputPath `
    --self-contained false `
    --runtime linux-x64

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nPublish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Backend build for Linux x64 completed successfully!" -ForegroundColor Green
Write-Host "Output location: $outputPath" -ForegroundColor Green
Write-Host "Runtime: Linux x64" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`nNote: This build requires .NET 8.0 Runtime on Linux server" -ForegroundColor Yellow
Write-Host "To run on Linux: dotnet Maba.Api.dll" -ForegroundColor Yellow
