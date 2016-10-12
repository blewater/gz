namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CarouselEntry_SoftDelete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CarouselEntries", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CarouselEntries", "Deleted");
        }
    }
}
