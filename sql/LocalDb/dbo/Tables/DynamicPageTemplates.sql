CREATE TABLE [dbo].[DynamicPageTemplates] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (MAX) NOT NULL,
    [Html] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.DynamicPageTemplates] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
GRANT UPDATE
    ON OBJECT::[dbo].[DynamicPageTemplates] TO [gzAdminUser]
    AS [dbo];




GO
GRANT SELECT
    ON OBJECT::[dbo].[DynamicPageTemplates] TO [gzAdminUser]
    AS [dbo];




GO
GRANT INSERT
    ON OBJECT::[dbo].[DynamicPageTemplates] TO [gzAdminUser]
    AS [dbo];




GO
GRANT DELETE
    ON OBJECT::[dbo].[DynamicPageTemplates] TO [gzAdminUser]
    AS [dbo];



