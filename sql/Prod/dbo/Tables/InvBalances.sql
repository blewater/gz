CREATE TABLE [dbo].[InvBalances] (
    [Id]             INT              IDENTITY (1, 1) NOT NULL,
    [Balance]        DECIMAL (29, 16) NOT NULL,
    [CustomerId]     INT              NOT NULL,
    [YearMonth]      NCHAR (6)        NOT NULL,
    [UpdatedOnUTC]   DATETIME         NOT NULL,
    [InvGainLoss]    DECIMAL (29, 16) CONSTRAINT [DF_dbo.InvBalances_InvGainLoss] DEFAULT ((0)) NOT NULL,
    [CashInvestment] DECIMAL (29, 16) DEFAULT ((0)) NOT NULL,
    [CashBalance]    DECIMAL (29, 16) NULL,
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
CREATE NONCLUSTERED INDEX [YearMonth_Only_idx_invbal]
    ON [dbo].[InvBalances]([YearMonth] ASC);

