namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change_DynamicPages_From_And_To_To_DateTime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DynamicPages", "LiveFrom", c => c.DateTime(nullable: false));
            AlterColumn("dbo.DynamicPages", "LiveTo", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DynamicPages", "LiveTo", c => c.Boolean(nullable: false));
            AlterColumn("dbo.DynamicPages", "LiveFrom", c => c.Boolean(nullable: false));
        }
    }
}
