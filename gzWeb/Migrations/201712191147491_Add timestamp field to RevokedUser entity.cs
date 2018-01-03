namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddtimestampfieldtoRevokedUserentity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RevokedUsers", "TimeStampUtc", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RevokedUsers", "TimeStampUtc");
        }
    }
}
