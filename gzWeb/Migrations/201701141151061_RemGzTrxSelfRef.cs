using gzDAL.Models;

namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemGzTrxSelfRef : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.PlayerRevRpt");
            DropForeignKey("dbo.GzTrxs", "ParentTrxId", "dbo.GzTrxs");
            Sql(
                @"IF object_id(N'[dbo].[FK_dbo.GzTransactions_dbo.GzTransactions_ParentTrxId]', N'F') IS NOT NULL
                    ALTER TABLE [dbo].[GzTrxs] DROP CONSTRAINT [FK_dbo.GzTransactions_dbo.GzTransactions_ParentTrxId]");
            DropIndex("dbo.GzTrxs", new[] { "ParentTrxId" });
            DropColumn("dbo.GzTrxs", "ParentTrxId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GzTrxs", "ParentTrxId", c => c.Int());
            CreateIndex("dbo.GzTrxs", "ParentTrxId");
            AddForeignKey("dbo.GzTrxs", "ParentTrxId", "dbo.GzTrxs", "Id");
        }
    }
}
