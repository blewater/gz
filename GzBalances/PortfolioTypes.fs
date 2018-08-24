namespace GzBalances

module PortfolioTypes =
    open System
    open GzDb.DbUtil

    type Risk = Low | Medium | High

    type PortfolioId = int

    type Shares = decimal

    type Money = decimal

    type YyyyMm = string

    type Portfolio = {
        PortfolioId : PortfolioId;
        Risk : Risk
    }

    [<Literal>]
    let LowRiskPortfolioId = 1
    [<Literal>]
    let MediumRiskPortfolioId = 3
    [<Literal>]
    let HighRiskPortfolioId = 5
    [<Literal>]
    let LowRiskArrayIndex = 0
    [<Literal>]
    let MediumRiskArrayIndex = 1
    [<Literal>]
    let HighRiskArrayIndex = 2

    /// Input monthly argument for calculating portfolio
    type PortfolioShares = {
        SharesLowRisk : Shares;
        SharesMediumRisk : Shares;
        SharesHighRisk : Shares;
    } with 
        static member (-) (soldShares, owned : PortfolioShares) =
            {
                SharesLowRisk = owned.SharesLowRisk - soldShares.SharesLowRisk; 
                SharesMediumRisk = owned.SharesMediumRisk - soldShares.SharesMediumRisk; 
                SharesHighRisk = owned.SharesHighRisk - soldShares.SharesHighRisk;
            }
        static member (+) (owned, soldShares : PortfolioShares) =
            {
                SharesLowRisk = owned.SharesLowRisk + soldShares.SharesLowRisk; 
                SharesMediumRisk = owned.SharesMediumRisk + soldShares.SharesMediumRisk; 
                SharesHighRisk = owned.SharesHighRisk + soldShares.SharesHighRisk;
            }

    type PortfolioPriced = {
        PortfolioShares : PortfolioShares;
        Worth : Money
    }

    type VintagesSold = {
        PortfolioShares : PortfolioShares; 
        BoughtAt : Money; 
        SoldAt : Money;
    }

    type PortfolioSharePrice = {
        PortfolioId : PortfolioId;
        Price : float;
        TradedOn : DateTime;
    }

    type PortfoliosPrices = {
        PortfolioLowRiskPrice : PortfolioSharePrice;
        PortfolioMediumRiskPrice : PortfolioSharePrice;
        PortfolioHighRiskPrice : PortfolioSharePrice;
    }

    type PortfoliosPricesMap = Map<string, PortfoliosPrices>

    type DbUserMonth = {
        Db : DbContext;
        UserId : int;
        Month : string;
    }

    type UserPortfolioShares = {
        PrevPortfolioShares : PortfolioPriced;
        NewPortfolioShares : PortfolioPriced
    }

    type UserPortfolioInput = {
        DbUserMonth : DbUserMonth;
        VintagesSold : VintagesSold;
        Portfolio : Portfolio;
        PortfoliosPrices : PortfoliosPrices
    }

    type UserFinance = {
        BegBalance : Money;
        Deposits : Money;
        Withdrawals : Money;
        GainLoss : Money;
        EndBalance : Money;
        CashDeposits : Money;
        CashBonus : Money;
        CashToInvest: Money;
    }

    type Symbol = string
    type FundQuote = { Symbol : Symbol; TradedOn : DateTime; ClosingPrice : float }
    type PortfolioWeight = float32
    type PortfolioFundRecord = {PortfolioId : PortfolioId; PortfolioWeight : PortfolioWeight; Fund : FundQuote}

    /// Convert .net dictionary to map 
    let inline toMap kvps =
        kvps
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    let getPortfolioRiskById (portfolioId : PortfolioId) : Risk =
        match portfolioId with
        | LowRiskPortfolioId 
            -> Low
        | MediumRiskPortfolioId 
            -> Medium
        | HighRiskPortfolioId 
            -> High
        | _ 
            -> invalidArg "Portfolio Id" (sprintf "Unknown portfolio id: %d" portfolioId)

    let getPortfolioIdByRisk (portfolioRisk : Risk) : PortfolioId =
        match portfolioRisk with
        | Low -> LowRiskPortfolioId
        | Medium -> MediumRiskPortfolioId
        | High -> HighRiskPortfolioId

