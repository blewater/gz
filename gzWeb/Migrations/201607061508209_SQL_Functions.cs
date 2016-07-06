namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SQL_Functions : DbMigration
    {
        public override void Up()
        {

            //**** GetMonthsTrxAmount
            Sql(@"IF object_id('GetMonthsTrxAmount') IS NOT NULL DROP FUNCTION GetMonthsTrxAmount;
                EXEC('CREATE FUNCTION [dbo].[GetMonthsTrxAmount]
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
                )')");

            //**** GetTotalDeposits
            Sql(@"IF object_id('GetTotalDeposits') IS NOT NULL DROP FUNCTION GetTotalDeposits;
                EXEC('CREATE FUNCTION [dbo].[GetTotalDeposits]
                (
	                @CustomerId int
                )
                RETURNS TABLE AS RETURN
                (
	                SELECT TOP 1 [Total deposits amount] as TotalDeposit
		                From PlayerRevRpt p JOIN AspNetUsers u ON p.[User ID] = u.GmCustomerId
		                Where u.Id = @CustomerId
		                Order by YearMonthDay desc
                )')");

            //**** GetVintages
            Sql(@"IF object_id('GetVintages') IS NOT NULL DROP FUNCTION GetVintages");
            Sql(@"EXEC('CREATE FUNCTION [dbo].[GetVintages]
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
                END;')");

            //**** GetMinDateTrx
            Sql(@"IF object_id('GetMinDateTrx') IS NOT NULL DROP FUNCTION GetMinDateTrx;
                EXEC('CREATE FUNCTION [dbo].[GetMinDateTrx] (@CustomerId INT, @TrxType INT)
                RETURNS DATETIME AS
                BEGIN

                  DECLARE @RET DATETIME

                  SELECT @RET = IsNull(MIN([T].[CreatedOnUTC]), ''1/1/1900'')
                  FROM [GzTrxs] AS [T]
                  INNER JOIN [GzTrxTypes] AS [TT] ON [TT].[Id] = [T].[TypeId]
                  WHERE ([T].[CustomerId] = @CustomerId) AND ([TT].[Code] = @TrxType)
  	
                  RETURN @RET

                END')");

            //***** GetLatestTrxAmount Not Needed
            Sql(@"IF object_id('GetLatestTrxAmount') IS NOT NULL DROP FUNCTION GetLatestTrxAmount");

            //***** GetTotalTrxAmount
            Sql(@"IF object_id('GetTotalTrxAmount') IS NOT NULL DROP FUNCTION GetTotalTrxAmount;
                EXEC('CREATE FUNCTION [dbo].[GetTotalTrxAmount] (@CustomerId INT, @TrxType INT)
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

                END')");
        }

        public override void Down() {
            //**** GetMonthsTrxAmount
            Sql(@"IF object_id('GetMonthsTrxAmount') IS NOT NULL DROP FUNCTION GetMonthsTrxAmount");
            //**** GetTotalDeposits
            Sql(@"IF object_id('GetTotalDeposits') IS NOT NULL DROP FUNCTION GetTotalDeposits");
            //**** GetVintages
            Sql(@"IF object_id('GetVintages') IS NOT NULL DROP FUNCTION GetVintages");
            //**** GetMinDateTrx
            Sql(@"IF object_id('GetMinDateTrx') IS NOT NULL DROP FUNCTION GetMinDateTrx");
            //**** GetLatestTrxAmount
            Sql(@"IF object_id('GetLatestTrxAmount') IS NOT NULL DROP FUNCTION GetLatestTrxAmount");
            //**** GetTotalTrxAmount
            Sql(@"IF object_id('GetTotalTrxAmount') IS NOT NULL DROP FUNCTION GetTotalTrxAmount");
        }
    }
}
