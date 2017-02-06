namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackLowMediumHighSharesformonth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBalances", "LowRiskShares", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValueSql:"0"));
            AddColumn("dbo.InvBalances", "MediumRiskShares", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValueSql: "0"));
            AddColumn("dbo.InvBalances", "HighRiskShares", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValueSql: "0"));
            AddColumn("dbo.GzTrxs", "GainLoss", c => c.Decimal(precision: 29, scale: 16, defaultValueSql: "0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GzTrxs", "GainLoss");
            DropColumn("dbo.InvBalances", "HighRiskShares");
            DropColumn("dbo.InvBalances", "MediumRiskShares");
            DropColumn("dbo.InvBalances", "LowRiskShares");
        }
    }
}
