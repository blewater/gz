using gzDAL.Models;

namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustPortfoliosPortfolioPricePortfolioIdinInvBalance : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustPortfoliosShares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        YearMonth = c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false),
                        PortfolioLowShares = c.Decimal(nullable: false, precision: 29, scale: 16),
                        PortfolioMediumShares = c.Decimal(nullable: false, precision: 29, scale: 16),
                        PortfolioHighShares = c.Decimal(nullable: false, precision: 29, scale: 16),
                        BuyPortfolioLowPrice = c.Single(nullable: false),
                        BuyPortfolioMediumPrice = c.Single(nullable: false),
                        BuyPortfolioHighPrice = c.Single(nullable: false),
                        SoldPortfolioLowPriceId = c.Int(),
                        SoldPortfolioMediumPriceId = c.Int(),
                        SoldPortfolioHighPriceId = c.Int(),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PortfolioPrices", t => t.SoldPortfolioHighPriceId)
                .ForeignKey("dbo.PortfolioPrices", t => t.SoldPortfolioLowPriceId)
                .ForeignKey("dbo.PortfolioPrices", t => t.SoldPortfolioMediumPriceId)
                .Index(t => new { t.CustomerId, t.YearMonth }, unique: true, name: "CustPortfoliosShareId_YMD_idx")
                .Index(t => t.SoldPortfolioLowPriceId)
                .Index(t => t.SoldPortfolioMediumPriceId)
                .Index(t => t.SoldPortfolioHighPriceId);
            
            CreateTable(
                "dbo.PortfolioPrices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PortfolioId = c.Int(nullable: false),
                        YearMonthDay = c.String(nullable: false, maxLength: 8, fixedLength: true, unicode: false),
                        Price = c.Single(nullable: false),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Portfolios", t => t.PortfolioId, cascadeDelete: true)
                .Index(t => new { t.PortfolioId, t.YearMonthDay }, unique: true, name: "IDX_PortfolioPrice_Id_YMD");
            
            AddColumn("dbo.InvBalances", "PortfolioId", c => c.Int(nullable: false, defaultValue:(int)RiskToleranceEnum.Medium, defaultValueSql: RiskToleranceEnum.Medium.ToString()));
            CreateIndex("dbo.InvBalances", "PortfolioId");
            AddForeignKey("dbo.InvBalances", "PortfolioId", "dbo.Portfolios", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioMediumPriceId", "dbo.PortfolioPrices");
            DropForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioLowPriceId", "dbo.PortfolioPrices");
            DropForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioHighPriceId", "dbo.PortfolioPrices");
            DropForeignKey("dbo.PortfolioPrices", "PortfolioId", "dbo.Portfolios");
            DropForeignKey("dbo.InvBalances", "PortfolioId", "dbo.Portfolios");
            DropIndex("dbo.PortfolioPrices", "IDX_PortfolioPrice_Id_YMD");
            DropIndex("dbo.CustPortfoliosShares", new[] { "SoldPortfolioHighPriceId" });
            DropIndex("dbo.CustPortfoliosShares", new[] { "SoldPortfolioMediumPriceId" });
            DropIndex("dbo.CustPortfoliosShares", new[] { "SoldPortfolioLowPriceId" });
            DropIndex("dbo.CustPortfoliosShares", "CustPortfoliosShareId_YMD_idx");
            DropIndex("dbo.InvBalances", new[] { "PortfolioId" });
            DropColumn("dbo.InvBalances", "PortfolioId");
            DropTable("dbo.PortfolioPrices");
            DropTable("dbo.CustPortfoliosShares");
        }
    }
}
