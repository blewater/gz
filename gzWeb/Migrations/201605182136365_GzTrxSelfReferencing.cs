namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GzTrxSelfReferencing : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GzTransactions", "CurrencyRateId", "dbo.CurrencyRates");
            DropIndex("dbo.GzTransactions", new[] { "CurrencyRateId" });
            DropIndex("dbo.GzTransactions", new[] { "CreatedOnUTC" });
            AddColumn("dbo.GzTransactions", "ParentTrxId", c => c.Int());
            CreateIndex("dbo.GzTransactions", "ParentTrxId");
            AddForeignKey("dbo.GzTransactions", "ParentTrxId", "dbo.GzTransactions", "Id");
            DropColumn("dbo.GzTransactions", "CurrencyRateId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GzTransactions", "CurrencyRateId", c => c.Int());
            DropForeignKey("dbo.GzTransactions", "ParentTrxId", "dbo.GzTransactions");
            DropIndex("dbo.GzTransactions", new[] { "ParentTrxId" });
            DropColumn("dbo.GzTransactions", "ParentTrxId");
            CreateIndex("dbo.GzTransactions", "CreatedOnUTC");
            CreateIndex("dbo.GzTransactions", "CurrencyRateId");
            AddForeignKey("dbo.GzTransactions", "CurrencyRateId", "dbo.CurrencyRates", "Id");
        }
    }
}
