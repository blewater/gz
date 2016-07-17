namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sold_CustFund_Columns : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.CustFundShares", name: "SoldInvBalanceId", newName: "InvBalanceId");
            RenameIndex(table: "dbo.CustFundShares", name: "IX_SoldInvBalanceId", newName: "IX_InvBalanceId");
            AddColumn("dbo.CustFundShares", "SoldSharesValue", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.CustFundShares", "SoldSharesFundPriceId", c => c.Int());
            AddColumn("dbo.CustFundShares", "SoldOnUtc", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustFundShares", "SoldOnUtc");
            DropColumn("dbo.CustFundShares", "SoldSharesFundPriceId");
            DropColumn("dbo.CustFundShares", "SoldSharesValue");
            RenameIndex(table: "dbo.CustFundShares", name: "IX_InvBalanceId", newName: "IX_SoldInvBalanceId");
            RenameColumn(table: "dbo.CustFundShares", name: "InvBalanceId", newName: "SoldInvBalanceId");
        }
    }
}
