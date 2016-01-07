namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlatformTies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PlatformCustomerId", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "ActiveCustomerIdInPlatform", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "PlatformBalance", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.AspNetUsers", "LastUpdatedBalance", c => c.DateTime());
            CreateIndex("dbo.AspNetUsers", "PlatformCustomerId", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.AspNetUsers", new[] { "PlatformCustomerId" });
            DropColumn("dbo.AspNetUsers", "LastUpdatedBalance");
            DropColumn("dbo.AspNetUsers", "PlatformBalance");
            DropColumn("dbo.AspNetUsers", "ActiveCustomerIdInPlatform");
            DropColumn("dbo.AspNetUsers", "PlatformCustomerId");
        }
    }
}
