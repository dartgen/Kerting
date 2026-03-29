/*
  Featured users (home carousel) patch (data-first, idempotent)
  Run on existing database.
*/

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.FeaturedUserSlot', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeaturedUserSlot
    (
        SlotNo tinyint NOT NULL,
        UserId int NOT NULL,
        CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_FeaturedUserSlot_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc datetime2 NOT NULL CONSTRAINT DF_FeaturedUserSlot_UpdatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_FeaturedUserSlot PRIMARY KEY (SlotNo),
        CONSTRAINT CK_FeaturedUserSlot_SlotNo_Range CHECK (SlotNo BETWEEN 1 AND 5)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FeaturedUserSlot_User_UserId'
)
BEGIN
    ALTER TABLE dbo.FeaturedUserSlot
    ADD CONSTRAINT FK_FeaturedUserSlot_User_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.[User](Id)
        ON DELETE CASCADE;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.FeaturedUserSlot')
      AND name = 'UX_FeaturedUserSlot_UserId'
)
BEGIN
    CREATE UNIQUE INDEX UX_FeaturedUserSlot_UserId
        ON dbo.FeaturedUserSlot(UserId);
END
GO
