CREATE TABLE [dbo].[Funds] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Symbol]       NVARCHAR (10)  NULL,
    [HoldingName]  NVARCHAR (128) NOT NULL,
    [UpdatedOnUTC] DATETIME       NOT NULL,
    [YearToDate]   REAL           DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.Funds] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_HoldingName]
    ON [dbo].[Funds]([HoldingName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Symbol]
    ON [dbo].[Funds]([Symbol] ASC);

