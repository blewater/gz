namespace GzBalances

module DailyPortfolioShares =
    open System
    open GzBatchCommon
    open GzDb.DbUtil
    open GzBalances.PortfolioTypes

    let getLatestPortfolioPrices (db : DbContext)(yyyyMmDd : string) : PortfoliosPrices =

        let dbClosingPrices =
            query {
                for row in db.PortfolioPrices do
                where (String.Compare(row.YearMonthDay, yyyyMmDd, StringComparison.Ordinal) <= 0)
                sortByDescending row.Id
                select row
                take 1
                exactlyOne
            }
        // Return PortfolioPrices
        {
            PortfolioLowRiskPrice = 
                { 
                    PortfolioId = LowRiskPortfolioId; 
                    Price = dbClosingPrices.PortfolioLowPrice; 
                    TradedOn = dbClosingPrices.YearMonthDay.ToDateWithDay 
                };
            PortfolioMediumRiskPrice = 
                { 
                    PortfolioId = MediumRiskPortfolioId; 
                    Price = dbClosingPrices.PortfolioMediumPrice; 
                    TradedOn = dbClosingPrices.YearMonthDay.ToDateWithDay 
                };
            PortfolioHighRiskPrice = 
                { 
                    PortfolioId = HighRiskPortfolioId; 
                    Price = dbClosingPrices.PortfolioHighPrice; 
                    TradedOn = dbClosingPrices.YearMonthDay.ToDateWithDay 
                };
        }