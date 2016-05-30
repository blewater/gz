namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoldVintages : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustFundShares", "CustomerId", "dbo.AspNetUsers");
            CreateTable(
                "dbo.SoldVintages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        VintageYearMonth = c.String(nullable: false, maxLength: 6),
                        MarketAmount = c.Decimal(nullable: false, precision: 29, scale: 16),
                        Fees = c.Decimal(nullable: false, precision: 29, scale: 16),
                        SoldOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => new { t.CustomerId, t.VintageYearMonth }, name: "CustomerId_Mon_idx_gzSoldVintage");
            
            AddColumn("dbo.CustFundShares", "SoldNewSharesNum", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.CustFundShares", "BoughtNewSharesValue", c => c.Decimal(precision: 29, scale: 16));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SoldVintages", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            DropColumn("dbo.CustFundShares", "BoughtNewSharesValue");
            DropColumn("dbo.CustFundShares", "SoldNewSharesNum");
            DropTable("dbo.SoldVintages");
            AddForeignKey("dbo.CustFundShares", "CustomerId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
