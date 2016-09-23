namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUpdatedFieldToGameCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GameCategories", "Updated", c => c.Int(nullable: false));
            AlterColumn("dbo.GameCategories", "GameSlugs", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GameCategories", "GameSlugs", c => c.String());
            DropColumn("dbo.GameCategories", "Updated");
        }
    }
}
