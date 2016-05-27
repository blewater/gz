namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Portfolioenhacements : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Portfolios", "Title", c => c.String());
            AddColumn("dbo.Portfolios", "Color", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Portfolios", "Color");
            DropColumn("dbo.Portfolios", "Title");
        }
    }
}
