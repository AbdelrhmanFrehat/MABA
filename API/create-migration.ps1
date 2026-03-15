# Script to create and apply database migration for MABA Backend
# This will add all new tables and columns to the database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MABA Backend - Migration Creator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "Maba.Infrastructure")) {
    Write-Host "ERROR: Maba.Infrastructure directory not found!" -ForegroundColor Red
    Write-Host "Please run this script from the project root directory." -ForegroundColor Yellow
    exit 1
}

# Check if API is running
$apiProcess = Get-Process -Name "Maba.Api" -ErrorAction SilentlyContinue
if ($apiProcess) {
    Write-Host "WARNING: Maba.Api process is running!" -ForegroundColor Yellow
    Write-Host "The migration creation might fail if files are locked." -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Do you want to continue anyway? (y/n)"
    if ($response -ne "y" -and $response -ne "Y") {
        Write-Host "Migration creation cancelled." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Step 1: Creating migration..." -ForegroundColor Green
Write-Host ""

Set-Location Maba.Infrastructure

$migrationResult = dotnet ef migrations add CompleteBackendImplementation --startup-project ../Maba.Api/Maba.Api.csproj 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[SUCCESS] Migration created successfully!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Step 2: Applying migration to database..." -ForegroundColor Green
    Write-Host ""
    
    $updateResult = dotnet ef database update --startup-project ../Maba.Api/Maba.Api.csproj 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "[SUCCESS] Migration applied successfully!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "1. Restart the API: .\run.ps1" -ForegroundColor White
        Write-Host "2. All database errors should now be resolved!" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "ERROR: Failed to apply migration!" -ForegroundColor Red
        Write-Host $updateResult -ForegroundColor Red
        Write-Host ""
        Write-Host "Please check:" -ForegroundColor Yellow
        Write-Host "- SQL Server is running" -ForegroundColor White
        Write-Host "- Connection string in appsettings.json is correct" -ForegroundColor White
        Write-Host "- You have permissions to modify the database" -ForegroundColor White
    }
} else {
    Write-Host ""
    Write-Host "ERROR: Failed to create migration!" -ForegroundColor Red
    Write-Host $migrationResult -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "- API is still running (stop it first)" -ForegroundColor White
    Write-Host "- Build errors (check the output above)" -ForegroundColor White
    Write-Host "- EF Core tools not installed (run: dotnet tool install --global dotnet-ef)" -ForegroundColor White
}

Set-Location ..

