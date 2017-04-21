CREATE TABLE [dbo].[PortfolioPrices] (
    [Id]                   INT        IDENTITY (1, 1) NOT NULL,
    [YearMonthDay]         CHAR (8)   NOT NULL,
    [UpdatedOnUtc]         DATETIME   NOT NULL,
    [PortfolioLowPrice]    FLOAT (53) CONSTRAINT [DF_dbo.PortfolioPrices_PortfolioLowPrice] DEFAULT ((0)) NOT NULL,
    [PortfolioMediumPrice] FLOAT (53) CONSTRAINT [DF_dbo.PortfolioPrices_PortfolioMediumPrice] DEFAULT ((0)) NOT NULL,
    [PortfolioHighPrice]   FLOAT (53) CONSTRAINT [DF_dbo.PortfolioPrices_PortfolioHighPrice] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.PortfolioPrices] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_PortfolioPrices_YMD]
    ON [dbo].[PortfolioPrices]([YearMonthDay] ASC);

