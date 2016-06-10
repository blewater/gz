namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LogEntryTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Application = c.String(nullable: false),
                        Logged = c.DateTime(nullable: false),
                        Level = c.String(nullable: false),
                        Message = c.String(nullable: false),
                        UserName = c.String(maxLength: 250),
                        ServerName = c.String(),
                        Port = c.String(),
                        Url = c.String(),
                        Https = c.Boolean(nullable: false),
                        ServerAddress = c.String(),
                        RemoteAddress = c.String(),
                        Logger = c.String(),
                        CallSite = c.String(),
                        Exception = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LogEntries");
        }
    }
}
