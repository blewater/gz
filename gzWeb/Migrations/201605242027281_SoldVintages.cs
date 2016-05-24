namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoldVintages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SoldVintages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        VintageYearMonth = c.String(nullable: false, maxLength: 6),
                        Amount = c.Decimal(nullable: false, precision: 29, scale: 16),
                        SoldOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => new { t.CustomerId, t.VintageYearMonth }, name: "CustomerId_Mon_idx_gzSoldVintage");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SoldVintages", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            DropTable("dbo.SoldVintages");
        }
    }
}
