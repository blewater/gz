CREATE TABLE [dbo].[DynamicPages] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [Code]                  NVARCHAR (100) NOT NULL,
    [Live]                  BIT            NOT NULL,
    [LiveFrom]              DATETIME       NOT NULL,
    [LiveTo]                DATETIME       NOT NULL,
    [Html]                  NVARCHAR (MAX) NOT NULL,
    [ThumbImageUrl]         NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    [ThumbTitle]            NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    [ThumbText]             NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    [Updated]               DATETIME       DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    [DynamicPageTemplateId] INT            DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.DynamicPages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.DynamicPages_dbo.DynamicPageTemplates_DynamicPageTemplateId] FOREIGN KEY ([DynamicPageTemplateId]) REFERENCES [dbo].[DynamicPageTemplates] ([Id]) ON DELETE CASCADE
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [DynamicPage_Code]
    ON [dbo].[DynamicPages]([Code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_DynamicPageTemplateId]
    ON [dbo].[DynamicPages]([DynamicPageTemplateId] ASC);

