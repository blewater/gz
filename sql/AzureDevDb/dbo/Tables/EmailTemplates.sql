CREATE TABLE [dbo].[EmailTemplates] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Code]    NVARCHAR (100) NOT NULL,
    [Subject] NVARCHAR (MAX) NOT NULL,
    [Body]    NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.EmailTemplates] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [EmailTemplate_Code]
    ON [dbo].[EmailTemplates]([Code] ASC);

