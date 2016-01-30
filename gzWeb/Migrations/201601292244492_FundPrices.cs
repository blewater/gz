namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FundPrices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustPortfolios",
                c => new
                    {
                        CustomerId = c.Int(nullable: false),
                        YearMonthCtd = c.String(nullable: false, maxLength: 6),
                        PortfolioId = c.Int(nullable: false),
                        Weight = c.Single(nullable: false),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.CustomerId, t.YearMonthCtd })
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Portfolios", t => t.PortfolioId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.PortfolioId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 30),
                        LastName = c.String(nullable: false, maxLength: 30),
                        Birthday = c.DateTime(nullable: false),
                        PlatformCustomerId = c.Int(nullable: false),
                        ActiveCustomerIdInPlatform = c.Boolean(nullable: false),
                        GamBalance = c.Decimal(precision: 18, scale: 2),
                        GamBalanceUpdOnUTC = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PlatformCustomerId, unique: true)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.GzTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        YearMonthCtd = c.String(nullable: false, maxLength: 6),
                        TypeId = c.Int(nullable: false),
                        CreatedOnUTC = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.GzTransactionTypes", t => t.TypeId, cascadeDelete: true)
                .Index(t => new { t.CustomerId, t.YearMonthCtd }, name: "CustomerId_Mon_idx_gztransaction")
                .Index(t => t.TypeId)
                .Index(t => t.CreatedOnUTC);
            
            CreateTable(
                "dbo.GzTransactionTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.Int(nullable: false),
                        Description = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.InvBalances",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CustomerId = c.Int(nullable: false),
                        YearMonthCtd = c.String(maxLength: 6),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => new { t.CustomerId, t.YearMonthCtd }, unique: true, name: "CustomerId_Mon_idx_invbal");
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Portfolios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RiskTolerance = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.RiskTolerance, unique: true);
            
            CreateTable(
                "dbo.PortFunds",
                c => new
                    {
                        PortfolioId = c.Int(nullable: false),
                        FundId = c.Int(nullable: false),
                        Weight = c.Single(nullable: false),
                    })
                .PrimaryKey(t => new { t.PortfolioId, t.FundId })
                .ForeignKey("dbo.Funds", t => t.FundId, cascadeDelete: true)
                .ForeignKey("dbo.Portfolios", t => t.PortfolioId, cascadeDelete: true)
                .Index(t => t.PortfolioId)
                .Index(t => t.FundId);
            
            CreateTable(
                "dbo.Funds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Symbol = c.String(maxLength: 10),
                        HoldingName = c.String(nullable: false, maxLength: 128),
                        ThreeYrReturnPcnt = c.Single(nullable: false),
                        FiveYrReturnPcnt = c.Single(nullable: false),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Symbol, unique: true)
                .Index(t => t.HoldingName, unique: true);
            
            CreateTable(
                "dbo.FundPrices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FundId = c.Int(nullable: false),
                        ClosingPrice = c.Single(nullable: false),
                        YearMonthDay = c.String(maxLength: 8),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Funds", t => t.FundId, cascadeDelete: true)
                .Index(t => new { t.FundId, t.YearMonthDay }, unique: true, name: "FundId_YMD_idx");
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.CustPortfolios", "PortfolioId", "dbo.Portfolios");
            DropForeignKey("dbo.PortFunds", "PortfolioId", "dbo.Portfolios");
            DropForeignKey("dbo.PortFunds", "FundId", "dbo.Funds");
            DropForeignKey("dbo.FundPrices", "FundId", "dbo.Funds");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InvBalances", "CustomerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.GzTransactions", "TypeId", "dbo.GzTransactionTypes");
            DropForeignKey("dbo.GzTransactions", "CustomerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CustPortfolios", "CustomerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.FundPrices", "FundId_YMD_idx");
            DropIndex("dbo.Funds", new[] { "HoldingName" });
            DropIndex("dbo.Funds", new[] { "Symbol" });
            DropIndex("dbo.PortFunds", new[] { "FundId" });
            DropIndex("dbo.PortFunds", new[] { "PortfolioId" });
            DropIndex("dbo.Portfolios", new[] { "RiskTolerance" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.InvBalances", "CustomerId_Mon_idx_invbal");
            DropIndex("dbo.GzTransactionTypes", new[] { "Code" });
            DropIndex("dbo.GzTransactions", new[] { "CreatedOnUTC" });
            DropIndex("dbo.GzTransactions", new[] { "TypeId" });
            DropIndex("dbo.GzTransactions", "CustomerId_Mon_idx_gztransaction");
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "PlatformCustomerId" });
            DropIndex("dbo.CustPortfolios", new[] { "PortfolioId" });
            DropIndex("dbo.CustPortfolios", new[] { "CustomerId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.FundPrices");
            DropTable("dbo.Funds");
            DropTable("dbo.PortFunds");
            DropTable("dbo.Portfolios");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.InvBalances");
            DropTable("dbo.GzTransactionTypes");
            DropTable("dbo.GzTransactions");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.CustPortfolios");
        }
    }
}
