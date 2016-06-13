USE [master]
GO
/****** Object:  Database [gzDbProd]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE DATABASE [gzDbProd]
GO
ALTER DATABASE [gzDbProd] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [gzDbProd].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [gzDbProd] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [gzDbProd] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [gzDbProd] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [gzDbProd] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [gzDbProd] SET ARITHABORT OFF 
GO
ALTER DATABASE [gzDbProd] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [gzDbProd] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [gzDbProd] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [gzDbProd] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [gzDbProd] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [gzDbProd] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [gzDbProd] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [gzDbProd] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [gzDbProd] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [gzDbProd] SET  ENABLE_BROKER 
GO
ALTER DATABASE [gzDbProd] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [gzDbProd] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [gzDbProd] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [gzDbProd] SET ALLOW_SNAPSHOT_ISOLATION ON 
GO
ALTER DATABASE [gzDbProd] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [gzDbProd] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [gzDbProd] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [gzDbProd] SET RECOVERY FULL 
GO
ALTER DATABASE [gzDbProd] SET  MULTI_USER 
GO
ALTER DATABASE [gzDbProd] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [gzDbProd] SET DB_CHAINING OFF 
GO
USE [gzDbProd]
GO
/****** Object:  Table [dbo].[__MigrationHistory]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[__MigrationHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ContextKey] [nvarchar](300) NOT NULL,
	[Model] [varbinary](max) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC,
	[ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](30) NOT NULL,
	[LastName] [nvarchar](30) NOT NULL,
	[Birthday] [datetime] NOT NULL,
	[ActiveCustomerIdInPlatform] [bit] NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[Currency] [nvarchar](max) NOT NULL,
	[GmCustomerId] [int] NULL,
	[DisabledGzCustomer] [bit] NOT NULL DEFAULT ((0)),
	[ClosedGzAccount] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[CurrencyListXes]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CurrencyListXes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[From] [nvarchar](max) NOT NULL,
	[To] [nvarchar](max) NOT NULL,
	[UpdatedOnUTC] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CurrencyListXes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[CurrencyRates]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CurrencyRates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UpdatedOnUTC] [datetime] NOT NULL,
	[rate] [decimal](29, 16) NOT NULL,
	[TradeDateTime] [datetime] NOT NULL DEFAULT ('1900-01-01T00:00:00.000'),
	[FromTo] [nvarchar](6) NOT NULL DEFAULT (''),
 CONSTRAINT [PK_dbo.CurrencyRates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[CustFundShares]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustFundShares](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[YearMonth] [nvarchar](6) NOT NULL,
	[FundId] [int] NOT NULL,
	[UpdatedOnUTC] [datetime] NOT NULL,
	[SharesNum] [decimal](29, 16) NOT NULL DEFAULT ((0)),
	[SharesValue] [decimal](29, 16) NOT NULL DEFAULT ((0)),
	[SharesFundPriceId] [int] NULL,
	[NewSharesNum] [decimal](29, 16) NULL,
	[NewSharesValue] [decimal](29, 16) NULL,
	[SoldVintageId] [int] NULL,
 CONSTRAINT [PK_dbo.CustFundShares] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[CustPortfolios]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustPortfolios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[YearMonth] [nvarchar](6) NOT NULL,
	[PortfolioId] [int] NOT NULL,
	[Weight] [real] NOT NULL,
	[UpdatedOnUTC] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CustPortfolios] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[EmailTemplates]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailTemplates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](max) NOT NULL,
	[Subject] [nvarchar](max) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.EmailTemplates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[FundPrices]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FundPrices](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FundId] [int] NOT NULL,
	[ClosingPrice] [real] NOT NULL,
	[YearMonthDay] [nvarchar](8) NULL,
	[UpdatedOnUTC] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.FundPrices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[Funds]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Funds](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Symbol] [nvarchar](10) NULL,
	[HoldingName] [nvarchar](128) NOT NULL,
	[UpdatedOnUTC] [datetime] NOT NULL,
	[YearToDate] [real] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_dbo.Funds] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[GmTrxs]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GmTrxs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[YearMonthCtd] [nvarchar](6) NULL,
	[TypeId] [int] NOT NULL,
	[CreatedOnUtc] [datetime] NOT NULL,
	[Amount] [decimal](29, 16) NOT NULL,
	[GmCustomerId] [int] NULL,
	[CustomerEmail] [nvarchar](256) NULL,
 CONSTRAINT [PK_dbo.GmTrxs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[GmTrxTypes]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GmTrxTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [int] NOT NULL,
	[Description] [nvarchar](300) NOT NULL,
 CONSTRAINT [PK_dbo.GmTrxTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[GzConfigurations]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GzConfigurations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LOCK_IN_NUM_DAYS] [int] NOT NULL,
	[COMMISSION_PCNT] [real] NOT NULL,
	[FUND_FEE_PCNT] [real] NOT NULL,
	[CREDIT_LOSS_PCNT] [real] NOT NULL,
 CONSTRAINT [PK_dbo.GzConfigurations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[GzTrxs]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GzTrxs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[YearMonthCtd] [nvarchar](6) NOT NULL,
	[TypeId] [int] NOT NULL,
	[CreatedOnUTC] [datetime] NOT NULL,
	[Amount] [decimal](29, 16) NOT NULL,
	[CreditPcntApplied] [real] NULL,
	[ParentTrxId] [int] NULL,
	[GmTrxId] [int] NULL,
 CONSTRAINT [PK_dbo.GzTrxs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[GzTrxTypes]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GzTrxTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [int] NOT NULL,
	[Description] [nvarchar](300) NOT NULL,
 CONSTRAINT [PK_dbo.GzTrxTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[InvBalances]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InvBalances](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Balance] [decimal](29, 16) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[YearMonth] [nchar](6) NOT NULL,
	[UpdatedOnUTC] [datetime] NOT NULL,
	[InvGainLoss] [decimal](29, 16) NOT NULL CONSTRAINT [DF_dbo.InvBalances_InvGainLoss]  DEFAULT ((0)),
	[CashInvestment] [decimal](29, 16) NOT NULL DEFAULT ((0)),
	[CashBalance] [decimal](29, 16) NULL,
 CONSTRAINT [PK_dbo.InvBalances] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[LogEntries]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogEntries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Application] [nvarchar](max) NOT NULL,
	[Logged] [datetime] NOT NULL,
	[Level] [nvarchar](max) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[UserName] [nvarchar](250) NULL,
	[ServerName] [nvarchar](max) NULL,
	[Port] [nvarchar](max) NULL,
	[Url] [nvarchar](max) NULL,
	[Https] [bit] NOT NULL,
	[ServerAddress] [nvarchar](max) NULL,
	[RemoteAddress] [nvarchar](max) NULL,
	[Logger] [nvarchar](max) NULL,
	[CallSite] [nvarchar](max) NULL,
	[Exception] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.LogEntries] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[Portfolios]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Portfolios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RiskTolerance] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Color] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Portfolios] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[PortFunds]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PortFunds](
	[PortfolioId] [int] NOT NULL,
	[FundId] [int] NOT NULL,
	[Weight] [real] NOT NULL,
 CONSTRAINT [PK_dbo.PortFunds] PRIMARY KEY CLUSTERED 
(
	[PortfolioId] ASC,
	[FundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SoldVintages]    Script Date: 6/14/2016 12:01:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SoldVintages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[VintageYearMonth] [nvarchar](6) NOT NULL,
	[MarketAmount] [decimal](29, 16) NOT NULL,
	[Fees] [decimal](29, 16) NOT NULL,
	[YearMonth] [nvarchar](6) NOT NULL DEFAULT (''),
	[UpdatedOnUtc] [datetime] NOT NULL DEFAULT ('1900-01-01T00:00:00.000'),
 CONSTRAINT [PK_dbo.SoldVintages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [RoleNameIndex]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_UserId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_UserId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_RoleId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_UserId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserRoles]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_UQ_GmCustomerId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_UQ_GmCustomerId] ON [dbo].[AspNetUsers]
(
	[GmCustomerId] ASC
)
WHERE ([GmCustomerId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [UserNameIndex]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [CurrRate_ftd_idx]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [CurrRate_ftd_idx] ON [dbo].[CurrencyRates]
(
	[TradeDateTime] ASC,
	[FromTo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [CustFundShareId_YMD_idx]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [CustFundShareId_YMD_idx] ON [dbo].[CustFundShares]
(
	[CustomerId] ASC,
	[YearMonth] ASC,
	[FundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_SharesFundPriceId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_SharesFundPriceId] ON [dbo].[CustFundShares]
(
	[SharesFundPriceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_SoldVintageId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_SoldVintageId] ON [dbo].[CustFundShares]
(
	[SoldVintageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [CustomerId_Mon_idx_custp]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [CustomerId_Mon_idx_custp] ON [dbo].[CustPortfolios]
(
	[CustomerId] ASC,
	[YearMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_PortfolioId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_PortfolioId] ON [dbo].[CustPortfolios]
(
	[PortfolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [FundId_YMD_idx]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [FundId_YMD_idx] ON [dbo].[FundPrices]
(
	[FundId] ASC,
	[YearMonthDay] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_HoldingName]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_HoldingName] ON [dbo].[Funds]
(
	[HoldingName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Symbol]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Symbol] ON [dbo].[Funds]
(
	[Symbol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CustomerEmail]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_CustomerEmail] ON [dbo].[GmTrxs]
(
	[CustomerEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_CustomerId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_CustomerId] ON [dbo].[GmTrxs]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_GmCustomerId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_GmCustomerId] ON [dbo].[GmTrxs]
(
	[GmCustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_TypeId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_TypeId] ON [dbo].[GmTrxs]
(
	[TypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_YearMonthCtd]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_YearMonthCtd] ON [dbo].[GmTrxs]
(
	[YearMonthCtd] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_Code]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Code] ON [dbo].[GmTrxTypes]
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [CustomerId_Mon_idx_gztransaction]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [CustomerId_Mon_idx_gztransaction] ON [dbo].[GzTrxs]
(
	[CustomerId] ASC,
	[YearMonthCtd] ASC,
	[TypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_GmTrxId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_GmTrxId] ON [dbo].[GzTrxs]
(
	[GmTrxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_ParentTrxId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_ParentTrxId] ON [dbo].[GzTrxs]
(
	[ParentTrxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_Code]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Code] ON [dbo].[GzTrxTypes]
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [CustomerId_Mon_idx_invbal]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [CustomerId_Mon_idx_invbal] ON [dbo].[InvBalances]
(
	[CustomerId] ASC,
	[YearMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_RiskTolerance]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_RiskTolerance] ON [dbo].[Portfolios]
(
	[RiskTolerance] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_FundId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_FundId] ON [dbo].[PortFunds]
(
	[FundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
/****** Object:  Index [IX_PortfolioId]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE NONCLUSTERED INDEX [IX_PortfolioId] ON [dbo].[PortFunds]
(
	[PortfolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [CustomerId_Mon_idx_gzSoldVintage]    Script Date: 6/14/2016 12:01:22 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [CustomerId_Mon_idx_gzSoldVintage] ON [dbo].[SoldVintages]
(
	[CustomerId] ASC,
	[VintageYearMonth] ASC,
	[YearMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CustFundShares]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CustFundShares_dbo.FundPrices_SharesFundPriceId] FOREIGN KEY([SharesFundPriceId])
REFERENCES [dbo].[FundPrices] ([Id])
GO
ALTER TABLE [dbo].[CustFundShares] CHECK CONSTRAINT [FK_dbo.CustFundShares_dbo.FundPrices_SharesFundPriceId]
GO
ALTER TABLE [dbo].[CustFundShares]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CustFundShares_dbo.Funds_FundId] FOREIGN KEY([FundId])
REFERENCES [dbo].[Funds] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustFundShares] CHECK CONSTRAINT [FK_dbo.CustFundShares_dbo.Funds_FundId]
GO
ALTER TABLE [dbo].[CustFundShares]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CustFundShares_dbo.SoldVintages_SoldVintageId] FOREIGN KEY([SoldVintageId])
REFERENCES [dbo].[SoldVintages] ([Id])
GO
ALTER TABLE [dbo].[CustFundShares] CHECK CONSTRAINT [FK_dbo.CustFundShares_dbo.SoldVintages_SoldVintageId]
GO
ALTER TABLE [dbo].[CustPortfolios]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CustPortfolios_dbo.AspNetUsers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustPortfolios] CHECK CONSTRAINT [FK_dbo.CustPortfolios_dbo.AspNetUsers_CustomerId]
GO
ALTER TABLE [dbo].[CustPortfolios]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CustPortfolios_dbo.Portfolios_PortfolioId] FOREIGN KEY([PortfolioId])
REFERENCES [dbo].[Portfolios] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CustPortfolios] CHECK CONSTRAINT [FK_dbo.CustPortfolios_dbo.Portfolios_PortfolioId]
GO
ALTER TABLE [dbo].[FundPrices]  WITH NOCHECK ADD  CONSTRAINT [FK_dbo.FundPrices_dbo.Funds_FundId] FOREIGN KEY([FundId])
REFERENCES [dbo].[Funds] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FundPrices] CHECK CONSTRAINT [FK_dbo.FundPrices_dbo.Funds_FundId]
GO
ALTER TABLE [dbo].[GmTrxs]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GmTrxs_dbo.AspNetUsers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[GmTrxs] CHECK CONSTRAINT [FK_dbo.GmTrxs_dbo.AspNetUsers_CustomerId]
GO
ALTER TABLE [dbo].[GmTrxs]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GmTrxs_dbo.GmTrxTypes_TypeId] FOREIGN KEY([TypeId])
REFERENCES [dbo].[GmTrxTypes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GmTrxs] CHECK CONSTRAINT [FK_dbo.GmTrxs_dbo.GmTrxTypes_TypeId]
GO
ALTER TABLE [dbo].[GzTrxs]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GzTransactions_dbo.AspNetUsers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GzTrxs] CHECK CONSTRAINT [FK_dbo.GzTransactions_dbo.AspNetUsers_CustomerId]
GO
ALTER TABLE [dbo].[GzTrxs]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactions_ParentTrxId] FOREIGN KEY([ParentTrxId])
REFERENCES [dbo].[GzTrxs] ([Id])
GO
ALTER TABLE [dbo].[GzTrxs] CHECK CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactions_ParentTrxId]
GO
ALTER TABLE [dbo].[GzTrxs]  WITH NOCHECK ADD  CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactionTypes_TypeId] FOREIGN KEY([TypeId])
REFERENCES [dbo].[GzTrxTypes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GzTrxs] NOCHECK CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactionTypes_TypeId]
GO
ALTER TABLE [dbo].[GzTrxs]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GzTrxs_dbo.GmTrxs_GmTrxId] FOREIGN KEY([GmTrxId])
REFERENCES [dbo].[GmTrxs] ([Id])
GO
ALTER TABLE [dbo].[GzTrxs] CHECK CONSTRAINT [FK_dbo.GzTrxs_dbo.GmTrxs_GmTrxId]
GO
ALTER TABLE [dbo].[InvBalances]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InvBalances_dbo.AspNetUsers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InvBalances] CHECK CONSTRAINT [FK_dbo.InvBalances_dbo.AspNetUsers_CustomerId]
GO
ALTER TABLE [dbo].[PortFunds]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PortFunds_dbo.Funds_FundId] FOREIGN KEY([FundId])
REFERENCES [dbo].[Funds] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PortFunds] CHECK CONSTRAINT [FK_dbo.PortFunds_dbo.Funds_FundId]
GO
ALTER TABLE [dbo].[PortFunds]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PortFunds_dbo.Portfolios_PortfolioId] FOREIGN KEY([PortfolioId])
REFERENCES [dbo].[Portfolios] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PortFunds] CHECK CONSTRAINT [FK_dbo.PortFunds_dbo.Portfolios_PortfolioId]
GO
ALTER TABLE [dbo].[SoldVintages]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SoldVintages_dbo.AspNetUsers_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SoldVintages] CHECK CONSTRAINT [FK_dbo.SoldVintages_dbo.AspNetUsers_CustomerId]
GO
USE [master]
GO
ALTER DATABASE [gzDbProd] SET  READ_WRITE 
GO
