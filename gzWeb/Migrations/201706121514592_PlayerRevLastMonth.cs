namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerRevLastMonth : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlayerRevLastMonths",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: false, defaultValue:1 ),
                        YearMonth = c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PlayerRevLastMonths");
        }
    }
}
