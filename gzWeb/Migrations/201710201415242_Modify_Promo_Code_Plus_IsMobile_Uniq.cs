namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Modify_Promo_Code_Plus_IsMobile_Uniq : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DynamicPages", "DynamicPage_Code");
            CreateIndex("dbo.DynamicPages", new[] { "Code", "IsMobile" }, unique: true, name: "DynamicPage_Code");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DynamicPages", "DynamicPage_Code");
            CreateIndex("dbo.DynamicPages", "Code", unique: true, name: "DynamicPage_Code");
        }
    }
}
