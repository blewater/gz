namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addblockplayerstatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlayerRevRpt", "Player status", c => c.String(maxLength: 256));
            AddColumn("dbo.PlayerRevRpt", "Block reason", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PlayerRevRpt", "Block reason");
            DropColumn("dbo.PlayerRevRpt", "Player status");
        }
    }
}
