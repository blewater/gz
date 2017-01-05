namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_New_GmBalance_Amounts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBalances", "BegGmBalance", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "Deposits", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "Withdrawals", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "GamingGainLoss", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "EndGmBalance", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AlterColumn("dbo.AspNetUsers", "Currency", c => c.String(nullable: false, maxLength: 3, fixedLength: true, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "Currency", c => c.String(nullable: false));
            DropColumn("dbo.InvBalances", "EndGmBalance");
            DropColumn("dbo.InvBalances", "GamingGainLoss");
            DropColumn("dbo.InvBalances", "Withdrawals");
            DropColumn("dbo.InvBalances", "Deposits");
            DropColumn("dbo.InvBalances", "BegGmBalance");
        }
    }
}
