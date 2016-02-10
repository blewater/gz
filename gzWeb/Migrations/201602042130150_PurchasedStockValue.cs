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
            
            AddColumn("dbo.CustFundShares", "SharesNum", c => c.Decimal(nullable: false, precision: 18, scale: 2, defaultValue:0));
            AddColumn("dbo.CustFundShares", "SharesValue", c => c.Decimal(nullable: false, precision: 18, scale: 2, defaultValue: 0));
            AddColumn("dbo.CustFundShares", "SharesTradeDay", c => c.DateTime(nullable: false, defaultValue: new DateTime(2000,1,1)));
            AddColumn("dbo.CustFundShares", "SharesFundPriceId", c => c.Int());
            AddColumn("dbo.CustFundShares", "NewSharesNum", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.CustFundShares", "NewSharesValue", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.CustFundShares", "NewSharesTradeDay", c => c.DateTime());
            AddColumn("dbo.CustFundShares", "NewSharesFundPriceId", c => c.Int());
            AddColumn("dbo.InvBalances", "CashInvestment", c => c.Decimal(nullable: false, precision: 18, scale: 2, defaultValue: 0));
            AddColumn("dbo.InvBalances", "CashConvRateId", c => c.Int());
            Sql("UPDATE [dbo].[InvBalances] SET InvGainLoss = 0 WHERE InvGainLoss IS NULL");
            AlterColumn("dbo.InvBalances", "InvGainLoss", c => c.Decimal(nullable: false, precision: 18, scale: 2, defaultValue: 0));
            CreateIndex("dbo.CustFundShares", "SharesFundPriceId");
            CreateIndex("dbo.CustFundShares", "NewSharesFundPriceId");
            CreateIndex("dbo.InvBalances", "CashConvRateId");
            AddForeignKey("dbo.InvBalances", "CashConvRateId", "dbo.CurrencyRates", "Id");
            AddForeignKey("dbo.CustFundShares", "NewSharesFundPriceId", "dbo.FundPrices", "Id");
            AddForeignKey("dbo.CustFundShares", "SharesFundPriceId", "dbo.FundPrices", "Id");
            DropColumn("dbo.CustFundShares", "NumShares");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustFundShares", "NumShares", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropForeignKey("dbo.CustFundShares", "SharesFundPriceId", "dbo.FundPrices");
            DropForeignKey("dbo.CustFundShares", "NewSharesFundPriceId", "dbo.FundPrices");
            DropForeignKey("dbo.InvBalances", "CashConvRateId", "dbo.CurrencyRates");
            DropIndex("dbo.CurrencyRates", "CurrRate_ftd_idx");
            DropIndex("dbo.InvBalances", new[] { "CashConvRateId" });
            DropIndex("dbo.CustFundShares", new[] { "NewSharesFundPriceId" });
            DropIndex("dbo.CustFundShares", new[] { "SharesFundPriceId" });
            AlterColumn("dbo.InvBalances", "InvGainLoss", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.InvBalances", "CashConvRateId");
            DropColumn("dbo.InvBalances", "CashInvestment");
            DropColumn("dbo.CustFundShares", "NewSharesFundPriceId");
            DropColumn("dbo.CustFundShares", "NewSharesTradeDay");
            DropColumn("dbo.CustFundShares", "NewSharesValue");
            DropColumn("dbo.CustFundShares", "NewSharesNum");
            DropColumn("dbo.CustFundShares", "SharesFundPriceId");
            DropColumn("dbo.CustFundShares", "SharesTradeDay");
            DropColumn("dbo.CustFundShares", "SharesValue");
            DropColumn("dbo.CustFundShares", "SharesNum");
            DropTable("dbo.CurrencyRates");
        }
    }
}
