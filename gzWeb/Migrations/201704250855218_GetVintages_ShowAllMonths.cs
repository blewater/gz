namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GetVintages_ShowAllMonths : DbMigration
    {
        /// <summary>
        /// 
        /// Remove CashInvestment > 0 to show all months
        /// 
        /// </summary>
        public override void Up()
        {
            Sql(@"ALTER FUNCTION [dbo].[GetVintages]
                (
	                @CustomerId int
                )
                RETURNS @VintagesTable TABLE
                (
	                InvBalanceId INT,
	                YearMonthStr NVARCHAR(6),
	                InvestmentAmount DECIMAL(29,16),
	                Sold BIT,
	                SellingValue DECIMAL(29,16),
	                SoldFees DECIMAL(29,16),
	                SoldYearMonth CHAR(6)
                )
                AS
                BEGIN
	                INSERT @VintagesTable
	                SELECT Id, YearMonth, CashInvestment, Sold, ISNULL(SoldAmount, 0), SoldFees, SoldYearMonth
	                FROM dbo.InvBalances
	                WHERE 
		                CustomerId = @CustomerId
	                ORDER BY YearMonth DESC
	                RETURN
                END;");
        }

        /// <summary>
        /// 
        /// Add restriction back to function for cashinvestment > 0.
        /// 
        /// </summary>
        public override void Down()
        {
            Sql(@"ALTER FUNCTION [dbo].[GetVintages]
                (
	                @CustomerId int
                )
                RETURNS @VintagesTable TABLE
                (
	                InvBalanceId INT,
	                YearMonthStr NVARCHAR(6),
	                InvestmentAmount DECIMAL(29,16),
	                Sold BIT,
	                SellingValue DECIMAL(29,16),
	                SoldFees DECIMAL(29,16),
	                SoldYearMonth CHAR(6)
                )
                AS
                BEGIN
	                INSERT @VintagesTable
	                SELECT Id, YearMonth, CashInvestment, Sold, ISNULL(SoldAmount, 0), SoldFees, SoldYearMonth
	                FROM dbo.InvBalances
	                WHERE 
		                CustomerId = @CustomerId
		                AND CashInvestment > 0
	                ORDER BY YearMonth DESC
	                RETURN
                END;");
        }
    }
}
