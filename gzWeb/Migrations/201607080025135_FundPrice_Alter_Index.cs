namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FundPrice_Alter_Index : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.FundPrices", "FundId_YMD_idx");
            AlterColumn("dbo.FundPrices", "YearMonthDay", c => c.String(nullable: false, maxLength: 8, fixedLength: true, unicode: false));
            Sql(@"CREATE UNIQUE INDEX IDX_FundPrice_Id_YMD
                ON FundPrices (FundId, YearMonthDay DESC)
                INCLUDE (ClosingPrice)
                ON [PRIMARY]");
        }

        public override void Down()
        {
            DropIndex("dbo.FundPrices", "IDX_FundPrice_Id_YMD");
            AlterColumn("dbo.FundPrices", "YearMonthDay", c => c.String(maxLength: 8, fixedLength: true, unicode: false));
            CreateIndex("dbo.FundPrices", new[] { "FundId", "YearMonthDay" }, unique: true, name: "FundId_YMD_idx");
        }
    }
}
