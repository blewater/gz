CREATE TABLE [dbo].[EmailTemplates] (
    [Id]      INT             IDENTITY (1, 1) NOT NULL,
    [Code]    NVARCHAR (100)  NOT NULL,
    [Subject] NVARCHAR (1024) NOT NULL,
    [Body]    NVARCHAR (MAX)  NOT NULL,
    [Updated] DATETIME        DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    CONSTRAINT [PK_dbo.EmailTemplates] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [EmailTemplate_Code]
    ON [dbo].[EmailTemplates]([Code] ASC);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[EmailTemplates] TO [gzAdminUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[EmailTemplates] TO [gzAdminUser]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[EmailTemplates] TO [gzAdminUser]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[EmailTemplates] TO [gzAdminUser]
    AS [dbo];

