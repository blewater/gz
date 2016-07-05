CREATE TABLE [dbo].[CurrencyRates] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [TradeDateTime] DATETIME         NOT NULL,
    [FromTo]        CHAR (6)         NOT NULL,
    [rate]          DECIMAL (29, 16) NOT NULL,
    [UpdatedOnUTC]  DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.CurrencyRates] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [CurrRate_ftd_idx]
    ON [dbo].[CurrencyRates]([TradeDateTime] ASC, [FromTo] ASC);

