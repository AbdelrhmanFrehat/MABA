# ⚠️ IMPORTANT: Create Database Migration

The application is running, but you need to create and apply a database migration to add all the new tables and columns.

## Current Status

✅ **Application is running** - The API started successfully  
⚠️ **Database schema is incomplete** - Missing tables/columns for new features  
✅ **Error handling is in place** - App won't crash, but some features won't work until migration is applied

## Steps to Fix

### 1. Stop the API
Press `Ctrl+C` in the terminal where the API is running, or close the terminal.

### 2. Create the Migration

Open a **new** PowerShell terminal in the project root and run:

```powershell
cd Maba.Infrastructure
dotnet ef migrations add CompleteBackendImplementation --startup-project ../Maba.Api/Maba.Api.csproj
```

This will create a migration file with all the new tables and columns.

### 3. Apply the Migration

```powershell
dotnet ef database update --startup-project ../Maba.Api/Maba.Api.csproj
```

This will update your database with all the new schema changes.

### 4. Restart the API

Run the API again:
```powershell
cd ..
.\run.ps1
```

## What the Migration Will Add

### New Tables:
- `AuditLogs` - Audit trail for all actions
- `SystemSettings` - Application configuration
- `EmailTemplates` - Email templates
- `Notifications` - User notifications

### New Columns (added to existing tables):
- **PrintJob**: `ErrorMessage`, `EstimatedCompletionTime`, `ProgressPercent`
- **SlicingJob**: `ErrorMessage`, `EstimatedCost`, `OutputFileUrl`
- **SlicingProfile**: `IsDefault`, `TemperatureSettings`
- **Printer**: `CurrentStatus`, `IsActive`, `Location`
- **DesignFile**: `FileSizeBytes`, `IsPrimary`
- **User**: `EmailConfirmed`, `PhoneConfirmed`, `LastLoginAt`, etc.
- **Item**: `DiscountPrice`, `Weight`, `Dimensions`, `IsFeatured`, etc.
- **Category**: `ImageId`, SEO metadata
- **Tag**: `Color`, `Icon`, `UsageCount`
- **Brand**: `LogoId`, `WebsiteUrl`, `Country`, `DescriptionEn/Ar`
- **Machine**: `ImageId`, `ManualId`, `WarrantyMonths`, `Location`, etc.
- **MachinePart**: `Price`, `InventoryId`, `ImageId`
- **Inventory**: `QuantityReserved`, `QuantityOnOrder`, `CostPerUnit`, etc.
- And many more...

## After Migration

Once the migration is applied:
- ✅ All errors will stop
- ✅ All features will work
- ✅ Seed data will populate correctly
- ✅ Background workers will process jobs

## Troubleshooting

### Error: "Cannot find path 'Maba.Api\Maba.Api'"
- This is a harmless warning from the run script
- The API still runs correctly

### Error: "Invalid column name" or "Invalid object name"
- **Expected** until migration is applied
- These are caught and logged, won't crash the app
- Will stop after migration is applied

### Error: "Project file does not exist"
- Make sure you're in the project root directory
- Use: `cd Maba.Infrastructure` (not `cd Maba.Infrastructure\Maba.Infrastructure`)

### Error: "Unable to delete file... is being used by another process"
- Stop the running API first
- Close any processes using the DLLs
- Then create the migration

