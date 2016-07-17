namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvBalance_CashInv_Idx : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.InvBalances", "IDX_InvBalance_CustomerId_YearMonth_CashInv");
            DropIndex("dbo.InvBalances", "ID_InvBalance_CashInvestment_Only");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonth", "CashInvestment" }, unique: true, name: "IDX_InvBalance_CustomerId_YearMonth_CashInv");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InvBalances", "IDX_InvBalance_CustomerId_YearMonth_CashInv");
            CreateIndex("dbo.InvBalances", "CashInvestment", name: "ID_InvBalance_CashInvestment_Only");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonth", "CashBalance" }, unique: true, name: "IDX_InvBalance_CustomerId_YearMonth_CashInv");
        }
    }
}
