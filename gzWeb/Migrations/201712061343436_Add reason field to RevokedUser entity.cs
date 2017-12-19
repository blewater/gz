namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddreasonfieldtoRevokedUserentity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RevokedUsers", "Reason", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RevokedUsers", "Reason");
        }
    }
}
