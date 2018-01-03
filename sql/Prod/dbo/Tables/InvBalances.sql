CREATE TABLE [dbo].[InvBalances] (
    [Id]                     INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]             INT              NOT NULL,
    [YearMonth]              CHAR (6)         NOT NULL,
    [Balance]                DECIMAL (29, 16) NOT NULL,
    [InvGainLoss]            DECIMAL (29, 16) NOT NULL,
    [CashInvestment]         DECIMAL (29, 16) NOT NULL,
    [UpdatedOnUTC]           DATETIME         NOT NULL,
    [Sold]                   BIT              DEFAULT ((0)) NOT NULL,
    [SoldYearMonth]          CHAR (6)         NULL,
    [SoldAmount]             DECIMAL (29, 16) NULL,
    [SoldFees]               DECIMAL (29, 16) NULL,
    [SoldOnUtc]              DATETIME         NULL,
    [BegGmBalance]           DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [Deposits]               DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [Withdrawals]            DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [GmGainLoss]             DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [EndGmBalance]           DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [PortfolioId]            INT              DEFAULT ((3)) NOT NULL,
    [LowRiskShares]          DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [MediumRiskShares]       DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [HighRiskShares]         DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [TotalCashInvestments]   DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [TotalSoldVintagesValue] DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [TotalCashInvInHold]     DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [CashBonusAmount]        DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [Vendor2UserDeposits]    DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [AwardedSoldAmount]      BIT              DEFAULT ((0)) NOT NULL,
    [EarlyCashoutFee]        DECIMAL (29, 16) NULL,
    [HurdleFee]              DECIMAL (29, 16) NULL,
    CONSTRAINT [PK_dbo.InvBalances] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.InvBalances_dbo.AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.InvBalances_dbo.Portfolios_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [dbo].[Portfolios] ([Id])
);


















GO
CREATE UNIQUE NONCLUSTERED INDEX [CustomerId_Mon_idx_invbal]
    ON [dbo].[InvBalances]([CustomerId] ASC, [YearMonth] ASC);


GO
CREATE NONCLUSTERED INDEX [CustomerId_Only_idx_invbal]
    ON [dbo].[InvBalances]([CustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [YearMonth_Only_idx_invbal]
    ON [dbo].[InvBalances]([YearMonth] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_InvBalance_Cust_YM_Sold]
    ON [dbo].[InvBalances]([CustomerId] ASC, [YearMonth] ASC, [Sold] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_InvBalance_Cust_YM_CashInv]
    ON [dbo].[InvBalances]([CustomerId] ASC, [YearMonth] ASC, [CashInvestment] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_InvBalance_Cust_SoldYM_Sold]
    ON [dbo].[InvBalances]([CustomerId] ASC, [Sold] ASC, [SoldYearMonth] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PortfolioId]
    ON [dbo].[InvBalances]([PortfolioId] ASC);

