namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Many2ManyPortFund : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Funds", "Portfolio_Id", "dbo.Portfolios");
            DropIndex("dbo.Funds", new[] { "Portfolio_Id" });
            DropIndex("dbo.PortFunds", new[] { "PortfolioId" });
            DropPrimaryKey("dbo.PortFunds");
            AddPrimaryKey("dbo.PortFunds", new[] { "PortfolioId", "FundId" });
            CreateIndex("dbo.PortFunds", "PortfolioId");
            CreateIndex("dbo.PortFunds", "FundId");
            AddForeignKey("dbo.PortFunds", "FundId", "dbo.Funds", "Id", cascadeDelete: true);
            DropColumn("dbo.Funds", "Portfolio_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Funds", "Portfolio_Id", c => c.Int());
            DropForeignKey("dbo.PortFunds", "FundId", "dbo.Funds");
            DropIndex("dbo.PortFunds", new[] { "FundId" });
            DropIndex("dbo.PortFunds", new[] { "PortfolioId" });
            DropPrimaryKey("dbo.PortFunds");
            AddPrimaryKey("dbo.PortFunds", "Id");
            CreateIndex("dbo.PortFunds", "PortfolioId");
            CreateIndex("dbo.Funds", "Portfolio_Id");
            AddForeignKey("dbo.Funds", "Portfolio_Id", "dbo.Portfolios", "Id");
        }
    }
}
