CREATE TABLE [dbo].[AspNetRoles] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([Name] ASC);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[AspNetRoles] TO [gzAdminUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[AspNetRoles] TO [gzAdminUser]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[AspNetRoles] TO [gzAdminUser]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[AspNetRoles] TO [gzAdminUser]
    AS [dbo];

