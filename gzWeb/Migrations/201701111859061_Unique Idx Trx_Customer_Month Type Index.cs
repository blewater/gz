namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UniqueIdxTrx_Customer_MonthTypeIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TId_Amnt");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId", "Amount" }, unique: true, name: "IX_CustomerId_YM_TId_Amnt");

            //**** GetTotalDeposits no longer needed
            Sql(@"IF object_id('GetTotalDeposits') IS NOT NULL DROP FUNCTION GetTotalDeposits");
        }

        public override void Down()
        {
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TId_Amnt");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId", "Amount" }, name: "IX_CustomerId_YM_TId_Amnt");
            //**** Add back GetTotalDeposits
            Sql(@"IF object_id('GetTotalDeposits') IS NOT NULL DROP FUNCTION GetTotalDeposits;
                EXEC('CREATE FUNCTION [dbo].[GetTotalDeposits]
                (
	                @CustomerId int
                )
                RETURNS TABLE AS RETURN
                (
	                SELECT TOP 1 [Total deposits amount] as TotalDeposit
		                From PlayerRevRpt p JOIN AspNetUsers u ON p.[User ID] = u.GmCustomerId
		                Where u.Id = @CustomerId
		                Order by YearMonthDay desc
                )')");

        }
    }
}
