CREATE TABLE [dbo].[PlayerRevRpt] (
    [ID]                                   INT           IDENTITY (1, 1) NOT NULL,
    [User ID]                              INT           NOT NULL,
    [Username]                             VARCHAR (256) NOT NULL,
    [YearMonth]                            CHAR (6)      NOT NULL,
    [YearMonthDay]                         CHAR (8)      NOT NULL,
    [Role]                                 VARCHAR (256) NULL,
    [Player status]                        VARCHAR (256) NOT NULL,
    [Block reason]                         VARCHAR (256) NULL,
    [Email address]                        VARCHAR (256) NOT NULL,
    [Last login]                           DATETIME      NULL,
    [Accepts bonuses]                      BIT           CONSTRAINT [DF_PlayerRevRpt_Accepts bonuses] DEFAULT ((1)) NOT NULL,
    [Total deposits amount]                MONEY         NOT NULL,
    [Last played date]                     DATE          NULL,
    [Deposits made]                        MONEY         CONSTRAINT [DF_PlayerRevRpt_Deposits made] DEFAULT ((0)) NOT NULL,
    [Currency]                             CHAR (3)      NOT NULL,
    [Total bonuses accepted by the player] MONEY         CONSTRAINT [DF_PlayerRevRpt_Total bonuses accepted by the player] DEFAULT ((0)) NOT NULL,
    [Net revenue]                          MONEY         NOT NULL,
    [Gross revenue]                        MONEY         NOT NULL,
    [NetLossInUSD]                         MONEY         NOT NULL,
    [Real money balance]                   MONEY         NOT NULL,
    [Processed]                            BIT           NOT NULL,
    [CreatedOnUtc]                         DATETIME      NOT NULL,
    [UpdatedOnUtc]                         DATETIME      NOT NULL,
    CONSTRAINT [PK_PlayerRevRpt_ID] PRIMARY KEY NONCLUSTERED ([ID] ASC)
);






GO


