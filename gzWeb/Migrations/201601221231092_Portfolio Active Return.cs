namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PortfolioActiveReturn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Portfolios", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Portfolios", "IsActive");
        }
    }
}
