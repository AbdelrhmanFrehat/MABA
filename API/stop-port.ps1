# Stop any process using port 5153
Write-Host "Checking for processes using port 5153..." -ForegroundColor Yellow

$result = netstat -ano | findstr :5153 | findstr LISTENING

if ($result) {
    $pid = ($result -split '\s+')[-1]
    Write-Host "Found process $pid using port 5153. Stopping..." -ForegroundColor Yellow
    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
    
    # Verify it's stopped
    $check = netstat -ano | findstr :5153 | findstr LISTENING
    if ($check) {
        Write-Host "Failed to stop process. Trying again..." -ForegroundColor Red
        $pid2 = ($check -split '\s+')[-1]
        Stop-Process -Id $pid2 -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "Port 5153 is now free!" -ForegroundColor Green
    }
} else {
    Write-Host "Port 5153 is already free." -ForegroundColor Green
}

