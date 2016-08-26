namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmailTemplate_Code_length_changed : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.EmailTemplates", "EmailTemplate_Code");
            AlterColumn("dbo.EmailTemplates", "Code", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.EmailTemplates", "Code", unique: true, name: "EmailTemplate_Code");
        }
        
        public override void Down()
        {
            DropIndex("dbo.EmailTemplates", "EmailTemplate_Code");
            AlterColumn("dbo.EmailTemplates", "Code", c => c.String(nullable: false));
            CreateIndex("dbo.EmailTemplates", "Code", unique: true, name: "EmailTemplate_Code");
        }
    }
}
