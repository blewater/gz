namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class invbalancegain : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserPortfoliosShares", newName: "VintageShares");
            AddColumn("dbo.InvBalances", "TotalCashInvestments", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "TotalSoldVintagesValue", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            DropColumn("dbo.InvBalances", "CashBalance");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvBalances", "CashBalance", c => c.Decimal(precision: 29, scale: 16));
            DropColumn("dbo.InvBalances", "TotalSoldVintagesValue");
            DropColumn("dbo.InvBalances", "TotalCashInvestments");
            RenameTable(name: "dbo.VintageShares", newName: "UserPortfoliosShares");
        }
    }
}
