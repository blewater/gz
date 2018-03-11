namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CashDeposits_XReportDates : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.InvBalances", "Vendor2UserDeposits", "CashDeposits");
            RenameColumn("dbo.GzTrxs", "Vendor2UserDeposits", "CashDeposits");
            RenameColumn("dbo.PlayerRevRpt", "Vendor2UserDeposits", "CashDeposits");
            AddColumn("dbo.GzTrxs", "JoinDate", c => c.DateTime());
            AddColumn("dbo.GzTrxs", "FirstDeposit", c => c.DateTime());
            AddColumn("dbo.GzTrxs", "LastDeposit", c => c.DateTime());
            AddColumn("dbo.GzTrxs", "LastPlayedDate", c => c.DateTime());
            AddColumn("dbo.PlayerRevRpt", "JoinDate", c => c.DateTime());
            AddColumn("dbo.PlayerRevRpt", "FirstDeposit", c => c.DateTime());
            AddColumn("dbo.PlayerRevRpt", "LastDeposit", c => c.DateTime());
            AddColumn("dbo.PlayerRevRpt", "LastPlayedDate", c => c.DateTime());
            AlterColumn("dbo.PlayerRevRpt", "CashDeposits", c => c.Decimal(nullable: false, defaultValue: 0, defaultValueSql: "0"));
            AlterColumn("dbo.GzTrxs", "CashDeposits", c => c.Decimal(nullable: false, defaultValue: 0, defaultValueSql: "0"));
            AlterColumn("dbo.InvBalances", "CashDeposits", c => c.Decimal(nullable: false, defaultValue: 0, defaultValueSql: "0"));
        }

        public override void Down()
        {
            RenameColumn("dbo.InvBalances", "CashDeposits", "Vendor2UserDeposits");
            RenameColumn("dbo.GzTrxs", "CashDeposits", "Vendor2UserDeposits");
            RenameColumn("dbo.PlayerRevRpt", "CashDeposits", "Vendor2UserDeposits");
            DropColumn("dbo.PlayerRevRpt", "LastPlayedDate");
            DropColumn("dbo.PlayerRevRpt", "LastDeposit");
            DropColumn("dbo.PlayerRevRpt", "FirstDeposit");
            DropColumn("dbo.PlayerRevRpt", "JoinDate");
            DropColumn("dbo.GzTrxs", "LastPlayedDate");
            DropColumn("dbo.GzTrxs", "LastDeposit");
            DropColumn("dbo.GzTrxs", "FirstDeposit");
            DropColumn("dbo.GzTrxs", "JoinDate");
            AlterColumn("dbo.PlayerRevRpt", "Vendor2UserDeposits", c => c.Decimal(defaultValueSql: "0"));
            AlterColumn("dbo.GzTrxs", "Vendor2UserDeposits", c => c.Decimal(defaultValueSql: "0"));
            AlterColumn("dbo.InvBalances", "Vendor2UserDeposits", c => c.Decimal(defaultValueSql: "0"));
        }
    }
}
