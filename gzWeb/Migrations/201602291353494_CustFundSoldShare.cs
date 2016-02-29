namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustFundSoldShare : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Funds", t => t.FundId, cascadeDelete: true)
                .ForeignKey("dbo.FundPrices", t => t.SharesFundPriceId)
                .Index(t => new { t.CustomerId, t.YearMonth, t.FundId }, unique: true, name: "CustFundShareId_YMD_idx")
                .Index(t => t.SharesFundPriceId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustFundSoldShares", "SharesFundPriceId", "dbo.FundPrices");
            DropForeignKey("dbo.CustFundSoldShares", "FundId", "dbo.Funds");
            DropForeignKey("dbo.CustFundSoldShares", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.CustFundSoldShares", new[] { "SharesFundPriceId" });
            DropIndex("dbo.CustFundSoldShares", "CustFundShareId_YMD_idx");
            DropTable("dbo.CustFundSoldShares");
        }
    }
}
