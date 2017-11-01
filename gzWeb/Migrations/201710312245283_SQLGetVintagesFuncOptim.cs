using Glimpse.Ado.Tab;

namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SQLGetVintagesFuncOptim : DbMigration
    {
        public override void Up() {
            Sql(@"/****** Object:  UserDefinedFunction [dbo].[GetVintages]    Script Date: 11/1/2017 12:38:47 AM ******/
ALTER FUNCTION [dbo].[GetVintages]
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
	                SELECT Id, YearMonth, CashInvestment, Sold, ISNULL(SoldAmount, 0), ISNULL(SoldFees, 0), SoldYearMonth
	                FROM dbo.InvBalances
	                WHERE 
		                CustomerId = @CustomerId
	                ORDER BY YearMonth DESC
	                RETURN
                END;
GO");
        }
        
        public override void Down()
        {
            Sql(@"/****** Object:  UserDefinedFunction [dbo].[GetVintages]    Script Date: 11/1/2017 12:38:47 AM ******/
ALTER FUNCTION [dbo].[GetVintages]
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
                END;
GO");
        }
    }
}
