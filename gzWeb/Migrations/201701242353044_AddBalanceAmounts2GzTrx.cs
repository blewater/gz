namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBalanceAmounts2GzTrx : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GzTrxs", "BegGmBalance", c => c.Decimal(precision: 29, scale: 16, defaultValueSql:"0"));
            AddColumn("dbo.GzTrxs", "Deposits", c => c.Decimal(precision: 29, scale: 16, defaultValueSql: "0"));
            AddColumn("dbo.GzTrxs", "Withdrawals", c => c.Decimal(precision: 29, scale: 16, defaultValueSql: "0"));
            AddColumn("dbo.GzTrxs", "EndGmBalance", c => c.Decimal(precision: 29, scale: 16, defaultValueSql: "0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GzTrxs", "EndGmBalance");
            DropColumn("dbo.GzTrxs", "Withdrawals");
            DropColumn("dbo.GzTrxs", "Deposits");
            DropColumn("dbo.GzTrxs", "BegGmBalance");
        }
    }
}
