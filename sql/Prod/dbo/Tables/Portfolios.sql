CREATE TABLE [dbo].[Portfolios] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [RiskTolerance] INT            NOT NULL,
    [IsActive]      BIT            NOT NULL,
    [Title]         NVARCHAR (MAX) NULL,
    [Color]         NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.Portfolios] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RiskTolerance]
    ON [dbo].[Portfolios]([RiskTolerance] ASC);

