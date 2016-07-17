namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_CarouselEntry : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CarouselEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Title = c.String(nullable: false),
                        SubTitle = c.String(nullable: false),
                        ActionUrl = c.String(nullable: false),
                        BackroudImageUrl = c.String(nullable: false),
                        Live = c.Boolean(nullable: false),
                        LiveFrom = c.DateTime(nullable: false),
                        LiveTo = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true, name: "CarouselEntry_Code");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.CarouselEntries", "CarouselEntry_Code");
            DropTable("dbo.CarouselEntries");
        }
    }
}
