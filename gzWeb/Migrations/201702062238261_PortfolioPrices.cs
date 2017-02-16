namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PortfolioPrices : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PortfolioPrices", "PortfolioId", "dbo.Portfolios");
            DropForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioLowPriceId", "dbo.PortfolioPrices");
            DropForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioMediumPriceId", "dbo.PortfolioPrices");
            DropIndex("dbo.CustPortfoliosShares", new[] { "SoldPortfolioLowPriceId" });
            DropIndex("dbo.CustPortfoliosShares", new[] { "SoldPortfolioMediumPriceId" });
            DropIndex("dbo.PortfolioPrices", "IDX_PortfolioPrice_Id_YMD");
            RenameColumn(table: "dbo.CustPortfoliosShares", name: "SoldPortfolioHighPriceId", newName: "SoldPortfolioPriceId");
            RenameIndex(table: "dbo.CustPortfoliosShares", name: "IX_SoldPortfolioHighPriceId", newName: "IX_SoldPortfolioPriceId");
            AddColumn("dbo.PortfolioPrices", "PortfolioLowPrice", c => c.Single(nullable: false));
            AddColumn("dbo.PortfolioPrices", "PortfolioMediumPrice", c => c.Single(nullable: false));
            AddColumn("dbo.PortfolioPrices", "PortfolioHighPrice", c => c.Single(nullable: false));
            CreateIndex("dbo.PortfolioPrices", "YearMonthDay", unique: true, name: "IDX_PortfolioPrices_YMD");
            DropColumn("dbo.CustPortfoliosShares", "SoldPortfolioLowPriceId");
            DropColumn("dbo.CustPortfoliosShares", "SoldPortfolioMediumPriceId");
            DropColumn("dbo.PortfolioPrices", "PortfolioId");
            DropColumn("dbo.PortfolioPrices", "Price");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PortfolioPrices", "Price", c => c.Single(nullable: false));
            AddColumn("dbo.PortfolioPrices", "PortfolioId", c => c.Int(nullable: false));
            AddColumn("dbo.CustPortfoliosShares", "SoldPortfolioMediumPriceId", c => c.Int());
            AddColumn("dbo.CustPortfoliosShares", "SoldPortfolioLowPriceId", c => c.Int());
            DropIndex("dbo.PortfolioPrices", "IDX_PortfolioPrices_YMD");
            DropColumn("dbo.PortfolioPrices", "PortfolioHighPrice");
            DropColumn("dbo.PortfolioPrices", "PortfolioMediumPrice");
            DropColumn("dbo.PortfolioPrices", "PortfolioLowPrice");
            RenameIndex(table: "dbo.CustPortfoliosShares", name: "IX_SoldPortfolioPriceId", newName: "IX_SoldPortfolioHighPriceId");
            RenameColumn(table: "dbo.CustPortfoliosShares", name: "SoldPortfolioPriceId", newName: "SoldPortfolioHighPriceId");
            CreateIndex("dbo.PortfolioPrices", new[] { "PortfolioId", "YearMonthDay" }, unique: true, name: "IDX_PortfolioPrice_Id_YMD");
            CreateIndex("dbo.CustPortfoliosShares", "SoldPortfolioMediumPriceId");
            CreateIndex("dbo.CustPortfoliosShares", "SoldPortfolioLowPriceId");
            AddForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioMediumPriceId", "dbo.PortfolioPrices", "Id");
            AddForeignKey("dbo.CustPortfoliosShares", "SoldPortfolioLowPriceId", "dbo.PortfolioPrices", "Id");
            AddForeignKey("dbo.PortfolioPrices", "PortfolioId", "dbo.Portfolios", "Id", cascadeDelete: true);
        }
    }
}
