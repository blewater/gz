CREATE TABLE [dbo].[GmTrxs] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [CustomerId]    INT              NULL,
    [YearMonthCtd]  CHAR (6)         NULL,
    [TypeId]        INT              NOT NULL,
    [CreatedOnUtc]  DATETIME         NOT NULL,
    [Amount]        DECIMAL (29, 16) NOT NULL,
    [GmCustomerId]  INT              NULL,
    [CustomerEmail] NVARCHAR (256)   NULL,
    CONSTRAINT [PK_dbo.GmTrxs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.GmTrxs_dbo.AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_dbo.GmTrxs_dbo.GmTrxTypes_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[GmTrxTypes] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_CustomerEmail]
    ON [dbo].[GmTrxs]([CustomerEmail] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_GmCustomerId]
    ON [dbo].[GmTrxs]([GmCustomerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_YearMonthCtd]
    ON [dbo].[GmTrxs]([YearMonthCtd] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TypeId]
    ON [dbo].[GmTrxs]([TypeId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CustomerId]
    ON [dbo].[GmTrxs]([CustomerId] ASC);

