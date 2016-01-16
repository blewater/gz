namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PortfolioUniqRiskTol : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Portfolios", "RiskTolerance", c => c.Int(nullable: false));
            CreateIndex("dbo.Portfolios", "RiskTolerance", unique: true);
            DropColumn("dbo.Portfolios", "Name");
            DropColumn("dbo.Portfolios", "Note");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Portfolios", "Note", c => c.String(maxLength: 128));
            AddColumn("dbo.Portfolios", "Name", c => c.String(maxLength: 128));
            DropIndex("dbo.Portfolios", new[] { "RiskTolerance" });
            AlterColumn("dbo.Portfolios", "RiskTolerance", c => c.String(maxLength: 64));
        }
    }
}
