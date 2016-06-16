namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CurrencyListX : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.CurrencyRates", "CurrRate_ftd_idx");
            CreateTable(
                "dbo.CurrencyListXes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        From = c.String(nullable: false),
                        To = c.String(nullable: false),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.CurrencyRates", "TradeDateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.CurrencyRates", "FromTo", c => c.String(nullable: false, maxLength: 6));
            CreateIndex("dbo.CurrencyRates", new[] { "TradeDateTime", "FromTo" }, unique: true, name: "CurrRate_ftd_idx");
            DropColumn("dbo.CurrencyRates", "FromCurrency");
            DropColumn("dbo.CurrencyRates", "ToCurrency");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CurrencyRates", "ToCurrency", c => c.String(nullable: false, maxLength: 3));
            AddColumn("dbo.CurrencyRates", "FromCurrency", c => c.String(nullable: false, maxLength: 3));
            DropIndex("dbo.CurrencyRates", "CurrRate_ftd_idx");
            DropColumn("dbo.CurrencyRates", "FromTo");
            DropColumn("dbo.CurrencyRates", "TradeDateTime");
            DropTable("dbo.CurrencyListXes");
            CreateIndex("dbo.CurrencyRates", new[] { "UpdatedOnUTC", "FromCurrency", "ToCurrency" }, unique: true, name: "CurrRate_ftd_idx");
        }
    }
}
