namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Index_For_Code_In_EmailTemplates : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.EmailTemplates", "Code", b => b.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.EmailTemplates", "Code", unique: true, name: "EmailTemplate_Code");
        }
        
        public override void Down()
        {
            DropIndex("dbo.EmailTemplates", "EmailTemplate_Code");
            AlterColumn("dbo.EmailTemplates", "Code", b => b.String(nullable: false));
        }
    }
}
