namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix : DbMigration
    {
        public override void Up()
        {
            // Already added 2 migrations before
            //AddColumn("dbo.AspNetUsers", "DisabledGzCustomer", c => c.Boolean(nullable: false));
            //AddColumn("dbo.AspNetUsers", "ClosedGzAccount", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            // Already added 2 migrations before
            //DropColumn("dbo.AspNetUsers", "ClosedGzAccount");
            //DropColumn("dbo.AspNetUsers", "DisabledGzCustomer");
        }
    }
}
