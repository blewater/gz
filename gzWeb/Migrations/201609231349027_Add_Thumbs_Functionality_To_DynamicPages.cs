namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Thumbs_Functionality_To_DynamicPages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DynamicPages", "ThumbImageUrl", c => c.String(nullable: false));
            AddColumn("dbo.DynamicPages", "ThumbTitle", c => c.String(nullable: false));
            AddColumn("dbo.DynamicPages", "ThumbText", c => c.String(nullable: false));
            DropColumn("dbo.DynamicPages", "Category");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DynamicPages", "Category", c => c.String(nullable: false));
            DropColumn("dbo.DynamicPages", "ThumbText");
            DropColumn("dbo.DynamicPages", "ThumbTitle");
            DropColumn("dbo.DynamicPages", "ThumbImageUrl");
        }
    }
}
