namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUniquefromPlatformCustomerIdandmakeitnullable : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.AspNetUsers", new[] { "PlatformCustomerId" });
            AlterColumn("dbo.AspNetUsers", "PlatformCustomerId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "PlatformCustomerId", c => c.Int(nullable: false));
            CreateIndex("dbo.AspNetUsers", "PlatformCustomerId", unique: true);
        }
    }
}
