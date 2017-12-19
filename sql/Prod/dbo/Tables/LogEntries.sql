CREATE TABLE [dbo].[LogEntries] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Application]   NVARCHAR (MAX) NOT NULL,
    [Logged]        DATETIME       NOT NULL,
    [Level]         NVARCHAR (MAX) NOT NULL,
    [Message]       NVARCHAR (MAX) NOT NULL,
    [UserName]      NVARCHAR (250) NULL,
    [ServerName]    NVARCHAR (MAX) NULL,
    [Port]          NVARCHAR (MAX) NULL,
    [Url]           NVARCHAR (MAX) NULL,
    [Https]         BIT            NOT NULL,
    [ServerAddress] NVARCHAR (MAX) NULL,
    [RemoteAddress] NVARCHAR (MAX) NULL,
    [Logger]        NVARCHAR (MAX) NULL,
    [CallSite]      NVARCHAR (MAX) NULL,
    [Exception]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.LogEntries] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
GRANT SELECT
    ON OBJECT::[dbo].[LogEntries] TO [gzAdminUser]
    AS [dbo];


GO
CREATE NONCLUSTERED INDEX [nci_wi_LogEntries_E1B1E14B56A7BCEEFE774F1347CA3074]
    ON [dbo].[LogEntries]([Logged] ASC)
    INCLUDE([Level]);

