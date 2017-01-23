namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPlayerRevRpt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlayerRevRpt",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserID = c.Int(name: "User ID", nullable: false),
                        Username = c.String(nullable: false, maxLength: 256),
                        YearMonth = c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false),
                        YearMonthDay = c.String(nullable: false, maxLength: 8, fixedLength: true, unicode: false),
                        BegBalance = c.Decimal(precision: 29, scale: 16),
                        EndBalance = c.Decimal(precision: 29, scale: 16),
                        PlayerLoss = c.Decimal(precision: 29, scale: 16),
                        Role = c.String(maxLength: 256),
                        Playerstatus = c.String(name: "Player status", maxLength: 256),
                        Blockreason = c.String(name: "Block reason", maxLength: 256),
                        Emailaddress = c.String(name: "Email address", nullable: false, maxLength: 256),
                        Lastlogin = c.DateTime(name: "Last login"),
                        Acceptsbonuses = c.Boolean(name: "Accepts bonuses"),
                        Totaldepositsamount = c.Decimal(name: "Total deposits amount", precision: 29, scale: 16),
                        Withdrawsmade = c.Decimal(name: "Withdraws made", precision: 29, scale: 16),
                        Lastplayeddate = c.DateTime(name: "Last played date", storeType: "date"),
                        Depositsmade = c.Decimal(name: "Deposits made", precision: 29, scale: 16),
                        Currency = c.String(nullable: false, maxLength: 3, fixedLength: true, unicode: false),
                        Totalbonusesacceptedbytheplayer = c.Decimal(name: "Total bonuses accepted by the player", precision: 29, scale: 16),
                        Netrevenue = c.Decimal(name: "Net revenue", precision: 29, scale: 16),
                        Grossrevenue = c.Decimal(name: "Gross revenue", precision: 29, scale: 16),
                        NetLossInUSD = c.Decimal(precision: 29, scale: 16),
                        Realmoneybalance = c.Decimal(name: "Real money balance", precision: 29, scale: 16),
                        Processed = c.Int(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserID, t.YearMonth }, unique: true, name: "IX_UserId_YM")
                .Index(t => t.YearMonth, name: "IX_YM");
            
            AddColumn("dbo.GzTrxs", "PlayerRevRptId", c => c.Int());
            CreateIndex("dbo.GzTrxs", "PlayerRevRptId");
            AddForeignKey("dbo.GzTrxs", "PlayerRevRptId", "dbo.PlayerRevRpt", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GzTrxs", "PlayerRevRptId", "dbo.PlayerRevRpt");
            DropIndex("dbo.PlayerRevRpt", "IX_YM");
            DropIndex("dbo.PlayerRevRpt", "IX_UserId_YM");
            DropIndex("dbo.GzTrxs", new[] { "PlayerRevRptId" });
            DropColumn("dbo.GzTrxs", "PlayerRevRptId");
            DropTable("dbo.PlayerRevRpt");
        }
    }
}
