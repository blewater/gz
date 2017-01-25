namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WithdrawalsTotal : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlayerRevRpt", "PendingWithdrawals", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "TotalWithdrawals", c => c.Decimal(precision: 29, scale: 16));
            AlterColumn("dbo.PlayerRevRpt", "Deposits made", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PlayerRevRpt", "Deposits made", c => c.Decimal(precision: 29, scale: 16));
            DropColumn("dbo.PlayerRevRpt", "TotalWithdrawals");
            DropColumn("dbo.PlayerRevRpt", "PendingWithdrawals");
        }
    }
}
