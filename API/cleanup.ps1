# Comprehensive cleanup script for MABA API
Write-Host "=== MABA API Cleanup Script ===" -ForegroundColor Cyan
Write-Host ""

# Stop processes using port 5153
Write-Host "1. Checking port 5153..." -ForegroundColor Yellow
$result = netstat -ano | findstr :5153 | findstr LISTENING
if ($result) {
    $pid = ($result -split '\s+')[-1]
    Write-Host "   Found process $pid using port 5153. Stopping..." -ForegroundColor Yellow
    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
}

# Stop any dotnet processes that might be running Maba.Api
Write-Host "2. Checking for dotnet processes..." -ForegroundColor Yellow
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "   Found $($dotnetProcesses.Count) dotnet process(es)" -ForegroundColor Yellow
    # Be careful - we don't want to kill ALL dotnet processes
    # Only kill if they're in our project directory
    foreach ($proc in $dotnetProcesses) {
        try {
            $path = $proc.Path
            if ($path -like "*Maba*" -or $path -like "*MABA*") {
                Write-Host "   Stopping process $($proc.Id) ($path)" -ForegroundColor Yellow
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            }
        } catch {
            # Process might have exited or we don't have access
        }
    }
}

# Wait a moment for files to be released
Start-Sleep -Seconds 2

# Check if port is free
Write-Host "3. Verifying port 5153 is free..." -ForegroundColor Yellow
$check = netstat -ano | findstr :5153 | findstr LISTENING
if ($check) {
    Write-Host "   WARNING: Port 5153 is still in use!" -ForegroundColor Red
    Write-Host "   You may need to restart your IDE or manually kill processes." -ForegroundColor Yellow
} else {
    Write-Host "   ✓ Port 5153 is free" -ForegroundColor Green
}

Write-Host ""
Write-Host "Cleanup complete! You can now run the application." -ForegroundColor Green
Write-Host "Run: dotnet run --project Maba.Api/Maba.Api.csproj" -ForegroundColor Cyan

