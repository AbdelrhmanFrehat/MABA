# Database Setup Guide

## Common Database Connection Errors

If you see "An error occurred using the connection to database 'MabaDb'", check the following:

### 1. SQL Server is Running

**Check if SQL Server is running:**
```powershell
Get-Service -Name "MSSQLSERVER" | Select-Object Status, Name
```

**Start SQL Server if it's stopped:**
```powershell
Start-Service -Name "MSSQLSERVER"
```

**Or use SQL Server Configuration Manager:**
- Press `Win + R`, type `services.msc`
- Find "SQL Server (MSSQLSERVER)" or "SQL Server (SQLEXPRESS)"
- Right-click and select "Start"

### 2. Verify Connection String

The default connection string in `appsettings.json` is:
```
Server=localhost;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;
```

**If using SQL Server Express:**
```
Server=localhost\SQLEXPRESS;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;
```

**If using a named instance:**
```
Server=localhost\YourInstanceName;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;
```

### 3. Apply Migrations Manually

If the automatic migration fails, run manually:

```powershell
# Apply migrations
dotnet ef database update --project Maba.Infrastructure/Maba.Infrastructure.csproj --startup-project Maba.Api/Maba.Api.csproj
```

### 4. Create Database Manually (Alternative)

If migrations aren't working, you can create the database manually:

```sql
-- Connect to SQL Server Management Studio (SSMS)
-- Or use sqlcmd:

sqlcmd -S localhost -E -Q "CREATE DATABASE MabaDb"
```

Then run migrations:
```powershell
dotnet ef database update --project Maba.Infrastructure/Maba.Infrastructure.csproj --startup-project Maba.Api/Maba.Api.csproj
```

### 5. Check SQL Server Authentication

**For Windows Authentication (default):**
- Ensure you're logged in with a Windows account that has SQL Server permissions
- The connection string uses `Trusted_Connection=true`

**For SQL Server Authentication:**
If you need to use SQL Server authentication instead:
```
Server=localhost;Database=MabaDb;User Id=YourUsername;Password=YourPassword;TrustServerCertificate=true;
```

### 6. Verify Port and Instance

**Check SQL Server Browser service:**
```powershell
Get-Service -Name "SQLBrowser"
```

**Common SQL Server ports:**
- Default instance: 1433
- Named instances: Dynamic ports

### Troubleshooting Steps

1. **Test connection with sqlcmd:**
   ```powershell
   sqlcmd -S localhost -E -Q "SELECT @@VERSION"
   ```

2. **Check if database exists:**
   ```powershell
   sqlcmd -S localhost -E -Q "SELECT name FROM sys.databases WHERE name = 'MabaDb'"
   ```

3. **View full error details:**
   Check the application logs in the console output for detailed error messages.

### Quick Fix: Use LocalDB (Development)

For quick development, you can use LocalDB:

Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MabaDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

LocalDB is included with Visual Studio and doesn't require a separate SQL Server installation.

### Still Having Issues?

1. Check the full error message in the console/logs
2. Verify SQL Server version compatibility (SQL Server 2019+ recommended)
3. Ensure you have admin permissions on the SQL Server instance
4. Try restarting SQL Server service

