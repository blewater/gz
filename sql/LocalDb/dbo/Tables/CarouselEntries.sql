CREATE TABLE [dbo].[CarouselEntries] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [Code]               NVARCHAR (100) NOT NULL,
    [Title]              NVARCHAR (MAX) NOT NULL,
    [SubTitle]           NVARCHAR (MAX) NOT NULL,
    [ActionUrl]          NVARCHAR (MAX) NOT NULL,
    [Live]               BIT            NOT NULL,
    [LiveFrom]           DATETIME       NOT NULL,
    [LiveTo]             DATETIME       NOT NULL,
    [ActionText]         NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    [BackgroundImageUrl] NVARCHAR (MAX) DEFAULT ('') NULL,
    [ActionType]         INT            DEFAULT ((0)) NOT NULL,
    [Updated]            DATETIME       DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    CONSTRAINT [PK_dbo.CarouselEntries] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [CarouselEntry_Code]
    ON [dbo].[CarouselEntries]([Code] ASC);

