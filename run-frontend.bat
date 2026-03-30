@echo off
title MABA Frontend UI Server
echo ======================================
echo       MABA Frontend UI Server
echo ======================================
echo.

REM Ensure Node.js/npm are on PATH (e.g. if installed but not in system PATH)
where npm >nul 2>nul || set "PATH=%ProgramFiles%\nodejs;%PATH%"
where npm >nul 2>nul || (
    echo [X] npm not found. Install Node.js from https://nodejs.org and ensure "Add to PATH" is checked.
    pause
    exit /b 1
)

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
