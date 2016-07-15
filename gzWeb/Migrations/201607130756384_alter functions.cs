namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alterfunctions : DbMigration
    {
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
		                AND CashInvestment > 0
	                ORDER BY YearMonth DESC
	                RETURN
                END;");
            Sql(@"ALTER FUNCTION [dbo].[GetMonthsTrxAmount]
                (
	                @CustomerId INT,
	                @YearMonth CHAR(6),
	                @TypeCodeId INT
                )
                RETURNS TABLE AS RETURN
                (
	                SELECT T.Amount FROM GzTrxs T 
		                JOIN GzTrxTypes TT
			                ON T.TypeId = TT.Id
		                WHERE CustomerId = @CustomerId 
			                AND YearMonthCtd = @YearMonth
			                AND TT.Code = @TypeCodeId
                );");
        }
        
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
	                SoldAmount DECIMAL(29,16),
	                SoldFees DECIMAL(29,16),
	                SoldYearMonth CHAR(6)
                )
                AS
                BEGIN
	                INSERT @VintagesTable
	                SELECT Id, YearMonth, CashInvestment, Sold, SoldAmount, SoldFees, SoldYearMonth
	                FROM dbo.InvBalances
	                WHERE 
		                CustomerId = @CustomerId
		                AND CashInvestment > 0
	                ORDER BY YearMonth DESC
	                RETURN
                END;");
            Sql(@"ALTER FUNCTION [dbo].[GetMonthsTrxAmount]
                (
	                @CustomerId INT,
	                @YearMonth NVARCHAR(6),
	                @TypeCodeId INT
                )
                RETURNS TABLE AS RETURN
                (
	                SELECT T.Amount FROM GzTrxs T 
		                JOIN GzTrxTypes TT
			                ON T.TypeId = TT.Id
		                WHERE CustomerId = @CustomerId 
			                AND YearMonthCtd = @YearMonth
			                AND TT.Code = @TypeCodeId
                );");
        }
    }
}
