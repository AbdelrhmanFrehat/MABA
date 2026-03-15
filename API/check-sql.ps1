# Check SQL Server Status
Write-Host "=== SQL Server Status Check ===" -ForegroundColor Cyan
Write-Host ""

# Check SQL Server service
Write-Host "1. Checking SQL Server Service..." -ForegroundColor Yellow
$sqlService = Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue
if ($sqlService) {
    if ($sqlService.Status -eq "Running") {
        Write-Host "   ✓ SQL Server is running" -ForegroundColor Green
    } else {
        Write-Host "   ✗ SQL Server is stopped. Status: $($sqlService.Status)" -ForegroundColor Red
        Write-Host "   To start it, run: Start-Service -Name MSSQLSERVER" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠ SQL Server service not found (might be using a named instance)" -ForegroundColor Yellow
}

# Check SQL Server Express
Write-Host ""
Write-Host "2. Checking SQL Server Express..." -ForegroundColor Yellow
$sqlExpress = Get-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction SilentlyContinue
if ($sqlExpress) {
    if ($sqlExpress.Status -eq "Running") {
        Write-Host "   ✓ SQL Server Express is running" -ForegroundColor Green
    } else {
        Write-Host "   ✗ SQL Server Express is stopped. Status: $($sqlExpress.Status)" -ForegroundColor Red
    }
}

# Check LocalDB
Write-Host ""
Write-Host "3. Checking LocalDB..." -ForegroundColor Yellow
try {
    $localdbCheck = sqllocaldb info
    if ($localdbCheck) {
        Write-Host "   ✓ LocalDB is available" -ForegroundColor Green
        Write-Host "   LocalDB instances: $localdbCheck" -ForegroundColor Cyan
    }
} catch {
    Write-Host "   ⚠ LocalDB might not be installed" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Recommendation ===" -ForegroundColor Cyan
Write-Host "If SQL Server is not running, use LocalDB in appsettings.json:" -ForegroundColor Yellow
Write-Host 'Server=(localdb)\mssqllocaldb;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;' -ForegroundColor White

