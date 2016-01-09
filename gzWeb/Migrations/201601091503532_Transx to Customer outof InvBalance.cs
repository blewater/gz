namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TransxtoCustomeroutofInvBalance : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.InvBalances", "TransxId", "dbo.Transxes");
            DropIndex("dbo.InvBalances", new[] { "TransxId" });
            AddForeignKey("dbo.Transxes", "CustomerId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            DropColumn("dbo.InvBalances", "TransxId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InvBalances", "TransxId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Transxes", "CustomerId", "dbo.AspNetUsers");
            CreateIndex("dbo.InvBalances", "TransxId");
            AddForeignKey("dbo.InvBalances", "TransxId", "dbo.Transxes", "Id", cascadeDelete: true);
        }
    }
}
