namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TransactionssplittoGmGz : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.GzTransactions", newName: "GzTrxs");
            RenameTable(name: "dbo.GzTransactionTypes", newName: "GzTrxTypes");
            CreateTable(
                "dbo.GmTrxs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        YearMonthCtd = c.String(nullable: false, maxLength: 6),
                        TypeId = c.Int(nullable: false),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 29, scale: 16),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.GmTrxTypes", t => t.TypeId, cascadeDelete: true)
                .Index(t => new { t.CustomerId, t.YearMonthCtd }, name: "CustomerId_Mon_idx_gztransaction")
                .Index(t => t.TypeId);
            
            CreateTable(
                "dbo.GmTrxTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 300),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            AddColumn("dbo.AspNetUsers", "GmCustomerId", c => c.Int());
            AddColumn("dbo.GzTrxs", "GmTrxId", c => c.Int());

            //CreateIndex("dbo.AspNetUsers", "GmCustomerId");
            string indexName = "IX_UQ_GmCustomerId";
            string tableName = "dbo.AspNetUsers";
            string columnName = "GmCustomerId";

            Sql(string.Format(@"
                CREATE UNIQUE NONCLUSTERED INDEX {0}
                ON {1}({2}) 
                WHERE {2} IS NOT NULL;",
                indexName, tableName, columnName));

            CreateIndex("dbo.GzTrxs", "GmTrxId");
            AddForeignKey("dbo.GzTrxs", "GmTrxId", "dbo.GmTrxs", "Id");
            DropColumn("dbo.AspNetUsers", "PlatformCustomerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "PlatformCustomerId", c => c.Int());
            DropForeignKey("dbo.GzTrxs", "GmTrxId", "dbo.GmTrxs");
            DropForeignKey("dbo.GmTrxs", "TypeId", "dbo.GmTrxTypes");
            DropForeignKey("dbo.GmTrxs", "CustomerId", "dbo.AspNetUsers");
            DropIndex("dbo.GzTrxTypes", new[] { "Code" });
            DropIndex("dbo.GmTrxs", new[] { "TypeId" });
            DropIndex("dbo.GmTrxs", "CustomerId_Mon_idx_gztransaction");
            DropIndex("dbo.GzTrxs", new[] { "GmTrxId" });
            //DropIndex("dbo.AspNetUsers", new[] { "GmCustomerId" });
            string indexName = "IX_UQ_GmCustomerId";
            DropIndex("dbo.AspNetUsers", indexName);
            DropColumn("dbo.GzTrxs", "GmTrxId");
            DropColumn("dbo.AspNetUsers", "GmCustomerId");
            DropTable("dbo.GmTrxTypes");
            DropTable("dbo.GmTrxs");
            RenameTable(name: "dbo.GzTrxTypes", newName: "GzTransactionTypes");
            RenameTable(name: "dbo.GzTrxs", newName: "GzTransactions");
        }
    }
}
