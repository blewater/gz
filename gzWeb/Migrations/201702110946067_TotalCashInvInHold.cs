namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TotalCashInvInHold : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBalances", "TotalCashInvInHold", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValueSql:"0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvBalances", "TotalCashInvInHold");
        }
    }
}
