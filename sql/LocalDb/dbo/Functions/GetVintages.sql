CREATE FUNCTION [dbo].[GetVintages]
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
                END;