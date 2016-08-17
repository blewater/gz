CREATE TABLE [dbo].[GzTrxs] (
    [Id]                INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]        INT              NOT NULL,
    [YearMonthCtd]      CHAR (6)         NOT NULL,
    [TypeId]            INT              NOT NULL,
    [CreditPcntApplied] REAL             NULL,
    [CreatedOnUTC]      DATETIME         NOT NULL,
    [Amount]            DECIMAL (29, 16) NOT NULL,
    [ParentTrxId]       INT              NULL,
    [GmTrxId]           INT              NULL,
    CONSTRAINT [PK_dbo.GzTrxs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.GzTransactions_dbo.AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactions_ParentTrxId] FOREIGN KEY ([ParentTrxId]) REFERENCES [dbo].[GzTrxs] ([Id]),
    CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactionTypes_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[GzTrxTypes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.GzTrxs_dbo.GmTrxs_GmTrxId] FOREIGN KEY ([GmTrxId]) REFERENCES [dbo].[GmTrxs] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_CustomerId_TId_Amnt]
    ON [dbo].[GzTrxs]([CustomerId] ASC, [TypeId] ASC, [Amount] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ParentTrxId]
    ON [dbo].[GzTrxs]([ParentTrxId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_GmTrxId]
    ON [dbo].[GzTrxs]([GmTrxId] ASC);


GO



GO
CREATE NONCLUSTERED INDEX [IX_CustomerId_YM_TId_Amnt]
    ON [dbo].[GzTrxs]([CustomerId] ASC, [YearMonthCtd] ASC, [TypeId] ASC, [Amount] ASC);

