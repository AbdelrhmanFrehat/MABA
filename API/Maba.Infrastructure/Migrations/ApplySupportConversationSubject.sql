-- Run on production if POST /api/v1/support-conversations returns 500 and logs mention invalid column / Subject.
-- Safe to run multiple times (checks column existence).

IF COL_LENGTH('dbo.SupportConversations', 'Subject') IS NULL
BEGIN
    ALTER TABLE dbo.SupportConversations ADD [Subject] nvarchar(200) NOT NULL CONSTRAINT DF_SupportConversations_Subject DEFAULT (N'Support');
END

IF COL_LENGTH('dbo.SupportConversations', 'RelatedOrderId') IS NULL
BEGIN
    ALTER TABLE dbo.SupportConversations ADD [RelatedOrderId] uniqueidentifier NULL;
END

IF COL_LENGTH('dbo.SupportConversations', 'RelatedDesignId') IS NULL
BEGIN
    ALTER TABLE dbo.SupportConversations ADD [RelatedDesignId] uniqueidentifier NULL;
END
