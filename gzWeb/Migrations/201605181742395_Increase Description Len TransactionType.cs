namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncreaseDescriptionLenTransactionType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GzTransactionTypes", "Description", c => c.String(nullable: false, maxLength: 300));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GzTransactionTypes", "Description", c => c.String(maxLength: 128));
        }
    }
}
