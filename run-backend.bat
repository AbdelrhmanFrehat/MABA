@echo off
title MABA Backend API Server
echo ======================================
echo        MABA Backend API Server
echo ======================================
echo.

cd /d "%~dp0API\Maba.Api"
if errorlevel 1 (
    echo [X] Error: Maba.Api directory not found!
    pause
    exit /b 1
)

echo [*] Starting Backend API...
echo [*] Press Ctrl+C to stop the backend
echo.

dotnet run
pause
