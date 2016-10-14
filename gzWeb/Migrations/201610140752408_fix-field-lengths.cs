using gzDAL.Migrations;

namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixfieldlengths : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarouselEntries", "Title", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.CarouselEntries", "SubTitle", c => c.String(nullable: false, maxLength: 255));
            this.DeleteDefaultConstraint("dbo.CarouselEntries", "ActionText");
            AlterColumn("dbo.CarouselEntries", "ActionText", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.CarouselEntries", "ActionUrl", c => c.String(nullable: false, maxLength: 255));
            this.DeleteDefaultConstraint("dbo.CarouselEntries", "BackgroundImageUrl");
            AlterColumn("dbo.CarouselEntries", "BackgroundImageUrl", c => c.String(maxLength: 255));
            this.DeleteDefaultConstraint("dbo.DynamicPages", "ThumbImageUrl");
            AlterColumn("dbo.DynamicPages", "ThumbImageUrl", c => c.String(nullable: false, maxLength: 255));
            this.DeleteDefaultConstraint("dbo.DynamicPages", "ThumbTitle");
            AlterColumn("dbo.DynamicPages", "ThumbTitle", c => c.String(nullable: false, maxLength: 255));
            this.DeleteDefaultConstraint("dbo.DynamicPages", "ThumbText");
            AlterColumn("dbo.DynamicPages", "ThumbText", c => c.String(nullable: false, maxLength: 2048));
            AlterColumn("dbo.DynamicPageDatas", "DataName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.DynamicPageDatas", "DataType", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.EmailTemplates", "Subject", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.GameCategories", "Title", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.GameCategories", "GameSlugs", c => c.String(nullable: false, maxLength: 2048));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GameCategories", "GameSlugs", c => c.String(nullable: false));
            AlterColumn("dbo.GameCategories", "Title", c => c.String(nullable: false));
            AlterColumn("dbo.EmailTemplates", "Subject", c => c.String(nullable: false));
            AlterColumn("dbo.DynamicPageDatas", "DataType", c => c.String(nullable: false));
            AlterColumn("dbo.DynamicPageDatas", "DataName", c => c.String(nullable: false));
            AlterColumn("dbo.DynamicPages", "ThumbText", c => c.String());
            AlterColumn("dbo.DynamicPages", "ThumbTitle", c => c.String());
            AlterColumn("dbo.DynamicPages", "ThumbImageUrl", c => c.String());
            AlterColumn("dbo.CarouselEntries", "BackgroundImageUrl", c => c.String());
            AlterColumn("dbo.CarouselEntries", "ActionUrl", c => c.String(nullable: false));
            AlterColumn("dbo.CarouselEntries", "ActionText", c => c.String(nullable: false));
            AlterColumn("dbo.CarouselEntries", "SubTitle", c => c.String(nullable: false));
            AlterColumn("dbo.CarouselEntries", "Title", c => c.String(nullable: false));
        }
    }
}
