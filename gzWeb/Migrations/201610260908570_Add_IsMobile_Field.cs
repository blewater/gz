namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_IsMobile_Field : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CarouselEntries", "IsMobile", c => c.Boolean(nullable: false));
            AddColumn("dbo.DynamicPages", "IsMobile", c => c.Boolean(nullable: false));
            AddColumn("dbo.GameCategories", "IsMobile", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GameCategories", "IsMobile");
            DropColumn("dbo.DynamicPages", "IsMobile");
            DropColumn("dbo.CarouselEntries", "IsMobile");
        }
    }
}
