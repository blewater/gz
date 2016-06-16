namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveGamBalanceinApplicationUser : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "GamBalance");
            DropColumn("dbo.AspNetUsers", "GamBalanceUpdOnUTC");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "GamBalanceUpdOnUTC", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "GamBalance", c => c.Decimal(precision: 29, scale: 16));
        }
    }
}
