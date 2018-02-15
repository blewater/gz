CREATE TABLE [dbo].[InvAdjs] (
    [Id]           INT              IDENTITY (1, 1) NOT NULL,
    [YearMonth]    CHAR (6)         NOT NULL,
    [UserId]       INT              NOT NULL,
    [AmountType]   INT              NOT NULL,
    [Amount]       DECIMAL (29, 16) NOT NULL,
    [UpdatedOnUtc] DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.InvAdjs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.InvAdjs_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [Inv_Adjustment_Idx]
    ON [dbo].[InvAdjs]([YearMonth] ASC, [UserId] ASC);

