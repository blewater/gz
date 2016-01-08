namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TuningTransxInvBalance : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.InvBalances", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.InvBalances", new[] { "CustomerId" });
            DropPrimaryKey("dbo.InvBalances");
            AddColumn("dbo.InvBalances", "YearMonthCtd", c => c.String(maxLength: 6));
            AddColumn("dbo.AspNetUsers", "GamBalance", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.AspNetUsers", "GamBalanceUpdOnUTC", c => c.DateTime());
            AddColumn("dbo.Transxes", "CustomerId", c => c.Int(nullable: false));
            AddColumn("dbo.Transxes", "YearMonthCtd", c => c.String(maxLength: 6));
            AddColumn("dbo.Transxes", "CreatedOnUTC", c => c.DateTime(nullable: false));
            AlterColumn("dbo.TransxTypes", "Code", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.InvBalances", "Id");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonthCtd" }, unique: true, name: "CustomerId_Mon_idx_invbal");
            CreateIndex("dbo.Transxes", new[] { "CustomerId", "YearMonthCtd" }, unique: true, name: "CustomerId_Mon_idx_transx");
            CreateIndex("dbo.Transxes", "CreatedOnUTC");
            CreateIndex("dbo.TransxTypes", "Code", unique: true);
            AddForeignKey("dbo.InvBalances", "CustomerId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            DropColumn("dbo.InvBalances", "Month");
            DropColumn("dbo.InvBalances", "Year");
            DropColumn("dbo.AspNetUsers", "PlatformBalance");
            DropColumn("dbo.AspNetUsers", "LastUpdatedBalance");
            DropColumn("dbo.TransxTypes", "Reason");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransxTypes", "Reason", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "LastUpdatedBalance", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "PlatformBalance", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.InvBalances", "Year", c => c.Int(nullable: false));
            AddColumn("dbo.InvBalances", "Month", c => c.Int(nullable: false));
            DropForeignKey("dbo.InvBalances", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.TransxTypes", new[] { "Code" });
            DropIndex("dbo.Transxes", new[] { "CreatedOnUTC" });
            DropIndex("dbo.Transxes", "CustomerId_Mon_idx_transx");
            DropIndex("dbo.InvBalances", "CustomerId_Mon_idx_invbal");
            DropPrimaryKey("dbo.InvBalances");
            AlterColumn("dbo.TransxTypes", "Code", c => c.String(nullable: false, maxLength: 30));
            DropColumn("dbo.Transxes", "CreatedOnUTC");
            DropColumn("dbo.Transxes", "YearMonthCtd");
            DropColumn("dbo.Transxes", "CustomerId");
            DropColumn("dbo.AspNetUsers", "GamBalanceUpdOnUTC");
            DropColumn("dbo.AspNetUsers", "GamBalance");
            DropColumn("dbo.InvBalances", "YearMonthCtd");
            AddPrimaryKey("dbo.InvBalances", "CustomerId");
            CreateIndex("dbo.InvBalances", "CustomerId");
            AddForeignKey("dbo.InvBalances", "CustomerId", "dbo.AspNetUsers", "Id");
        }
    }
}
