CREATE TABLE [dbo].[FundPrices] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [FundId]       INT      NOT NULL,
    [YearMonthDay] CHAR (8) NULL,
    [ClosingPrice] REAL     NOT NULL,
    [UpdatedOnUTC] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.FundPrices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.FundPrices_dbo.Funds_FundId] FOREIGN KEY ([FundId]) REFERENCES [dbo].[Funds] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [FundId_YMD_idx]
    ON [dbo].[FundPrices]([FundId] ASC, [YearMonthDay] ASC);

