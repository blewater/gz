CREATE TABLE [dbo].[CurrencyListXes] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [From]         NVARCHAR (MAX) NOT NULL,
    [To]           NVARCHAR (MAX) NOT NULL,
    [UpdatedOnUTC] DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.CurrencyListXes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

