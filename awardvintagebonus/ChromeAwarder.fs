#if INTERACTIVE
#I __SOURCE_DIRECTORY__
#r "./packages/canopy/lib/canopy.dll"
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/Selenium.WebDriver/lib/net45/WebDriver.dll"
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#load "BonusReq.fs"
open canopy
open OpenQA.Selenium
open FSharp.Configuration
open BonusReq

type Settings = AppSettings<"App.config">
let configPath = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ ; "bin"; "debug"; "awardbonus.exe" |]
Settings.SelectExecutableFile configPath
printfn "%s" Settings.ConfigFileName
#else
module ChromeAwarder
open canopy
open OpenQA.Selenium
open FSharp.Configuration
open BonusReq

type Settings = AppSettings<"App.config">

#endif

let everymatrixUsername = Settings.Evuser
let everymatrixPassword = Settings.Evpwd
let everymatrixSecureToken = Settings.Evtoken

// Need to use --no-sandbox or chrome wont start
// https://github.com/elgalu/docker-selenium#chrome-not-reachable-or-timeout-after-60-secs
let setChromeOptions(visualSession : bool) : unit =
    let chromeOptions = OpenQA.Selenium.Chrome.ChromeOptions()
    chromeOptions.AddArgument("--no-sandbox")
    chromeOptions.AddArgument("--disable-extensions")
#if !INTERACTIVE
    if not visualSession then
        chromeOptions.AddArgument("--headless")
#endif
    chromeOptions.AddArgument("--disable-gpu")
    chromeOptions.AddArgument("--disable-client-side-phishing-detection")
    chromeOptions.AddArgument("--disable-suggestions-service")
    chromeOptions.AddArgument("--safebrowsing-disable-download-protection")
    chromeOptions.AddArgument("--no-first-run")
    let chromeNoSandbox = ChromeWithOptions(chromeOptions)
    #if INTERACTIVE
    canopy.configuration.chromeDir <- __SOURCE_DIRECTORY__
    #else
    canopy.configuration.chromeDir <- "."
    #endif
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

let searchCustomer(gmUserId : int) : unit =
    "#rtbSearch" << gmUserId.ToString()
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

let startBrowserSession (visualSession : bool) =
    setChromeOptions visualSession
    uiAutomateLoginEverymatrixReports everymatrixUsername everymatrixPassword everymatrixSecureToken

let endBrowserSession() =
    quit()

let awardUser (bonusReq : BonusReqType) : BonusReqType =
    try
        searchCustomer bonusReq.GmUserId
        // go to the portfolio page
        click "#cphPage_UsersControl1_gvData > tbody > tr:nth-child(2) > td:nth-child(2) > a"
        click "#cphPage_UserAccountsCompactControl1_gvData > tbody > tr > td:first-child > table > tbody > tr > td:nth-child(2) > a"
        // go into the bonus
        click "#cphPage_CasinoWalletAccountDataControl1_btnGiveManualBonus"

        selCashBackBonusinSelectList()

        // Set Comment
        (element "#ctl00_cphPage_rtbComment_text") << sprintf "User %d bonus granted for vintages sold on %s" bonusReq.GmUserId bonusReq.YearMonthSold

        // Bonus amount text input
        let bonusStr = bonusReq.Amount.ToString()
        let rec retrySetAmount(tries : int) : bool =
            let bonusAmountEl = element "#ctl00_cphPage_rtbBonusAmount_text"
            try
                bonusAmountEl << bonusStr
                let readAmount = read bonusAmountEl
                assert (readAmount = bonusStr)
                printfn "Amount to award: %s" readAmount
                true
            with ex ->
                TblLogger.insert (Some ex) bonusReq
                printfn "Setting bonus amount to Element failed! Remaining tries: %d" tries
                if tries > 0 then 
                    retrySetAmount (tries - 1)
                else
                    false
        
        retrySetAmount 3
        |> function
            | true -> 
                printfn "Succeeded in setting the bonus amount"
                // Press Give bonus button
                click "#cphPage_btnConfirm"
            | false ->
                failwith "Failed in setting the bonus amount."

        bonusReq
    with ex ->
        TblLogger.insert (Some ex) bonusReq
        raise ex