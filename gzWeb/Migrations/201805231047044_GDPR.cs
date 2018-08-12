namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GDPR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "AllowGzEmail", c => c.Boolean());
            AddColumn("dbo.AspNetUsers", "AllowGzSms", c => c.Boolean());
            AddColumn("dbo.AspNetUsers", "Allow3rdPartySms", c => c.Boolean());
            AddColumn("dbo.AspNetUsers", "AcceptedGdprTc", c => c.Boolean());
            AddColumn("dbo.AspNetUsers", "AcceptedGdprPp", c => c.Boolean());
            AddColumn("dbo.AspNetUsers", "AcceptedGdpr3rdParties", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "AcceptedGdpr3rdParties");
            DropColumn("dbo.AspNetUsers", "AcceptedGdprPp");
            DropColumn("dbo.AspNetUsers", "AcceptedGdprTc");
            DropColumn("dbo.AspNetUsers", "Allow3rdPartySms");
            DropColumn("dbo.AspNetUsers", "AllowGzSms");
            DropColumn("dbo.AspNetUsers", "AllowGzEmail");
        }
    }
}
