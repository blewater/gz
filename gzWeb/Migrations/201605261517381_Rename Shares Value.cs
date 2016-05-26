namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameSharesValue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustFundShares", "CashedNewSharesValue", c => c.Decimal(precision: 29, scale: 16));
            DropColumn("dbo.CustFundShares", "BoughtNewSharesValue");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustFundShares", "BoughtNewSharesValue", c => c.Decimal(precision: 29, scale: 16));
            DropColumn("dbo.CustFundShares", "CashedNewSharesValue");
        }
    }
}
