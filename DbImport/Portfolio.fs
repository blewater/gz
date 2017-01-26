namespace DbImport

module Portfolio =

    open System

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
