namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeDynamicPageModel_SoftDelete_And_UseInPromoList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DynamicPages", "UseInPromoList", c => c.Boolean(nullable: false));
            AddColumn("dbo.DynamicPages", "Deleted", c => c.Boolean(nullable: false));
            AlterColumn("dbo.DynamicPages", "ThumbImageUrl", c => c.String());
            AlterColumn("dbo.DynamicPages", "ThumbTitle", c => c.String());
            AlterColumn("dbo.DynamicPages", "ThumbText", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DynamicPages", "ThumbText", c => c.String(nullable: false));
            AlterColumn("dbo.DynamicPages", "ThumbTitle", c => c.String(nullable: false));
            AlterColumn("dbo.DynamicPages", "ThumbImageUrl", c => c.String(nullable: false));
            DropColumn("dbo.DynamicPages", "Deleted");
            DropColumn("dbo.DynamicPages", "UseInPromoList");
        }
    }
}
