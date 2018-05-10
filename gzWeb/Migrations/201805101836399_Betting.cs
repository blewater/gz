namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Betting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 256),
                        YearMonthDay = c.String(nullable: false, maxLength: 8, fixedLength: true, unicode: false),
                        Rounds = c.Single(nullable: false),
                        UserWin = c.Single(nullable: false),
                        Payout = c.Single(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Bettings");
        }
    }
}
