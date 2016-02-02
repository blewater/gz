namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustFundShares : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustFundShares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        YearMonth = c.String(nullable: false, maxLength: 6),
                        FundId = c.Int(nullable: false),
                        NumShares = c.Single(nullable: false),
                        UpdatedOnUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Funds", t => t.FundId, cascadeDelete: true)
                .Index(t => new { t.CustomerId, t.YearMonth, t.FundId }, unique: true, name: "CustFundShareId_YMD_idx");
            
            AddColumn("dbo.GzTransactions", "CreditPcntApplied", c => c.Single());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustFundShares", "FundId", "dbo.Funds");
            DropForeignKey("dbo.CustFundShares", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.CustFundShares", "CustFundShareId_YMD_idx");
            DropColumn("dbo.GzTransactions", "CreditPcntApplied");
            DropTable("dbo.CustFundShares");
        }
    }
}
