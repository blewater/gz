namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DecimalPrecision2916 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CurrencyRates", "rate", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AlterColumn("dbo.CustFundShares", "SharesNum", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AlterColumn("dbo.CustFundShares", "SharesValue", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AlterColumn("dbo.CustFundShares", "NewSharesNum", c => c.Decimal(precision: 29, scale: 16));
            AlterColumn("dbo.CustFundShares", "NewSharesValue", c => c.Decimal(precision: 29, scale: 16));
            AlterColumn("dbo.AspNetUsers", "GamBalance", c => c.Decimal(precision: 29, scale: 16));
            AlterColumn("dbo.GzTransactions", "Amount", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AlterColumn("dbo.InvBalances", "Balance", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AlterColumn("dbo.InvBalances", "InvGainLoss", c => c.Decimal(nullable: false, precision: 29, scale: 16));
            AlterColumn("dbo.InvBalances", "CashInvestment", c => c.Decimal(nullable: false, precision: 29, scale: 16));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.InvBalances", "CashInvestment", c => c.Decimal(nullable: false, precision: 28, scale: 14));
            AlterColumn("dbo.InvBalances", "InvGainLoss", c => c.Decimal(nullable: false, precision: 28, scale: 14));
            AlterColumn("dbo.InvBalances", "Balance", c => c.Decimal(nullable: false, precision: 28, scale: 14));
            AlterColumn("dbo.GzTransactions", "Amount", c => c.Decimal(nullable: false, precision: 28, scale: 14));
            AlterColumn("dbo.AspNetUsers", "GamBalance", c => c.Decimal(precision: 28, scale: 14));
            AlterColumn("dbo.CustFundShares", "NewSharesValue", c => c.Decimal(precision: 28, scale: 14));
            AlterColumn("dbo.CustFundShares", "NewSharesNum", c => c.Decimal(precision: 28, scale: 14));
            AlterColumn("dbo.CustFundShares", "SharesValue", c => c.Decimal(nullable: false, precision: 28, scale: 14));
            AlterColumn("dbo.CustFundShares", "SharesNum", c => c.Decimal(nullable: false, precision: 28, scale: 14));
            AlterColumn("dbo.CurrencyRates", "rate", c => c.Decimal(nullable: false, precision: 28, scale: 14));
        }
    }
}
