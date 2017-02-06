namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class invbalancegain : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserPortfoliosShares", newName: "VintageShares");
            DropForeignKey("dbo.CustFundShares", "InvBalanceId", "dbo.InvBalances");
            DropIndex("dbo.CustFundShares", new[] { "InvBalanceId" });
            AddColumn("dbo.InvBalances", "TotalCashInvestments", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "TotalSoldVintagesValue", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            DropColumn("dbo.InvBalances", "CashBalance");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvBalances", "CashBalance", c => c.Decimal(precision: 29, scale: 16));
            DropColumn("dbo.InvBalances", "TotalSoldVintagesValue");
            DropColumn("dbo.InvBalances", "TotalCashInvestments");
            CreateIndex("dbo.CustFundShares", "InvBalanceId");
            AddForeignKey("dbo.CustFundShares", "InvBalanceId", "dbo.InvBalances", "Id");
            RenameTable(name: "dbo.VintageShares", newName: "UserPortfoliosShares");
        }
    }
}
