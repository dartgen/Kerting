/*
  Forum v1 patch (data-first, idempotent)
  Run on existing database.
*/

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.ForumPost', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumPost
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForumPost PRIMARY KEY,
        UserId int NOT NULL,
        Title nvarchar(150) NOT NULL,
        Description nvarchar(2000) NOT NULL,
        AttachedGalleryItemId int NULL,
        IsPinned bit NOT NULL CONSTRAINT DF_ForumPost_IsPinned DEFAULT (0),
        IsLocked bit NOT NULL CONSTRAINT DF_ForumPost_IsLocked DEFAULT (0),
        LockReason nvarchar(300) NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_ForumPost_IsDeleted DEFAULT (0),
        DeletedAtUtc datetime2 NULL,
        DeletedByUserId int NULL,
        CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_ForumPost_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc datetime2 NULL,
        LastActivityAtUtc datetime2 NOT NULL CONSTRAINT DF_ForumPost_LastActivityAtUtc DEFAULT (SYSUTCDATETIME()),
        ViewCount int NOT NULL CONSTRAINT DF_ForumPost_ViewCount DEFAULT (0)
    );
END
GO

IF OBJECT_ID('dbo.ForumComment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumComment
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForumComment PRIMARY KEY,
        ForumPostId int NOT NULL,
        ParentCommentId int NULL,
        UserId int NOT NULL,
        Message nvarchar(1000) NOT NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_ForumComment_IsDeleted DEFAULT (0),
        DeletedAtUtc datetime2 NULL,
        DeletedByUserId int NULL,
        CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_ForumComment_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc datetime2 NULL
    );
END
GO

IF OBJECT_ID('dbo.ForumPostReaction', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumPostReaction
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForumPostReaction PRIMARY KEY,
        ForumPostId int NOT NULL,
        UserId int NOT NULL,
        IsLike bit NOT NULL,
        CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_ForumPostReaction_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID('dbo.ForumCommentReaction', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumCommentReaction
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForumCommentReaction PRIMARY KEY,
        ForumCommentId int NOT NULL,
        UserId int NOT NULL,
        IsLike bit NOT NULL,
        CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_ForumCommentReaction_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID('dbo.ForumPostTag', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ForumPostTag
    (
        ForumPostId int NOT NULL,
        TagId int NOT NULL,
        CONSTRAINT PK_ForumPostTag PRIMARY KEY (ForumPostId, TagId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumPost_Login_UserId')
BEGIN
    ALTER TABLE dbo.ForumPost
    ADD CONSTRAINT FK_ForumPost_Login_UserId
    FOREIGN KEY (UserId) REFERENCES dbo.Login(Id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumPost_GalleryItem_AttachedGalleryItemId')
BEGIN
    ALTER TABLE dbo.ForumPost
    ADD CONSTRAINT FK_ForumPost_GalleryItem_AttachedGalleryItemId
    FOREIGN KEY (AttachedGalleryItemId) REFERENCES dbo.GalleryItem(Id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumComment_ForumPost')
BEGIN
    ALTER TABLE dbo.ForumComment
    ADD CONSTRAINT FK_ForumComment_ForumPost
    FOREIGN KEY (ForumPostId) REFERENCES dbo.ForumPost(Id) ON DELETE CASCADE;
END
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumComment_Parent')
BEGIN
    DECLARE @fkDeleteAction int;
    SELECT @fkDeleteAction = delete_referential_action
    FROM sys.foreign_keys
    WHERE name = 'FK_ForumComment_Parent';

    IF (@fkDeleteAction <> 0)
    BEGIN
        ALTER TABLE dbo.ForumComment DROP CONSTRAINT FK_ForumComment_Parent;
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumComment_Parent')
BEGIN
    ALTER TABLE dbo.ForumComment
    ADD CONSTRAINT FK_ForumComment_Parent
    FOREIGN KEY (ParentCommentId) REFERENCES dbo.ForumComment(Id) ON DELETE NO ACTION;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumComment_Login_UserId')
BEGIN
    ALTER TABLE dbo.ForumComment
    ADD CONSTRAINT FK_ForumComment_Login_UserId
    FOREIGN KEY (UserId) REFERENCES dbo.Login(Id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumPostReaction_ForumPost')
BEGIN
    ALTER TABLE dbo.ForumPostReaction
    ADD CONSTRAINT FK_ForumPostReaction_ForumPost
    FOREIGN KEY (ForumPostId) REFERENCES dbo.ForumPost(Id) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumPostReaction_Login_UserId')
BEGIN
    ALTER TABLE dbo.ForumPostReaction
    ADD CONSTRAINT FK_ForumPostReaction_Login_UserId
    FOREIGN KEY (UserId) REFERENCES dbo.Login(Id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumCommentReaction_ForumComment')
BEGIN
    ALTER TABLE dbo.ForumCommentReaction
    ADD CONSTRAINT FK_ForumCommentReaction_ForumComment
    FOREIGN KEY (ForumCommentId) REFERENCES dbo.ForumComment(Id) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumCommentReaction_Login_UserId')
BEGIN
    ALTER TABLE dbo.ForumCommentReaction
    ADD CONSTRAINT FK_ForumCommentReaction_Login_UserId
    FOREIGN KEY (UserId) REFERENCES dbo.Login(Id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumPostTag_ForumPost')
BEGIN
    ALTER TABLE dbo.ForumPostTag
    ADD CONSTRAINT FK_ForumPostTag_ForumPost
    FOREIGN KEY (ForumPostId) REFERENCES dbo.ForumPost(Id) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ForumPostTag_ActivityTag')
BEGIN
    ALTER TABLE dbo.ForumPostTag
    ADD CONSTRAINT FK_ForumPostTag_ActivityTag
    FOREIGN KEY (TagId) REFERENCES dbo.ActivityTag(Id) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ForumPost') AND name = 'IX_ForumPost_Deleted_Pinned_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_ForumPost_Deleted_Pinned_CreatedAtUtc
        ON dbo.ForumPost(IsDeleted, IsPinned, CreatedAtUtc DESC);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ForumPost') AND name = 'IX_ForumPost_Deleted_LastActivityAtUtc'
)
BEGIN
    CREATE INDEX IX_ForumPost_Deleted_LastActivityAtUtc
        ON dbo.ForumPost(IsDeleted, LastActivityAtUtc DESC);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ForumComment') AND name = 'IX_ForumComment_Post_Parent_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_ForumComment_Post_Parent_CreatedAtUtc
        ON dbo.ForumComment(ForumPostId, ParentCommentId, CreatedAtUtc DESC);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ForumComment') AND name = 'IX_ForumComment_Post_Deleted_CreatedAtUtc'
)
BEGIN
    CREATE INDEX IX_ForumComment_Post_Deleted_CreatedAtUtc
        ON dbo.ForumComment(ForumPostId, IsDeleted, CreatedAtUtc DESC);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ForumPostReaction') AND name = 'UX_ForumPostReaction_ForumPostId_UserId'
)
BEGIN
    CREATE UNIQUE INDEX UX_ForumPostReaction_ForumPostId_UserId
        ON dbo.ForumPostReaction(ForumPostId, UserId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ForumCommentReaction') AND name = 'UX_ForumCommentReaction_ForumCommentId_UserId'
)
BEGIN
    CREATE UNIQUE INDEX UX_ForumCommentReaction_ForumCommentId_UserId
        ON dbo.ForumCommentReaction(ForumCommentId, UserId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.ForumPostTag') AND name = 'IX_ForumPostTag_TagId'
)
BEGIN
    CREATE INDEX IX_ForumPostTag_TagId
        ON dbo.ForumPostTag(TagId);
END
GO
