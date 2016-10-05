namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Portfolio_Risk_RoR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GzConfigurations", "CONSERVATIVE_RISK_ROI", c => c.Single(nullable: false, defaultValue: 3.5f));
            AddColumn("dbo.GzConfigurations", "MEDIUM_RISK_ROI", c => c.Single(nullable: false, defaultValue: 5f));
            AddColumn("dbo.GzConfigurations", "AGGRESSIVE_RISK_ROI", c => c.Single(nullable: false, defaultValue: 7.5f));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GzConfigurations", "AGGRESSIVE_RISK_ROI");
            DropColumn("dbo.GzConfigurations", "MEDIUM_RISK_ROI");
            DropColumn("dbo.GzConfigurations", "CONSERVATIVE_RISK_ROI");
        }
    }
}
