CREATE TABLE [dbo].[FundPrices_bak] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [FundId]       INT      NOT NULL,
    [YearMonthDay] CHAR (8) NULL,
    [ClosingPrice] REAL     NOT NULL,
    [UpdatedOnUTC] DATETIME NOT NULL
);

