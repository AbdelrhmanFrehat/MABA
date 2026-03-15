@echo off
title MABA Frontend UI Server
echo ======================================
echo       MABA Frontend UI Server
echo ======================================
echo.

cd /d "%~dp0UI"
if errorlevel 1 (
    echo [X] Error: UI directory not found!
    pause
    exit /b 1
)

echo [*] Starting Frontend UI...
echo [*] Press Ctrl+C to stop the frontend
echo.

if not exist "node_modules" (
    echo [!] node_modules not found, installing dependencies...
    call npm install
    echo.
)

call npm start
pause
