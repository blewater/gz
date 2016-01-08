namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DupIndexTransx : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Transxes", "CustomerId_Mon_idx_transx");
            AlterColumn("dbo.Transxes", "YearMonthCtd", c => c.String(nullable: false, maxLength: 6));
            CreateIndex("dbo.Transxes", new[] { "CustomerId", "YearMonthCtd" }, name: "CustomerId_Mon_idx_transx");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Transxes", "CustomerId_Mon_idx_transx");
            AlterColumn("dbo.Transxes", "YearMonthCtd", c => c.String(maxLength: 6));
            CreateIndex("dbo.Transxes", new[] { "CustomerId", "YearMonthCtd" }, unique: true, name: "CustomerId_Mon_idx_transx");
        }
    }
}
