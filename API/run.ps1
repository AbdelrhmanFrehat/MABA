# Run script for MABA API
# Get the script directory and navigate to Maba.Api
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiPath = Join-Path $scriptPath "Maba.Api"
if (Test-Path $apiPath) {
    Set-Location $apiPath
    dotnet run
} else {
    Write-Error "Maba.Api directory not found. Please run this script from the project root."
}

