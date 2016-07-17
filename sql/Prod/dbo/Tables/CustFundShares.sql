CREATE TABLE [dbo].[CustFundShares] (
    [Id]                INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]        INT              NOT NULL,
    [YearMonth]         NVARCHAR (6)     NOT NULL,
    [FundId]            INT              NOT NULL,
    [UpdatedOnUTC]      DATETIME         NOT NULL,
    [SharesNum]         DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [SharesValue]       DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [SharesFundPriceId] INT              NULL,
    [NewSharesNum]      DECIMAL (29, 16) NULL,
    [NewSharesValue]    DECIMAL (29, 16) NULL,
    [SoldVintageId]     INT              NULL,
    CONSTRAINT [PK_dbo.CustFundShares] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.CustFundShares_dbo.FundPrices_SharesFundPriceId] FOREIGN KEY ([SharesFundPriceId]) REFERENCES [dbo].[FundPrices] ([Id]),
    CONSTRAINT [FK_dbo.CustFundShares_dbo.Funds_FundId] FOREIGN KEY ([FundId]) REFERENCES [dbo].[Funds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.CustFundShares_dbo.SoldVintages_SoldVintageId] FOREIGN KEY ([SoldVintageId]) REFERENCES [dbo].[SoldVintages] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [CustFundShareId_YMD_idx]
    ON [dbo].[CustFundShares]([CustomerId] ASC, [YearMonth] ASC, [FundId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SharesFundPriceId]
    ON [dbo].[CustFundShares]([SharesFundPriceId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SoldVintageId]
    ON [dbo].[CustFundShares]([SoldVintageId] ASC);

