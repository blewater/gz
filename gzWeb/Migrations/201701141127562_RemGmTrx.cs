namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemGmTrx : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GmTrxs", "CustomerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.GmTrxs", "TypeId", "dbo.GmTrxTypes");
            DropForeignKey("dbo.GzTrxs", "GmTrxId", "dbo.GmTrxs");
            DropIndex("dbo.GzTrxs", new[] { "GmTrxId" });
            DropIndex("dbo.GmTrxs", new[] { "CustomerId" });
            DropIndex("dbo.GmTrxs", new[] { "YearMonthCtd" });
            DropIndex("dbo.GmTrxs", new[] { "GmCustomerId" });
            DropIndex("dbo.GmTrxs", new[] { "CustomerEmail" });
            DropIndex("dbo.GmTrxs", new[] { "TypeId" });
            DropColumn("dbo.GzTrxs", "GmTrxId");
            DropTable("dbo.GmTrxs");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.GmTrxs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(),
                        YearMonthCtd = c.String(maxLength: 6, fixedLength: true, unicode: false),
                        GmCustomerId = c.Int(),
                        CustomerEmail = c.String(maxLength: 256),
                        TypeId = c.Int(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 29, scale: 16),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.GzTrxs", "GmTrxId", c => c.Int());
            CreateIndex("dbo.GmTrxs", "TypeId");
            CreateIndex("dbo.GmTrxs", "CustomerEmail");
            CreateIndex("dbo.GmTrxs", "GmCustomerId");
            CreateIndex("dbo.GmTrxs", "YearMonthCtd");
            CreateIndex("dbo.GmTrxs", "CustomerId");
            CreateIndex("dbo.GzTrxs", "GmTrxId");
            AddForeignKey("dbo.GzTrxs", "GmTrxId", "dbo.GmTrxs", "Id");
            AddForeignKey("dbo.GmTrxs", "TypeId", "dbo.GmTrxTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.GmTrxs", "CustomerId", "dbo.AspNetUsers", "Id");
        }
    }
}
