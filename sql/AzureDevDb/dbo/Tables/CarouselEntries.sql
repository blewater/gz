CREATE TABLE [dbo].[CarouselEntries] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [Code]               NVARCHAR (100) NOT NULL,
    [Title]              NVARCHAR (255) NOT NULL,
    [SubTitle]           NVARCHAR (255) NOT NULL,
    [ActionUrl]          NVARCHAR (255) NOT NULL,
    [Live]               BIT            NOT NULL,
    [LiveFrom]           DATETIME       NOT NULL,
    [LiveTo]             DATETIME       NOT NULL,
    [ActionText]         NVARCHAR (255) NOT NULL,
    [BackgroundImageUrl] NVARCHAR (255) NULL,
    [ActionType]         INT            DEFAULT ((0)) NOT NULL,
    [Updated]            DATETIME       DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    [Deleted]            BIT            DEFAULT ((0)) NOT NULL,
    [IsMobile]           BIT            DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.CarouselEntries] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [CarouselEntry_Code]
    ON [dbo].[CarouselEntries]([Code] ASC);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[CarouselEntries] TO [gzAdminUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[CarouselEntries] TO [gzAdminUser]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[CarouselEntries] TO [gzAdminUser]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[CarouselEntries] TO [gzAdminUser]
    AS [dbo];

