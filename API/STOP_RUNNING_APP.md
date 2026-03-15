# How to Stop a Running Application

If you get a "file is locked" error, it means the application is already running. Here's how to stop it:

## Option 1: Stop in the Terminal (If running in foreground)
Press `Ctrl+C` in the terminal where the app is running

## Option 2: Kill the Process in PowerShell
```powershell
# Find and kill the process
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"} | Stop-Process -Force

# Or find by port (if app is running on port 5153)
netstat -ano | findstr :5153
# Then kill the process ID shown in the last column
taskkill /PID <process_id> /F
```

## Option 3: Use Task Manager
1. Press `Ctrl+Shift+Esc` to open Task Manager
2. Look for "dotnet" or "Maba.Api" processes
3. Right-click and select "End Task"

## Option 4: Restart your IDE/Editor
Sometimes your IDE (like Visual Studio or VS Code) keeps the process running. Restart it.

## After Stopping
Once stopped, you can run the application again:
```powershell
dotnet run --project Maba.Api/Maba.Api.csproj
```

