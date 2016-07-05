/* 

-- Check if missing CustPortfolio Rows for any row of InvBalance
SELECT c.PortfolioId, c.YearMonth, c.CustomerId, b.YearMonth, b.CustomerId FROM invBalances b
Left JOIN dbo.CustPortfolios c ON b.CustomerId = c.CustomerId AND b.YearMonth = c.YearMonth
--WHERE c.PortfolioId IS NULL
ORDER BY b.CustomerId, b.YearMonth

*/

Begin Tran

DECLARE userIds_cursor CURSOR
FOR
   SELECT id 
   FROM dbo.AspNetUsers
OPEN userIds_cursor
DECLARE @CustomerId INT
FETCH NEXT FROM userIds_cursor INTO @CustomerId
WHILE (@@FETCH_STATUS <> -1)
BEGIN
	IF (@@FETCH_STATUS <> -2)
	BEGIN   
		PRINT '**** Begin Customer Ids Outer Cursor, CustomerId: ' + CAST(@CustomerId AS VARCHAR)
​
		DECLARE 
			@yearMonthDay DATE = '01/01/2015',
			@yearMonth NVARCHAR(6),
			@portfolioId INT,
			@monthsDiff INT
​
/*** Loop through each customer and insert the missing monthly portfolio ***/
		SET @yearMonth = FORMAT(@yearMonthDay,'yyyyMM')
		WHILE (1=1)
		BEGIN
​
			/** Insert missing CustPortfolios ***/
			SELECT @portfolioId = PortfolioId
				FROM dbo.CustPortfolios
				WHERE CustomerId = @CustomerId AND YearMonth = @yearMonth
			IF @@ROWCOUNT = 0
			BEGIN   
​
				SET @portfolioId = 3
				INSERT INTO dbo.CustPortfolios (CustomerId, YearMonth, PortfolioId, Weight, UpdatedOnUTC)
	  			SELECT @CustomerId, @yearMonth, @portfolioId, 100, GETUTCDATE()
​
				PRINT 'Inserted CustomerPortfolio: CustomerId: ' + CAST(@CustomerId AS VARCHAR) + ', YearMonth: ' + @yearMonth + '.'
			END
​
			SET @yearMonthDay = DATEADD(m, 1, @yearMonthDay)
			SET @yearMonth = FORMAT(@yearMonthDay,'yyyyMM')
			SET @monthsDiff = DATEDIFF(m, GETUTCDATE(), @yearMonthDay)
			IF @monthsDiff > 2 BREAK; /* + 2 months from present */
		END
​
		PRINT '**** End Inserting loop into CustPortfolios'
	END
	FETCH NEXT FROM userIds_cursor INTO @CustomerId
    PRINT '**** End Customer Ids Outer Cursor'
END
CLOSE userIds_cursor
DEALLOCATE userIds_cursor

commit tran;