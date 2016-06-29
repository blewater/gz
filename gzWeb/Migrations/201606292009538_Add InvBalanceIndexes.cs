namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInvBalanceIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.InvBalances", "CustomerId", name: "CustomerId_Only_idx_invbal");
            CreateIndex("dbo.InvBalances", "YearMonth", name: "YearMonth_Only_idx_invbal");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InvBalances", "YearMonth_Only_idx_invbal");
            DropIndex("dbo.InvBalances", "CustomerId_Only_idx_invbal");
        }
    }
}
