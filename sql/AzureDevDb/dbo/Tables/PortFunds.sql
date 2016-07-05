CREATE TABLE [dbo].[PortFunds] (
    [PortfolioId] INT  NOT NULL,
    [FundId]      INT  NOT NULL,
    [Weight]      REAL NOT NULL,
    CONSTRAINT [PK_dbo.PortFunds] PRIMARY KEY CLUSTERED ([PortfolioId] ASC, [FundId] ASC),
    CONSTRAINT [FK_dbo.PortFunds_dbo.Funds_FundId] FOREIGN KEY ([FundId]) REFERENCES [dbo].[Funds] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.PortFunds_dbo.Portfolios_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [dbo].[Portfolios] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PortfolioId]
    ON [dbo].[PortFunds]([PortfolioId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FundId]
    ON [dbo].[PortFunds]([FundId] ASC);

