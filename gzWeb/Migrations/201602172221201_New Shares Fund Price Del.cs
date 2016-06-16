namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewSharesFundPriceDel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustFundShares", "NewSharesFundPriceId", "dbo.FundPrices");
            DropIndex("dbo.CustFundShares", new[] { "NewSharesFundPriceId" });
            DropColumn("dbo.CustFundShares", "NewSharesFundPriceId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustFundShares", "NewSharesFundPriceId", c => c.Int());
            CreateIndex("dbo.CustFundShares", "NewSharesFundPriceId");
            AddForeignKey("dbo.CustFundShares", "NewSharesFundPriceId", "dbo.FundPrices", "Id");
        }
    }
}
