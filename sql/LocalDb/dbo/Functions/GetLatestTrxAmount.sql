CREATE FUNCTION dbo.GetLatestTrxAmount (@CustomerId INT, @TrxType INT)
RETURNS DECIMAL AS
BEGIN

  DECLARE @RET DECIMAL(29, 16)

  SELECT @RET = T.Amount
  FROM GzTrxs AS T
  INNER JOIN GzTrxTypes AS TT ON TT.Id = T.TypeId
  WHERE (T.CustomerId = @CustomerId) AND (TT.Code = @TrxType) AND (T.Id = ((
      SELECT MAX(M.Id)
      FROM GzTrxs AS M
      INNER JOIN GzTrxTypes AS IT ON IT.Id = M.TypeId
      WHERE (M.CustomerId = T.CustomerId) AND (IT.Code = TT.Code)
      )))
  	
  RETURN @RET

END