namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CurrRate_Index : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.CurrencyRates", "CurrRate_ftd_idx");
            Sql(@"CREATE UNIQUE INDEX IDX_CurrRate_FT_TDT
              ON dbo.CurrencyRates (FromTo, TradeDateTime DESC)
              INCLUDE (rate)
              ON [PRIMARY]");
        }
        
        public override void Down()
        {
            DropIndex("dbo.CurrencyRates", "IDX_CurrRate_FT_TDT");
            CreateIndex("dbo.CurrencyRates", new[] { "TradeDateTime", "FromTo" }, unique: true, name: "CurrRate_ftd_idx");
        }
    }
}
