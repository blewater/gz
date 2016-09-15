namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renames_at_carousel_entry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CarouselEntries", "ActionText", c => c.String(nullable: false));
            AddColumn("dbo.CarouselEntries", "BackgroundImageUrl", c => c.String(nullable: false));
            DropColumn("dbo.CarouselEntries", "BackroudImageUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CarouselEntries", "BackroudImageUrl", c => c.String(nullable: false));
            DropColumn("dbo.CarouselEntries", "BackgroundImageUrl");
            DropColumn("dbo.CarouselEntries", "ActionText");
        }
    }
}
