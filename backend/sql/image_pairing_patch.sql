-- SQL Migration: Add RelatedImageId to WorkImage table for before/after image pairing
-- Date: 2026-04-07
-- Purpose: Enable image pairing for before/after comparisons

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'WorkImage' AND COLUMN_NAME = 'RelatedImageId'
)
BEGIN
    ALTER TABLE [WorkImage]
    ADD RelatedImageId INT NULL;

    -- Add foreign key constraint to allow self-referencing
    ALTER TABLE [WorkImage]
    ADD CONSTRAINT FK_WorkImage_RelatedImage 
    FOREIGN KEY (RelatedImageId) 
    REFERENCES [WorkImage]([Id]);

    PRINT 'Migration completed: Added RelatedImageId column and foreign key to WorkImage table';
END
ELSE
BEGIN
    PRINT 'Warning: RelatedImageId column already exists in WorkImage table. Skipping migration.';
END
