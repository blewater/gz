namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IndexOptimizations : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.GzTrxs", "CustomerId_Mon_idx_gztransaction");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth" }, unique: true, name: "IX_Sold_Vintages_Cust_VintageYearMonth");
            CreateIndex("dbo.SoldVintages", "VintageYearMonth", name: "IX_Sold_Vintages_VintageYearMonth");
            CreateIndex("dbo.CustPortfolios", "PortfolioId", name: "IX_CustPortfolio_PortfolioId_Only");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "TypeId", "Amount" }, name: "IX_CustomerId_TId_Amnt");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId", "Amount" }, name: "IX_CustomerId_YM_TId_Amnt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TId_Amnt");
            DropIndex("dbo.GzTrxs", "IX_CustomerId_TId_Amnt");
            DropIndex("dbo.CustPortfolios", "IX_CustPortfolio_PortfolioId_Only");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_VintageYearMonth");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_Cust_VintageYearMonth");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId" }, name: "CustomerId_Mon_idx_gztransaction");
        }
    }
}
