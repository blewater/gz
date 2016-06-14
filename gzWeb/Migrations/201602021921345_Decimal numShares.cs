namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DecimalnumShares : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CustFundShares", "NumShares", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CustFundShares", "NumShares", c => c.Single(nullable: false));
        }
    }
}
