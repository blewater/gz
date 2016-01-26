namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerInvestments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustPortfolios", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.CustPortfolios", "UpdatedOnUTC", c => c.DateTime(nullable: false));
            DropColumn("dbo.CustPortfolios", "Weight");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustPortfolios", "Weight", c => c.Single(nullable: false));
            DropColumn("dbo.CustPortfolios", "UpdatedOnUTC");
            DropColumn("dbo.CustPortfolios", "Amount");
        }
    }
}
