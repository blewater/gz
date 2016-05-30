namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoldVintagesShares : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SoldVintageShares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SoldVintageId = c.Int(nullable: false),
                        FundId = c.Int(nullable: false),
                        SharesNum = c.Decimal(nullable: false, precision: 29, scale: 16),
                        SharesValue = c.Decimal(nullable: false, precision: 29, scale: 16),
                        SharesFundPriceId = c.Int(),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Funds", t => t.FundId, cascadeDelete: true)
                .ForeignKey("dbo.FundPrices", t => t.SharesFundPriceId)
                .ForeignKey("dbo.SoldVintages", t => t.SoldVintageId, cascadeDelete: true)
                .Index(t => t.SoldVintageId)
                .Index(t => t.FundId, unique: true, name: "CustFundShareId_YMD_idx")
                .Index(t => t.SharesFundPriceId);
            
            AddColumn("dbo.SoldVintages", "UpdatedOnUtc", c => c.DateTime(nullable: false));
            DropColumn("dbo.SoldVintages", "SoldOnUtc");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SoldVintages", "SoldOnUtc", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.SoldVintageShares", "SoldVintageId", "dbo.SoldVintages");
            DropForeignKey("dbo.SoldVintageShares", "SharesFundPriceId", "dbo.FundPrices");
            DropForeignKey("dbo.SoldVintageShares", "FundId", "dbo.Funds");
            DropIndex("dbo.SoldVintageShares", new[] { "SharesFundPriceId" });
            DropIndex("dbo.SoldVintageShares", "CustFundShareId_YMD_idx");
            DropIndex("dbo.SoldVintageShares", new[] { "SoldVintageId" });
            DropColumn("dbo.SoldVintages", "UpdatedOnUtc");
            DropTable("dbo.SoldVintageShares");
        }
    }
}
