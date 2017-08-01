namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Awarded_Vintage_Bonus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBalances", "AwardedSoldAmount", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InvBalances", "AwardedSoldAmount");
        }
    }
}
