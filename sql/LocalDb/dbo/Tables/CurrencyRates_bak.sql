CREATE TABLE [dbo].[CurrencyRates_bak] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [TradeDateTime] DATETIME         NOT NULL,
    [FromTo]        CHAR (6)         NOT NULL,
    [rate]          DECIMAL (29, 16) NOT NULL,
    [UpdatedOnUTC]  DATETIME         NOT NULL
);

