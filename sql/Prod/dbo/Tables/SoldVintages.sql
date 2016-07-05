CREATE TABLE [dbo].[SoldVintages] (
    [Id]               INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]       INT              NOT NULL,
    [VintageYearMonth] NVARCHAR (6)     NOT NULL,
    [MarketAmount]     DECIMAL (29, 16) NOT NULL,
    [Fees]             DECIMAL (29, 16) NOT NULL,
    [YearMonth]        NVARCHAR (6)     DEFAULT ('') NOT NULL,
    [UpdatedOnUtc]     DATETIME         DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    CONSTRAINT [PK_dbo.SoldVintages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.SoldVintages_dbo.AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [CustomerId_Mon_idx_gzSoldVintage]
    ON [dbo].[SoldVintages]([CustomerId] ASC, [VintageYearMonth] ASC, [YearMonth] ASC);

