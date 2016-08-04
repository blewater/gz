CREATE FUNCTION [dbo].[GetMinDateTrx] (@CustomerId INT, @TrxType INT)
                RETURNS DATETIME AS
                BEGIN

                  DECLARE @RET DATETIME

                  SELECT @RET = IsNull(MIN([T].[CreatedOnUTC]), '1/1/1900')
                  FROM [GzTrxs] AS [T]
                  INNER JOIN [GzTrxTypes] AS [TT] ON [TT].[Id] = [T].[TypeId]
                  WHERE ([T].[CustomerId] = @CustomerId) AND ([TT].[Code] = @TrxType)
  	
                  RETURN @RET

                END