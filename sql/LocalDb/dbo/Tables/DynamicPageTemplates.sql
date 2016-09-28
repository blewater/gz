CREATE TABLE [dbo].[DynamicPageTemplates] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (MAX) NOT NULL,
    [Html] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.DynamicPageTemplates] PRIMARY KEY CLUSTERED ([Id] ASC)
);

