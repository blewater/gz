﻿CREATE TABLE [dbo].[PlayerRevRpt] (
    [Id]                                   INT              IDENTITY (1, 1) NOT NULL,
    [User ID]                              INT              NOT NULL,
    [Username]                             NVARCHAR (256)   NOT NULL,
    [YearMonth]                            CHAR (6)         NOT NULL,
    [YearMonthDay]                         CHAR (8)         NOT NULL,
    [BegBalance]                           DECIMAL (29, 16) NULL,
    [EndBalance]                           DECIMAL (29, 16) NULL,
    [PlayerLoss]                           DECIMAL (29, 16) NULL,
    [Role]                                 NVARCHAR (256)   NULL,
    [Player status]                        NVARCHAR (256)   NULL,
    [Block reason]                         NVARCHAR (256)   NULL,
    [Email address]                        NVARCHAR (256)   NOT NULL,
    [Last login]                           DATETIME         NULL,
    [Accepts bonuses]                      BIT              NULL,
    [Total deposits amount]                DECIMAL (29, 16) NULL,
    [Withdraws made]                       DECIMAL (29, 16) NULL,
    [Last played date]                     DATE             NULL,
    [Deposits made]                        DECIMAL (29, 16) NULL,
    [Currency]                             CHAR (3)         NOT NULL,
    [Total bonuses accepted by the player] DECIMAL (29, 16) NULL,
    [Net revenue]                          DECIMAL (29, 16) NULL,
    [Gross revenue]                        DECIMAL (29, 16) NULL,
    [NetLossInUSD]                         DECIMAL (29, 16) NULL,
    [Real money balance]                   DECIMAL (29, 16) NULL,
    [Processed]                            INT              NOT NULL,
    [CreatedOnUtc]                         DATETIME         NOT NULL,
    [UpdatedOnUtc]                         DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.PlayerRevRpt] PRIMARY KEY CLUSTERED ([Id] ASC)
);










GO
CREATE NONCLUSTERED INDEX [IX_YM]
    ON [dbo].[PlayerRevRpt]([YearMonth] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserId_YM]
    ON [dbo].[PlayerRevRpt]([User ID] ASC, [YearMonth] ASC);

