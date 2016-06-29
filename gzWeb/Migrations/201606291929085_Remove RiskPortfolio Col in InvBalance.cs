namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRiskPortfolioColinInvBalance : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.InvBalances", "PortfolioRiskEnum");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvBalances", "PortfolioRiskEnum", c => c.Int(nullable: false));
        }
    }
}
