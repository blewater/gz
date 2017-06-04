namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Vendor2User : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBalances", "CashBonusAmount", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValue: 0));
            AddColumn("dbo.InvBalances", "Vendor2UserDeposits", c => c.Decimal(nullable: false, precision: 29, scale: 16, defaultValue: 0));
            AddColumn("dbo.PlayerRevRpt", "CashBonusAmount", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "Vendor2UserDeposits", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.PlayerRevRpt", "Phone", c => c.String(maxLength: 30));
            AddColumn("dbo.PlayerRevRpt", "Mobile", c => c.String(maxLength: 30));
            AddColumn("dbo.PlayerRevRpt", "City", c => c.String(maxLength: 80));
            AddColumn("dbo.PlayerRevRpt", "Country", c => c.String(maxLength: 80));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PlayerRevRpt", "Country");
            DropColumn("dbo.PlayerRevRpt", "City");
            DropColumn("dbo.PlayerRevRpt", "Mobile");
            DropColumn("dbo.PlayerRevRpt", "Phone");
            DropColumn("dbo.PlayerRevRpt", "Vendor2UserDeposits");
            DropColumn("dbo.PlayerRevRpt", "CashBonusAmount");
            DropColumn("dbo.InvBalances", "Vendor2UserDeposits");
            DropColumn("dbo.InvBalances", "CashBonusAmount");
        }
    }
}
