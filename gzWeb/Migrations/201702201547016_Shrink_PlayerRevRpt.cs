namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Shrink_PlayerRevRpt : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PlayerRevRpt", "Role");
            DropColumn("dbo.PlayerRevRpt", "Player status");
            DropColumn("dbo.PlayerRevRpt", "Block reason");
            DropColumn("dbo.PlayerRevRpt", "Last login");
            DropColumn("dbo.PlayerRevRpt", "Accepts bonuses");
            DropColumn("dbo.PlayerRevRpt", "Last played date");
            DropColumn("dbo.PlayerRevRpt", "Total bonuses accepted by the player");
            DropColumn("dbo.PlayerRevRpt", "Net revenue");
            DropColumn("dbo.PlayerRevRpt", "Gross revenue");
            DropColumn("dbo.PlayerRevRpt", "NetLossInUSD");
            DropColumn("dbo.PlayerRevRpt", "Real money balance");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlayerRevRpt", "Real money balance", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "NetLossInUSD", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "Gross revenue", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "Net revenue", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "Total bonuses accepted by the player", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "Last played date", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.PlayerRevRpt", "Accepts bonuses", c => c.Boolean());
            AddColumn("dbo.PlayerRevRpt", "Last login", c => c.DateTime());
            AddColumn("dbo.PlayerRevRpt", "Block reason", c => c.String(maxLength: 256));
            AddColumn("dbo.PlayerRevRpt", "Player status", c => c.String(maxLength: 256));
            AddColumn("dbo.PlayerRevRpt", "Role", c => c.String(maxLength: 256));
        }
    }
}
