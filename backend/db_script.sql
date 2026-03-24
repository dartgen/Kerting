CREATE TABLE [Role] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [User] (
    [Id] int NOT NULL IDENTITY,
    [VezetekNev] nvarchar(max) NOT NULL,
    [KeresztNev] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Telepules] nvarchar(max) NOT NULL,
    [RoleId] int NOT NULL,
    [ProfileIMGId] int NOT NULL,
    [Rolam] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Login] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_Login] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Login_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [dbo].[GalleryItem] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(150) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [FileExtension] nvarchar(10) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_GalleryItem] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_GalleryItem_FileExtension] CHECK (LOWER([FileExtension]) IN (N'.jpg', N'.jpeg', N'.png', N'.webp', N'.gif', N'.bmp', N'.avif')),
    CONSTRAINT [FK_GalleryItem_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
);
GO


CREATE TABLE [dbo].[GalleryComment] (
    [Id] int NOT NULL IDENTITY,
    [GalleryItemId] int NOT NULL,
    [UserId] int NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_GalleryComment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GalleryComment_GalleryItem_GalleryItemId] FOREIGN KEY ([GalleryItemId]) REFERENCES [dbo].[GalleryItem] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GalleryComment_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
);
GO


CREATE TABLE [dbo].[GalleryReaction] (
    [Id] int NOT NULL IDENTITY,
    [GalleryItemId] int NOT NULL,
    [UserId] int NOT NULL,
    [IsLike] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_GalleryReaction] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GalleryReaction_GalleryItem_GalleryItemId] FOREIGN KEY ([GalleryItemId]) REFERENCES [dbo].[GalleryItem] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GalleryReaction_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
);
GO


CREATE INDEX [IX_GalleryComment_GalleryItemId_CreatedAtUtc] ON [dbo].[GalleryComment] ([GalleryItemId], [CreatedAtUtc]);
GO


CREATE INDEX [IX_GalleryComment_UserId] ON [dbo].[GalleryComment] ([UserId]);
GO


CREATE INDEX [IX_GalleryItem_UserId_CreatedAtUtc] ON [dbo].[GalleryItem] ([UserId], [CreatedAtUtc]);
GO


CREATE INDEX [IX_GalleryReaction_UserId] ON [dbo].[GalleryReaction] ([UserId]);
GO


CREATE UNIQUE INDEX [UX_GalleryReaction_GalleryItemId_UserId] ON [dbo].[GalleryReaction] ([GalleryItemId], [UserId]);
GO


CREATE INDEX [IX_Login_RoleId] ON [Login] ([RoleId]);
GO


