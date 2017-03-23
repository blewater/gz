namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StandardizeGmbalancenaming : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.InvBalances", "GamingGainLoss", "GmGainLoss");
            RenameColumn("dbo.GzTrxs", "GainLoss", "GmGainLoss");
            RenameColumn( "dbo.PlayerRevRpt", "BegBalance", "BegGmBalance");
            RenameColumn("dbo.PlayerRevRpt", "EndBalance", "EndGmBalance");
            RenameColumn("dbo.PlayerRevRpt", "PlayerGainLoss", "GmGainLoss" );
        }
        
        public override void Down()
        {
            RenameColumn("dbo.InvBalances", "GmGainLoss", "GamingGainLoss");
            RenameColumn("dbo.GzTrxs", "GmGainLoss", "GainLoss");
            RenameColumn("dbo.PlayerRevRpt", "BegGmBalance", "BegBalance");
            RenameColumn("dbo.PlayerRevRpt", "EndGmBalance", "EndBalance");
            RenameColumn("dbo.PlayerRevRpt", "GmGainLoss", "PlayerGainLoss");
        }
    }
}
