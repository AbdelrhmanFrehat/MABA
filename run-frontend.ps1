# ===================================
# MABA Frontend Runner Script
# ===================================
# This script runs the Angular frontend
# You can stop and restart this without affecting the backend

Write-Host "======================================" -ForegroundColor Magenta
Write-Host "      MABA Frontend UI Server         " -ForegroundColor Magenta
Write-Host "======================================" -ForegroundColor Magenta
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$uiPath = Join-Path $scriptPath "UI"

if (Test-Path $uiPath) {
    Write-Host "[*] Starting Frontend UI..." -ForegroundColor Green
    Write-Host "[*] Path: $uiPath" -ForegroundColor Gray
    Write-Host "[*] Press Ctrl+C to stop the frontend" -ForegroundColor Yellow
    Write-Host ""
    
    Set-Location $uiPath
    
    # Check if node_modules exists
    $nodeModulesPath = Join-Path $uiPath "node_modules"
    if (-not (Test-Path $nodeModulesPath)) {
        Write-Host "[!] node_modules not found, installing dependencies..." -ForegroundColor Yellow
        npm install
        Write-Host ""
    }
    
    npm start
} else {
    Write-Host "[X] Error: UI directory not found!" -ForegroundColor Red
    Write-Host "    Expected path: $uiPath" -ForegroundColor Red
    exit 1
}
