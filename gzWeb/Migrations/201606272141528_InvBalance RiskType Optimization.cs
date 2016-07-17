namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvBalanceRiskTypeOptimization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBalances", "PortfolioRiskEnum", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvBalances", "PortfolioRiskEnum");
        }
    }
}
