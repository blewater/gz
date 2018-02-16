namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvAjustment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvAdjs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        YearMonth = c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false),
                        UserId = c.Int(nullable: false),
                        AmountType = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 29, scale: 16),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.YearMonth, t.UserId }, unique: true, name: "Inv_Adjustment_Idx");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InvAdjs", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.InvAdjs", "Inv_Adjustment_Idx");
            DropTable("dbo.InvAdjs");
        }
    }
}
