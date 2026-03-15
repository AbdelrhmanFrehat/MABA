# Quick Fix Guide for Database Connection Error

## The Problem
You're seeing: "An error occurred using the connection to database 'MabaDb'"

## Most Common Causes:

### 1. SQL Server is NOT Running
**Fix:**
```powershell
# Check if SQL Server is running
Get-Service -Name "MSSQLSERVER"

# If it says "Stopped", start it:
Start-Service -Name "MSSQLSERVER"
```

**OR** check in Services:
- Press `Win + R`, type `services.msc`
- Find "SQL Server (MSSQLSERVER)"
- Right-click → Start

### 2. Use LocalDB Instead (Easiest for Development)

Update `Maba.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

LocalDB is included with Visual Studio and doesn't need a full SQL Server installation.

### 3. Apply Migrations Manually First

Before running the app, apply migrations:

```powershell
# Stop any running app first
.\cleanup.ps1

# Apply migrations
dotnet ef database update --project Maba.Infrastructure/Maba.Infrastructure.csproj --startup-project Maba.Api/Maba.Api.csproj

# Then run the app
dotnet run --project Maba.Api/Maba.Api.csproj
```

### 4. Check SQL Server Instance Name

If you're using SQL Server Express or a named instance:

**For SQL Server Express:**
```
Server=localhost\SQLEXPRESS;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;
```

**For default instance:**
```
Server=localhost;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;
```

## Recommended: Use LocalDB for Development

LocalDB is the easiest option - just change the connection string as shown in step 2 above, and you're done!

