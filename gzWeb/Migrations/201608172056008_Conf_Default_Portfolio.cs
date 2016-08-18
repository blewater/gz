using gzDAL.Models;

namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Conf_Default_Portfolio : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GzConfigurations", "FIRST_PORTFOLIO_RISK_VAL", c => c.Int(nullable: false, defaultValue: (int)RiskToleranceEnum.Medium));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GzConfigurations", "FIRST_PORTFOLIO_RISK_VAL");
        }
    }
}
