CREATE TABLE [dbo].[PlayerRevRpt] (
    [ID]                    INT           IDENTITY (1, 1) NOT NULL,
    [User ID]               INT           NOT NULL,
    [Username]              VARCHAR (256) NOT NULL,
    [YearMonth]             CHAR (6)      NOT NULL,
    [YearMonthDay]          CHAR (8)      NOT NULL,
    [Role]                  VARCHAR (256) NULL,
    [Player status]         VARCHAR (256) NOT NULL,
    [Block reason]          VARCHAR (256) NULL,
    [Email address]         VARCHAR (256) NOT NULL,
    [Last login]            DATETIME      NULL,
    [Total deposits amount] MONEY         NOT NULL,
    [Currency]              CHAR (3)      NOT NULL,
    [Net revenue]           MONEY         NOT NULL,
    [Gross revenue]         MONEY         NOT NULL,
    [NetLossInUSD]          MONEY         NOT NULL,
    [Real money balance]    MONEY         NOT NULL,
    [Processed]             BIT           NOT NULL,
    [CreatedOnUtc]          DATETIME      NOT NULL,
    [UpdatedOnUtc]          DATETIME      NOT NULL,
    CONSTRAINT [PK_PlayerRevRpt_ID] PRIMARY KEY NONCLUSTERED ([ID] ASC)
);

