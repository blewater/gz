namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ActionType_To_CarouselEntry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CarouselEntries", "ActionType", c => c.Int(nullable: false));
            AlterColumn("dbo.CarouselEntries", "BackgroundImageUrl", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarouselEntries", "BackgroundImageUrl", c => c.String(nullable: false));
            DropColumn("dbo.CarouselEntries", "ActionType");
        }
    }
}
