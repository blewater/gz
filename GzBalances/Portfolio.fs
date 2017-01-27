namespace GzBalances

module Portfolio =

    open System
    open System.Net
    open GzDb.DbUtil

    //type Risk = Low | Medium | High

    type Risk = Low | Medium | High

    type PortfolioRisk = {
        Id : int;
        Risk : Risk
    }

    let LowRiskPortfolio = { Id = 1; Risk = Low}
    let MediumRiskPortfolio = { Id = 3; Risk = Medium }
    let HighRiskPortfolio = { Id = 5; Risk = High }

    let getPortfolioById (portfolioId : int) : PortfolioRisk =
        if LowRiskPortfolio.Id = portfolioId then
            LowRiskPortfolio
        elif MediumRiskPortfolio.Id = portfolioId then
            MediumRiskPortfolio
        elif HighRiskPortfolio.Id = portfolioId then
            HighRiskPortfolio
        else invalidArg "Portfolio Id" (sprintf "Unknown portfolio id: %d" portfolioId)

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

    type FundQuote = { Symbol : string; TradedOn : DateTime; ClosingPrice : float }
    type PortfolioId = int
    type PortfolioWeight = float32
    type PortfolioFundRecord = {PortfolioId : PortfolioId; PortfolioWeight : PortfolioWeight; Fund : FundQuote}
    type PortfolioShare = {
        PortfolioId : PortfolioId;
        Price : float;
        TradedOn : DateTime;
    }
    //type PortfolioFunds = PortfolioFund list

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
                  let tradedOn = 
                    match DateTime.TryParseExact(infos.[0], "yyyy-MM-dd", null, Globalization.DateTimeStyles.None) with
                    | (true, dt) -> dt
                    | (false, _) -> invalidArg "Yahoo Finance Api Date" (sprintf "Couldn't parse this yahoo api date %s" infos.[0])
                  let yQuote = { Symbol = stock; TradedOn = tradedOn; ClosingPrice = float infos.[4]}
                  yield yQuote }
        |> Seq.take count |> Seq.rev


    /// 1. Read The portfolio funds from Db
    /// 2. Ask yahoo their closing prices
    /// 3. Calculate each portfolio single virtual share closing price
    let getPortfolioPrices(db : DbContext) : PortfolioShare seq =

        let gSeq =
            query { 
                for f in db.Funds do
                join fp in db.PortFunds on (f.Id = fp.FundId)
                sortBy fp.PortfolioId
                select (f, fp)
            }
            // Ask yahoo api funds closing pricing
            |> Seq.map(fun (f : DbFunds, fp : DbPortfolioFunds) -> 
                let quote = getStockPrices f.Symbol 1 |> Seq.head
                let portfolioFundRec = {PortfolioId = fp.PortfolioId; PortfolioWeight = fp.Weight; Fund = quote}
                portfolioFundRec)
            // Group per portfolio
            |> Seq.groupBy (fun (portfolioFundRec : PortfolioFundRecord) -> portfolioFundRec.PortfolioId)
            // Flatten the seq<fundquote> to a single price per portfolioId group
            |> Seq.map (
                fun (p : PortfolioId, spsf : (seq<PortfolioFundRecord>)) ->
                    let price = 
                        spsf 
                        |> Seq.map (fun (pfr : PortfolioFundRecord) -> pfr.Fund.ClosingPrice * float pfr.PortfolioWeight / 100.0) 
                        |> Seq.reduce (+)
                    let topPortfolioFundRecord = spsf |> Seq.head
                    { PortfolioId = p; Price = price; TradedOn = topPortfolioFundRecord.Fund.TradedOn }
            )
//        gSeq |> Seq.iter(fun i -> printfn "%A" i)
        gSeq
