CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                         INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]                  NVARCHAR (30)  NOT NULL,
    [LastName]                   NVARCHAR (30)  NOT NULL,
    [Birthday]                   DATETIME       NOT NULL,
    [ActiveCustomerIdInPlatform] BIT            NOT NULL,
    [Email]                      NVARCHAR (256) NULL,
    [EmailConfirmed]             BIT            NOT NULL,
    [PasswordHash]               NVARCHAR (MAX) NULL,
    [SecurityStamp]              NVARCHAR (MAX) NULL,
    [PhoneNumber]                NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed]       BIT            NOT NULL,
    [TwoFactorEnabled]           BIT            NOT NULL,
    [LockoutEndDateUtc]          DATETIME       NULL,
    [LockoutEnabled]             BIT            NOT NULL,
    [AccessFailedCount]          INT            NOT NULL,
    [UserName]                   NVARCHAR (256) NOT NULL,
    [Currency]                   CHAR (3)       NOT NULL,
    [GmCustomerId]               INT            NULL,
    [DisabledGzCustomer]         BIT            DEFAULT ((0)) NOT NULL,
    [ClosedGzAccount]            BIT            DEFAULT ((0)) NOT NULL,
    [LastLogin]                  DATETIME       NULL,
    [IsRegistrationFinalized]    BIT            NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UQ_GmCustomerId]
    ON [dbo].[AspNetUsers]([GmCustomerId] ASC) WHERE ([GmCustomerId] IS NOT NULL);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([UserName] ASC);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[AspNetUsers] TO [gzAdminUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[AspNetUsers] TO [gzAdminUser]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[AspNetUsers] TO [gzAdminUser]
    AS [dbo];

