# Build script for both MABA Frontend and Backend
# This script builds both applications for deployment

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "Building MABA Application (Full Build)" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

$ErrorActionPreference = "Stop"

# Build Backend
Write-Host "`n[1/2] Building Backend..." -ForegroundColor Cyan
& "$PSScriptRoot\build-backend.ps1"
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBackend build failed!" -ForegroundColor Red
    exit 1
}

# Build Frontend
Write-Host "`n[2/2] Building Frontend..." -ForegroundColor Cyan
& "$PSScriptRoot\build-frontend.ps1"
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nFrontend build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "All builds completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`nBuild outputs:" -ForegroundColor Yellow
Write-Host "  Backend:  publish\backend" -ForegroundColor White
Write-Host "  Frontend: publish\frontend" -ForegroundColor White
Write-Host "`nYou can now deploy these folders to your server." -ForegroundColor Green
