CREATE TABLE [dbo].[CustPortfolios] (
    [Id]           INT          IDENTITY (1, 1) NOT NULL,
    [CustomerId]   INT          NOT NULL,
    [YearMonth]    NVARCHAR (6) NOT NULL,
    [PortfolioId]  INT          NOT NULL,
    [Weight]       REAL         NOT NULL,
    [UpdatedOnUTC] DATETIME     NOT NULL,
    CONSTRAINT [PK_dbo.CustPortfolios] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.CustPortfolios_dbo.AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.CustPortfolios_dbo.Portfolios_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [dbo].[Portfolios] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [CustomerId_Mon_idx_custp]
    ON [dbo].[CustPortfolios]([CustomerId] ASC, [YearMonth] ASC, [PortfolioId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PortfolioId]
    ON [dbo].[CustPortfolios]([PortfolioId] ASC);

