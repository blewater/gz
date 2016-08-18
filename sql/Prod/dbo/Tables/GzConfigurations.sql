CREATE TABLE [dbo].[GzConfigurations] (
    [Id]                       INT  IDENTITY (1, 1) NOT NULL,
    [LOCK_IN_NUM_DAYS]         INT  NOT NULL,
    [COMMISSION_PCNT]          REAL NOT NULL,
    [FUND_FEE_PCNT]            REAL NOT NULL,
    [CREDIT_LOSS_PCNT]         REAL NOT NULL,
    [FIRST_PORTFOLIO_RISK_VAL] INT  DEFAULT ((3)) NOT NULL,
    CONSTRAINT [PK_dbo.GzConfigurations] PRIMARY KEY CLUSTERED ([Id] ASC)
);



