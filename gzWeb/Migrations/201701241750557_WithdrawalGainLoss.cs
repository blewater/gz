namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WithdrawalGainLoss : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlayerRevRpt", "PlayerGainLoss", c => c.Decimal(precision: 29, scale: 16));
            DropColumn("dbo.PlayerRevRpt", "PlayerLoss");
            DropColumn("dbo.PlayerRevRpt", "TotalWithdrawals");
            DropColumn("dbo.PlayerRevRpt", "Deposits made");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlayerRevRpt", "Deposits made", c => c.Int());
            AddColumn("dbo.PlayerRevRpt", "TotalWithdrawals", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "PlayerLoss", c => c.Decimal(precision: 29, scale: 16));
            DropColumn("dbo.PlayerRevRpt", "PlayerGainLoss");
        }
    }
}
