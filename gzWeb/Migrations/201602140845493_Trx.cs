namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Trx : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.InvBalances", "CashConvRateId", "dbo.CurrencyRates");
            DropIndex("dbo.InvBalances", new[] { "CashConvRateId" });
            AddColumn("dbo.GzTransactions", "CurrencyRateId", c => c.Int());
            CreateIndex("dbo.GzTransactions", "CurrencyRateId");
            AddForeignKey("dbo.GzTransactions", "CurrencyRateId", "dbo.CurrencyRates", "Id");
            DropColumn("dbo.InvBalances", "CashConvRateId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvBalances", "CashConvRateId", c => c.Int());
            DropForeignKey("dbo.GzTransactions", "CurrencyRateId", "dbo.CurrencyRates");
            DropIndex("dbo.GzTransactions", new[] { "CurrencyRateId" });
            DropColumn("dbo.GzTransactions", "CurrencyRateId");
            CreateIndex("dbo.InvBalances", "CashConvRateId");
            AddForeignKey("dbo.InvBalances", "CashConvRateId", "dbo.CurrencyRates", "Id");
        }
    }
}
