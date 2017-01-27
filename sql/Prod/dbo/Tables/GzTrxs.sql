CREATE TABLE [dbo].[GzTrxs] (
    [Id]                INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]        INT              NOT NULL,
    [YearMonthCtd]      CHAR (6)         NOT NULL,
    [TypeId]            INT              NOT NULL,
    [CreditPcntApplied] REAL             NULL,
    [CreatedOnUTC]      DATETIME         NOT NULL,
    [Amount]            DECIMAL (29, 16) NOT NULL,
    [PlayerRevRptId]    INT              NULL,
    [BegGmBalance]      DECIMAL (29, 16) DEFAULT ((0)) NULL,
    [Deposits]          DECIMAL (29, 16) DEFAULT ((0)) NULL,
    [Withdrawals]       DECIMAL (29, 16) DEFAULT ((0)) NULL,
    [EndGmBalance]      DECIMAL (29, 16) DEFAULT ((0)) NULL,
    CONSTRAINT [PK_dbo.GzTrxs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.GzTransactions_dbo.AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactionTypes_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[GzTrxTypes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.GzTrxs_dbo.PlayerRevRpt_PlayerRevRptId] FOREIGN KEY ([PlayerRevRptId]) REFERENCES [dbo].[PlayerRevRpt] ([ID])
);










GO



GO



GO



GO



GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CustomerId_YM_TId_Amnt]
    ON [dbo].[GzTrxs]([CustomerId] ASC, [YearMonthCtd] ASC, [TypeId] ASC, [Amount] ASC);




GO
CREATE NONCLUSTERED INDEX [IX_CustomerId_TId_Amnt]
    ON [dbo].[GzTrxs]([CustomerId] ASC, [TypeId] ASC, [Amount] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PlayerRevRptId]
    ON [dbo].[GzTrxs]([PlayerRevRptId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CustomerId_YM_TTyp]
    ON [dbo].[GzTrxs]([CustomerId] ASC, [YearMonthCtd] ASC, [TypeId] ASC);

