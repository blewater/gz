namespace GzBalances

module Portfolio =

    open System
    open System.Net

    type Risk = Low | Medium | High

    
    type PortfolioRisk = {
        Id : int;
        Risk : Risk
    }

    let LowRiskPortfolio = { Id = 1; Risk = Low }
    let MediumRiskPortfolio = { Id = 3; Risk = Medium }
    let HighRiskPortfolio = { Id = 5; Risk = High }

    type Portfolio = { 
        ClosingPrice : float32;
        Risk : PortfolioRisk;
    }

    type CustomerPortfolio = {
        PortfolioLowRisk : Portfolio;
        SharesLowRisk : Decimal;
        PortfolioMediumRisk : Portfolio;
        SharesMediumRisk : Decimal;
        PortfolioHighRisk : Portfolio;
        SharesHighRisk : Decimal;
    }

    type InvBalance = {
        CustomerPortfolio : CustomerPortfolio;
        PortfolioRisk : PortfolioRisk;
        YearMonth : string;
        Balance : Decimal;
        LastUpdated : DateTime;
    }

    type Vintages = InvBalance list

    type YQ = { Symbol : string; TradingDate : DateTime; ClosingPrice : float32 }

    // URL of a service that generates price data
    let url = "http://ichart.finance.yahoo.com/table.csv?s="

    /// Returns prices (as YQ type) of a given stock for a 
    /// specified number of days on ascending order of trading time
    let getStockPrices stock count =
        // Download the data and split it into lines
        let wc = new WebClient()
        let data = wc.DownloadString(url + stock)
        let dataLines = 
            data.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries) 

        // Parse lines of the CSV file and take specified
        // number of days using in the oldest to newest order
        seq { for line in dataLines |> Seq.skip 1 do
                  let infos = line.Split(',')
                  let tradingDay = 
                    match DateTime.TryParseExact(infos.[0], "yyyy-MM-dd", null, Globalization.DateTimeStyles.None) with
                    | (true, dt) -> dt
                    | (false, _) -> invalidArg "Yahoo Finance Api Date" (sprintf "Couldn't parse this yahoo api date %s" infos.[0])
                  let yQuote = { Symbol = stock; TradingDate = tradingDay; ClosingPrice = float32 infos.[4]}
                  yield yQuote }
        |> Seq.take count |> Array.ofSeq |> Array.rev
