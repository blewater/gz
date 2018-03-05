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
open System.Text.RegularExpressions

type Settings = AppSettings<"App.config">
let configPath = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ ; "bin"; "debug"; "awardbonus.exe" |]
Settings.SelectExecutableFile configPath
printfn "%s" Settings.ConfigFileName
#else
module ChromeAwarder
open System
open canopy
open OpenQA.Selenium
open FSharp.Configuration
open BonusReq
open System.Text.RegularExpressions

type Settings = AppSettings<"App.config">

#endif

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

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
    chromeOptions.AddArgument("--allow-insecure-localhost");
    let chromeNoSandbox = ChromeWithOptions(chromeOptions)
    #if INTERACTIVE
    canopy.configuration.chromeDir <- __SOURCE_DIRECTORY__
    #else
    canopy.configuration.chromeDir <- "."
    #endif
    start chromeNoSandbox
    pin FullScreen


let switchToWindow window =
    browser.SwitchTo().Window(window) |> ignore

let getOtherWindow currentWindow =
    browser.WindowHandles |> Seq.find (fun w -> w <> currentWindow)

let switchToOtherWindow currentWindow =
    switchToWindow (getOtherWindow currentWindow) |> ignore

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

let checkAwardingResult(bonusAmount : decimal) : bool =
    let resEl = element "#cphPage_lblSuccessMessage"
    let resText = read resEl
    match resText with
    | Regex @"bonus amount=\w+ (\S+(\.\d{1,2})?)," [ resTxtAmount ; decimalPart ] ->
        let parsedAmount = Decimal.Parse(resTxtAmount)
        printfn "Bonus amount awarded %M" parsedAmount
        parsedAmount = bonusAmount
    | _ -> 
        printfn "No match given"
        false

let cancelLastBonusAward() =
    let baseWindow = browser.CurrentWindowHandle
    switchToOtherWindow baseWindow
    //click cancel
    click "#cphPage_gvData_lnkCancel_0"
    acceptAlert()
    click "#cphPage_btnCloseWindow"
    switchToWindow baseWindow

let rec submitInBonusForm(bonusReq : BonusReqType)(tries : int) =

    // go into the bonus
    click "#cphPage_CasinoWalletAccountDataControl1_btnGiveManualBonus"

    selCashBackBonusinSelectList()
    press tab

    let rec retrySetAmount(tries : int) : bool =
        try
            waitForElement "#ctl00_cphPage_rtbBonusAmount_text"
            sleep 1
            let bonusAmountEl = element "#ctl00_cphPage_rtbBonusAmount_text"
            bonusAmountEl.Clear()
            // Bonus amount text input
            let bonusStr = bonusReq.Amount.ToString()
            bonusStr
            |> Seq.iter(
                fun(c : char) ->
                    let cs = c.ToString()
                    bonusAmountEl.SendKeys cs
                )
            let readAmount = Decimal.Parse(read bonusAmountEl)
            assert (readAmount = bonusReq.Amount)
            printfn "Amount to award: %M" readAmount
            true
        with ex ->
            printfn "Setting bonus amount to Element failed! Remaining tries: %d" tries
            if tries > 0 then 
                retrySetAmount (tries - 1)
            else
#if !INTERACTIVE
                TblLogger.Upsert (Some ex) bonusReq |> ignore
#endif
                false
        
    retrySetAmount 30
    |> function
        | true -> 
            // Set Comment
            (element "#ctl00_cphPage_rtbComment_text") << sprintf "User %d bonus granted for vintages sold on %s" bonusReq.GmUserId bonusReq.YearMonthSold
            // Press Give bonus button
            click "#cphPage_btnConfirm"
            printfn "Succeeded awarding the bonus amount"
        | false ->
            failwith "Failed in setting the bonus amount."

    if not <| checkAwardingResult bonusReq.Amount then
        click "#cphPage_btnReturn"
        click "#cphPage_CasinoWalletAccountDataControl1_btnBonusDetails"
        cancelLastBonusAward()
        printfn "Failed to award the right amount: %M, tries left: %d" bonusReq.Amount (tries-1)
        submitInBonusForm bonusReq (tries - 1)

let awardUser (bonusReq : BonusReqType) : BonusReqType =
    searchCustomer bonusReq.GmUserId
    // go to the user portfolio page
    click "#cphPage_UsersControl1_gvData > tbody > tr:nth-child(2) > td:nth-child(2) > a"
    // click userportfolio page
    click "#cphPage_UserAccountsCompactControl1_gvData > tbody > tr > td:first-child > table > tbody > tr > td:nth-child(2) > a"

    submitInBonusForm bonusReq 30

    bonusReq
