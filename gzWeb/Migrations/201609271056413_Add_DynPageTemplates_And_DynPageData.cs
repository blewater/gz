namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_DynPageTemplates_And_DynPageData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DynamicPageTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Html = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DynamicPageDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DynamicPageId = c.Int(nullable: false),
                        DataName = c.String(nullable: false),
                        DataType = c.String(nullable: false),
                        DataValue = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DynamicPages", t => t.DynamicPageId, cascadeDelete: true)
                .Index(t => t.DynamicPageId);
            
            AddColumn("dbo.DynamicPages", "DynamicPageTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.DynamicPages", "DynamicPageTemplateId");
            AddForeignKey("dbo.DynamicPages", "DynamicPageTemplateId", "dbo.DynamicPageTemplates", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DynamicPageDatas", "DynamicPageId", "dbo.DynamicPages");
            DropForeignKey("dbo.DynamicPages", "DynamicPageTemplateId", "dbo.DynamicPageTemplates");
            DropIndex("dbo.DynamicPageDatas", new[] { "DynamicPageId" });
            DropIndex("dbo.DynamicPages", new[] { "DynamicPageTemplateId" });
            DropColumn("dbo.DynamicPages", "DynamicPageTemplateId");
            DropTable("dbo.DynamicPageDatas");
            DropTable("dbo.DynamicPageTemplates");
        }
    }
}
