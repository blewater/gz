namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUpdatedField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CarouselEntries", "Updated", c => c.DateTime(nullable: false));
            AddColumn("dbo.DynamicPages", "Updated", c => c.DateTime(nullable: false));
            AddColumn("dbo.EmailTemplates", "Updated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailTemplates", "Updated");
            DropColumn("dbo.DynamicPages", "Updated");
            DropColumn("dbo.CarouselEntries", "Updated");
        }
    }
}
