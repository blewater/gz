CREATE TABLE [dbo].[PlayerRevLastMonths] (
    [Id]           INT      DEFAULT ((1)) NOT NULL,
    [YearMonth]    CHAR (6) NOT NULL,
    [UpdatedOnUtc] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.PlayerRevLastMonths] PRIMARY KEY CLUSTERED ([Id] ASC)
);

