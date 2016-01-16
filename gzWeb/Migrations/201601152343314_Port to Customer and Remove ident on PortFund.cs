namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PorttoCustomerandRemoveidentonPortFund : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Portfolios", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.CustPortfolios", new[] { "CustomerId" });
            DropIndex("dbo.Portfolios", new[] { "ApplicationUser_Id" });
            DropPrimaryKey("dbo.CustPortfolios");
            AddPrimaryKey("dbo.CustPortfolios", new[] { "CustomerId", "PortfolioId" });
            CreateIndex("dbo.CustPortfolios", "PortfolioId");
            AddForeignKey("dbo.CustPortfolios", "PortfolioId", "dbo.Portfolios", "Id", cascadeDelete: true);
            DropColumn("dbo.CustPortfolios", "Id");
            DropColumn("dbo.PortFunds", "Id");
            DropColumn("dbo.Portfolios", "ApplicationUser_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Portfolios", "ApplicationUser_Id", c => c.Int());
            AddColumn("dbo.PortFunds", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.CustPortfolios", "Id", c => c.Int(nullable: false, identity: true));
            DropForeignKey("dbo.CustPortfolios", "PortfolioId", "dbo.Portfolios");
            DropIndex("dbo.CustPortfolios", new[] { "PortfolioId" });
            DropPrimaryKey("dbo.CustPortfolios");
            AddPrimaryKey("dbo.CustPortfolios", "Id");
            CreateIndex("dbo.Portfolios", "ApplicationUser_Id");
            CreateIndex("dbo.CustPortfolios", "CustomerId");
            AddForeignKey("dbo.Portfolios", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
