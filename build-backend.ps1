# Build script for MABA Backend API
# This script builds and publishes the .NET API for deployment

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building MABA Backend API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# Set paths
$apiPath = Join-Path $PSScriptRoot "API"
$projectPath = Join-Path $apiPath "Maba.Api\Maba.Api.csproj"
$outputPath = Join-Path $PSScriptRoot "publish\backend"

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

Write-Host "`nPublishing project for deployment..." -ForegroundColor Yellow
# Create output directory if it doesn't exist
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

# Restore with runtime identifier for Linux x64
Write-Host "Restoring with runtime identifier (linux-x64)..." -ForegroundColor Yellow
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
Write-Host "Backend build completed successfully!" -ForegroundColor Green
Write-Host "Output location: $outputPath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
