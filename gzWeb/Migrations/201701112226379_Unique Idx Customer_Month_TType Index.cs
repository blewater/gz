namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UniqueIdxCustomer_Month_TTypeIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId" }, unique: true, name: "IX_CustomerId_YM_TTyp");
        }
        
        public override void Down()
        {
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TTyp");
        }
    }
}
