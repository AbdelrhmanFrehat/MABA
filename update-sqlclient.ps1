# Update Microsoft.Data.SqlClient to fix Linux compatibility issue
# This script updates the package and rebuilds the project

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Updating Microsoft.Data.SqlClient" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# Set paths
$apiPath = Join-Path $PSScriptRoot "API"
$infrastructureProject = Join-Path $apiPath "Maba.Infrastructure\Maba.Infrastructure.csproj"

Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $infrastructureProject

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nRestore failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nBuilding project..." -ForegroundColor Yellow
dotnet build $infrastructureProject --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Package updated successfully!" -ForegroundColor Green
Write-Host "Now run build-backend.ps1 to rebuild the application" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Green
