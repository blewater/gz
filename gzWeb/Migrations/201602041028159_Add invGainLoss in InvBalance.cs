namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddinvGainLossinInvBalance : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.InvBalances", "CustomerId_Mon_idx_invbal");
            RenameColumn("dbo.InvBalances", "YearMonthCtd", "YearMonth");
            AlterColumn("dbo.InvBalances", "YearMonth", c => c.String(nullable: false, maxLength:6, fixedLength:true));
            AddColumn("dbo.InvBalances", "InvGainLoss", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonth" }, unique: true, name: "CustomerId_Mon_idx_invbal");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InvBalances", "CustomerId_Mon_idx_invbal");
            RenameColumn("dbo.InvBalances", "YearMonth", "YearMonthCtd");
            AlterColumn("dbo.InvBalances", "YearMonthCtd", c => c.String(nullable: true, maxLength: 6));
            DropColumn("dbo.InvBalances", "InvGainLoss");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonthCtd" }, unique: true, name: "CustomerId_Mon_idx_invbal");
        }
    }
}
