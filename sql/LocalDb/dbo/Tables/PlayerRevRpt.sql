CREATE TABLE [dbo].[PlayerRevRpt] (
    [RowNum]                INT           NOT NULL,
    [User ID]               BIGINT        NOT NULL,
    [Username]              VARCHAR (256) NOT NULL,
    [YearMonth]             CHAR (6)      NOT NULL,
    [YearMonthDay]          CHAR (8)      NOT NULL,
    [Role]                  VARCHAR (256) NULL,
    [Player status]         VARCHAR (256) NOT NULL,
    [Block reason]          VARCHAR (256) NULL,
    [Email address]         VARCHAR (256) NOT NULL,
    [Total deposits amount] MONEY         NOT NULL,
    [Currency]              CHAR (3)      NOT NULL,
    [Net revenue]           MONEY         NOT NULL,
    [Gross revenue]         MONEY         NOT NULL,
    [NetLossInUSD]          MONEY         NOT NULL,
    [Real money balance]    MONEY         NOT NULL,
    [CreatedOnUtc]          DATETIME      NOT NULL,
    [UpdatedOnUtc]          DATETIME      NOT NULL
);


GO
CREATE UNIQUE CLUSTERED INDEX [IDX_PlayerRevRpt_ID_YMD]
    ON [dbo].[PlayerRevRpt]([User ID] ASC, [YearMonthDay] DESC);

