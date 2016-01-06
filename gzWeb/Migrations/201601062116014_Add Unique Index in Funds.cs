namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUniqueIndexinFunds : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Funds", new[] { "Symbol" });
            DropIndex("dbo.Funds", new[] { "HoldingName" });
            CreateIndex("dbo.Funds", "Symbol", unique: true);
            CreateIndex("dbo.Funds", "HoldingName", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Funds", new[] { "HoldingName" });
            DropIndex("dbo.Funds", new[] { "Symbol" });
            CreateIndex("dbo.Funds", "HoldingName");
            CreateIndex("dbo.Funds", "Symbol");
        }
    }
}
