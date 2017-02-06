CREATE TABLE [dbo].[UserPortfoliosShares] (
    [Id]                    INT              IDENTITY (1, 1) NOT NULL,
    [UserId]                INT              NOT NULL,
    [YearMonth]             CHAR (6)         NOT NULL,
    [PortfolioLowShares]    DECIMAL (29, 16) NOT NULL,
    [PortfolioMediumShares] DECIMAL (29, 16) NOT NULL,
    [PortfolioHighShares]   DECIMAL (29, 16) NOT NULL,
    [BuyPortfolioTradeDay]  DATETIME         NOT NULL,
    [SoldPortfolioTradeDay] DATETIME         NULL,
    [UpdatedOnUtc]          DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.UserPortfoliosShares] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserPortfoliosShareId_YMD_idx]
    ON [dbo].[UserPortfoliosShares]([UserId] ASC, [YearMonth] ASC);

