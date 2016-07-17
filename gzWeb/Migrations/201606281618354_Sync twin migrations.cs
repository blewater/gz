namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Synctwinmigrations : DbMigration
    {
        public override void Up()
        {
            /*************** Already Occurred This empty migration syncs up the duplicate id from different migrations
            DropIndex("dbo.CustPortfolios", "CustomerId_Mon_idx_custp");
            DropIndex("dbo.CustPortfolios", new[] { "PortfolioId" });
            AddColumn("dbo.InvBalances", "PortfolioRiskEnum", c => c.Int(nullable: false));
            CreateIndex("dbo.CustPortfolios", new[] { "CustomerId", "YearMonth", "PortfolioId" }, unique: true, name: "CustomerId_Mon_idx_custp");
            ****************/
        }
        
        public override void Down()
        {
            /****************
            DropIndex("dbo.CustPortfolios", "CustomerId_Mon_idx_custp");
            DropColumn("dbo.InvBalances", "PortfolioRiskEnum");
            CreateIndex("dbo.CustPortfolios", "PortfolioId");
            CreateIndex("dbo.CustPortfolios", new[] { "CustomerId", "YearMonth" }, name: "CustomerId_Mon_idx_custp");
            ********/
        }
    }
}
