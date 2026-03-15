# ===================================
# MABA Backend Runner Script
# ===================================
# This script runs the .NET API backend
# You can stop and restart this without affecting the frontend

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "       MABA Backend API Server        " -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiPath = Join-Path $scriptPath "API\Maba.Api"

if (Test-Path $apiPath) {
    Write-Host "[*] Starting Backend API..." -ForegroundColor Green
    Write-Host "[*] Path: $apiPath" -ForegroundColor Gray
    Write-Host "[*] Press Ctrl+C to stop the backend" -ForegroundColor Yellow
    Write-Host ""
    
    Set-Location $apiPath
    dotnet run
} else {
    Write-Host "[X] Error: Maba.Api directory not found!" -ForegroundColor Red
    Write-Host "    Expected path: $apiPath" -ForegroundColor Red
    exit 1
}
