CREATE TABLE [dbo].[CustFundShares] (
    [Id]                    INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]            INT              NOT NULL,
    [YearMonth]             CHAR (6)         NOT NULL,
    [FundId]                INT              NOT NULL,
    [UpdatedOnUTC]          DATETIME         NOT NULL,
    [SharesNum]             DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [SharesValue]           DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [SharesFundPriceId]     INT              NULL,
    [NewSharesNum]          DECIMAL (29, 16) NULL,
    [NewSharesValue]        DECIMAL (29, 16) NULL,
    [InvBalanceId]          INT              NULL,
    [SoldSharesValue]       DECIMAL (29, 16) NULL,
    [SoldSharesFundPriceId] INT              NULL,
    [SoldOnUtc]             DATETIME         NULL,
    CONSTRAINT [PK_dbo.CustFundShares] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.CustFundShares_dbo.FundPrices_SharesFundPriceId] FOREIGN KEY ([SharesFundPriceId]) REFERENCES [dbo].[FundPrices] ([Id]),
    CONSTRAINT [FK_dbo.CustFundShares_dbo.Funds_FundId] FOREIGN KEY ([FundId]) REFERENCES [dbo].[Funds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.CustFundShares_dbo.InvBalances_SoldInvBalanceId] FOREIGN KEY ([InvBalanceId]) REFERENCES [dbo].[InvBalances] ([Id])
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [CustFundShareId_YMD_idx]
    ON [dbo].[CustFundShares]([CustomerId] ASC, [YearMonth] ASC, [FundId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SharesFundPriceId]
    ON [dbo].[CustFundShares]([SharesFundPriceId] ASC);


GO


