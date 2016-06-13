namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActiveGzCustomer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DisabledGzCustomer", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "ClosedGzAccount", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ClosedGzAccount");
            DropColumn("dbo.AspNetUsers", "DisabledGzCustomer");
        }
    }
}
