CREATE FUNCTION [dbo].[GetMonthsTrxAmount]
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

)
