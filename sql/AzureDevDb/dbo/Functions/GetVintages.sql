CREATE FUNCTION [dbo].[GetVintages]
(
	@CustomerId int
)
RETURNS @VintagesTable TABLE
(
	InvBalanceId INT,
	YearMonthStr NVARCHAR(6),
	InvestmentAmount DECIMAL(29,16),
	Sold BIT
)
AS
BEGIN
	INSERT @VintagesTable
	SELECT Id, YearMonth, CashInvestment, Sold
	FROM dbo.InvBalances
	WHERE 
		CustomerId = @CustomerId
		AND CashInvestment > 0
	ORDER BY YearMonth DESC
	RETURN
END
