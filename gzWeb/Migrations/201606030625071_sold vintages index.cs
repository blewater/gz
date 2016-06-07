namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class soldvintagesindex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth", "YearMonth" }, unique: true, name: "CustomerId_Mon_idx_gzSoldVintage");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "YearMonth", "VintageYearMonth" }, unique: true, name: "CustomerId_Mon_idx_gzSoldVintage");
        }
    }
}
