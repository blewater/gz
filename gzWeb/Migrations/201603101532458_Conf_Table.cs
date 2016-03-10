namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Conf_Table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GzConfigurations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LOCK_IN_NUM_DAYS = c.Int(nullable: false),
                        COMMISSION_PCNT = c.Single(nullable: false),
                        FUND_FEE_PCNT = c.Single(nullable: false),
                        CREDIT_LOSS_PCNT = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GzConfigurations");
        }
    }
}
