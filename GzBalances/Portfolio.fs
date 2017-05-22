namespace GzBalances

module DailyPortfolioShares =
    open System
    open System.Net
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes
    open GzCommon.ErrorHandling
    open FSharp.Data

    type Funds = CsvProvider<"quotes.csv", HasHeaders=false, Schema = "TradedOn (Date),Symbol (string),ClosedPrice (float)">

    //http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20(%22VTI%22)&env=store://datatables.org/alltableswithkeys
    //http://finance.yahoo.com/d/quotes.csv?s=MUB,VSGBX,VTI,VBMFX,VSTCX,CFA&f=d1sp

    // URL of a service that generates price data
    [<Literal>]
    let YahooFinanceUrl = "http://download.finance.yahoo.com/d/quotes.csv?s="

    let getLatestStockPrice (funds : string) : Map<string, FundQuote> =

        let urlLoad = YahooFinanceUrl + funds + "&f=d1sp"
        let tradedFunds = Funds.Load(urlLoad)

        // Save to map for easy reference
        [ 
            for row in tradedFunds.Rows do
                let tradedOn = row.TradedOn
                let symbol = row.Symbol
                let closedPrice = row.ClosedPrice
                let yQuote = 
                    { 
                        Symbol = symbol;
                        TradedOn = tradedOn
                        ClosingPrice = closedPrice
                    }
                yield (symbol, yQuote)
        ]
        |> Map.ofList

    let getSymbolPrice (symbol : string)(symbolPrices : Map<string, FundQuote>) : float =
        let fundQuote = symbolPrices.[symbol]
        fundQuote.ClosingPrice

    let getSymbolTradeDay (symbol : string)(symbolPrices : Map<string, FundQuote>) : DateTime =
        let fundQuote = symbolPrices.[symbol]
        fundQuote.TradedOn

    /// Update an existing row of PortfolioPrices
    let updDbPortfolioPrice
                (dbPortfolioPrices : DbPortfolioPrices)
                (portfolioPrices : PortfoliosPrices) =

        dbPortfolioPrices.PortfolioLowPrice <- portfolioPrices.PortfolioLowRiskPrice.Price
        dbPortfolioPrices.PortfolioMediumPrice <- portfolioPrices.PortfolioMediumRiskPrice.Price
        dbPortfolioPrices.PortfolioHighPrice <- portfolioPrices.PortfolioHighRiskPrice.Price
        dbPortfolioPrices.UpdatedOnUtc <- DateTime.UtcNow

    /// insert a new portfolio price for a trading day and price
    let insDbNewRowPortfolioPrice (db : DbContext)(portfolioPrices : PortfoliosPrices) =
        let newPortfolioPriceRow = new DbPortfolioPrices(
                                        YearMonthDay = portfolioPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd
                                    )
        (newPortfolioPriceRow, portfolioPrices) 
            ||> updDbPortfolioPrice

        db.PortfolioPrices.InsertOnSubmit(newPortfolioPriceRow)
        
    let getPortfolioPriceList (db : DbContext)(tradingDay : string) : DbPortfolioPrices =
        query {
            for row in db.PortfolioPrices do
            where (row.YearMonthDay = tradingDay)
            select row
            exactlyOneOrDefault
        }

    /// Insert the portfolio prices for a trading day if not existing already
    let setDbAskToSavePortfolioPrices(db : DbContext)(portfoliosPrices : PortfoliosPrices) : unit =

        // Any portfolio risk type trading day is the same
        let portfolioPricesRow = (db, portfoliosPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd) ||> getPortfolioPriceList

        if isNull portfolioPricesRow then

            (db, portfoliosPrices) ||> insDbNewRowPortfolioPrice
        else
            (portfolioPricesRow, portfoliosPrices) 
                ||> updDbPortfolioPrice

        db.DataContext.SubmitChanges()

    /// Save all trading days portfolio prices
    let setDbPortfoliosPrices(db : DbContext)(portfoliosPrices : PortfoliosPricesMap) : PortfoliosPricesMap=
        portfoliosPrices 
        |> Map.iter (fun key value 
                        -> (db, value) 
                            ||> setDbAskToSavePortfolioPrices)
        portfoliosPrices

    /// Cast an array of 3 risk portfolios to PortfoliosPrices
    let portfolioPricesArrToType(portfolioPricesArr : PortfolioSharePrice[]) : PortfoliosPrices =

        // Low : 0, Medium, High
        (portfolioPricesArr.Length = 3, "Incoming Portfolio prices array is not 3 (Low, Medium, High)")
        ||> traceExc

        let portfoliosPrices = { 
            PortfolioLowRiskPrice = {PortfolioId = portfolioPricesArr.[LowRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[LowRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[LowRiskArrayIndex].TradedOn} 
            PortfolioMediumRiskPrice = {PortfolioId = portfolioPricesArr.[MediumRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[MediumRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[MediumRiskArrayIndex].TradedOn}; 
            PortfolioHighRiskPrice = {PortfolioId = portfolioPricesArr.[HighRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[HighRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[HighRiskArrayIndex].TradedOn} 
        }
        portfoliosPrices

    /// Get the portfolio funds weighted
    let pFundsQry (db : DbContext) =
            query { 
                for f in db.Funds do
                join fp in db.PortFunds on (f.Id = fp.FundId)
                join p in db.Portfolios on (fp.PortfolioId = p.Id)
                where (p.IsActive)
                sortBy fp.PortfolioId
                thenBy f.Id
                select (f, fp)
            }

    /// 1. Read The portfolio funds from Db (in another function)
    /// 2. Ask yahoo for the fund market closing prices
    /// 3. Fold fund prices into a portfolio single share market price

    (* Return from (Dbfunds, DbPortfolioFunds) }   
            Key         Value
            20170213    PortfolioLowRiskPrice     PortfolioId 1
                                                  Price 59.5350015
                                                  TradedOn 2/13/2017 0:00
                        PortfolioMediumRiskPrice  PortfolioId 3
                                                  Price 65.4049995
                                                  TradedOn 2/13/2017 0:00
                        PortfolioHighRiskPrice    PortfolioId 5
                                                  Price 38.8099995
                                                  TradedOn 2/13/2017 0:00
            20170214    PortfolioLowRiskPrice     PortfolioId 1
                                                  Price 59.385001
                                                  TradedOn 2/14/2017 0:00
                        PortfolioMediumRiskPrice  PortfolioId 3
                                                  Price 65.615001
                                                  TradedOn 2/14/2017 0:00
                        PortfolioHighRiskPrice    PortfolioId 5
                                                  Price 38.8670005
                                                  TradedOn 2/14/2017 0:00*)
    let getPortfoliosPrices (dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) Linq.IQueryable) : PortfoliosPricesMap =

        let portfolioPrices =
            let fundTradedPrices =
                dbFundsWithPortfolioFunds
                |> Seq.map(fun (f : DbFunds, fp : DbPortfolioFunds) -> f.Symbol)
                |> String.concat ","
                |> getLatestStockPrice
            dbFundsWithPortfolioFunds
            |> Seq.map(fun (f : DbFunds, fp : DbPortfolioFunds) -> 
                let portfolioFundRec = {
                    PortfolioId = fp.PortfolioId; 
                    PortfolioWeight = fp.Weight; 
                    Fund = fundTradedPrices.[f.Symbol]
                }
                portfolioFundRec
               )
//            |> ~~ (printfn "%A")
            |> Seq.groupBy(fun (portfolioFundRec : PortfolioFundRecord) -> portfolioFundRec.Fund.TradedOn)
//            |> ~~ (printfn "%A")
            |> Seq.map(fun (tradedOnKey : DateTime, spfr : PortfolioFundRecord seq) -> 
                let gpfr = spfr |> Seq.groupBy (fun pfr -> pfr.PortfolioId)
                let pricedPortfoliosGroup = 
                    gpfr
                    |> Seq.map (fun (p, spsf) ->
                        let price = 
                            spsf 
                            |> Seq.sumBy (fun pfr -> pfr.Fund.ClosingPrice * float pfr.PortfolioWeight / 100.0) 
                        let topPortfolioFundRecord = spsf |> Seq.head
                        { PortfolioId = p; Price = price; TradedOn = topPortfolioFundRecord.Fund.TradedOn }
                    )
                (tradedOnKey.ToYyyyMmDd, pricedPortfoliosGroup 
                                            |> Seq.toArray
                                            |> portfolioPricesArrToType)
            )
            |> Map
//        portfolioPrices |> Seq.iter(fun i -> printfn "%A" i)
        portfolioPrices

    /// Ask & Store portfolio shares
    let storeShares (db : DbContext): PortfoliosPricesMap =
        db
        |> pFundsQry
        |> getPortfoliosPrices
        |> setDbPortfoliosPrices db

module UserPortfolio =
    open System
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes
        
    (*** Default Portfolio for new Users ***)
    let defaultPortfolio (db : DbContext) =  
        let gzConf = query {
            for gzConf in db.GzConfigurations do
            select gzConf
            exactlyOne
        } 
        let portfolioRisk = gzConf.FIRST_PORTFOLIO_RISK_VAL |> getPortfolioRiskById
        { PortfolioId = gzConf.FIRST_PORTFOLIO_RISK_VAL; Risk = portfolioRisk}

    let private updDbUserPortfolio 
                (userPortfolioRisk : Risk)
                (userPortfolioRow : DbCustoPortfolios) =

        let portfolioId = userPortfolioRisk |> getPortfolioIdByRisk 
        (**** Hard coded to 100% for now *****)
        userPortfolioRow.Weight <- 100.00F
        userPortfolioRow.PortfolioId <- portfolioId
        userPortfolioRow.UpdatedOnUTC <- DateTime.UtcNow

    let private insDbUserPortfolio (userInputPortfolio : UserPortfolioInput) : unit =
        let newCustomerPortfolioRow = new DbCustoPortfolios(
                                        CustomerId = userInputPortfolio.DbUserMonth.UserId,
                                        YearMonth = userInputPortfolio.DbUserMonth.Month
                                    )
        (userInputPortfolio.Portfolio.Risk, newCustomerPortfolioRow) ||> updDbUserPortfolio
        userInputPortfolio.DbUserMonth.Db.CustPortfolios.InsertOnSubmit(newCustomerPortfolioRow)

    /// get single Customer Portfolio of the desired month param in the yyyyMM format
    let private getUserPortfolio (dbUserMonth : DbUserMonth) : DbCustoPortfolios= 
        let userPortfolio =
            query {
                for userPortfolioRow in dbUserMonth.Db.CustPortfolios do
                where (
                    String.Compare(userPortfolioRow.YearMonth, dbUserMonth.Month, StringComparison.Ordinal) <= 0
                    && userPortfolioRow.CustomerId = dbUserMonth.UserId
                )
                sortByDescending userPortfolioRow.YearMonth
                select userPortfolioRow
                headOrDefault
            }
        userPortfolio

    /// get a Customer Portfolio joined with portfolio of the desired month param in the yyyyMM format
    let getUserPortfolioDetail (dbUserMonth : DbUserMonth) : Portfolio = 
        query {
            for userPortfolioRow in dbUserMonth.Db.CustPortfolios do
            join portfolioRow in dbUserMonth.Db.Portfolios on (userPortfolioRow.PortfolioId = portfolioRow.Id)
            where (
                String.Compare(userPortfolioRow.YearMonth, dbUserMonth.Month, StringComparison.Ordinal) <= 0
                && userPortfolioRow.CustomerId = dbUserMonth.UserId
            )
            sortByDescending userPortfolioRow.YearMonth
            select (portfolioRow)
            headOrDefault
        } 
        |> (fun(portfolioRow : DbPortfolios) ->
                if not <| isNull portfolioRow then
                    {PortfolioId = portfolioRow.Id; Risk = portfolioRow.RiskTolerance |> getPortfolioRiskById}
                else
                    dbUserMonth.Db |> defaultPortfolio
           )

    /// Upsert a customer portfolio
    let upsDbUserPortfolio(userPortfolioInput : UserPortfolioInput) : unit =
        let currentCustomerPortfolio = userPortfolioInput.DbUserMonth |> getUserPortfolio

        if isNull currentCustomerPortfolio then
            userPortfolioInput |> insDbUserPortfolio 
        else
            (userPortfolioInput.Portfolio.Risk, currentCustomerPortfolio) ||> updDbUserPortfolio

        userPortfolioInput.DbUserMonth.Db.DataContext.SubmitChanges()

module CalcUserPortfolioShares =
    open GzDb
    open PortfolioTypes
        
    let private priceLowRiskPortfolio (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =

        let lowRiskPrice = userPortfolioInput.PortfoliosPrices.PortfolioLowRiskPrice.Price
        {
            PortfolioShares = { 
                                SharesLowRisk = userPortfolioInput.CashToInvest / decimal lowRiskPrice;
                                SharesMediumRisk = 0M;
                                SharesHighRisk = 0M
            };
            Worth = userPortfolioInput.CashToInvest
        }

    let private priceMediumRiskPortfolio (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =

        let mediumRiskPrice = userPortfolioInput.PortfoliosPrices.PortfolioMediumRiskPrice.Price
        { 
            PortfolioShares = { 
                                SharesLowRisk = 0M;
                                SharesMediumRisk = userPortfolioInput.CashToInvest / decimal mediumRiskPrice;
                                SharesHighRisk = 0M;
            };
            Worth = userPortfolioInput.CashToInvest
        }

    let private priceHighRiskPortfolio (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =

        let highRiskPrice = userPortfolioInput.PortfoliosPrices.PortfolioHighRiskPrice.Price
        { 
            PortfolioShares = { 
                                SharesLowRisk = 0M;
                                SharesMediumRisk = 0M;
                                SharesHighRisk = userPortfolioInput.CashToInvest / decimal highRiskPrice
            }
            Worth = userPortfolioInput.CashToInvest
        }

    /// Get cash -> shares by looking at the investment cash amount and user portfolio
    let getNewCustomerShares (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =
        match userPortfolioInput.Portfolio.Risk with
        | Low 
            -> 
            userPortfolioInput 
            |> priceLowRiskPortfolio
        | Medium 
            -> 
            userPortfolioInput 
            |> priceMediumRiskPortfolio
        | High 
            -> 
            userPortfolioInput 
            |> priceHighRiskPortfolio

module VintageShares =
    open System
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes
        
    /// update a row to VintageShares
    let private updDbVintageShares
            (userPortfolioShares : PortfolioShares)
            (tradingDay : DateTime)
            (userPortfolioSharesRow : DbVintageShares) : unit =

        userPortfolioSharesRow.PortfolioLowShares <- userPortfolioShares.SharesLowRisk
        userPortfolioSharesRow.PortfolioMediumShares <- userPortfolioShares.SharesMediumRisk
        userPortfolioSharesRow.PortfolioHighShares <- userPortfolioShares.SharesHighRisk
        userPortfolioSharesRow.BuyPortfolioTradeDay <- tradingDay
        userPortfolioSharesRow.UpdatedOnUtc <- DateTime.UtcNow

    /// insert a row to VintageShares
    let private insDbVintageShares 
                (userPortfolioShares : PortfolioShares)
                (tradingDay : DateTime)
                (dbUserMonth : DbUserMonth) : unit =

        let newCustPortfoliosSharesRow = new DbVintageShares
                                            (UserId = dbUserMonth.UserId, YearMonth = dbUserMonth.Month)
        (userPortfolioShares, tradingDay, newCustPortfoliosSharesRow) |||> updDbVintageShares

        dbUserMonth.Db.VintageShares.InsertOnSubmit(newCustPortfoliosSharesRow)

    /// get CustomerPortfolioShares of the desired month param in the yyyyMM format  
    let private getDbVintageSharesRow(dbUserMonth : DbUserMonth) : DbVintageShares = 
        let vintageShares =
            query {
                for row in dbUserMonth.Db.VintageShares do
                where (
                    row.YearMonth = dbUserMonth.Month
                    && row.UserId = dbUserMonth.UserId
                )
                select row
                exactlyOneOrDefault
            }
        vintageShares

    /// upsert user portfolio shares for the desired month
    let upsDbVintageShares
            (dbUserMonth : DbUserMonth)
            (tradingDay : DateTime)
            (userPortfolioShares : PortfolioShares) : unit =

        let dbUserSharesRow = dbUserMonth |> getDbVintageSharesRow

        if isNull dbUserSharesRow then
            (userPortfolioShares, tradingDay, dbUserMonth) 
            |||> insDbVintageShares

        else
            (userPortfolioShares, tradingDay, dbUserSharesRow) 
            |||> updDbVintageShares

        dbUserMonth.Db.DataContext.SubmitChanges()

    /// Price portfolio shares
    let private getPricedPortfolioShares (shares : PortfolioShares)(prices :PortfoliosPrices) : PortfolioPriced=
        { 
            PortfolioShares = shares; 
            Worth = shares.SharesLowRisk * decimal prices.PortfolioLowRiskPrice.Price 
                + shares.SharesMediumRisk * decimal prices.PortfolioMediumRiskPrice.Price 
                + shares.SharesHighRisk * decimal prices.PortfolioHighRiskPrice.Price
        }

    /// Price a db row of portfolio shares
    let private getDbPortfolioShares
                (portfolioSharesRow : DbVintageShares) : PortfolioShares =

        if not <| isNull portfolioSharesRow then
            { 
                SharesLowRisk = portfolioSharesRow.PortfolioLowShares;
                SharesMediumRisk = portfolioSharesRow.PortfolioMediumShares;
                SharesHighRisk = portfolioSharesRow.PortfolioHighShares;
            }
        else // Default of 0 shares 
            {SharesLowRisk = 0M; SharesMediumRisk = 0M; SharesHighRisk = 0M}

    /// Price portfolio shares
    let private getPricedDbPortfolioShares
                (portfoliosPrices :PortfoliosPrices) 
                (portfolioSharesRow : PortfolioShares) : PortfolioPriced =

            (portfolioSharesRow, portfoliosPrices) ||> getPricedPortfolioShares

    /// get the previous month's priced with latest prices and adjusted by vintages sold of present month
    (* Note when selling vintages on the site does not affect the user VintageShares balance other than
    ** the subtraction below *)
    let getPricedPrevMonthShares(userPortfolioInput : UserPortfolioInput) : PortfolioPriced =
        let prevMonth = { userPortfolioInput.DbUserMonth with Month = userPortfolioInput.DbUserMonth.Month.ToPrevYyyyMm }

        prevMonth   |> getDbVintageSharesRow
                    |> getDbPortfolioShares 
                    |> (-) userPortfolioInput.VintagesSold.PortfolioShares
                    |> getPricedDbPortfolioShares userPortfolioInput.PortfoliosPrices

module InvBalance =
    open System
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes

    type InvBalancePrevTotals = {
        TotalCashInvestments : decimal;
        TotalCashInvInHold : decimal;
        TotalSoldVintagesSold : decimal
    }

    type InvBalanceInput = {
        InvBalancePrevTotals : InvBalancePrevTotals;
        UserPortfolioShares : UserPortfolioShares;
        UserInputPortfolio : UserPortfolioInput;
        UserFinance : UserFinance
    }

    /// get the invBalance row of the desired month in the format of yyyyMm
    let private getInvBalance (dbUserMonth : DbUserMonth) : DbInvBalances = 
        let invBalance =
            query {
                for row in dbUserMonth.Db.InvBalances do
                where (
                    row.YearMonth = dbUserMonth.Month
                    && row.CustomerId = dbUserMonth.UserId
                )
                select row
                exactlyOneOrDefault
            }
        invBalance

    /// update an invBalance row
    let private updDbInvBalance(input : InvBalanceInput)(invBalanceRow : DbInvBalances) : unit =

        let prevShares = input.UserPortfolioShares.PrevPortfolioShares
        let newShares = input.UserPortfolioShares.NewPortfolioShares
        
        let nowBalance = 
            prevShares.Worth 
            + newShares.Worth

        let cashInv = input.UserInputPortfolio.CashToInvest

        let vintagesSoldThisMonth = input.UserInputPortfolio.VintagesSold

        let totalCashInvestedForAllVintages = 
            input.InvBalancePrevTotals.TotalCashInvestments 
            + cashInv

        let sellingPriceOfAllSoldVintages = 
            input.InvBalancePrevTotals.TotalSoldVintagesSold 
            + vintagesSoldThisMonth.SoldAt

        let ``bought Price of Sold Vintages`` = vintagesSoldThisMonth.BoughtAt

        let totalCashInvestedForUnsoldVintages = 
            input.InvBalancePrevTotals.TotalCashInvInHold 
            + cashInv
            - ``bought Price of Sold Vintages``

        invBalanceRow.Balance <- nowBalance
        (* Note: See line below for investment gain including sold vintages
        ** invBalanceRow.InvGainLoss <- (nowBalance + ``selling Price Of All Sold Vintages``) - ``total Cash invested for All vintages``
        ** Present business choice: Investment gain of vintages in hold. *)
        invBalanceRow.InvGainLoss <- nowBalance - totalCashInvestedForUnsoldVintages
        invBalanceRow.PortfolioId <- input.UserInputPortfolio.Portfolio.PortfolioId
        invBalanceRow.CashInvestment <- cashInv

        // Vintage Shares
        invBalanceRow.LowRiskShares <- newShares.PortfolioShares.SharesLowRisk
        invBalanceRow.MediumRiskShares <- newShares.PortfolioShares.SharesMediumRisk
        invBalanceRow.HighRiskShares <- newShares.PortfolioShares.SharesHighRisk

        // Totals
        invBalanceRow.TotalCashInvestments <- totalCashInvestedForAllVintages
        invBalanceRow.TotalCashInvInHold <- totalCashInvestedForUnsoldVintages
        invBalanceRow.TotalSoldVintagesValue <- sellingPriceOfAllSoldVintages

        // Gaming Activity
        invBalanceRow.BegGmBalance <- input.UserFinance.BegBalance
        invBalanceRow.Deposits <- input.UserFinance.Deposits
        invBalanceRow.Withdrawals <- input.UserFinance.Withdrawals
        invBalanceRow.GmGainLoss <- input.UserFinance.GainLoss
        invBalanceRow.EndGmBalance <- input.UserFinance.EndBalance

        invBalanceRow.UpdatedOnUTC <- DateTime.UtcNow

    /// insert a InvBalance row
    let private insDbInvBalance (input : InvBalanceInput) : unit =
        let newInvBalanceRow = 
            new DbInvBalances(
                CustomerId = input.UserInputPortfolio.DbUserMonth.UserId, 
                YearMonth = input.UserInputPortfolio.DbUserMonth.Month
            )
        (input, newInvBalanceRow) 
        ||> updDbInvBalance

        input.UserInputPortfolio.DbUserMonth.Db.InvBalances.InsertOnSubmit(newInvBalanceRow)

    /// upsert a InvBalance row
    let upsDbInvBalance (input : InvBalanceInput) : unit =
        let invBalanceRow = input.UserInputPortfolio.DbUserMonth |> getInvBalance

        if isNull invBalanceRow then
            input 
            |> insDbInvBalance
        else
            (input, invBalanceRow) 
            ||> updDbInvBalance

        input.UserInputPortfolio.DbUserMonth.Db.DataContext.SubmitChanges()

    /// get db invBalance totals of previous month
    let getInvBalancePrevTotals(dbUserMonth : DbUserMonth) : InvBalancePrevTotals =
        let invB = { dbUserMonth with Month = dbUserMonth.Month.ToPrevYyyyMm }
                |> getInvBalance
        if not <| isNull invB then
            { TotalCashInvestments = invB.TotalCashInvestments; TotalCashInvInHold = invB.TotalCashInvInHold; TotalSoldVintagesSold = invB.TotalSoldVintagesValue }
        else
            { TotalCashInvestments = 0M; TotalCashInvInHold = 0M; TotalSoldVintagesSold = 0M }

    /// get any sold vintages for this user during the month we're presently clearing
    (* Note this is the only table that is affected by sold vintages and available to read them back and deduct them from user shares *)
    let getSoldVintages (dbUserMonth : DbUserMonth) : VintagesSold = 
        let totalVintagesSold =
            query {
                for row in dbUserMonth.Db.InvBalances do
                where (
                    row.SoldYearMonth = dbUserMonth.Month
                    && row.CustomerId = dbUserMonth.UserId
                )
                // Cast drop nullable from SoldAmount
                select (row.LowRiskShares, row.MediumRiskShares, row.HighRiskShares, row.CashInvestment, decimal row.SoldAmount)
            }
            // Sum fold. The query linq would not allow for Null return of grouped summed tuple so we do summation in F#
            // it's also much shorter
            |> Seq.fold (fun (ls, ms, hs, cs, ss) (l, m, h, c, s) -> (ls+l, ms+m, hs+h, cs+c, ss + s)) (0M, 0M, 0M, 0M, 0M)

        // Package summed sold vintage shares into return type of VintagesSold
        totalVintagesSold 
                |> 
                (fun (lowShares, mediumShares, highShares, cashInv, soldAmount) ->
                    let vintShares = {
                        SharesLowRisk = lowShares;
                        SharesMediumRisk = mediumShares;
                        SharesHighRisk = highShares
                    }
                    { PortfolioShares = vintShares; BoughtAt = cashInv; SoldAt = soldAmount }
                )

    /// get instance of input values for InvBalance processing
    let getInvBalanceInput (userPortfolioInput : UserPortfolioInput)(userFinance : UserFinance) : InvBalanceInput = 
        let portfolioShares = { 
            PrevPortfolioShares = userPortfolioInput |> VintageShares.getPricedPrevMonthShares;
            NewPortfolioShares = userPortfolioInput |> CalcUserPortfolioShares.getNewCustomerShares
        }
        { 
            InvBalancePrevTotals =  userPortfolioInput.DbUserMonth |> getInvBalancePrevTotals;
            UserPortfolioShares = portfolioShares;
            UserInputPortfolio = userPortfolioInput;
            UserFinance = userFinance
        }

module UserTrx =
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes
    open InvBalance
    open System.Collections.Generic
    open System
    open NLog
    open GzDb.Trx

    let logger = LogManager.GetCurrentClassLogger()

    /// Upsert UserPortfolio, VintageShares, InvBalance
    let private upsDbClearMonth (userPortfolioInput : UserPortfolioInput)(userFinance:UserFinance)(invBalanceInput : InvBalanceInput) : unit =
        // Ups User Portfolios
        userPortfolioInput |> UserPortfolio.upsDbUserPortfolio
        
        // Ups VintageShares
        let buyTradingDay = userPortfolioInput.PortfoliosPrices.PortfolioLowRiskPrice.TradedOn
        (
            invBalanceInput.UserPortfolioShares.PrevPortfolioShares.PortfolioShares 
            + invBalanceInput.UserPortfolioShares.NewPortfolioShares.PortfolioShares
        ) 
        |> VintageShares.upsDbVintageShares userPortfolioInput.DbUserMonth buyTradingDay
        
        // Ups InvBalance
        invBalanceInput 
        |> InvBalance.upsDbInvBalance

    /// Process input balances
    let private setDbProcessUserBalances (userPortfolioInput : UserPortfolioInput)(userFinance:UserFinance)(invBalanceInput : InvBalanceInput): unit = 
        if userPortfolioInput.CashToInvest > 0M || userFinance.EndBalance > 0M then
            logger.Info(sprintf "Processing investment balances for user id %d on month of %s having these financial amounts\n%A" 
                userPortfolioInput.DbUserMonth.UserId userPortfolioInput.DbUserMonth.Month userFinance)

        let dbTrxOper() = 
            transactionWith 
            <| 
                fun () -> 
                    (userPortfolioInput, userFinance, invBalanceInput) 
                    |||> upsDbClearMonth
        (3, dbTrxOper) 
        ||> retry

    /// get the portfolio market quote that's latest within the month processing
    let private findNearestPortfolioPrice (portfoliosPrices:PortfoliosPricesMap)(month : string) =
        let nextMonth = month.ToNextMonth1st

        let getMonthLateQuote (portfoliosPrices)= 
            portfoliosPrices
            |> Map.filter(fun key _ -> key < nextMonth)
            |> Seq.maxBy(fun kvp -> kvp.Key)
            |> (fun (kvp : KeyValuePair<string, PortfoliosPrices>) -> kvp.Value)
        let boxedGetmonthLateQuote() = portfoliosPrices |> getMonthLateQuote
        let monthLateQuote = tryF boxedGetmonthLateQuote NoCurrentMarketQuote (sprintf "Couldn't find a portfolio market quote within %s" month)

        // assert quote is within the month
        (monthLateQuote.PortfolioHighRiskPrice.TradedOn.Month = Int32.Parse(month.Substring(4, 2)), "Found a portfolio market quote not within the processing month: " + month)
        ||> traceExc
        monthLateQuote
        
    /// Input type for portfolio processing
    let private getUserPortfolioInput (dbUserMonth : DbUserMonth)(trxRow : DbGzTrx)(portfoliosPricesMap:PortfoliosPrices) =
        { 
            DbUserMonth = dbUserMonth;
            VintagesSold = dbUserMonth |> InvBalance.getSoldVintages;
            Portfolio = dbUserMonth |> UserPortfolio.getUserPortfolioDetail;
            CashToInvest = trxRow.Amount;
            PortfoliosPrices = portfoliosPricesMap
        }

    /// Input type for gaming activities reporting
    let private getUserFinance (trxRow : DbGzTrx): UserFinance =
        {
            // Get value or default to 0
            BegBalance = if trxRow.BegGmBalance.HasValue then trxRow.BegGmBalance.Value else 0m
            EndBalance = if trxRow.EndGmBalance.HasValue then trxRow.EndGmBalance.Value else 0m
            Deposits = if trxRow.Deposits.HasValue then trxRow.Deposits.Value else 0m
            Withdrawals = if trxRow.Withdrawals.HasValue then trxRow.Withdrawals.Value else 0m
            GainLoss = if trxRow.GmGainLoss.HasValue then trxRow.GmGainLoss.Value else 0m
            AmountToBuyStock = trxRow.Amount
        }

    /// Main entry to process credit losses for the month and put forth balance amounts from gaming activities
    let processGzTrx(db : DbContext)(yyyyMmDd : string)(portfoliosPricesMap : PortfoliosPricesMap) =

        let yyyyMm = yyyyMmDd.Substring(0, 6)
        
        let latestInMonthPortfoliosPrices = 
            (portfoliosPricesMap, yyyyMm) 
                ||> findNearestPortfolioPrice

        query { 
            for trxRow in db.GzTrxs do
                where (
                    trxRow.YearMonthCtd = yyyyMm
                    && trxRow.GzTrxTypes.Code = int GzTransactionType.CreditedPlayingLoss
                )
                sortBy trxRow.CustomerId
                select trxRow
        }
        |> Seq.iter (fun (trxRow : DbGzTrx) ->
                    // set input types
                    let dbUserMonth = {Db = db; UserId = trxRow.CustomerId; Month = yyyyMm}

                    let userPortfolioInput = 
                        (dbUserMonth, trxRow, latestInMonthPortfoliosPrices) 
                        |||> getUserPortfolioInput
                    
                    let userFinance = 
                        trxRow 
                        |> getUserFinance
                    
                    let invBalanceInput = 
                        (userPortfolioInput, userFinance) 
                        ||> getInvBalanceInput

                    (userPortfolioInput, userFinance, invBalanceInput) 
                    |||> setDbProcessUserBalances
                )
