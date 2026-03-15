# Create Database Migration

The database schema is out of sync with the entity model. You need to create and apply a migration.

## Steps

1. **Stop the running API** (if it's running)

2. **Create the migration:**
   ```powershell
   cd Maba.Infrastructure
   dotnet ef migrations add CompleteBackendImplementation --startup-project ../Maba.Api/Maba.Api.csproj
   ```

3. **Apply the migration:**
   ```powershell
   dotnet ef database update --startup-project ../Maba.Api/Maba.Api.csproj
   ```

4. **Restart the API**

## Alternative: Let the app create it automatically

The app will try to apply migrations on startup in Development mode, but you need to create the migration first.

## What the migration will add:

- New properties to existing entities (PrintJob, SlicingJob, SlicingProfile, Printer, DesignFile, etc.)
- New entities (AuditLog, SystemSetting, EmailTemplate, Notification)
- New properties to User, Role, Permission, Item, Category, Tag, Brand, Machine, MachinePart, Inventory, etc.

## If you get errors:

- Make sure SQL Server is running
- Check the connection string in `appsettings.json`
- Ensure you have permissions to create/modify the database

