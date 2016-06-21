namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveCustSoldFurndSharesAddCashBalance : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustFundSoldShares", "CustomerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CustFundSoldShares", "FundId", "dbo.Funds");
            DropForeignKey("dbo.CustFundSoldShares", "SharesFundPriceId", "dbo.FundPrices");
            DropIndex("dbo.CustFundSoldShares", "CustFundShareId_YMD_idx");
            DropIndex("dbo.CustFundSoldShares", new[] { "SharesFundPriceId" });
            AddColumn("dbo.InvBalances", "CashBalance", c => c.Decimal(precision: 29, scale: 16));
            DropTable("dbo.CustFundSoldShares");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CustFundSoldShares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        YearMonth = c.String(nullable: false, maxLength: 6),
                        FundId = c.Int(nullable: false),
                        SharesNum = c.Decimal(nullable: false, precision: 29, scale: 16),
                        SharesValue = c.Decimal(nullable: false, precision: 29, scale: 16),
                        SharesFundPriceId = c.Int(),
                        Rationale = c.String(maxLength: 128),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.InvBalances", "CashBalance");
            CreateIndex("dbo.CustFundSoldShares", "SharesFundPriceId");
            CreateIndex("dbo.CustFundSoldShares", new[] { "CustomerId", "YearMonth", "FundId" }, unique: true, name: "CustFundShareId_YMD_idx");
            AddForeignKey("dbo.CustFundSoldShares", "SharesFundPriceId", "dbo.FundPrices", "Id");
            AddForeignKey("dbo.CustFundSoldShares", "FundId", "dbo.Funds", "Id", cascadeDelete: true);
            AddForeignKey("dbo.CustFundSoldShares", "CustomerId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
