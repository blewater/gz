﻿#if INTERACTIVE
#r "./packages/canopy/lib/canopy.dll"
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/Selenium.WebDriver/lib/net45/WebDriver.dll"
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#else
module BonusAwarder
#endif

open canopy
open System
open OpenQA.Selenium
open FSharp.Configuration
open BonusReq

type Settings = AppSettings<"App.config">

let everymatrixUsername = Settings.Evuser
let everymatrixPassword = Settings.Evpwd
let everymatrixSecureToken = Settings.Evtoken
let queueConnString = Settings.QueueConnString.ToString()
let queueName = Settings.QueueName

// Need to use --no-sandbox or chrome wont start
// https://github.com/elgalu/docker-selenium#chrome-not-reachable-or-timeout-after-60-secs
let chromeOptions = OpenQA.Selenium.Chrome.ChromeOptions()
chromeOptions.AddArgument("--no-sandbox")
chromeOptions.AddArgument("--disable-extensions")
chromeOptions.AddArgument("--headless")
chromeOptions.AddArgument("--disable-gpu")
chromeOptions.AddArgument("--disable-client-side-phishing-detection")
chromeOptions.AddArgument("--disable-suggestions-service")
chromeOptions.AddArgument("--safebrowsing-disable-download-protection")
chromeOptions.AddArgument("--no-first-run")
let chromeNoSandbox = ChromeWithOptions(chromeOptions)
canopy.configuration.chromeDir <- "./"
start chromeNoSandbox

let uiAutomateLoginEverymatrixReports 
        (everyMatrixUserName : string)
        (everymatrixPassword : string)
        (everymatrixLoginToken : string) : unit =

    url "https://admin3.gammatrix.com/Admin/Login.aspx"
    "#rtbTop_text" << everyMatrixUserName
    "#rtbMid_text" << everymatrixPassword
    "#rtbBottom_text" << everymatrixLoginToken
    click "#btnLogin"

let searchCustomer() : unit =
    "#rtbSearch" << "4300962"
    click "#imgSearch"

let selCashBackBonusinSelectList() : unit =
    click "#ctl00_cphPage_rcbBonus"
    let bonusInput = element "#ctl00_cphPage_rcbBonus_Input"
    
    let rec selBonusLi(bonusInput : IWebElement) : unit =
        press down
        let bonusName = read bonusInput
        if bonusName <> "CASHBACK - 1029498" then
            selBonusLi(bonusInput)
    
    do selBonusLi bonusInput
    press enter

let start(bonusReq : BonusReqType) : BonusReqType =
    uiAutomateLoginEverymatrixReports everymatrixUsername everymatrixPassword everymatrixSecureToken
    searchCustomer()
    // go to the portfolio page
    click "#cphPage_UsersControl1_gvData > tbody > tr:nth-child(2) > td:nth-child(2) > a"
    click "#cphPage_UserAccountsCompactControl1_gvData > tbody > tr > td:first-child > table > tbody > tr > td:nth-child(2) > a"
    // go into the bonus
    click "#cphPage_CasinoWalletAccountDataControl1_btnGiveManualBonus"

    selCashBackBonusinSelectList()

    let bonusAmountEl = element "#ctl00_cphPage_rtbBonusAmount_text"
    bonusAmountEl << bonusReq.Amount.ToString()
    let amount = read bonusAmountEl

    printfn "Amount: %s" amount

    quit()
    bonusReq