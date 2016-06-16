namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustFundShares_Simplified : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.CustFundShares", "SharesTradeDay");
            DropColumn("dbo.CustFundShares", "NewSharesTradeDay");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustFundShares", "NewSharesTradeDay", c => c.DateTime());
            AddColumn("dbo.CustFundShares", "SharesTradeDay", c => c.DateTime(nullable: false));
        }
    }
}
