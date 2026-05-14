param(
    [string]$ApiBaseUrl = "https://api.mabasol.com",
    [string]$Channel = "stable",
    [string]$Version = "0.1.20",
    [string]$InstallerPath = "C:\Users\PC\Desktop\maba\artifacts\desktop-installer\stable\MabaControlCenter-0.1.20-Setup.exe",
    [string]$Email = "admin@maba.com",
    [string]$Password = "Password123!"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Net.Http

$loginBody = @{ email = $Email; password = $Password } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/auth/login" -Method Post -ContentType 'application/json' -Body $loginBody
if (-not $loginResponse.token) {
    throw "Login succeeded but no token was returned."
}

$client = New-Object System.Net.Http.HttpClient
$client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $loginResponse.token)

$multipart = New-Object System.Net.Http.MultipartFormDataContent
$fileStream = [System.IO.File]::OpenRead($InstallerPath)
$fileContent = New-Object System.Net.Http.StreamContent($fileStream)
$fileContent.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue('application/octet-stream')
$multipart.Add($fileContent, 'file', [System.IO.Path]::GetFileName($InstallerPath))
$multipart.Add((New-Object System.Net.Http.StringContent($Version)), 'version')
$multipart.Add((New-Object System.Net.Http.StringContent("Installer release $Version")), 'notes')
$multipart.Add((New-Object System.Net.Http.StringContent($Channel)), 'channel')

$publishHttp = $client.PostAsync("$ApiBaseUrl/api/v1/desktop-updates/publish-installer", $multipart).GetAwaiter().GetResult()
$publishBody = $publishHttp.Content.ReadAsStringAsync().GetAwaiter().GetResult()

$fileStream.Dispose()
$multipart.Dispose()
$client.Dispose()

if (-not $publishHttp.IsSuccessStatusCode) {
    throw "Publish installer failed: $($publishHttp.StatusCode) $publishBody"
}

$publishBody
