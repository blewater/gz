namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPortfolioShares : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioPriceId", "dbo.PortfolioPrices");
            DropIndex("dbo.CustPortfoliosShares", "CustPortfoliosShareId_YMD_idx");
            DropIndex("dbo.CustPortfoliosShares", new[] { "SoldPortfolioPriceId" });
            CreateTable(
                "dbo.UserPortfoliosShares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        YearMonth = c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false),
                        PortfolioLowShares = c.Decimal(nullable: false, precision: 29, scale: 16),
                        PortfolioMediumShares = c.Decimal(nullable: false, precision: 29, scale: 16),
                        PortfolioHighShares = c.Decimal(nullable: false, precision: 29, scale: 16),
                        BuyPortfolioTradeDay = c.DateTime(nullable: false),
                        SoldPortfolioTradeDay = c.DateTime(),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.YearMonth }, unique: true, name: "UserPortfoliosShareId_YMD_idx");
            
            DropTable("dbo.CustPortfoliosShares");
        }
        
        public override void Down()
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
                        SoldPortfolioPriceId = c.Int(),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropIndex("dbo.UserPortfoliosShares", "UserPortfoliosShareId_YMD_idx");
            DropTable("dbo.UserPortfoliosShares");
            CreateIndex("dbo.CustPortfoliosShares", "SoldPortfolioPriceId");
            CreateIndex("dbo.CustPortfoliosShares", new[] { "CustomerId", "YearMonth" }, unique: true, name: "CustPortfoliosShareId_YMD_idx");
            AddForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioPriceId", "dbo.PortfolioPrices", "Id");
        }
    }
}
