namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_GameCategory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Title = c.String(nullable: false),
                        GameSlugs = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true, name: "GameCategory_Code");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.GameCategories", "GameCategory_Code");
            DropTable("dbo.GameCategories");
        }
    }
}
