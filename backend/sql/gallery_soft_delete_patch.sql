/*
  Gallery soft-delete and publish patch (data-first, idempotent)
  Run on existing database.
*/

SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.GalleryItem', 'IsPublished') IS NULL
BEGIN
    ALTER TABLE dbo.GalleryItem
    ADD IsPublished bit NOT NULL CONSTRAINT DF_GalleryItem_IsPublished DEFAULT (1);
END
GO

IF COL_LENGTH('dbo.GalleryItem', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE dbo.GalleryItem
    ADD IsDeleted bit NOT NULL CONSTRAINT DF_GalleryItem_IsDeleted DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.GalleryItem', 'DeletedAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.GalleryItem
    ADD DeletedAtUtc datetime2 NULL;
END
GO

IF COL_LENGTH('dbo.GalleryItem', 'DeletedByUserId') IS NULL
BEGIN
    ALTER TABLE dbo.GalleryItem
    ADD DeletedByUserId int NULL;
END
GO

IF COL_LENGTH('dbo.GalleryComment', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE dbo.GalleryComment
    ADD IsDeleted bit NOT NULL CONSTRAINT DF_GalleryComment_IsDeleted DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.GalleryComment', 'DeletedAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.GalleryComment
    ADD DeletedAtUtc datetime2 NULL;
END
GO

IF COL_LENGTH('dbo.GalleryComment', 'DeletedByUserId') IS NULL
BEGIN
    ALTER TABLE dbo.GalleryComment
    ADD DeletedByUserId int NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.GalleryItem')
      AND name = 'IX_GalleryItem_Published_Deleted_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_GalleryItem_Published_Deleted_CreatedAtUtc
        ON dbo.GalleryItem (IsPublished, IsDeleted, CreatedAtUtc DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.GalleryComment')
      AND name = 'IX_GalleryComment_GalleryItemId_IsDeleted_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_GalleryComment_GalleryItemId_IsDeleted_CreatedAtUtc
        ON dbo.GalleryComment (GalleryItemId, IsDeleted, CreatedAtUtc DESC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_GalleryItem_DeletedBy_User'
)
BEGIN
    ALTER TABLE dbo.GalleryItem
    ADD CONSTRAINT FK_GalleryItem_DeletedBy_User
        FOREIGN KEY (DeletedByUserId) REFERENCES dbo.[User](Id);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_GalleryComment_DeletedBy_User'
)
BEGIN
    ALTER TABLE dbo.GalleryComment
    ADD CONSTRAINT FK_GalleryComment_DeletedBy_User
        FOREIGN KEY (DeletedByUserId) REFERENCES dbo.[User](Id);
END
GO
