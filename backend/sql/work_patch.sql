-- Work Tables Patch

CREATE OR ALTER TABLE [dbo].[Work] (
    [Id] int NOT NULL IDENTITY,
    [AuthorId] int NOT NULL,
    [TargetAudience] nvarchar(50) NOT NULL,
    [Title] nvarchar(150) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [BasePrice] decimal(18, 2) NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT 'Open',
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Work] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Work_User_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [User] ([Id])
);
GO

CREATE OR ALTER TABLE [dbo].[WorkApplicant] (
    [Id] int NOT NULL IDENTITY,
    [WorkId] int NOT NULL,
    [UserId] int NOT NULL,
    [OfferedPrice] decimal(18, 2) NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT 'Pending',
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_WorkApplicant] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkApplicant_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WorkApplicant_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
);
GO

CREATE OR ALTER TABLE [dbo].[WorkTodo] (
    [Id] int NOT NULL IDENTITY,
    [WorkId] int NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [IsDone] bit NOT NULL DEFAULT 0,
    [DoneMessage] nvarchar(500) NULL,
    [DoneByUserId] int NULL,
    CONSTRAINT [PK_WorkTodo] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkTodo_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WorkTodo_User_DoneByUserId] FOREIGN KEY ([DoneByUserId]) REFERENCES [User] ([Id])
);
GO

CREATE OR ALTER TABLE [dbo].[WorkImage] (
    [Id] int NOT NULL IDENTITY,
    [WorkId] int NOT NULL,
    [ImageUrl] nvarchar(250) NOT NULL,
    [IsShowcase] bit NOT NULL DEFAULT 0,
    [UploadedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_WorkImage] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkImage_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE
);
GO

CREATE OR ALTER TABLE [dbo].[FeaturedWork] (
    [Id] int NOT NULL IDENTITY,
    [WorkId] int NOT NULL,
    [FeaturedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_FeaturedWork] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FeaturedWork_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE
);
GO

CREATE OR ALTER TABLE [dbo].[WorkTag] (
    [WorkId] int NOT NULL,
    [TagId] int NOT NULL,
    CONSTRAINT [PK_WorkTag] PRIMARY KEY ([WorkId], [TagId]),
    CONSTRAINT [FK_WorkTag_Work_WorkId] FOREIGN KEY ([WorkId]) REFERENCES [Work] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WorkTag_ActivityTag_TagId] FOREIGN KEY ([TagId]) REFERENCES [ActivityTag] ([Id]) ON DELETE CASCADE
);
GO
