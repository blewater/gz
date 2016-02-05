namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PurchasedStockValue : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CurrencyRates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                        FromCurrency = c.String(nullable: false, maxLength: 3),
                        ToCurrency = c.String(nullable: false, maxLength: 3),
                        rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UpdatedOnUTC, t.FromCurrency, t.ToCurrency }, unique: true, name: "CurrRate_ftd_idx");
            
            AddColumn("dbo.CustFundShares", "NewNumShares", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.CustFundShares", "Value", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.FundPrices", "CustNewFundShareId", c => c.Int());
            AddColumn("dbo.InvBalances", "CashInvestment", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.InvBalances", "CashConvRateId", c => c.Int(nullable: false));
            AlterColumn("dbo.InvBalances", "InvGainLoss", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            CreateIndex("dbo.InvBalances", "CashConvRateId");
            CreateIndex("dbo.FundPrices", "CustNewFundShareId");
            AddForeignKey("dbo.InvBalances", "CashConvRateId", "dbo.CurrencyRates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.FundPrices", "CustNewFundShareId", "dbo.CustFundShares", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FundPrices", "CustNewFundShareId", "dbo.CustFundShares");
            DropForeignKey("dbo.InvBalances", "CashConvRateId", "dbo.CurrencyRates");
            DropIndex("dbo.FundPrices", new[] { "CustNewFundShareId" });
            DropIndex("dbo.CurrencyRates", "CurrRate_ftd_idx");
            DropIndex("dbo.InvBalances", new[] { "CashConvRateId" });
            AlterColumn("dbo.InvBalances", "InvGainLoss", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.InvBalances", "CashConvRateId");
            DropColumn("dbo.InvBalances", "CashInvestment");
            DropColumn("dbo.FundPrices", "CustNewFundShareId");
            DropColumn("dbo.CustFundShares", "Value");
            DropColumn("dbo.CustFundShares", "NewNumShares");
            DropTable("dbo.CurrencyRates");
        }
    }
}
