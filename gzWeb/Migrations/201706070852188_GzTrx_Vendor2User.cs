namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GzTrx_Vendor2User : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GzTrxs", "CashBonusAmount", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValue: 0));
            AddColumn("dbo.GzTrxs", "Vendor2UserDeposits", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GzTrxs", "Vendor2UserDeposits");
            DropColumn("dbo.GzTrxs", "CashBonusAmount");
        }
    }
}
