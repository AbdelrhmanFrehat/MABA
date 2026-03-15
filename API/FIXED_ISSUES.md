# Fixed Issues

## Cascade Delete Error Fix

**Problem**: SQL Server error when creating database:
```
Introducing FOREIGN KEY constraint 'FK_PrintJobs_SlicingJobs_SlicingJobId' on table 'PrintJobs' may cause cycles or multiple cascade paths.
```

**Solution**: Changed cascade delete behavior to `Restrict` for foreign key relationships that create cycles:

1. Created `PrintJobConfiguration.cs` - Changed all FKs to `DeleteBehavior.Restrict`
2. Created `SlicingJobConfiguration.cs` - Changed all FKs to `DeleteBehavior.Restrict`
3. Recreated migration with fixed configurations

**Files Modified**:
- `Maba.Infrastructure/Data/Configurations/PrintJobConfiguration.cs` (new)
- `Maba.Infrastructure/Data/Configurations/SlicingJobConfiguration.cs` (new)
- Migration recreated: `20251130101657_InitialCreate.cs`

Now the application should start successfully without cascade path errors!

