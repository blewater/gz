CREATE TABLE [dbo].[PlayerRevRpt] (
    [Id]                    INT              IDENTITY (1, 1) NOT NULL,
    [User ID]               INT              NOT NULL,
    [Username]              NVARCHAR (256)   NOT NULL,
    [YearMonth]             CHAR (6)         NOT NULL,
    [YearMonthDay]          CHAR (8)         NOT NULL,
    [BegGmBalance]          DECIMAL (29, 16) NULL,
    [EndGmBalance]          DECIMAL (29, 16) NULL,
    [Email address]         NVARCHAR (256)   NOT NULL,
    [Total deposits amount] DECIMAL (29, 16) NULL,
    [Withdraws made]        DECIMAL (29, 16) NULL,
    [Currency]              CHAR (3)         NOT NULL,
    [Processed]             INT              NOT NULL,
    [CreatedOnUtc]          DATETIME         NOT NULL,
    [UpdatedOnUtc]          DATETIME         NOT NULL,
    [PendingWithdrawals]    DECIMAL (29, 16) NULL,
    [GmGainLoss]            DECIMAL (29, 16) NULL,
    [Player status]         NVARCHAR (256)   NULL,
    [Block reason]          NVARCHAR (256)   NULL,
    CONSTRAINT [PK_dbo.PlayerRevRpt] PRIMARY KEY CLUSTERED ([Id] ASC)
);
















GO
CREATE NONCLUSTERED INDEX [IX_YM]
    ON [dbo].[PlayerRevRpt]([YearMonth] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserId_YM]
    ON [dbo].[PlayerRevRpt]([User ID] ASC, [YearMonth] ASC);

