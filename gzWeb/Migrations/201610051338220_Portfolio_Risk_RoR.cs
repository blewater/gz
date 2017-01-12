namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Portfolio_Risk_RoR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GzConfigurations", "CONSERVATIVE_RISK_ROI", c => c.Single(nullable: false, defaultValue: 3f));
            AddColumn("dbo.GzConfigurations", "MEDIUM_RISK_ROI", c => c.Single(nullable: false, defaultValue: 6f));
            AddColumn("dbo.GzConfigurations", "AGGRESSIVE_RISK_ROI", c => c.Single(nullable: false, defaultValue: 10f));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GzConfigurations", "AGGRESSIVE_RISK_ROI");
            DropColumn("dbo.GzConfigurations", "MEDIUM_RISK_ROI");
            DropColumn("dbo.GzConfigurations", "CONSERVATIVE_RISK_ROI");
        }
    }
}
