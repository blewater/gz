CREATE TABLE [dbo].[CurrencyRates] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [TradeDateTime] DATETIME         NOT NULL,
    [FromTo]        CHAR (6)         NOT NULL,
    [rate]          DECIMAL (29, 16) NOT NULL,
    [UpdatedOnUTC]  DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.CurrencyRates] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_CurrRate_FT_TDT]
    ON [dbo].[CurrencyRates]([FromTo] ASC, [TradeDateTime] DESC)
    INCLUDE([rate]);

