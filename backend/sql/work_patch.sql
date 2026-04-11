-- Work Tables Patch

IF OBJECT_ID(N'[dbo].[Work]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Work] (
        [Id] int NOT NULL IDENTITY,
        [AuthorId] int NOT NULL,
        [TargetAudience] nvarchar(50) NOT NULL,
        [Title] nvarchar(150) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [BasePrice] decimal(18, 2) NULL,
        [Status] nvarchar(50) NOT NULL CONSTRAINT [DF_Work_Status] DEFAULT 'Open',
        [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_Work_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Work] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Work_User_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [User] ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[WorkApplicant]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WorkApplicant] (
        [Id] int NOT NULL IDENTITY,
        [WorkId] int NOT NULL,
        [UserId] int NOT NULL,
        [OfferedPrice] decimal(18, 2) NULL,
        [Status] nvarchar(50) NOT NULL CONSTRAINT [DF_WorkApplicant_Status] DEFAULT 'Pending',
        [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_WorkApplicant_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_WorkApplicant] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkApplicant_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkApplicant_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[WorkTodo]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WorkTodo] (
        [Id] int NOT NULL IDENTITY,
        [WorkId] int NOT NULL,
        [Title] nvarchar(250) NOT NULL,
        [IsDone] bit NOT NULL CONSTRAINT [DF_WorkTodo_IsDone] DEFAULT 0,
        [DoneMessage] nvarchar(500) NULL,
        [DoneByUserId] int NULL,
        CONSTRAINT [PK_WorkTodo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkTodo_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkTodo_User_DoneByUserId] FOREIGN KEY ([DoneByUserId]) REFERENCES [User] ([Id])
    );
END;
GO

IF OBJECT_ID(N'[dbo].[WorkImage]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WorkImage] (
        [Id] int NOT NULL IDENTITY,
        [WorkId] int NOT NULL,
        [ImageUrl] nvarchar(250) NOT NULL,
        [IsShowcase] bit NOT NULL CONSTRAINT [DF_WorkImage_IsShowcase] DEFAULT 0,
        [RelatedImageId] int NULL,
        [UploadedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_WorkImage_UploadedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_WorkImage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkImage_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkImage_RelatedImage] FOREIGN KEY ([RelatedImageId]) REFERENCES [WorkImage] ([Id])
    );
END;

IF OBJECT_ID(N'[dbo].[WorkImage]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.WorkImage', 'IsShowcase') IS NULL
    BEGIN
        ALTER TABLE [dbo].[WorkImage]
        ADD [IsShowcase] bit NOT NULL CONSTRAINT [DF_WorkImage_IsShowcase] DEFAULT 0 WITH VALUES;
    END;

    IF COL_LENGTH('dbo.WorkImage', 'UploadedAtUtc') IS NULL
    BEGIN
        ALTER TABLE [dbo].[WorkImage]
        ADD [UploadedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_WorkImage_UploadedAtUtc] DEFAULT (SYSUTCDATETIME()) WITH VALUES;
    END;

    IF COL_LENGTH('dbo.WorkImage', 'RelatedImageId') IS NULL
    BEGIN
        ALTER TABLE [dbo].[WorkImage] ADD [RelatedImageId] int NULL;
    END;
END;
GO

IF OBJECT_ID(N'[dbo].[FeaturedWork]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FeaturedWork] (
        [Id] int NOT NULL IDENTITY,
        [WorkId] int NOT NULL,
        [FeaturedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_FeaturedWork_FeaturedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_FeaturedWork] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FeaturedWork_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[dbo].[WorkTag]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WorkTag] (
        [WorkId] int NOT NULL,
        [TagId] int NOT NULL,
        CONSTRAINT [PK_WorkTag] PRIMARY KEY ([WorkId], [TagId]),
        CONSTRAINT [FK_WorkTag_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkTag_ActivityTag_TagId] FOREIGN KEY ([TagId]) REFERENCES [ActivityTag] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[dbo].[WorkImage]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[FK_WorkImage_RelatedImage]', N'F') IS NULL
BEGIN
    ALTER TABLE [dbo].[WorkImage]
    ADD CONSTRAINT [FK_WorkImage_RelatedImage]
    FOREIGN KEY ([RelatedImageId]) REFERENCES [WorkImage]([Id]);
END;
GO
