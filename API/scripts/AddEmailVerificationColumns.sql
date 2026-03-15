-- Add email verification columns to Users (if missing)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmailVerificationToken')
BEGIN
    ALTER TABLE [Users] ADD [EmailVerificationToken] nvarchar(500) NULL;
END
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmailVerificationTokenExpiresAt')
BEGIN
    ALTER TABLE [Users] ADD [EmailVerificationTokenExpiresAt] datetime2 NULL;
END
-- Record migration so EF doesn't try to re-apply (optional; only if you use this as the migration)
-- INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250304170000_AddEmailVerificationTokenToUser', N'8.0.11');
