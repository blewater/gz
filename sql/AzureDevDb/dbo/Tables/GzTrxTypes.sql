CREATE TABLE [dbo].[GzTrxTypes] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Code]        INT            NOT NULL,
    [Description] NVARCHAR (300) NOT NULL,
    CONSTRAINT [PK_dbo.GzTrxTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Code]
    ON [dbo].[GzTrxTypes]([Code] ASC);

