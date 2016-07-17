CREATE TABLE [dbo].[CurrencyRates] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [UpdatedOnUTC]  DATETIME         NOT NULL,
    [rate]          DECIMAL (29, 16) NOT NULL,
    [TradeDateTime] DATETIME         DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    [FromTo]        NVARCHAR (6)     DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_dbo.CurrencyRates] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [CurrRate_ftd_idx]
    ON [dbo].[CurrencyRates]([TradeDateTime] ASC, [FromTo] ASC);

