namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UniqueIndexCustPortfolio : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.CustPortfolios", "CustomerId_Mon_idx_custp");
            DropIndex("dbo.CustPortfolios", new[] { "PortfolioId" });
            CreateIndex("dbo.CustPortfolios", new[] { "CustomerId", "YearMonth", "PortfolioId" }, unique: true, name: "CustomerId_Mon_idx_custp");
        }
        
        public override void Down()
        {
            DropIndex("dbo.CustPortfolios", "CustomerId_Mon_idx_custp");
            CreateIndex("dbo.CustPortfolios", "PortfolioId");
            CreateIndex("dbo.CustPortfolios", new[] { "CustomerId", "YearMonth" }, name: "CustomerId_Mon_idx_custp");
        }
    }
}
