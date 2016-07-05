CREATE FUNCTION dbo.GetTotalTrxAmount (@CustomerId INT, @TrxType INT)
RETURNS DECIMAL AS
BEGIN

  DECLARE @RET DECIMAL(29, 16)

  SELECT @RET = ISNULL(SUM(Amount), 0)
  FROM (
      SELECT [Amount], T.[CustomerId], C.[Code]
      FROM [GzTrxs] AS T
      INNER JOIN [GzTrxTypes] AS C ON C.[Id] = T.[TypeId]
      ) AS [T2]
  WHERE (T2.[CustomerId] = @CustomerId) AND (T2.[Code] = @TrxType)
  	
  RETURN @RET

END