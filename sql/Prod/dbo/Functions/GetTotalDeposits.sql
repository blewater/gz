CREATE FUNCTION [dbo].[GetTotalDeposits]
                (
	                @CustomerId int
                )
                RETURNS TABLE AS RETURN
                (
	                SELECT TOP 1 [Total deposits amount] as TotalDeposit
		                From PlayerRevRpt p JOIN AspNetUsers u ON p.[User ID] = u.GmCustomerId
		                Where u.Id = @CustomerId
		                Order by YearMonthDay desc
                )