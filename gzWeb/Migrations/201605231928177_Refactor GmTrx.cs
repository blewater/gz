namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefactorGmTrx : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GmTrxs", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.GmTrxs", "CustomerId_Mon_idx_gztransaction");
            AddColumn("dbo.GmTrxs", "GmCustomerId", c => c.Int());
            AddColumn("dbo.GmTrxs", "CustomerEmail", c => c.String(maxLength: 256));
            AlterColumn("dbo.GmTrxs", "CustomerId", c => c.Int());
            AlterColumn("dbo.GmTrxs", "YearMonthCtd", c => c.String(maxLength: 6));
            CreateIndex("dbo.GmTrxs", "CustomerId");
            CreateIndex("dbo.GmTrxs", "YearMonthCtd");
            CreateIndex("dbo.GmTrxs", "GmCustomerId");
            CreateIndex("dbo.GmTrxs", "CustomerEmail");
            AddForeignKey("dbo.GmTrxs", "CustomerId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GmTrxs", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.GmTrxs", new[] { "CustomerEmail" });
            DropIndex("dbo.GmTrxs", new[] { "GmCustomerId" });
            DropIndex("dbo.GmTrxs", new[] { "YearMonthCtd" });
            DropIndex("dbo.GmTrxs", new[] { "CustomerId" });
            AlterColumn("dbo.GmTrxs", "YearMonthCtd", c => c.String(nullable: false, maxLength: 6));
            AlterColumn("dbo.GmTrxs", "CustomerId", c => c.Int(nullable: false));
            DropColumn("dbo.GmTrxs", "CustomerEmail");
            DropColumn("dbo.GmTrxs", "GmCustomerId");
            CreateIndex("dbo.GmTrxs", new[] { "CustomerId", "YearMonthCtd" }, name: "CustomerId_Mon_idx_gztransaction");
            AddForeignKey("dbo.GmTrxs", "CustomerId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
