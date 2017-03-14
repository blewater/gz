namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClearGzTrxIndices : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TId_Amnt");
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TTyp");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId", "Amount" }, name: "IX_CustomerId_YM_TId_Amnt");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId" }, name: "IX_CustomerId_YM_TTyp");
        }
        
        public override void Down()
        {
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TTyp");
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TId_Amnt");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId" }, unique: true, name: "IX_CustomerId_YM_TTyp");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId", "Amount" }, unique: true, name: "IX_CustomerId_YM_TId_Amnt");
        }
    }
}
