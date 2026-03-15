-- Run this script against your database to create Support Chat tables
-- (e.g. in SSMS or: sqlcmd -S your_server -d your_database -i ApplySupportChat.sql)
-- Then restart the API so it sees the tables.

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SupportConversations')
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
    CREATE INDEX [IX_SupportConversations_AssignedToUserId] ON [SupportConversations] ([AssignedToUserId]);
    CREATE INDEX [IX_SupportConversations_CustomerId] ON [SupportConversations] ([CustomerId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SupportMessages')
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
    CREATE INDEX [IX_SupportMessages_ConversationId] ON [SupportMessages] ([ConversationId]);
    CREATE INDEX [IX_SupportMessages_SenderUserId] ON [SupportMessages] ([SenderUserId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260202120000_AddSupportChat')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260202120000_AddSupportChat', N'9.0.0');
GO
