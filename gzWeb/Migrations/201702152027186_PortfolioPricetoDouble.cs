namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PortfolioPricetoDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PortfolioPrices", "PortfolioLowPrice", c => c.Double(nullable: false, defaultValueSql:"0"));
            AlterColumn("dbo.PortfolioPrices", "PortfolioMediumPrice", c => c.Double(nullable: false, defaultValueSql: "0"));
            AlterColumn("dbo.PortfolioPrices", "PortfolioHighPrice", c => c.Double(nullable: false, defaultValueSql: "0"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PortfolioPrices", "PortfolioHighPrice", c => c.Single(nullable: false));
            AlterColumn("dbo.PortfolioPrices", "PortfolioMediumPrice", c => c.Single(nullable: false));
            AlterColumn("dbo.PortfolioPrices", "PortfolioLowPrice", c => c.Single(nullable: false));
        }
    }
}
