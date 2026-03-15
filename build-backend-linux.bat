@echo off
echo ========================================
echo Building MABA Backend API for Linux x64
echo ========================================

cd API

echo.
echo Cleaning previous build...
dotnet clean Maba.Api\Maba.Api.csproj --configuration Release

echo.
echo Restoring NuGet packages...
dotnet restore Maba.Api\Maba.Api.csproj --runtime linux-x64

echo.
echo Building project...
dotnet build Maba.Api\Maba.Api.csproj --configuration Release --no-restore

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed!
    exit /b 1
)

cd ..

echo.
echo Publishing project for Linux x64 deployment...
if exist "publish\backend-linux" (
    rmdir /s /q "publish\backend-linux"
)
mkdir "publish\backend-linux"

dotnet publish API\Maba.Api\Maba.Api.csproj ^
    --configuration Release ^
    --output "publish\backend-linux" ^
    --self-contained false ^
    --runtime linux-x64

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Publish failed!
    exit /b 1
)

echo.
echo ========================================
echo Backend build for Linux x64 completed successfully!
echo Output location: publish\backend-linux
echo Runtime: Linux x64
echo ========================================
echo.
echo Note: This build requires .NET 8.0 Runtime on Linux server
echo To run on Linux: dotnet Maba.Api.dll
echo.

pause
