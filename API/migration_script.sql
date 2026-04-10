IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [AiSenderTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AiSenderTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [AiSessionSources] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AiSessionSources] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Brands] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] uniqueidentifier NOT NULL,
        [ParentId] uniqueidentifier NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [Slug] nvarchar(200) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Categories_Categories_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [ExpenseCategories] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ExpenseCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [IncomeSources] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_IncomeSources] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [InstallmentStatuses] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_InstallmentStatuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [InvoiceStatuses] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_InvoiceStatuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemStatuses] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ItemStatuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [LayoutTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LayoutTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Machines] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [Manufacturer] nvarchar(max) NULL,
        [Model] nvarchar(max) NULL,
        [YearFrom] int NULL,
        [YearTo] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Machines] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Materials] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [PricePerGram] decimal(18,2) NOT NULL,
        [Density] decimal(18,2) NOT NULL,
        [Color] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Materials] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [MediaTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_MediaTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [MediaUsageTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_MediaUsageTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderStatuses] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_OrderStatuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Pages] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [Path] nvarchar(max) NOT NULL,
        [TitleEn] nvarchar(max) NULL,
        [TitleAr] nvarchar(max) NULL,
        [MetaTitleEn] nvarchar(max) NULL,
        [MetaTitleAr] nvarchar(max) NULL,
        [MetaDescriptionEn] nvarchar(max) NULL,
        [MetaDescriptionAr] nvarchar(max) NULL,
        [IsHome] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Pages] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PageSectionTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PageSectionTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PaymentMethods] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PaymentMethods] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PrintingTechnologies] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PrintingTechnologies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PrintJobStatuses] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PrintJobStatuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [SiteSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SiteSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [SlicingJobStatuses] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SlicingJobStatuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Tags] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [Slug] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [Phone] nvarchar(50) NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Items] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(500) NOT NULL,
        [NameAr] nvarchar(500) NOT NULL,
        [Sku] nvarchar(100) NOT NULL,
        [GeneralDescriptionEn] nvarchar(max) NULL,
        [GeneralDescriptionAr] nvarchar(max) NULL,
        [ItemStatusId] uniqueidentifier NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'USD',
        [BrandId] uniqueidentifier NULL,
        [CategoryId] uniqueidentifier NULL,
        [AverageRating] decimal(3,2) NOT NULL DEFAULT 0.0,
        [ReviewsCount] int NOT NULL DEFAULT 0,
        [ViewsCount] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Items] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Items_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Items_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Items_ItemStatuses_ItemStatusId] FOREIGN KEY ([ItemStatusId]) REFERENCES [ItemStatuses] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [MachineParts] (
        [Id] uniqueidentifier NOT NULL,
        [MachineId] uniqueidentifier NOT NULL,
        [PartNameEn] nvarchar(max) NOT NULL,
        [PartNameAr] nvarchar(max) NOT NULL,
        [PartCode] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_MachineParts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MachineParts_Machines_MachineId] FOREIGN KEY ([MachineId]) REFERENCES [Machines] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [MediaAssets] (
        [Id] uniqueidentifier NOT NULL,
        [FileUrl] nvarchar(max) NOT NULL,
        [MimeType] nvarchar(max) NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FileExtension] nvarchar(max) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [Width] int NULL,
        [Height] int NULL,
        [TitleEn] nvarchar(max) NULL,
        [TitleAr] nvarchar(max) NULL,
        [AltEn] nvarchar(max) NULL,
        [AltAr] nvarchar(max) NULL,
        [UploadedByUserId] uniqueidentifier NULL,
        [MediaTypeId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_MediaAssets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MediaAssets_MediaTypes_MediaTypeId] FOREIGN KEY ([MediaTypeId]) REFERENCES [MediaTypes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Printers] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [Vendor] nvarchar(max) NULL,
        [BuildVolumeX] decimal(18,2) NOT NULL,
        [BuildVolumeY] decimal(18,2) NOT NULL,
        [BuildVolumeZ] decimal(18,2) NOT NULL,
        [PrintingTechnologyId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Printers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Printers_PrintingTechnologies_PrintingTechnologyId] FOREIGN KEY ([PrintingTechnologyId]) REFERENCES [PrintingTechnologies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [RoleId] uniqueidentifier NOT NULL,
        [PermissionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [AiSessions] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [AiSessionSourceId] uniqueidentifier NULL,
        [StartedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AiSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiSessions_AiSessionSources_AiSessionSourceId] FOREIGN KEY ([AiSessionSourceId]) REFERENCES [AiSessionSources] ([Id]),
        CONSTRAINT [FK_AiSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Designs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Designs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Designs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Incomes] (
        [Id] uniqueidentifier NOT NULL,
        [IncomeSourceId] uniqueidentifier NOT NULL,
        [RefId] nvarchar(max) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        [EnteredByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Incomes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Incomes_IncomeSources_IncomeSourceId] FOREIGN KEY ([IncomeSourceId]) REFERENCES [IncomeSources] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Incomes_Users_EnteredByUserId] FOREIGN KEY ([EnteredByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [OrderStatusId] uniqueidentifier NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'USD',
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_OrderStatuses_OrderStatusId] FOREIGN KEY ([OrderStatusId]) REFERENCES [OrderStatuses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PageSectionDrafts] (
        [Id] uniqueidentifier NOT NULL,
        [PageId] uniqueidentifier NOT NULL,
        [PageSectionTypeId] uniqueidentifier NOT NULL,
        [LayoutTypeId] uniqueidentifier NOT NULL,
        [TitleEn] nvarchar(max) NULL,
        [TitleAr] nvarchar(max) NULL,
        [SubtitleEn] nvarchar(max) NULL,
        [SubtitleAr] nvarchar(max) NULL,
        [ConfigJson] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PageSectionDrafts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PageSectionDrafts_LayoutTypes_LayoutTypeId] FOREIGN KEY ([LayoutTypeId]) REFERENCES [LayoutTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PageSectionDrafts_PageSectionTypes_PageSectionTypeId] FOREIGN KEY ([PageSectionTypeId]) REFERENCES [PageSectionTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PageSectionDrafts_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Pages] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PageSectionDrafts_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]),
        CONSTRAINT [FK_PageSectionDrafts_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PageSectionPublished] (
        [Id] uniqueidentifier NOT NULL,
        [PageId] uniqueidentifier NOT NULL,
        [PageSectionTypeId] uniqueidentifier NOT NULL,
        [LayoutTypeId] uniqueidentifier NOT NULL,
        [TitleEn] nvarchar(max) NULL,
        [TitleAr] nvarchar(max) NULL,
        [SubtitleEn] nvarchar(max) NULL,
        [SubtitleAr] nvarchar(max) NULL,
        [ConfigJson] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [PublishedAt] datetime2 NOT NULL,
        [PublishedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PageSectionPublished] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PageSectionPublished_LayoutTypes_LayoutTypeId] FOREIGN KEY ([LayoutTypeId]) REFERENCES [LayoutTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PageSectionPublished_PageSectionTypes_PageSectionTypeId] FOREIGN KEY ([PageSectionTypeId]) REFERENCES [PageSectionTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PageSectionPublished_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Pages] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PageSectionPublished_Users_PublishedByUserId] FOREIGN KEY ([PublishedByUserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Comments] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [ParentCommentId] uniqueidentifier NULL,
        [IsApproved] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_Comments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [Comments] ([Id]),
        CONSTRAINT [FK_Comments_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Inventories] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [QuantityOnHand] int NOT NULL,
        [ReorderLevel] int NOT NULL,
        [LastStockInAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Inventories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Inventories_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemSections] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [TitleEn] nvarchar(max) NULL,
        [TitleAr] nvarchar(max) NULL,
        [DescriptionEn] nvarchar(max) NULL,
        [DescriptionAr] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ItemSections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItemSections_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemTags] (
        [ItemId] uniqueidentifier NOT NULL,
        [TagId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ItemTags] PRIMARY KEY ([ItemId], [TagId]),
        CONSTRAINT [FK_ItemTags_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ItemTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Reviews] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Rating] int NOT NULL,
        [Title] nvarchar(max) NULL,
        [Body] nvarchar(max) NULL,
        [IsApproved] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reviews_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemMachineLinks] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [MachineId] uniqueidentifier NOT NULL,
        [MachinePartId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ItemMachineLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItemMachineLinks_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ItemMachineLinks_MachineParts_MachinePartId] FOREIGN KEY ([MachinePartId]) REFERENCES [MachineParts] ([Id]),
        CONSTRAINT [FK_ItemMachineLinks_Machines_MachineId] FOREIGN KEY ([MachineId]) REFERENCES [Machines] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [EntityMediaLinks] (
        [Id] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] uniqueidentifier NOT NULL,
        [MediaAssetId] uniqueidentifier NOT NULL,
        [MediaUsageTypeId] uniqueidentifier NOT NULL,
        [SortOrder] int NOT NULL,
        [IsPrimary] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EntityMediaLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EntityMediaLinks_MediaAssets_MediaAssetId] FOREIGN KEY ([MediaAssetId]) REFERENCES [MediaAssets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EntityMediaLinks_MediaUsageTypes_MediaUsageTypeId] FOREIGN KEY ([MediaUsageTypeId]) REFERENCES [MediaUsageTypes] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Expenses] (
        [Id] uniqueidentifier NOT NULL,
        [ExpenseCategoryId] uniqueidentifier NOT NULL,
        [DescriptionEn] nvarchar(max) NULL,
        [DescriptionAr] nvarchar(max) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [SpentAt] datetime2 NOT NULL,
        [ReceiptMediaId] uniqueidentifier NULL,
        [EnteredByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Expenses_ExpenseCategories_ExpenseCategoryId] FOREIGN KEY ([ExpenseCategoryId]) REFERENCES [ExpenseCategories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Expenses_MediaAssets_ReceiptMediaId] FOREIGN KEY ([ReceiptMediaId]) REFERENCES [MediaAssets] ([Id]),
        CONSTRAINT [FK_Expenses_Users_EnteredByUserId] FOREIGN KEY ([EnteredByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [SlicingProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [PrintingTechnologyId] uniqueidentifier NOT NULL,
        [LayerHeightMm] decimal(18,2) NOT NULL,
        [InfillPercent] decimal(18,2) NOT NULL,
        [SupportsEnabled] bit NOT NULL,
        [MaterialId] uniqueidentifier NOT NULL,
        [PrinterId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SlicingProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SlicingProfiles_Materials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [Materials] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SlicingProfiles_Printers_PrinterId] FOREIGN KEY ([PrinterId]) REFERENCES [Printers] ([Id]),
        CONSTRAINT [FK_SlicingProfiles_PrintingTechnologies_PrintingTechnologyId] FOREIGN KEY ([PrintingTechnologyId]) REFERENCES [PrintingTechnologies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [AiMessages] (
        [Id] uniqueidentifier NOT NULL,
        [SessionId] uniqueidentifier NOT NULL,
        [AiSenderTypeId] uniqueidentifier NOT NULL,
        [Text] nvarchar(max) NOT NULL,
        [MetaJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AiMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AiMessages_AiSenderTypes_AiSenderTypeId] FOREIGN KEY ([AiSenderTypeId]) REFERENCES [AiSenderTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AiMessages_AiSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [AiSessions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [DesignFiles] (
        [Id] uniqueidentifier NOT NULL,
        [DesignId] uniqueidentifier NOT NULL,
        [MediaAssetId] uniqueidentifier NOT NULL,
        [Format] nvarchar(max) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DesignFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DesignFiles_Designs_DesignId] FOREIGN KEY ([DesignId]) REFERENCES [Designs] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DesignFiles_MediaAssets_MediaAssetId] FOREIGN KEY ([MediaAssetId]) REFERENCES [MediaAssets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [InvoiceNumber] nvarchar(max) NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [DueDate] datetime2 NULL,
        [Total] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [InvoiceStatusId] uniqueidentifier NOT NULL,
        [PdfUrl] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoices_InvoiceStatuses_InvoiceStatusId] FOREIGN KEY ([InvoiceStatusId]) REFERENCES [InvoiceStatuses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Invoices_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [MetaJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItems_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PaymentPlans] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [DownPayment] decimal(18,2) NOT NULL,
        [InstallmentsCount] int NOT NULL,
        [InstallmentFrequency] nvarchar(max) NOT NULL,
        [InterestRate] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PaymentPlans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentPlans_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PageSectionItemDrafts] (
        [Id] uniqueidentifier NOT NULL,
        [PageSectionDraftId] uniqueidentifier NOT NULL,
        [LinkedEntityType] nvarchar(max) NOT NULL,
        [LinkedEntityId] uniqueidentifier NOT NULL,
        [ExtraConfigJson] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PageSectionItemDrafts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PageSectionItemDrafts_PageSectionDrafts_PageSectionDraftId] FOREIGN KEY ([PageSectionDraftId]) REFERENCES [PageSectionDrafts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PageSectionItemPublished] (
        [Id] uniqueidentifier NOT NULL,
        [PageSectionPublishedId] uniqueidentifier NOT NULL,
        [LinkedEntityType] nvarchar(max) NOT NULL,
        [LinkedEntityId] uniqueidentifier NOT NULL,
        [ExtraConfigJson] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PageSectionItemPublished] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PageSectionItemPublished_PageSectionPublished_PageSectionPublishedId] FOREIGN KEY ([PageSectionPublishedId]) REFERENCES [PageSectionPublished] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemSectionFeatures] (
        [Id] uniqueidentifier NOT NULL,
        [ItemSectionId] uniqueidentifier NOT NULL,
        [TextEn] nvarchar(max) NULL,
        [TextAr] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ItemSectionFeatures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItemSectionFeatures_ItemSections_ItemSectionId] FOREIGN KEY ([ItemSectionId]) REFERENCES [ItemSections] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [SlicingJobs] (
        [Id] uniqueidentifier NOT NULL,
        [DesignFileId] uniqueidentifier NOT NULL,
        [SlicingProfileId] uniqueidentifier NOT NULL,
        [SlicingJobStatusId] uniqueidentifier NOT NULL,
        [EstimatedTimeMin] int NULL,
        [EstimatedMaterialGrams] decimal(18,2) NULL,
        [PriceEstimate] decimal(18,2) NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SlicingJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SlicingJobs_DesignFiles_DesignFileId] FOREIGN KEY ([DesignFileId]) REFERENCES [DesignFiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SlicingJobs_SlicingJobStatuses_SlicingJobStatusId] FOREIGN KEY ([SlicingJobStatusId]) REFERENCES [SlicingJobStatuses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SlicingJobs_SlicingProfiles_SlicingProfileId] FOREIGN KEY ([SlicingProfileId]) REFERENCES [SlicingProfiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NULL,
        [InvoiceId] uniqueidentifier NULL,
        [PaymentMethodId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        [RefNo] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]),
        CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]),
        CONSTRAINT [FK_Payments_PaymentMethods_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethods] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [Installments] (
        [Id] uniqueidentifier NOT NULL,
        [PaymentPlanId] uniqueidentifier NOT NULL,
        [Seq] int NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [InstallmentStatusId] uniqueidentifier NOT NULL,
        [PaidAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Installments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Installments_InstallmentStatuses_InstallmentStatusId] FOREIGN KEY ([InstallmentStatusId]) REFERENCES [InstallmentStatuses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Installments_PaymentPlans_PaymentPlanId] FOREIGN KEY ([PaymentPlanId]) REFERENCES [PaymentPlans] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE TABLE [PrintJobs] (
        [Id] uniqueidentifier NOT NULL,
        [SlicingJobId] uniqueidentifier NOT NULL,
        [PrinterId] uniqueidentifier NOT NULL,
        [PrintJobStatusId] uniqueidentifier NOT NULL,
        [StartedAt] datetime2 NULL,
        [FinishedAt] datetime2 NULL,
        [ActualMaterialGrams] decimal(18,2) NULL,
        [ActualTimeMin] int NULL,
        [FinalPrice] decimal(18,2) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PrintJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrintJobs_PrintJobStatuses_PrintJobStatusId] FOREIGN KEY ([PrintJobStatusId]) REFERENCES [PrintJobStatuses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PrintJobs_Printers_PrinterId] FOREIGN KEY ([PrinterId]) REFERENCES [Printers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PrintJobs_SlicingJobs_SlicingJobId] FOREIGN KEY ([SlicingJobId]) REFERENCES [SlicingJobs] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiMessages_AiSenderTypeId] ON [AiMessages] ([AiSenderTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiMessages_SessionId] ON [AiMessages] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiSessions_AiSessionSourceId] ON [AiSessions] ([AiSessionSourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AiSessions_UserId] ON [AiSessions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_ParentId] ON [Categories] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_ItemId] ON [Comments] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_ParentCommentId] ON [Comments] ([ParentCommentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DesignFiles_DesignId] ON [DesignFiles] ([DesignId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DesignFiles_MediaAssetId] ON [DesignFiles] ([MediaAssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Designs_UserId] ON [Designs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EntityMediaLinks_MediaAssetId] ON [EntityMediaLinks] ([MediaAssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EntityMediaLinks_MediaUsageTypeId] ON [EntityMediaLinks] ([MediaUsageTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_EnteredByUserId] ON [Expenses] ([EnteredByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_ExpenseCategoryId] ON [Expenses] ([ExpenseCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_ReceiptMediaId] ON [Expenses] ([ReceiptMediaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Incomes_EnteredByUserId] ON [Incomes] ([EnteredByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Incomes_IncomeSourceId] ON [Incomes] ([IncomeSourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Installments_InstallmentStatusId] ON [Installments] ([InstallmentStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Installments_PaymentPlanId] ON [Installments] ([PaymentPlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Inventories_ItemId] ON [Inventories] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_InvoiceStatusId] ON [Invoices] ([InvoiceStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_OrderId] ON [Invoices] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ItemMachineLinks_ItemId] ON [ItemMachineLinks] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ItemMachineLinks_MachineId] ON [ItemMachineLinks] ([MachineId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ItemMachineLinks_MachinePartId] ON [ItemMachineLinks] ([MachinePartId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Items_BrandId] ON [Items] ([BrandId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Items_CategoryId] ON [Items] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Items_ItemStatusId] ON [Items] ([ItemStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Items_Sku] ON [Items] ([Sku]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ItemSectionFeatures_ItemSectionId] ON [ItemSectionFeatures] ([ItemSectionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ItemSections_ItemId] ON [ItemSections] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ItemStatuses_Key] ON [ItemStatuses] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ItemTags_TagId] ON [ItemTags] ([TagId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MachineParts_MachineId] ON [MachineParts] ([MachineId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MediaAssets_MediaTypeId] ON [MediaAssets] ([MediaTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ItemId] ON [OrderItems] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_OrderStatusId] ON [Orders] ([OrderStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderStatuses_Key] ON [OrderStatuses] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionDrafts_CreatedByUserId] ON [PageSectionDrafts] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionDrafts_LayoutTypeId] ON [PageSectionDrafts] ([LayoutTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionDrafts_PageId] ON [PageSectionDrafts] ([PageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionDrafts_PageSectionTypeId] ON [PageSectionDrafts] ([PageSectionTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionDrafts_UpdatedByUserId] ON [PageSectionDrafts] ([UpdatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionItemDrafts_PageSectionDraftId] ON [PageSectionItemDrafts] ([PageSectionDraftId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionItemPublished_PageSectionPublishedId] ON [PageSectionItemPublished] ([PageSectionPublishedId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionPublished_LayoutTypeId] ON [PageSectionPublished] ([LayoutTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionPublished_PageId] ON [PageSectionPublished] ([PageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionPublished_PageSectionTypeId] ON [PageSectionPublished] ([PageSectionTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PageSectionPublished_PublishedByUserId] ON [PageSectionPublished] ([PublishedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentPlans_OrderId] ON [PaymentPlans] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_InvoiceId] ON [Payments] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_PaymentMethodId] ON [Payments] ([PaymentMethodId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Key] ON [Permissions] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Printers_PrintingTechnologyId] ON [Printers] ([PrintingTechnologyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrintJobs_PrinterId] ON [PrintJobs] ([PrinterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrintJobs_PrintJobStatusId] ON [PrintJobs] ([PrintJobStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrintJobs_SlicingJobId] ON [PrintJobs] ([SlicingJobId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PrintJobStatuses_Key] ON [PrintJobStatuses] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_ItemId] ON [Reviews] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_UserId] ON [Reviews] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SlicingJobs_DesignFileId] ON [SlicingJobs] ([DesignFileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SlicingJobs_SlicingJobStatusId] ON [SlicingJobs] ([SlicingJobStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SlicingJobs_SlicingProfileId] ON [SlicingJobs] ([SlicingProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SlicingJobStatuses_Key] ON [SlicingJobStatuses] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SlicingProfiles_MaterialId] ON [SlicingProfiles] ([MaterialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SlicingProfiles_PrinterId] ON [SlicingProfiles] ([PrinterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SlicingProfiles_PrintingTechnologyId] ON [SlicingProfiles] ([PrintingTechnologyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tags_Slug] ON [Tags] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130101657_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251130101657_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [AccessFailedCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [City] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [Country] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [EmailConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [LastLoginAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [LockoutEnabled] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [LockoutEnd] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [PasswordResetExpiresAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [PasswordResetToken] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [PhoneConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [PostalCode] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [ProfileImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [RefreshToken] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [RefreshTokenExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [Street] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Tags] ADD [Color] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Tags] ADD [Icon] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Tags] ADD [UsageCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [SlicingProfiles] ADD [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [SlicingProfiles] ADD [TemperatureSettings] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [SlicingJobs] ADD [ErrorMessage] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [SlicingJobs] ADD [EstimatedCost] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [SlicingJobs] ADD [OutputFileUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Roles] ADD [IsSystemRole] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Roles] ADD [Priority] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PrintJobs] ADD [ErrorMessage] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PrintJobs] ADD [EstimatedCompletionTime] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PrintJobs] ADD [ProgressPercent] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Printers] ADD [CurrentStatus] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Printers] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Printers] ADD [Location] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Permissions] ADD [Category] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Permissions] ADD [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Payments] ADD [GatewayResponse] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Payments] ADD [IsRefunded] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Payments] ADD [RefundedAmount] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Payments] ADD [RefundedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Payments] ADD [TransactionId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PaymentPlans] ADD [RemainingAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PaymentPlans] ADD [TotalAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PageSectionPublished] ADD [UnpublishedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PageSectionPublished] ADD [UnpublishedByUserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PageSectionPublished] ADD [Version] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PageSectionDrafts] ADD [PreviewUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD [IsPublished] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD [PageTemplateId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD [PublishedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD [PublishedByUserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD [TemplateKey] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD [Version] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [BillingAddress] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [EstimatedDeliveryDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [Notes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [OrderNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [ShippingAddress] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [ShippingCost] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [ShippingMethodId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [SubTotal] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [TaxAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Orders] ADD [TrackingNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [OrderItems] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [OrderItems] ADD [TaxAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MediaAssets] ADD [AltTextAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MediaAssets] ADD [AltTextEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MediaAssets] ADD [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MediaAssets] ADD [StorageKey] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MediaAssets] ADD [StorageProvider] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MediaAssets] ADD [ThumbnailUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Materials] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Materials] ADD [StockQuantity] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Machines] ADD [ImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Machines] ADD [Location] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Machines] ADD [ManualId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Machines] ADD [PurchaseDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Machines] ADD [PurchasePrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Machines] ADD [WarrantyMonths] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MachineParts] ADD [ImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MachineParts] ADD [InventoryId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [MachineParts] ADD [Price] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Items]') AND [c].[name] = N'AverageRating');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Items] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Items] ALTER COLUMN [AverageRating] decimal(18,2) NOT NULL;
    ALTER TABLE [Items] ADD DEFAULT 0.0 FOR [AverageRating];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [Dimensions] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [DiscountPrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [IsFeatured] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [IsNew] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [IsOnSale] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [MaxOrderQuantity] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [MetaDescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [MetaDescriptionEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [MetaTitleAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [MetaTitleEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [MinOrderQuantity] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [TaxRate] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [WarrantyPeriodMonths] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Items] ADD [Weight] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Inventories] ADD [CostPerUnit] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Inventories] ADD [LastStockOutAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Inventories] ADD [QuantityOnOrder] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Inventories] ADD [QuantityReserved] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Inventories] ADD [WarehouseId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Installments] ADD [PaymentId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Designs] ADD [DownloadCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Designs] ADD [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Designs] ADD [LicenseType] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Designs] ADD [LikeCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Designs] ADD [Tags] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [DesignFiles] ADD [FileSizeBytes] bigint NOT NULL DEFAULT CAST(0 AS bigint);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [DesignFiles] ADD [IsPrimary] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Categories] ADD [ImageId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Categories] ADD [MetaDescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Categories] ADD [MetaDescriptionEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Categories] ADD [MetaTitleAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Categories] ADD [MetaTitleEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Brands] ADD [Country] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Brands] ADD [DescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Brands] ADD [DescriptionEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Brands] ADD [LogoId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Brands] ADD [WebsiteUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiSessions] ADD [EndedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiSessions] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiSessions] ADD [Title] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiMessages] ADD [EditedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiMessages] ADD [IsEdited] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiMessages] ADD [Model] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiMessages] ADD [ResponseTimeMs] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [AiMessages] ADD [TokensUsed] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [AiChatConfigs] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AiChatConfigs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] uniqueidentifier NULL,
        [Action] nvarchar(50) NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [UserId] uniqueidentifier NULL,
        [UserEmail] nvarchar(256) NULL,
        [IpAddress] nvarchar(45) NULL,
        [UserAgent] nvarchar(500) NULL,
        [RequestPath] nvarchar(500) NULL,
        [RequestMethod] nvarchar(10) NULL,
        [StatusCode] int NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [DurationMs] bigint NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [EmailTemplates] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [SubjectEn] nvarchar(500) NOT NULL,
        [SubjectAr] nvarchar(500) NOT NULL,
        [BodyHtmlEn] nvarchar(max) NOT NULL,
        [BodyHtmlAr] nvarchar(max) NOT NULL,
        [BodyTextEn] nvarchar(max) NULL,
        [BodyTextAr] nvarchar(max) NULL,
        [Variables] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EmailTemplates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [InventoryTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [InventoryId] uniqueidentifier NOT NULL,
        [TransactionType] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [CostPerUnit] decimal(18,2) NULL,
        [Reason] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [OrderId] uniqueidentifier NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryTransactions_Inventories_InventoryId] FOREIGN KEY ([InventoryId]) REFERENCES [Inventories] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [Type] nvarchar(50) NOT NULL,
        [TitleEn] nvarchar(200) NOT NULL,
        [TitleAr] nvarchar(200) NOT NULL,
        [MessageEn] nvarchar(2000) NOT NULL,
        [MessageAr] nvarchar(2000) NOT NULL,
        [LinkUrl] nvarchar(500) NULL,
        [Icon] nvarchar(100) NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [RelatedEntityId] uniqueidentifier NULL,
        [RelatedEntityType] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [PageTemplates] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [DescriptionEn] nvarchar(max) NULL,
        [DescriptionAr] nvarchar(max) NULL,
        [TemplateJson] nvarchar(max) NULL,
        [PreviewImageUrl] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PageTemplates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [PageVersions] (
        [Id] uniqueidentifier NOT NULL,
        [PageId] uniqueidentifier NOT NULL,
        [VersionNumber] int NOT NULL,
        [TitleEn] nvarchar(max) NULL,
        [TitleAr] nvarchar(max) NULL,
        [MetaTitleEn] nvarchar(max) NULL,
        [MetaTitleAr] nvarchar(max) NULL,
        [MetaDescriptionEn] nvarchar(max) NULL,
        [MetaDescriptionAr] nvarchar(max) NULL,
        [ContentJson] nvarchar(max) NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [RestoredAt] datetime2 NULL,
        [RestoredByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PageVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PageVersions_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Pages] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PageVersions_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]),
        CONSTRAINT [FK_PageVersions_Users_RestoredByUserId] FOREIGN KEY ([RestoredByUserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [DescriptionEn] nvarchar(500) NULL,
        [DescriptionAr] nvarchar(500) NULL,
        [Category] nvarchar(50) NULL,
        [DataType] nvarchar(20) NULL DEFAULT N'String',
        [IsPublic] bit NOT NULL,
        [IsEncrypted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Users_ProfileImageId] ON [Users] ([ProfileImageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_PageSectionPublished_UnpublishedByUserId] ON [PageSectionPublished] ([UnpublishedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Pages_PageTemplateId] ON [Pages] ([PageTemplateId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Pages_PublishedByUserId] ON [Pages] ([PublishedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Installments_PaymentId] ON [Installments] ([PaymentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [AuditLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityId] ON [AuditLogs] ([EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType] ON [AuditLogs] ([EntityType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_EmailTemplates_IsActive] ON [EmailTemplates] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmailTemplates_Key] ON [EmailTemplates] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_InventoryId] ON [InventoryTransactions] ([InventoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Notifications_RelatedEntityType_RelatedEntityId] ON [Notifications] ([RelatedEntityType], [RelatedEntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_PageVersions_CreatedByUserId] ON [PageVersions] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_PageVersions_PageId] ON [PageVersions] ([PageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_PageVersions_RestoredByUserId] ON [PageVersions] ([RestoredByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE INDEX [IX_SystemSettings_Category] ON [SystemSettings] ([Category]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SystemSettings_Key] ON [SystemSettings] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Installments] ADD CONSTRAINT [FK_Installments_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD CONSTRAINT [FK_Pages_PageTemplates_PageTemplateId] FOREIGN KEY ([PageTemplateId]) REFERENCES [PageTemplates] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Pages] ADD CONSTRAINT [FK_Pages_Users_PublishedByUserId] FOREIGN KEY ([PublishedByUserId]) REFERENCES [Users] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [PageSectionPublished] ADD CONSTRAINT [FK_PageSectionPublished_Users_UnpublishedByUserId] FOREIGN KEY ([UnpublishedByUserId]) REFERENCES [Users] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_MediaAssets_ProfileImageId] FOREIGN KEY ([ProfileImageId]) REFERENCES [MediaAssets] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201152815_CompleteBackendImplementation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251201152815_CompleteBackendImplementation', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260127134057_AddPrintQualityProfile'
)
BEGIN
    CREATE TABLE [PrintQualityProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [DescriptionEn] nvarchar(max) NULL,
        [DescriptionAr] nvarchar(max) NULL,
        [LayerHeightMm] decimal(18,2) NOT NULL,
        [SpeedCategory] nvarchar(max) NOT NULL,
        [PriceMultiplier] decimal(18,2) NOT NULL,
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PrintQualityProfiles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260127134057_AddPrintQualityProfile'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260127134057_AddPrintQualityProfile', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128171834_AddCartEntities'
)
BEGIN
    CREATE TABLE [Carts] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [SessionId] nvarchar(max) NULL,
        [CouponCode] nvarchar(max) NULL,
        [CouponDiscount] decimal(18,2) NOT NULL,
        [ExpiresAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Carts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128171834_AddCartEntities'
)
BEGIN
    CREATE TABLE [CartItems] (
        [Id] uniqueidentifier NOT NULL,
        [CartId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128171834_AddCartEntities'
)
BEGIN
    CREATE INDEX [IX_CartItems_CartId] ON [CartItems] ([CartId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128171834_AddCartEntities'
)
BEGIN
    CREATE INDEX [IX_CartItems_ItemId] ON [CartItems] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128171834_AddCartEntities'
)
BEGIN
    CREATE INDEX [IX_Carts_UserId] ON [Carts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260128171834_AddCartEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260128171834_AddCartEntities', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120000_AddSupportChat'
)
BEGIN
    CREATE TABLE [SupportConversations] (
        [Id] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [AssignedToUserId] uniqueidentifier NULL,
        [Status] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SupportConversations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportConversations_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]),
        CONSTRAINT [FK_SupportConversations_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120000_AddSupportChat'
)
BEGIN
    CREATE TABLE [SupportMessages] (
        [Id] uniqueidentifier NOT NULL,
        [ConversationId] uniqueidentifier NOT NULL,
        [SenderUserId] uniqueidentifier NOT NULL,
        [Content] nvarchar(4000) NOT NULL,
        [ReadAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SupportMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportMessages_SupportConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [SupportConversations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SupportMessages_Users_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120000_AddSupportChat'
)
BEGIN
    CREATE INDEX [IX_SupportConversations_AssignedToUserId] ON [SupportConversations] ([AssignedToUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120000_AddSupportChat'
)
BEGIN
    CREATE INDEX [IX_SupportConversations_CustomerId] ON [SupportConversations] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120000_AddSupportChat'
)
BEGIN
    CREATE INDEX [IX_SupportMessages_ConversationId] ON [SupportMessages] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120000_AddSupportChat'
)
BEGIN
    CREATE INDEX [IX_SupportMessages_SenderUserId] ON [SupportMessages] ([SenderUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120000_AddSupportChat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260202120000_AddSupportChat', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202150000_AddSupportMessageAttachments'
)
BEGIN
    ALTER TABLE [SupportMessages] ADD [AttachmentFileName] nvarchar(512) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202150000_AddSupportMessageAttachments'
)
BEGIN
    ALTER TABLE [SupportMessages] ADD [AttachmentUrl] nvarchar(2048) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202150000_AddSupportMessageAttachments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260202150000_AddSupportMessageAttachments', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010049_AddLaserMaterialsTable'
)
BEGIN
    ALTER TABLE [SupportConversations] DROP CONSTRAINT [FK_SupportConversations_Users_AssignedToUserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010049_AddLaserMaterialsTable'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'Currency');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Orders] ADD DEFAULT N'ILS' FOR [Currency];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010049_AddLaserMaterialsTable'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Items]') AND [c].[name] = N'Currency');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Items] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Items] ADD DEFAULT N'ILS' FOR [Currency];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010049_AddLaserMaterialsTable'
)
BEGIN
    CREATE TABLE [LaserMaterials] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [MinThicknessMm] decimal(18,2) NULL,
        [MaxThicknessMm] decimal(18,2) NULL,
        [NotesEn] nvarchar(max) NULL,
        [NotesAr] nvarchar(max) NULL,
        [IsMetal] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LaserMaterials] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010049_AddLaserMaterialsTable'
)
BEGIN
    ALTER TABLE [SupportConversations] ADD CONSTRAINT [FK_SupportConversations_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010049_AddLaserMaterialsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260221010049_AddLaserMaterialsTable', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010914_AddLaserServiceRequests'
)
BEGIN
    CREATE TABLE [LaserServiceRequests] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenceNumber] nvarchar(50) NOT NULL,
        [MaterialId] uniqueidentifier NOT NULL,
        [OperationMode] nvarchar(20) NOT NULL,
        [ImagePath] nvarchar(500) NOT NULL,
        [ImageFileName] nvarchar(255) NOT NULL,
        [CustomerName] nvarchar(200) NULL,
        [CustomerEmail] nvarchar(255) NULL,
        [CustomerPhone] nvarchar(50) NULL,
        [CustomerNotes] nvarchar(2000) NULL,
        [AdminNotes] nvarchar(2000) NULL,
        [QuotedPrice] decimal(18,2) NULL,
        [Status] int NOT NULL,
        [ReviewedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LaserServiceRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LaserServiceRequests_LaserMaterials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [LaserMaterials] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010914_AddLaserServiceRequests'
)
BEGIN
    CREATE INDEX [IX_LaserServiceRequests_CreatedAt] ON [LaserServiceRequests] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010914_AddLaserServiceRequests'
)
BEGIN
    CREATE INDEX [IX_LaserServiceRequests_MaterialId] ON [LaserServiceRequests] ([MaterialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010914_AddLaserServiceRequests'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LaserServiceRequests_ReferenceNumber] ON [LaserServiceRequests] ([ReferenceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010914_AddLaserServiceRequests'
)
BEGIN
    CREATE INDEX [IX_LaserServiceRequests_Status] ON [LaserServiceRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221010914_AddLaserServiceRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260221010914_AddLaserServiceRequests', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221014252_AddLaserRequestDimensions'
)
BEGIN
    ALTER TABLE [LaserServiceRequests] ADD [HeightCm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221014252_AddLaserRequestDimensions'
)
BEGIN
    ALTER TABLE [LaserServiceRequests] ADD [WidthCm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221014252_AddLaserRequestDimensions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260221014252_AddLaserRequestDimensions', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221024202_AddPrint3dServiceRequest'
)
BEGIN
    CREATE TABLE [Print3dServiceRequests] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenceNumber] nvarchar(max) NOT NULL,
        [UserId] uniqueidentifier NULL,
        [MaterialId] uniqueidentifier NOT NULL,
        [ProfileId] uniqueidentifier NULL,
        [DesignId] uniqueidentifier NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [CustomerName] nvarchar(max) NULL,
        [CustomerEmail] nvarchar(max) NULL,
        [CustomerPhone] nvarchar(max) NULL,
        [CustomerNotes] nvarchar(max) NULL,
        [AdminNotes] nvarchar(max) NULL,
        [EstimatedPrice] decimal(18,2) NULL,
        [FinalPrice] decimal(18,2) NULL,
        [Currency] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [ReviewedAt] datetime2 NULL,
        [ApprovedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Print3dServiceRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Print3dServiceRequests_Designs_DesignId] FOREIGN KEY ([DesignId]) REFERENCES [Designs] ([Id]),
        CONSTRAINT [FK_Print3dServiceRequests_Materials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [Materials] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Print3dServiceRequests_PrintQualityProfiles_ProfileId] FOREIGN KEY ([ProfileId]) REFERENCES [PrintQualityProfiles] ([Id]),
        CONSTRAINT [FK_Print3dServiceRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221024202_AddPrint3dServiceRequest'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_DesignId] ON [Print3dServiceRequests] ([DesignId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221024202_AddPrint3dServiceRequest'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_MaterialId] ON [Print3dServiceRequests] ([MaterialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221024202_AddPrint3dServiceRequest'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_ProfileId] ON [Print3dServiceRequests] ([ProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221024202_AddPrint3dServiceRequest'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_UserId] ON [Print3dServiceRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221024202_AddPrint3dServiceRequest'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260221024202_AddPrint3dServiceRequest', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221092636_AddSoftwareModule'
)
BEGIN
    CREATE TABLE [SoftwareProducts] (
        [Id] uniqueidentifier NOT NULL,
        [Slug] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [SummaryEn] nvarchar(500) NOT NULL,
        [SummaryAr] nvarchar(500) NOT NULL,
        [DescriptionEn] nvarchar(max) NULL,
        [DescriptionAr] nvarchar(max) NULL,
        [Category] nvarchar(100) NULL,
        [IconKey] nvarchar(100) NULL,
        [LicenseEn] nvarchar(max) NULL,
        [LicenseAr] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SoftwareProducts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221092636_AddSoftwareModule'
)
BEGIN
    CREATE TABLE [SoftwareReleases] (
        [Id] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Version] nvarchar(50) NOT NULL,
        [ReleaseDate] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ChangelogEn] nvarchar(max) NULL,
        [ChangelogAr] nvarchar(max) NULL,
        [RequirementsEn] nvarchar(max) NULL,
        [RequirementsAr] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SoftwareReleases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SoftwareReleases_SoftwareProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [SoftwareProducts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221092636_AddSoftwareModule'
)
BEGIN
    CREATE TABLE [SoftwareFiles] (
        [Id] uniqueidentifier NOT NULL,
        [ReleaseId] uniqueidentifier NOT NULL,
        [Os] nvarchar(20) NOT NULL,
        [Arch] nvarchar(20) NOT NULL,
        [FileType] nvarchar(20) NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [StoredPath] nvarchar(500) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [Sha256] nvarchar(64) NULL,
        [DownloadCount] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SoftwareFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SoftwareFiles_SoftwareReleases_ReleaseId] FOREIGN KEY ([ReleaseId]) REFERENCES [SoftwareReleases] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221092636_AddSoftwareModule'
)
BEGIN
    CREATE INDEX [IX_SoftwareFiles_ReleaseId] ON [SoftwareFiles] ([ReleaseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221092636_AddSoftwareModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SoftwareProducts_Slug] ON [SoftwareProducts] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221092636_AddSoftwareModule'
)
BEGIN
    CREATE INDEX [IX_SoftwareReleases_ProductId] ON [SoftwareReleases] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260221092636_AddSoftwareModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260221092636_AddSoftwareModule', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222183927_AddMaterialColors'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [MaterialColorId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222183927_AddMaterialColors'
)
BEGIN
    CREATE TABLE [MaterialColors] (
        [Id] uniqueidentifier NOT NULL,
        [MaterialId] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [NameAr] nvarchar(100) NOT NULL,
        [HexCode] nvarchar(7) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SortOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_MaterialColors] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MaterialColors_Materials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [Materials] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222183927_AddMaterialColors'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_MaterialColorId] ON [Print3dServiceRequests] ([MaterialColorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222183927_AddMaterialColors'
)
BEGIN
    CREATE INDEX [IX_MaterialColors_MaterialId_SortOrder] ON [MaterialColors] ([MaterialId], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222183927_AddMaterialColors'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD CONSTRAINT [FK_Print3dServiceRequests_MaterialColors_MaterialColorId] FOREIGN KEY ([MaterialColorId]) REFERENCES [MaterialColors] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222183927_AddMaterialColors'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260222183927_AddMaterialColors', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE TABLE [Projects] (
        [Id] uniqueidentifier NOT NULL,
        [Slug] nvarchar(200) NOT NULL,
        [TitleEn] nvarchar(300) NOT NULL,
        [TitleAr] nvarchar(300) NOT NULL,
        [SummaryEn] nvarchar(500) NOT NULL,
        [SummaryAr] nvarchar(500) NOT NULL,
        [DescriptionEn] nvarchar(max) NOT NULL,
        [DescriptionAr] nvarchar(max) NOT NULL,
        [Category] int NOT NULL,
        [Status] int NOT NULL,
        [Year] int NOT NULL,
        [CoverImageUrl] nvarchar(500) NOT NULL,
        [TechStackJson] nvarchar(max) NOT NULL,
        [HighlightsJson] nvarchar(max) NOT NULL,
        [GalleryJson] nvarchar(max) NOT NULL,
        [IsFeatured] bit NOT NULL DEFAULT CAST(0 AS bit),
        [SortOrder] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Projects] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE TABLE [ProjectRequests] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenceNumber] nvarchar(50) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NULL,
        [RequestType] int NOT NULL,
        [ProjectId] uniqueidentifier NULL,
        [Category] int NULL,
        [BudgetRange] nvarchar(100) NULL,
        [Timeline] nvarchar(100) NULL,
        [Description] nvarchar(max) NOT NULL,
        [AttachmentUrl] nvarchar(500) NULL,
        [Status] int NOT NULL DEFAULT 0,
        [AdminNotes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProjectRequests_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE INDEX [IX_ProjectRequests_CreatedAt] ON [ProjectRequests] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE INDEX [IX_ProjectRequests_ProjectId] ON [ProjectRequests] ([ProjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProjectRequests_ReferenceNumber] ON [ProjectRequests] ([ReferenceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE INDEX [IX_ProjectRequests_Status] ON [ProjectRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE INDEX [IX_Projects_Category] ON [Projects] ([Category]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE INDEX [IX_Projects_IsActive_SortOrder] ON [Projects] ([IsActive], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Projects_Slug] ON [Projects] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    CREATE INDEX [IX_Projects_Status] ON [Projects] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222224434_AddProjectsModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260222224434_AddProjectsModule', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222233534_AddCncEntities'
)
BEGIN
    CREATE TABLE [CncMaterials] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(max) NOT NULL,
        [NameAr] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [MinThicknessMm] decimal(18,2) NULL,
        [MaxThicknessMm] decimal(18,2) NULL,
        [NotesEn] nvarchar(max) NULL,
        [NotesAr] nvarchar(max) NULL,
        [IsMetal] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CncMaterials] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222233534_AddCncEntities'
)
BEGIN
    CREATE TABLE [CncServiceRequests] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenceNumber] nvarchar(max) NOT NULL,
        [ServiceMode] nvarchar(max) NOT NULL,
        [MaterialId] uniqueidentifier NOT NULL,
        [WidthCm] decimal(18,2) NOT NULL,
        [HeightCm] decimal(18,2) NOT NULL,
        [ThicknessMm] decimal(18,2) NULL,
        [Quantity] int NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [CustomerName] nvarchar(max) NULL,
        [CustomerEmail] nvarchar(max) NULL,
        [CustomerPhone] nvarchar(max) NULL,
        [CustomerNotes] nvarchar(max) NULL,
        [AdminNotes] nvarchar(max) NULL,
        [QuotedPrice] decimal(18,2) NULL,
        [Status] int NOT NULL,
        [ReviewedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CncServiceRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CncServiceRequests_CncMaterials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [CncMaterials] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222233534_AddCncEntities'
)
BEGIN
    CREATE INDEX [IX_CncServiceRequests_MaterialId] ON [CncServiceRequests] ([MaterialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222233534_AddCncEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260222233534_AddCncEntities', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'CustomerEmail');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [CustomerEmail];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'CustomerName');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [CustomerName];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[WidthCm]', N'WidthMm', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[HeightCm]', N'HeightMm', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[CustomerPhone]', N'Phone', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[CustomerNotes]', N'Description', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [Email] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [FullName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [Operation] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [PcbSide] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223001857_UpdateCncServiceRequestFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260223001857_UpdateCncServiceRequestFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223011540_AddCncInputTypeAndDesignNotes'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [DesignNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223011540_AddCncInputTypeAndDesignNotes'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [EngraveType] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223011540_AddCncInputTypeAndDesignNotes'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [HoleCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223011540_AddCncInputTypeAndDesignNotes'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [HoleDiameterMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223011540_AddCncInputTypeAndDesignNotes'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [InputType] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223011540_AddCncInputTypeAndDesignNotes'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [PocketDepthMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223011540_AddCncInputTypeAndDesignNotes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260223011540_AddCncInputTypeAndDesignNotes', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [FK_CncServiceRequests_CncMaterials_MaterialId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [ProjectRequests] DROP CONSTRAINT [FK_ProjectRequests_Projects_ProjectId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DROP INDEX [IX_Projects_Category] ON [Projects];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DROP INDEX [IX_Projects_IsActive_SortOrder] ON [Projects];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DROP INDEX [IX_Projects_Slug] ON [Projects];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DROP INDEX [IX_Projects_Status] ON [Projects];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DROP INDEX [IX_ProjectRequests_CreatedAt] ON [ProjectRequests];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DROP INDEX [IX_ProjectRequests_ReferenceNumber] ON [ProjectRequests];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DROP INDEX [IX_ProjectRequests_Status] ON [ProjectRequests];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'AttachmentUrl');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [AttachmentUrl];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'BudgetRange');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [BudgetRange];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'Category');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [Category];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'Email');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [Email];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'FullName');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [FullName];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'Phone');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [Phone];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'ReferenceNumber');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [ReferenceNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'RequestType');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [RequestType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'Timeline');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [ProjectRequests] DROP COLUMN [Timeline];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'AdminNotes');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [AdminNotes];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'CompletedAt');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [CompletedAt];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'Email');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [Email];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'FileName');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [FileName];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'FilePath');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [FilePath];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'FullName');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [FullName];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'HeightMm');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [HeightMm];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'HoleCount');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [HoleCount];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'HoleDiameterMm');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [HoleDiameterMm];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'InputType');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [InputType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'Operation');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [Operation];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'PcbSide');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [PcbSide];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'PocketDepthMm');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [PocketDepthMm];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'Quantity');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [Quantity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'QuotedPrice');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [QuotedPrice];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'ReferenceNumber');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [ReferenceNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'ReviewedAt');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [ReviewedAt];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'ThicknessMm');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [ThicknessMm];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'WidthMm');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [CncServiceRequests] DROP COLUMN [WidthMm];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    EXEC sp_rename N'[ProjectRequests].[AdminNotes]', N'Notes', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[ServiceMode]', N'CustomerName', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[Phone]', N'Notes', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[EngraveType]', N'CustomerPhone', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[DesignNotes]', N'CustomerEmail', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'TitleEn');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [TitleEn] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'TitleAr');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [TitleAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'TechStackJson');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [TechStackJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'SummaryEn');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [SummaryEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'SummaryAr');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [SummaryAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'SortOrder');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var38 + '];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'Slug');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [Slug] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'IsFeatured');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var40 + '];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'IsActive');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var41 + '];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'HighlightsJson');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [HighlightsJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'GalleryJson');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [GalleryJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'DescriptionEn');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [DescriptionEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'DescriptionAr');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [DescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'CoverImageUrl');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [Projects] ALTER COLUMN [CoverImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [Projects] ADD [ImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var47 sysname;
    SELECT @var47 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'Status');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var47 + '];');
    ALTER TABLE [ProjectRequests] ALTER COLUMN [Status] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var48 sysname;
    SELECT @var48 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'Description');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var48 + '];');
    ALTER TABLE [ProjectRequests] ALTER COLUMN [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [CustomerEmail] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [CustomerName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [CustomerPhone] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var49 sysname;
    SELECT @var49 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'Status');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var49 + '];');
    ALTER TABLE [CncServiceRequests] ALTER COLUMN [Status] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var50 sysname;
    SELECT @var50 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'MaterialId');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var50 + '];');
    ALTER TABLE [CncServiceRequests] ALTER COLUMN [MaterialId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var51 sysname;
    SELECT @var51 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncMaterials]') AND [c].[name] = N'Type');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [CncMaterials] DROP CONSTRAINT [' + @var51 + '];');
    ALTER TABLE [CncMaterials] ALTER COLUMN [Type] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    DECLARE @var52 sysname;
    SELECT @var52 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncMaterials]') AND [c].[name] = N'NameAr');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [CncMaterials] DROP CONSTRAINT [' + @var52 + '];');
    ALTER TABLE [CncMaterials] ALTER COLUMN [NameAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [AllowCut] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [AllowDrill] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [AllowEngrave] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [AllowPocket] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [CutNotesAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [CutNotesEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [DescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [DescriptionEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [DrillNotesAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [DrillNotesEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [EngraveNotesAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [EngraveNotesEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [IsPcbOnly] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [MaxCutDepthMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [MaxDrillDepthMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [MaxEngraveDepthMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [MaxPocketDepthMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [PocketNotesAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [PocketNotesEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD CONSTRAINT [FK_CncServiceRequests_CncMaterials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [CncMaterials] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD CONSTRAINT [FK_ProjectRequests_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224010458_AddCncMaterialRulesEngine'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260224010458_AddCncMaterialRulesEngine', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[Notes]', N'ProjectDescription', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    EXEC sp_rename N'[CncServiceRequests].[Description]', N'PcbSide', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    DECLARE @var53 sysname;
    SELECT @var53 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'Status');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var53 + '];');
    ALTER TABLE [CncServiceRequests] ALTER COLUMN [Status] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    DECLARE @var54 sysname;
    SELECT @var54 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CncServiceRequests]') AND [c].[name] = N'CustomerEmail');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [CncServiceRequests] DROP CONSTRAINT [' + @var54 + '];');
    EXEC(N'UPDATE [CncServiceRequests] SET [CustomerEmail] = N'''' WHERE [CustomerEmail] IS NULL');
    ALTER TABLE [CncServiceRequests] ALTER COLUMN [CustomerEmail] nvarchar(max) NOT NULL;
    ALTER TABLE [CncServiceRequests] ADD DEFAULT N'' FOR [CustomerEmail];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [AdminNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [CompletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [DepthMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [DepthMode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [DesignNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [DesignSourceType] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [EstimatedPrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [FileName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [FilePath] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [FinalPrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [HeightMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [OperationType] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [PcbMaterial] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [PcbOperation] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [PcbThickness] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [Quantity] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [ReferenceNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [ReviewedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [ServiceMode] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [ThicknessMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [WidthMm] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    CREATE TABLE [ContactMessages] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NULL,
        [Subject] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [AdminNotes] nvarchar(max) NULL,
        [ReadAt] datetime2 NULL,
        [RepliedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224173824_AddContactMessages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260224173824_AddContactMessages', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224174234_AddCoupons'
)
BEGIN
    CREATE TABLE [Coupons] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [DescriptionEn] nvarchar(max) NULL,
        [DescriptionAr] nvarchar(max) NULL,
        [Type] int NOT NULL,
        [Value] decimal(18,2) NOT NULL,
        [MinOrderAmount] decimal(18,2) NULL,
        [MaxDiscountAmount] decimal(18,2) NULL,
        [UsageLimit] int NULL,
        [UsageCount] int NOT NULL,
        [UsagePerCustomer] int NULL,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CategoryId] uniqueidentifier NULL,
        [AppliesToAllCategories] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Coupons] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224174234_AddCoupons'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260224174234_AddCoupons', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224174635_AddCouponsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260224174635_AddCouponsTable', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    DROP TABLE [ContactMessages];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    DROP TABLE [Coupons];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    EXEC sp_rename N'[ProjectRequests].[Notes]', N'Timeline', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    EXEC sp_rename N'[ProjectRequests].[CustomerPhone]', N'Phone', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    EXEC sp_rename N'[ProjectRequests].[CustomerName]', N'ReferenceNumber', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    EXEC sp_rename N'[ProjectRequests].[CustomerEmail]', N'BudgetRange', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    DECLARE @var55 sysname;
    SELECT @var55 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectRequests]') AND [c].[name] = N'Status');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [ProjectRequests] DROP CONSTRAINT [' + @var55 + '];');
    ALTER TABLE [ProjectRequests] ALTER COLUMN [Status] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [AdminNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [AttachmentFileName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [AttachmentUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [Category] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [Email] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [FullName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [RequestType] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224194108_AddProjectRequestFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260224194108_AddProjectRequestFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302203249_AddFaqItems'
)
BEGIN
    CREATE TABLE [FaqItems] (
        [Id] uniqueidentifier NOT NULL,
        [Category] int NOT NULL,
        [QuestionEn] nvarchar(1000) NOT NULL,
        [QuestionAr] nvarchar(1000) NULL,
        [AnswerEn] nvarchar(max) NOT NULL,
        [AnswerAr] nvarchar(max) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsFeatured] bit NOT NULL DEFAULT CAST(0 AS bit),
        [SortOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_FaqItems] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302203249_AddFaqItems'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260302203249_AddFaqItems', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260304162058_AddHeroTickerItems'
)
BEGIN
    CREATE TABLE [HeroTickerItems] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(500) NULL,
        [ImageUrl] nvarchar(2000) NOT NULL,
        [SortOrder] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_HeroTickerItems] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260304162058_AddHeroTickerItems'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260304162058_AddHeroTickerItems', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE TABLE [DesignCadServiceRequests] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenceNumber] nvarchar(50) NOT NULL,
        [UserId] uniqueidentifier NULL,
        [RequestType] int NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [TargetProcess] int NULL,
        [IntendedUse] nvarchar(200) NULL,
        [MaterialNotes] nvarchar(500) NULL,
        [DimensionsNotes] nvarchar(1000) NULL,
        [ToleranceNotes] nvarchar(1000) NULL,
        [WhatNeedsChange] nvarchar(2000) NULL,
        [CriticalSurfaces] nvarchar(1000) NULL,
        [FitmentRequirements] nvarchar(1000) NULL,
        [PurposeAndConstraints] nvarchar(2000) NULL,
        [Deadline] nvarchar(100) NULL,
        [HasPhysicalPart] bit NOT NULL,
        [LegalConfirmation] bit NOT NULL,
        [CanDeliverPhysicalPart] bit NOT NULL,
        [CustomerNotes] nvarchar(2000) NULL,
        [AdminNotes] nvarchar(4000) NULL,
        [QuotedPrice] decimal(18,2) NULL,
        [FinalPrice] decimal(18,2) NULL,
        [Status] int NOT NULL,
        [ReviewedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CustomerName] nvarchar(200) NULL,
        [CustomerEmail] nvarchar(255) NULL,
        [CustomerPhone] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DesignCadServiceRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DesignCadServiceRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE TABLE [DesignServiceRequests] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenceNumber] nvarchar(50) NOT NULL,
        [UserId] uniqueidentifier NULL,
        [RequestType] int NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [IntendedUse] nvarchar(100) NULL,
        [MaterialPreference] nvarchar(500) NULL,
        [DimensionsNotes] nvarchar(1000) NULL,
        [ToleranceLevel] int NULL,
        [BudgetRange] nvarchar(100) NULL,
        [Timeline] nvarchar(100) NULL,
        [IpOwnershipConfirmed] bit NOT NULL,
        [Status] int NOT NULL,
        [CustomerName] nvarchar(200) NULL,
        [CustomerEmail] nvarchar(255) NULL,
        [CustomerPhone] nvarchar(50) NULL,
        [AdminNotes] nvarchar(4000) NULL,
        [QuotedPrice] decimal(18,2) NULL,
        [FinalPrice] decimal(18,2) NULL,
        [DeliveryNotes] nvarchar(2000) NULL,
        [ReviewedAt] datetime2 NULL,
        [QuotedAt] datetime2 NULL,
        [DeliveredAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DesignServiceRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DesignServiceRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE TABLE [DesignCadServiceRequestAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DesignCadServiceRequestAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DesignCadServiceRequestAttachments_DesignCadServiceRequests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [DesignCadServiceRequests] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE TABLE [DesignServiceRequestAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DesignServiceRequestAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DesignServiceRequestAttachments_DesignServiceRequests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [DesignServiceRequests] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignCadServiceRequestAttachments_RequestId] ON [DesignCadServiceRequestAttachments] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignCadServiceRequests_CreatedAt] ON [DesignCadServiceRequests] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DesignCadServiceRequests_ReferenceNumber] ON [DesignCadServiceRequests] ([ReferenceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignCadServiceRequests_RequestType] ON [DesignCadServiceRequests] ([RequestType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignCadServiceRequests_Status] ON [DesignCadServiceRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignCadServiceRequests_UserId] ON [DesignCadServiceRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignServiceRequestAttachments_RequestId] ON [DesignServiceRequestAttachments] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignServiceRequests_CreatedAt] ON [DesignServiceRequests] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DesignServiceRequests_ReferenceNumber] ON [DesignServiceRequests] ([ReferenceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignServiceRequests_RequestType] ON [DesignServiceRequests] ([RequestType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignServiceRequests_Status] ON [DesignServiceRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    CREATE INDEX [IX_DesignServiceRequests_UserId] ON [DesignServiceRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306002452_AddDesignCadServiceRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260306002452_AddDesignCadServiceRequests', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE TABLE [CcAuditEvents] (
        [Id] uniqueidentifier NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [OrgId] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NULL,
        [InstanceId] uniqueidentifier NULL,
        [DeviceId] uniqueidentifier NULL,
        [Action] nvarchar(200) NOT NULL,
        [MetadataJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CcAuditEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE TABLE [CcCommands] (
        [Id] uniqueidentifier NOT NULL,
        [OrgId] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NOT NULL,
        [TargetType] nvarchar(50) NOT NULL,
        [TargetId] uniqueidentifier NOT NULL,
        [CommandType] nvarchar(100) NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [QueuedAt] datetime2 NOT NULL,
        [AcknowledgedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [ResultPayloadJson] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CcCommands] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE TABLE [CcDevices] (
        [Id] uniqueidentifier NOT NULL,
        [OrgId] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [SerialNumber] nvarchar(200) NOT NULL,
        [Type] nvarchar(100) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [FirmwareVersion] nvarchar(max) NULL,
        [ModuleId] uniqueidentifier NULL,
        [Location] nvarchar(500) NULL,
        [TagsJson] nvarchar(max) NULL,
        [MetadataJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CcDevices] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE TABLE [CcJobs] (
        [Id] uniqueidentifier NOT NULL,
        [OrgId] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NOT NULL,
        [TemplateId] uniqueidentifier NOT NULL,
        [DeviceId] uniqueidentifier NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Progress] float NULL,
        [StartedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [ResultSummary] nvarchar(2000) NULL,
        [ParametersJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CcJobs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE TABLE [CcJobTemplates] (
        [Id] uniqueidentifier NOT NULL,
        [OrgId] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [DeviceType] nvarchar(100) NOT NULL,
        [ModuleId] uniqueidentifier NULL,
        [DefinitionJson] nvarchar(max) NOT NULL,
        [Version] nvarchar(50) NOT NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [UpdatedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CcJobTemplates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE TABLE [CcTelemetryRecords] (
        [Id] uniqueidentifier NOT NULL,
        [OrgId] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NULL,
        [Timestamp] datetime2 NOT NULL,
        [InstanceId] uniqueidentifier NULL,
        [DeviceId] uniqueidentifier NULL,
        [MetricType] nvarchar(200) NOT NULL,
        [Value] nvarchar(2000) NOT NULL,
        [Unit] nvarchar(50) NULL,
        [TagsJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CcTelemetryRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE TABLE [ControlCenterInstances] (
        [Id] uniqueidentifier NOT NULL,
        [OrgId] uniqueidentifier NOT NULL,
        [SiteId] uniqueidentifier NULL,
        [MachineId] uniqueidentifier NULL,
        [Hostname] nvarchar(200) NOT NULL,
        [OsInfo] nvarchar(1000) NULL,
        [CoreVersion] nvarchar(50) NOT NULL,
        [LastSeenAt] datetime2 NULL,
        [InstalledModulesJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ControlCenterInstances] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_CcAuditEvents_OrgId_SiteId_Timestamp] ON [CcAuditEvents] ([OrgId], [SiteId], [Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_CcCommands_OrgId_SiteId_TargetId_Status] ON [CcCommands] ([OrgId], [SiteId], [TargetId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_CcDevices_OrgId_SiteId] ON [CcDevices] ([OrgId], [SiteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_CcDevices_SerialNumber] ON [CcDevices] ([SerialNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_CcJobs_OrgId_SiteId_DeviceId] ON [CcJobs] ([OrgId], [SiteId], [DeviceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_CcJobTemplates_OrgId_SiteId] ON [CcJobTemplates] ([OrgId], [SiteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_CcTelemetryRecords_OrgId_SiteId_Timestamp] ON [CcTelemetryRecords] ([OrgId], [SiteId], [Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    CREATE INDEX [IX_ControlCenterInstances_OrgId_SiteId] ON [ControlCenterInstances] ([OrgId], [SiteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260317180951_AddControlCenterCoreEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260317180951_AddControlCenterCoreEntities', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402133902_AddReviewIsRejected'
)
BEGIN
    ALTER TABLE [Reviews] ADD [IsRejected] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402133902_AddReviewIsRejected'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260402133902_AddReviewIsRejected', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402140000_AddSupportConversationSubjectAndLinks'
)
BEGIN
    ALTER TABLE [SupportConversations] ADD [Subject] nvarchar(200) NOT NULL DEFAULT N'Support';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402140000_AddSupportConversationSubjectAndLinks'
)
BEGIN
    ALTER TABLE [SupportConversations] ADD [RelatedOrderId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402140000_AddSupportConversationSubjectAndLinks'
)
BEGIN
    ALTER TABLE [SupportConversations] ADD [RelatedDesignId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402140000_AddSupportConversationSubjectAndLinks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260402140000_AddSupportConversationSubjectAndLinks', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402153000_AddCncMaterialPcbFields'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [PcbMaterialType] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402153000_AddCncMaterialPcbFields'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [SupportedBoardThicknesses] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402153000_AddCncMaterialPcbFields'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [SupportsDoubleSided] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402153000_AddCncMaterialPcbFields'
)
BEGIN
    ALTER TABLE [CncMaterials] ADD [SupportsSingleSided] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402153000_AddCncMaterialPcbFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260402153000_AddCncMaterialPcbFields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403194946_AddFilamentSpools'
)
BEGIN
    CREATE TABLE [FilamentSpools] (
        [Id] uniqueidentifier NOT NULL,
        [MaterialId] uniqueidentifier NOT NULL,
        [MaterialColorId] uniqueidentifier NULL,
        [Name] nvarchar(200) NULL,
        [InitialWeightGrams] int NOT NULL DEFAULT 1000,
        [RemainingWeightGrams] int NOT NULL DEFAULT 1000,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_FilamentSpools] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FilamentSpools_MaterialColors_MaterialColorId] FOREIGN KEY ([MaterialColorId]) REFERENCES [MaterialColors] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_FilamentSpools_Materials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [Materials] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403194946_AddFilamentSpools'
)
BEGIN
    CREATE INDEX [IX_FilamentSpools_MaterialColorId] ON [FilamentSpools] ([MaterialColorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403194946_AddFilamentSpools'
)
BEGIN
    CREATE INDEX [IX_FilamentSpools_MaterialId] ON [FilamentSpools] ([MaterialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403194946_AddFilamentSpools'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403194946_AddFilamentSpools', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403200309_AddPrint3dRequestUsedSpoolAndEstimatedGrams'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [EstimatedFilamentGrams] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403200309_AddPrint3dRequestUsedSpoolAndEstimatedGrams'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [UsedSpoolId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403200309_AddPrint3dRequestUsedSpoolAndEstimatedGrams'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_UsedSpoolId] ON [Print3dServiceRequests] ([UsedSpoolId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403200309_AddPrint3dRequestUsedSpoolAndEstimatedGrams'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD CONSTRAINT [FK_Print3dServiceRequests_FilamentSpools_UsedSpoolId] FOREIGN KEY ([UsedSpoolId]) REFERENCES [FilamentSpools] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403200309_AddPrint3dRequestUsedSpoolAndEstimatedGrams'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403200309_AddPrint3dRequestUsedSpoolAndEstimatedGrams', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403200809_AddPrint3dRequestIsFilamentDeducted'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [IsFilamentDeducted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403200809_AddPrint3dRequestIsFilamentDeducted'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403200809_AddPrint3dRequestIsFilamentDeducted', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403201326_AddPrint3dRequestActualFilamentGrams'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [ActualFilamentGrams] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403201326_AddPrint3dRequestActualFilamentGrams'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403201326_AddPrint3dRequestActualFilamentGrams', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403202128_AddPrint3dEstimatedPrintTimeAndSuggestedPrice'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [EstimatedPrintTimeHours] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403202128_AddPrint3dEstimatedPrintTimeAndSuggestedPrice'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [SuggestedPrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403202128_AddPrint3dEstimatedPrintTimeAndSuggestedPrice'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403202128_AddPrint3dEstimatedPrintTimeAndSuggestedPrice', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [InventoryTransactions] ADD [UnitOfMeasureId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [InventoryTransactions] ADD [WarehouseId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [InventoryTransactions] ADD [DocumentType] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [InventoryTransactions] ADD [DocumentId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [InventoryTransactions] ADD [BaseQuantity] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD [BaseUnitOfMeasureId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD [WholesalePrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD [CostPrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD [IsTaxable] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD [AccountId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD [DefaultTaxConfigurationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Orders] ADD [IsStorefrontOrder] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD [ApprovalStatusId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD [ApprovedByUserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD [ApprovedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD [IsPosted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD [JournalEntryId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD [AccountId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD [SupplierId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [AccountTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(50) NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [NameAr] nvarchar(100) NOT NULL,
        [NormalBalance] nvarchar(10) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_AccountTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [DocumentSequences] (
        [Id] uniqueidentifier NOT NULL,
        [DocumentType] nvarchar(100) NOT NULL,
        [Prefix] nvarchar(20) NOT NULL,
        [Separator] nvarchar(5) NULL,
        [IncludeYear] bit NOT NULL,
        [CurrentYear] int NOT NULL,
        [LastNumber] int NOT NULL,
        [PadLength] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DocumentSequences] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [FiscalYears] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [IsClosed] bit NOT NULL,
        [ClosedByUserId] uniqueidentifier NULL,
        [ClosedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_FiscalYears] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [LookupTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsSystem] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LookupTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [UnitsOfMeasure] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [NameAr] nvarchar(100) NOT NULL,
        [Abbreviation] nvarchar(20) NOT NULL,
        [IsBase] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_UnitsOfMeasure] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [Warehouses] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [Address] nvarchar(max) NULL,
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [ManagerUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [Accounts] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [AccountTypeId] uniqueidentifier NOT NULL,
        [ParentId] uniqueidentifier NULL,
        [Level] int NOT NULL,
        [IsPostable] bit NOT NULL,
        [IsSystem] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'ILS',
        [CurrentBalance] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Accounts_AccountTypes_AccountTypeId] FOREIGN KEY ([AccountTypeId]) REFERENCES [AccountTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Accounts_Accounts_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [FiscalPeriods] (
        [Id] uniqueidentifier NOT NULL,
        [FiscalYearId] uniqueidentifier NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [PeriodNumber] int NOT NULL,
        [IsClosed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_FiscalPeriods] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FiscalPeriods_FiscalYears_FiscalYearId] FOREIGN KEY ([FiscalYearId]) REFERENCES [FiscalYears] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [LookupValues] (
        [Id] uniqueidentifier NOT NULL,
        [LookupTypeId] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [IsDefault] bit NOT NULL,
        [IsSystem] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Color] nvarchar(32) NULL,
        [Icon] nvarchar(100) NULL,
        [ParentId] uniqueidentifier NULL,
        [MetaJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_LookupValues] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LookupValues_LookupTypes_LookupTypeId] FOREIGN KEY ([LookupTypeId]) REFERENCES [LookupTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LookupValues_LookupValues_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [LookupValues] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [TaxConfigurations] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [NameAr] nvarchar(100) NOT NULL,
        [Rate] decimal(18,2) NOT NULL,
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [AccountId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_TaxConfigurations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TaxConfigurations_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [UnitConversions] (
        [Id] uniqueidentifier NOT NULL,
        [FromUnitId] uniqueidentifier NOT NULL,
        [ToUnitId] uniqueidentifier NOT NULL,
        [ConversionFactor] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_UnitConversions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UnitConversions_UnitsOfMeasure_FromUnitId] FOREIGN KEY ([FromUnitId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UnitConversions_UnitsOfMeasure_ToUnitId] FOREIGN KEY ([ToUnitId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [NameEn] nvarchar(250) NOT NULL,
        [NameAr] nvarchar(250) NOT NULL,
        [Email] nvarchar(255) NULL,
        [Phone] nvarchar(50) NULL,
        [Phone2] nvarchar(50) NULL,
        [CustomerTypeId] uniqueidentifier NOT NULL,
        [TaxNumber] nvarchar(max) NULL,
        [CreditLimit] decimal(18,2) NOT NULL,
        [CurrentBalance] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'ILS',
        [BillingAddress] nvarchar(max) NULL,
        [ShippingAddress] nvarchar(max) NULL,
        [ContactPerson] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [UserId] uniqueidentifier NULL,
        [PriceListId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Customers_LookupValues_CustomerTypeId] FOREIGN KEY ([CustomerTypeId]) REFERENCES [LookupValues] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Customers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [JournalEntries] (
        [Id] uniqueidentifier NOT NULL,
        [EntryNumber] nvarchar(50) NOT NULL,
        [EntryDate] datetime2 NOT NULL,
        [FiscalPeriodId] uniqueidentifier NULL,
        [JournalEntryTypeId] uniqueidentifier NOT NULL,
        [Description] nvarchar(max) NULL,
        [SourceDocumentType] nvarchar(100) NULL,
        [SourceDocumentId] uniqueidentifier NULL,
        [SourceDocumentNumber] nvarchar(100) NULL,
        [TotalDebit] decimal(18,2) NOT NULL,
        [TotalCredit] decimal(18,2) NOT NULL,
        [IsPosted] bit NOT NULL,
        [PostedAt] datetime2 NULL,
        [PostedByUserId] uniqueidentifier NULL,
        [IsReversed] bit NOT NULL,
        [ReversedByEntryId] uniqueidentifier NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JournalEntries_FiscalPeriods_FiscalPeriodId] FOREIGN KEY ([FiscalPeriodId]) REFERENCES [FiscalPeriods] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JournalEntries_LookupValues_JournalEntryTypeId] FOREIGN KEY ([JournalEntryTypeId]) REFERENCES [LookupValues] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [PriceLists] (
        [Id] uniqueidentifier NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [NameAr] nvarchar(200) NOT NULL,
        [PriceListTypeId] uniqueidentifier NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'ILS',
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [ValidFrom] datetime2 NULL,
        [ValidTo] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PriceLists] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceLists_LookupValues_PriceListTypeId] FOREIGN KEY ([PriceListTypeId]) REFERENCES [LookupValues] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [NameEn] nvarchar(250) NOT NULL,
        [NameAr] nvarchar(250) NOT NULL,
        [Email] nvarchar(255) NULL,
        [Phone] nvarchar(50) NULL,
        [Phone2] nvarchar(50) NULL,
        [SupplierTypeId] uniqueidentifier NULL,
        [TaxNumber] nvarchar(max) NULL,
        [CreditLimit] decimal(18,2) NOT NULL,
        [CurrentBalance] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'ILS',
        [Address] nvarchar(max) NULL,
        [ContactPerson] nvarchar(max) NULL,
        [PaymentTermDays] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Suppliers_LookupValues_SupplierTypeId] FOREIGN KEY ([SupplierTypeId]) REFERENCES [LookupValues] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [ItemUnits] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [UnitOfMeasureId] uniqueidentifier NOT NULL,
        [ConversionToBase] decimal(18,2) NOT NULL,
        [Barcode] nvarchar(100) NULL,
        [IsDefault] bit NOT NULL,
        [IsPurchaseDefault] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ItemUnits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItemUnits_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ItemUnits_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [InventoryCostLayers] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [WarehouseId] uniqueidentifier NOT NULL,
        [Quantity] int NOT NULL,
        [CostPerUnit] decimal(18,2) NOT NULL,
        [SourceDocumentType] nvarchar(100) NOT NULL,
        [SourceDocumentId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_InventoryCostLayers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InventoryCostLayers_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryCostLayers_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [JournalEntryLines] (
        [Id] uniqueidentifier NOT NULL,
        [JournalEntryId] uniqueidentifier NOT NULL,
        [AccountId] uniqueidentifier NOT NULL,
        [Debit] decimal(18,2) NOT NULL,
        [Credit] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NULL,
        [CurrencyAmount] decimal(18,2) NULL,
        [ExchangeRate] decimal(18,2) NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JournalEntryLines_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JournalEntryLines_JournalEntries_JournalEntryId] FOREIGN KEY ([JournalEntryId]) REFERENCES [JournalEntries] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [PriceListItems] (
        [Id] uniqueidentifier NOT NULL,
        [PriceListId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [UnitOfMeasureId] uniqueidentifier NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [MinQuantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PriceListItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceListItems_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PriceListItems_PriceLists_PriceListId] FOREIGN KEY ([PriceListId]) REFERENCES [PriceLists] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PriceListItems_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE TABLE [SupplierItemPrices] (
        [Id] uniqueidentifier NOT NULL,
        [SupplierId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [UnitOfMeasureId] uniqueidentifier NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'ILS',
        [MinQuantity] int NOT NULL,
        [LeadTimeDays] int NULL,
        [ValidFrom] datetime2 NULL,
        [ValidTo] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SupplierItemPrices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupplierItemPrices_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SupplierItemPrices_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SupplierItemPrices_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryTransactions_UnitOfMeasureId] ON [InventoryTransactions] ([UnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Items_BaseUnitOfMeasureId] ON [Items] ([BaseUnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Items_DefaultTaxConfigurationId] ON [Items] ([DefaultTaxConfigurationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Expenses_AccountId] ON [Expenses] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Expenses_ApprovalStatusId] ON [Expenses] ([ApprovalStatusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Expenses_ApprovedByUserId] ON [Expenses] ([ApprovedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Expenses_JournalEntryId] ON [Expenses] ([JournalEntryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Expenses_SupplierId] ON [Expenses] ([SupplierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Accounts_AccountTypeId] ON [Accounts] ([AccountTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Accounts_Code] ON [Accounts] ([Code]) WHERE [Code] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Accounts_ParentId] ON [Accounts] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_Code] ON [Customers] ([Code]) WHERE [Code] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Customers_CustomerTypeId] ON [Customers] ([CustomerTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Customers_PriceListId] ON [Customers] ([PriceListId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Customers_UserId] ON [Customers] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DocumentSequences_DocumentType] ON [DocumentSequences] ([DocumentType]) WHERE [DocumentType] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_FiscalPeriods_FiscalYearId] ON [FiscalPeriods] ([FiscalYearId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryCostLayers_ItemId] ON [InventoryCostLayers] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_InventoryCostLayers_WarehouseId] ON [InventoryCostLayers] ([WarehouseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ItemUnits_ItemId_UnitOfMeasureId] ON [ItemUnits] ([ItemId], [UnitOfMeasureId]) WHERE [ItemId] IS NOT NULL AND [UnitOfMeasureId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_ItemUnits_UnitOfMeasureId] ON [ItemUnits] ([UnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_JournalEntries_EntryNumber] ON [JournalEntries] ([EntryNumber]) WHERE [EntryNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_FiscalPeriodId] ON [JournalEntries] ([FiscalPeriodId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_JournalEntryTypeId] ON [JournalEntries] ([JournalEntryTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_AccountId] ON [JournalEntryLines] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntryLines_JournalEntryId] ON [JournalEntryLines] ([JournalEntryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LookupTypes_Key] ON [LookupTypes] ([Key]) WHERE [Key] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LookupValues_LookupTypeId_Key] ON [LookupValues] ([LookupTypeId], [Key]) WHERE [LookupTypeId] IS NOT NULL AND [Key] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_LookupValues_ParentId] ON [LookupValues] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_PriceListItems_ItemId] ON [PriceListItems] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PriceListItems_PriceListId_ItemId_UnitOfMeasureId_MinQuantity] ON [PriceListItems] ([PriceListId], [ItemId], [UnitOfMeasureId], [MinQuantity]) WHERE [PriceListId] IS NOT NULL AND [ItemId] IS NOT NULL AND [UnitOfMeasureId] IS NOT NULL AND [MinQuantity] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_PriceListItems_UnitOfMeasureId] ON [PriceListItems] ([UnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_PriceLists_PriceListTypeId] ON [PriceLists] ([PriceListTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_SupplierItemPrices_ItemId] ON [SupplierItemPrices] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_SupplierItemPrices_SupplierId_ItemId_UnitOfMeasureId_MinQuantity] ON [SupplierItemPrices] ([SupplierId], [ItemId], [UnitOfMeasureId], [MinQuantity]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_SupplierItemPrices_UnitOfMeasureId] ON [SupplierItemPrices] ([UnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Suppliers_Code] ON [Suppliers] ([Code]) WHERE [Code] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_Suppliers_SupplierTypeId] ON [Suppliers] ([SupplierTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_TaxConfigurations_AccountId] ON [TaxConfigurations] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_UnitConversions_FromUnitId] ON [UnitConversions] ([FromUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    CREATE INDEX [IX_UnitConversions_ToUnitId] ON [UnitConversions] ([ToUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Warehouses_Code] ON [Warehouses] ([Code]) WHERE [Code] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Customers] ADD CONSTRAINT [FK_Customers_PriceLists_PriceListId] FOREIGN KEY ([PriceListId]) REFERENCES [PriceLists] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_JournalEntries_JournalEntryId] FOREIGN KEY ([JournalEntryId]) REFERENCES [JournalEntries] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_LookupValues_ApprovalStatusId] FOREIGN KEY ([ApprovalStatusId]) REFERENCES [LookupValues] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_Users_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [InventoryTransactions] ADD CONSTRAINT [FK_InventoryTransactions_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_TaxConfigurations_DefaultTaxConfigurationId] FOREIGN KEY ([DefaultTaxConfigurationId]) REFERENCES [TaxConfigurations] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_UnitsOfMeasure_BaseUnitOfMeasureId] FOREIGN KEY ([BaseUnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260405164000_AddCommercialFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260405164000_AddCommercialFoundation', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170000_UpgradeProjectRequestsForEngineeringWorkflow'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [AttachmentsJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170000_UpgradeProjectRequestsForEngineeringWorkflow'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [MainDomain] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170000_UpgradeProjectRequestsForEngineeringWorkflow'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [ProjectStage] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170000_UpgradeProjectRequestsForEngineeringWorkflow'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [ProjectType] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170000_UpgradeProjectRequestsForEngineeringWorkflow'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [RequiredCapabilitiesJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407170000_UpgradeProjectRequestsForEngineeringWorkflow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260407170000_UpgradeProjectRequestsForEngineeringWorkflow', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407180000_DeactivateAutonomousDroneProject'
)
BEGIN
    UPDATE Projects SET IsActive = 0 WHERE Slug = 'autonomous-inspection-drone'
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407180000_DeactivateAutonomousDroneProject'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260407180000_DeactivateAutonomousDroneProject', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [WorkflowStatus] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN

                    UPDATE ProjectRequests SET WorkflowStatus =
                        CASE Status
                            WHEN 0 THEN 'New'
                            WHEN 1 THEN 'UnderReview'
                            WHEN 2 THEN 'QuoteSent'
                            WHEN 3 THEN 'InExecution'
                            WHEN 4 THEN 'Completed'
                            ELSE 'New'
                        END
                    WHERE WorkflowStatus IS NULL
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [AssignedToUserId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [AssignedToName] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [Priority] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [TechnicalFeasibility] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [EstimatedCost] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [EstimatedTimeline] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [ComplexityLevel] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [InternalNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    CREATE TABLE [ProjectRequestActivities] (
        [Id] uniqueidentifier NOT NULL,
        [ProjectRequestId] uniqueidentifier NOT NULL,
        [ActionType] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ProjectRequestActivities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProjectRequestActivities_ProjectRequests_ProjectRequestId] FOREIGN KEY ([ProjectRequestId]) REFERENCES [ProjectRequests] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    CREATE INDEX [IX_ProjectRequestActivities_ProjectRequestId_CreatedAt] ON [ProjectRequestActivities] ([ProjectRequestId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260407190000_AddEngineeringWorkflowToProjectRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260407190000_AddEngineeringWorkflowToProjectRequests', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408193000_EnsureEmailVerificationColumnsExist'
)
BEGIN
    IF COL_LENGTH('Users', 'EmailVerificationToken') IS NULL
    BEGIN
        ALTER TABLE [Users] ADD [EmailVerificationToken] nvarchar(500) NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408193000_EnsureEmailVerificationColumnsExist'
)
BEGIN
    IF COL_LENGTH('Users', 'EmailVerificationTokenExpiresAt') IS NULL
    BEGIN
        ALTER TABLE [Users] ADD [EmailVerificationTokenExpiresAt] datetime2 NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408193000_EnsureEmailVerificationColumnsExist'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260408193000_EnsureEmailVerificationColumnsExist', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    CREATE TABLE [PurchaseReturns] (
        [Id] uniqueidentifier NOT NULL,
        [ReturnNumber] nvarchar(50) NOT NULL,
        [SupplierId] uniqueidentifier NULL,
        [SupplierName] nvarchar(250) NOT NULL,
        [StatusKey] nvarchar(50) NOT NULL,
        [StatusName] nvarchar(100) NOT NULL,
        [StatusColor] nvarchar(20) NULL,
        [PurchaseInvoiceId] uniqueidentifier NULL,
        [PurchaseInvoiceNumber] nvarchar(50) NULL,
        [ReturnReasonLookupId] nvarchar(50) NULL,
        [ReturnReasonName] nvarchar(100) NULL,
        [ReturnDate] datetime2 NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [IsPosted] bit NOT NULL,
        [DeductFromInventory] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [WarehouseId] uniqueidentifier NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PurchaseReturns] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    CREATE TABLE [SalesReturns] (
        [Id] uniqueidentifier NOT NULL,
        [ReturnNumber] nvarchar(50) NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [CustomerName] nvarchar(250) NOT NULL,
        [StatusKey] nvarchar(50) NOT NULL,
        [StatusName] nvarchar(100) NOT NULL,
        [StatusColor] nvarchar(20) NULL,
        [SalesInvoiceId] uniqueidentifier NULL,
        [SalesInvoiceNumber] nvarchar(50) NULL,
        [ReturnReasonLookupId] nvarchar(50) NULL,
        [ReturnReasonName] nvarchar(100) NULL,
        [ReturnDate] datetime2 NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [IsPosted] bit NOT NULL,
        [RestockItems] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [WarehouseId] uniqueidentifier NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SalesReturns] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    CREATE TABLE [PurchaseReturnLines] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseReturnId] uniqueidentifier NOT NULL,
        [PurchaseInvoiceLineId] uniqueidentifier NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [ItemName] nvarchar(250) NULL,
        [ItemSku] nvarchar(100) NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PurchaseReturnLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseReturnLines_PurchaseReturns_PurchaseReturnId] FOREIGN KEY ([PurchaseReturnId]) REFERENCES [PurchaseReturns] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    CREATE TABLE [SalesReturnLines] (
        [Id] uniqueidentifier NOT NULL,
        [SalesReturnId] uniqueidentifier NOT NULL,
        [SalesInvoiceLineId] uniqueidentifier NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [ItemName] nvarchar(250) NULL,
        [ItemSku] nvarchar(100) NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_SalesReturnLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalesReturnLines_SalesReturns_SalesReturnId] FOREIGN KEY ([SalesReturnId]) REFERENCES [SalesReturns] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    CREATE INDEX [IX_PurchaseReturnLines_PurchaseReturnId] ON [PurchaseReturnLines] ([PurchaseReturnId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PurchaseReturns_ReturnNumber] ON [PurchaseReturns] ([ReturnNumber]) WHERE [ReturnNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    CREATE INDEX [IX_SalesReturnLines_SalesReturnId] ON [SalesReturnLines] ([SalesReturnId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SalesReturns_ReturnNumber] ON [SalesReturns] ([ReturnNumber]) WHERE [ReturnNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260408210000_AddCommercialReturns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260408210000_AddCommercialReturns', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [UserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [CustomerId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    CREATE INDEX [IX_CncServiceRequests_UserId] ON [CncServiceRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    CREATE INDEX [IX_CncServiceRequests_CustomerId] ON [CncServiceRequests] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD CONSTRAINT [FK_CncServiceRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD CONSTRAINT [FK_CncServiceRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [LaserServiceRequests] ADD [UserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [LaserServiceRequests] ADD [CustomerId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    CREATE INDEX [IX_LaserServiceRequests_UserId] ON [LaserServiceRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    CREATE INDEX [IX_LaserServiceRequests_CustomerId] ON [LaserServiceRequests] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [LaserServiceRequests] ADD CONSTRAINT [FK_LaserServiceRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [LaserServiceRequests] ADD CONSTRAINT [FK_LaserServiceRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [UserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [CustomerId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    CREATE INDEX [IX_ProjectRequests_UserId] ON [ProjectRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    CREATE INDEX [IX_ProjectRequests_CustomerId] ON [ProjectRequests] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD CONSTRAINT [FK_ProjectRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD CONSTRAINT [FK_ProjectRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [CustomerId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_CustomerId] ON [Print3dServiceRequests] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD CONSTRAINT [FK_Print3dServiceRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409000000_AddCustomerIdToServiceRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260409000000_AddCustomerIdToServiceRequests', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE TABLE [Quotations] (
        [Id] uniqueidentifier NOT NULL,
        [QuotationNumber] nvarchar(50) NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [SourceRequestId] uniqueidentifier NULL,
        [SourceRequestType] nvarchar(20) NULL,
        [SourceRequestReference] nvarchar(50) NULL,
        [Status] int NOT NULL DEFAULT 0,
        [QuotationDate] datetime2 NOT NULL,
        [ValidUntil] datetime2 NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Currency] nvarchar(10) NOT NULL DEFAULT N'ILS',
        [Notes] nvarchar(max) NULL,
        [InternalNotes] nvarchar(max) NULL,
        [TermsAndConditions] nvarchar(max) NULL,
        [ConvertedToOrderId] uniqueidentifier NULL,
        [ConvertedAt] datetime2 NULL,
        [CreatedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Quotations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Quotations_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_Quotations_CustomerId] ON [Quotations] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_Quotations_SourceRequestId] ON [Quotations] ([SourceRequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_Quotations_Status] ON [Quotations] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Quotations_QuotationNumber] ON [Quotations] ([QuotationNumber]) WHERE [QuotationNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE TABLE [QuotationItems] (
        [Id] uniqueidentifier NOT NULL,
        [QuotationId] uniqueidentifier NOT NULL,
        [LineNumber] int NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Quantity] decimal(18,4) NOT NULL,
        [Unit] nvarchar(20) NOT NULL DEFAULT N'pcs',
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountPercent] decimal(5,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxPercent] decimal(5,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_QuotationItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuotationItems_Quotations_QuotationId] FOREIGN KEY ([QuotationId]) REFERENCES [Quotations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_QuotationItems_QuotationId] ON [QuotationItems] ([QuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Orders] ADD [CustomerId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Orders] ADD [SourceQuotationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Orders] ADD [SourceRequestId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Orders] ADD [SourceRequestType] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Orders] ADD [SourceRequestReference] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_Orders_SourceQuotationId] ON [Orders] ([SourceQuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Quotations_SourceQuotationId] FOREIGN KEY ([SourceQuotationId]) REFERENCES [Quotations] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [OrderItems] ADD [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [ProjectRequests] ADD [LinkedQuotationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_ProjectRequests_LinkedQuotationId] ON [ProjectRequests] ([LinkedQuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [CncServiceRequests] ADD [LinkedQuotationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_CncServiceRequests_LinkedQuotationId] ON [CncServiceRequests] ([LinkedQuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [LaserServiceRequests] ADD [LinkedQuotationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_LaserServiceRequests_LinkedQuotationId] ON [LaserServiceRequests] ([LinkedQuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [Print3dServiceRequests] ADD [LinkedQuotationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_Print3dServiceRequests_LinkedQuotationId] ON [Print3dServiceRequests] ([LinkedQuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [DesignServiceRequests] ADD [CustomerId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [DesignServiceRequests] ADD [LinkedQuotationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_DesignServiceRequests_CustomerId] ON [DesignServiceRequests] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_DesignServiceRequests_LinkedQuotationId] ON [DesignServiceRequests] ([LinkedQuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [DesignServiceRequests] ADD CONSTRAINT [FK_DesignServiceRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [DesignCadServiceRequests] ADD [CustomerId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [DesignCadServiceRequests] ADD [LinkedQuotationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_DesignCadServiceRequests_CustomerId] ON [DesignCadServiceRequests] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    CREATE INDEX [IX_DesignCadServiceRequests_LinkedQuotationId] ON [DesignCadServiceRequests] ([LinkedQuotationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    ALTER TABLE [DesignCadServiceRequests] ADD CONSTRAINT [FK_DesignCadServiceRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409210000_AddQuotationsAndCommercialLinks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260409210000_AddQuotationsAndCommercialLinks', N'8.0.11');
END;
GO

COMMIT;
GO

