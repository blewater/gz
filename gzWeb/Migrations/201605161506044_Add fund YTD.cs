namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddfundYTD : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Funds", "YearToDate", c => c.Single(nullable: false));
            DropColumn("dbo.Funds", "ThreeYrReturnPcnt");
            DropColumn("dbo.Funds", "FiveYrReturnPcnt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Funds", "FiveYrReturnPcnt", c => c.Single(nullable: false));
            AddColumn("dbo.Funds", "ThreeYrReturnPcnt", c => c.Single(nullable: false));
            DropColumn("dbo.Funds", "YearToDate");
        }
    }
}
