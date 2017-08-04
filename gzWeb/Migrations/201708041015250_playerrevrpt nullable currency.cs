namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class playerrevrptnullablecurrency : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PlayerRevRpt", "Currency", c => c.String(maxLength: 3, fixedLength: true, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PlayerRevRpt", "Currency", c => c.String(nullable: false, maxLength: 3, fixedLength: true, unicode: false));
        }
    }
}
