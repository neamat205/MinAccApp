CREATE TABLE [dbo].[RoleModulePermission] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [RoleId]     NVARCHAR (450) NOT NULL,
    [ModuleName] NVARCHAR (100) NOT NULL,
    [CanView]    BIT            DEFAULT ((0)) NOT NULL,
    [CanEdit]    BIT            DEFAULT ((0)) NOT NULL,
    [CanDelete]  BIT            DEFAULT ((0)) NOT NULL,
    [CanCreate]  BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);