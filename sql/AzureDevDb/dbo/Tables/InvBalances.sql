CREATE TABLE [dbo].[InvBalances] (
    [Id]             INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]     INT              NOT NULL,
    [YearMonth]      CHAR (6)         NOT NULL,
    [Balance]        DECIMAL (29, 16) NOT NULL,
    [InvGainLoss]    DECIMAL (29, 16) NOT NULL,
    [CashInvestment] DECIMAL (29, 16) NOT NULL,
    [UpdatedOnUTC]   DATETIME         NOT NULL,
    [CashBalance]    DECIMAL (29, 16) NULL,
    [Sold]           BIT              DEFAULT ((0)) NOT NULL,
    [SoldYearMonth]  CHAR (6)         NULL,
    [SoldAmount]     DECIMAL (29, 16) NULL,
    [SoldFees]       DECIMAL (29, 16) NULL,
    [SoldOnUtc]      DATETIME         NULL,
    CONSTRAINT [PK_dbo.InvBalances] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.InvBalances_dbo.AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [CustomerId_Mon_idx_invbal]
    ON [dbo].[InvBalances]([CustomerId] ASC, [YearMonth] ASC);


GO
CREATE NONCLUSTERED INDEX [CustomerId_Only_idx_invbal]
    ON [dbo].[InvBalances]([CustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_InvBalance_Cust_SoldYM_Sold]
    ON [dbo].[InvBalances]([CustomerId] ASC, [Sold] ASC, [SoldYearMonth] ASC);


GO
CREATE NONCLUSTERED INDEX [YearMonth_Only_idx_invbal]
    ON [dbo].[InvBalances]([YearMonth] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_InvBalance_Cust_YM_CashInv]
    ON [dbo].[InvBalances]([CustomerId] ASC, [YearMonth] ASC, [CashInvestment] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_InvBalance_Cust_YM_Sold]
    ON [dbo].[InvBalances]([CustomerId] ASC, [YearMonth] ASC, [Sold] ASC);

