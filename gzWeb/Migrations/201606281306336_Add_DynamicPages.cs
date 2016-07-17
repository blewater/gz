namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_DynamicPages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DynamicPages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Category = c.String(nullable: false),
                        Live = c.Boolean(nullable: false),
                        LiveFrom = c.Boolean(nullable: false),
                        LiveTo = c.Boolean(nullable: false),
                        Html = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true, name: "DynamicPage_Code");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.DynamicPages", "DynamicPage_Code");
            DropTable("dbo.DynamicPages");
        }
    }
}
