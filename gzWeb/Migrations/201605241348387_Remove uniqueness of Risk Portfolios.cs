namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveuniquenessofRiskPortfolios : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Portfolios", new[] { "IsActive" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.Portfolios", "IsActive");
        }
    }
}
