CREATE TABLE [dbo].[azAspNetUsersDev] (
    [Id]                         INT           NULL,
    [FirstName]                  NVARCHAR (50) NULL,
    [LastName]                   NVARCHAR (50) NULL,
    [Birthday]                   NVARCHAR (50) NULL,
    [ActiveCustomerIdInPlatform] BIT           NULL,
    [Email]                      NVARCHAR (50) NULL,
    [EmailConfirmed]             BIT           NULL,
    [PasswordHash]               NVARCHAR (50) NULL,
    [SecurityStamp]              NVARCHAR (50) NULL,
    [PhoneNumber]                NVARCHAR (50) NULL,
    [PhoneNumberConfirmed]       BIT           NULL,
    [TwoFactorEnabled]           BIT           NULL,
    [LockoutEndDateUtc]          DATETIME      NULL,
    [LockoutEnabled]             BIT           NULL,
    [AccessFailedCount]          INT           NULL,
    [UserName]                   NVARCHAR (50) NULL,
    [Currency]                   NVARCHAR (50) NULL,
    [GmCustomerId]               INT           NULL,
    [DisabledGzCustomer]         BIT           NULL,
    [ClosedGzAccount]            BIT           NULL
);

