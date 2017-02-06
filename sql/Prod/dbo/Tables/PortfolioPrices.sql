CREATE TABLE [dbo].[PortfolioPrices] (
    [Id]                   INT      IDENTITY (1, 1) NOT NULL,
    [YearMonthDay]         CHAR (8) NOT NULL,
    [UpdatedOnUtc]         DATETIME NOT NULL,
    [PortfolioLowPrice]    REAL     DEFAULT ((0)) NOT NULL,
    [PortfolioMediumPrice] REAL     DEFAULT ((0)) NOT NULL,
    [PortfolioHighPrice]   REAL     DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.PortfolioPrices] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_PortfolioPrices_YMD]
    ON [dbo].[PortfolioPrices]([YearMonthDay] ASC);

