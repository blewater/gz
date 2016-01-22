namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FundReturns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Funds", "ThreeYrReturnPcnt", c => c.Single(nullable: false));
            AddColumn("dbo.Funds", "FiveYrReturnPcnt", c => c.Single(nullable: false));
            AddColumn("dbo.Funds", "UpdatedOnUTC", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Funds", "UpdatedOnUTC");
            DropColumn("dbo.Funds", "FiveYrReturnPcnt");
            DropColumn("dbo.Funds", "ThreeYrReturnPcnt");
        }
    }
}
