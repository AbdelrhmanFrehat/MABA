# Build script for MABA Frontend (Angular)
# This script builds the Angular application for production deployment

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building MABA Frontend (Angular)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"

# Set paths
$uiPath = Join-Path $PSScriptRoot "UI"
$outputPath = Join-Path $PSScriptRoot "publish\frontend"

# Check if UI directory exists
if (-not (Test-Path $uiPath)) {
    Write-Host "Error: UI directory not found at $uiPath" -ForegroundColor Red
    exit 1
}

# Check if package.json exists
$packageJsonPath = Join-Path $uiPath "package.json"
if (-not (Test-Path $packageJsonPath)) {
    Write-Host "Error: package.json not found at $packageJsonPath" -ForegroundColor Red
    exit 1
}

# Change to UI directory
Push-Location $uiPath

try {
    Write-Host "`nChecking Node.js installation..." -ForegroundColor Yellow
    $nodeVersion = node --version
    Write-Host "Node.js version: $nodeVersion" -ForegroundColor Green
    
    Write-Host "`nChecking npm installation..." -ForegroundColor Yellow
    $npmVersion = npm --version
    Write-Host "npm version: $npmVersion" -ForegroundColor Green
    
    # Check if node_modules exists, if not install dependencies
    if (-not (Test-Path "node_modules")) {
        Write-Host "`nInstalling npm dependencies..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "`nFailed to install dependencies!" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "`nDependencies already installed. Skipping npm install." -ForegroundColor Green
    }
    
    Write-Host "`nBuilding Angular application for production..." -ForegroundColor Yellow
    npm run build -- --configuration production
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nBuild failed!" -ForegroundColor Red
        exit 1
    }
    
    # Copy build output to publish directory
    $distPath = Join-Path $uiPath "dist\maba-ng"
    if (Test-Path $distPath) {
        Write-Host "`nCopying build output to publish directory..." -ForegroundColor Yellow
        if (Test-Path $outputPath) {
            Remove-Item $outputPath -Recurse -Force
        }
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        Copy-Item -Path "$distPath\*" -Destination $outputPath -Recurse -Force
        Write-Host "Build output copied to: $outputPath" -ForegroundColor Green
    } else {
        Write-Host "`nWarning: Build output not found at $distPath" -ForegroundColor Yellow
    }
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Frontend build completed successfully!" -ForegroundColor Green
    Write-Host "Output location: $outputPath" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
}
finally {
    Pop-Location
}
