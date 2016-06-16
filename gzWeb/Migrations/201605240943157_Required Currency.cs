namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequiredCurrency : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "Currency", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "Currency", c => c.String());
        }
    }
}
