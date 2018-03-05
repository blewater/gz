#I __SOURCE_DIRECTORY__
#r "./packages/canopy/lib/canopy.dll"
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/NLog/lib/net45/NLog.dll"
#r "./packages/Selenium.WebDriver/lib/net45/WebDriver.dll"
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Json/lib/net45/FSharp.Json.dll"
#r "./packages/Selenium.WebDriver/lib/net45/WebDriver.dll"

open canopy
open System
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Queue
open FSharp.Json
open System.Text.RegularExpressions

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

[<Literal>]
let everymatrixUsername = "presto"
[<Literal>]
let everymatrixPassword = "gz2017!@"
[<Literal>]
let everymatrixSecureToken = "3DFEC757D808494"

type Settings = AppSettings<"App.config">

let path = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ ; "bin"; "debug"; "awardbonus.exe" |]
Settings.SelectExecutableFile path
printfn "%s" Settings.ConfigFileName
(*
// Connect via configuration file with named connection string.
printfn "%s %s" Settings.ConfigFileName Settings.Evuser

type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName=Settings.ConfigFileName>
let bonusQueue = FunctionsQueue.Queues.bonusreq

let qCnt() =
    let cnt = bonusQueue.GetCurrentLength()
    printfn "Queue length is %d." (cnt)
    cnt

let printMessage msg =
    printfn "Message %A with body '%s' has been dequeued %d times." msg.Id msg.AsString.Value msg.DequeueCount

let getNextQMsg() : ProvidedQueueMessage option =
    async {
        let! qMsg = bonusQueue.Dequeue()
        return qMsg
    } |> Async.RunSynchronously

let bonusQ2Obj(qMsg : ProvidedQueueMessage) : BonusReqType =
    printMessage qMsg
    let bonusReq = Json.deserialize<BonusReqType> qMsg.AsString.Value
    bonusReq
*)
type BonusReqType = {
    AdminEmailRecipients : string[];
    Currency : string;
    Amount : decimal;
    Fees : decimal;
    GmUserId : int;
    InvBalIds : int[];
    UserEmail : string;
    UserFirstName : string;
    YearMonthSold : string;
    ProcessedCnt : int;
    CreatedOn: DateTime;
    LastProcessedTime: DateTime;
}

let testBonusReq() : BonusReqType = {
    AdminEmailRecipients = [|"mario@greenzorro.com"|];
    Amount = 0.10m;
    Currency = "EUR";
    Fees = 0.025m;
    GmUserId = 4300962; // ladderman
    InvBalIds = [|588; 2783|];
    UserEmail = "salem8@gmail.com";
    UserFirstName = "Mario";
    YearMonthSold = "201802";
    ProcessedCnt = 0;
    CreatedOn = DateTime.UtcNow;
    LastProcessedTime = DateTime.UtcNow;
}

let bonusReq = testBonusReq()

// Need to use --no-sandbox or chrome wont start
// https://github.com/elgalu/docker-selenium#chrome-not-reachable-or-timeout-after-60-secs
let chromeOptions = OpenQA.Selenium.Chrome.ChromeOptions()
chromeOptions.AddArgument("--no-sandbox")
chromeOptions.AddArgument("--disable-extensions")
//chromeOptions.AddArgument("--headless")
chromeOptions.AddArgument("--disable-gpu")
chromeOptions.AddArgument("--disable-client-side-phishing-detection")
chromeOptions.AddArgument("--disable-suggestions-service")
chromeOptions.AddArgument("--safebrowsing-disable-download-protection")
chromeOptions.AddArgument("--no-first-run")
chromeOptions.AddArgument("--allow-insecure-localhost");
//chromeOptions.AddArgument("--remote-debugging-port=9222");
let chromeNoSandbox = ChromeWithOptions(chromeOptions)
canopy.configuration.chromeDir <- __SOURCE_DIRECTORY__
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

let checkAwardingResult(bonusAmount : decimal) : bool =
    let resEl = element "#cphPage_lblSuccessMessage"
    let resText = read resEl
    match resText with
    | Regex @"bonus amount=EUR (\d+(\.\d{1,2})?)," [ resTxtAmount ; decimalPart ] ->
        let parsedAmount = Decimal.Parse(resTxtAmount)
        printfn "Bonus amount awarded %M" parsedAmount
        parsedAmount = bonusAmount
    | _ -> 
        printfn "No match given"
        false

let switchToWindow window =
    browser.SwitchTo().Window(window) |> ignore

let getOtherWindow currentWindow =
    browser.WindowHandles |> Seq.find (fun w -> w <> currentWindow)

let switchToOtherWindow currentWindow =
    switchToWindow (getOtherWindow currentWindow) |> ignore

uiAutomateLoginEverymatrixReports everymatrixUsername everymatrixPassword everymatrixSecureToken
searchCustomer()
// go to the portfolio page
click "#cphPage_UsersControl1_gvData > tbody > tr:nth-child(2) > td:nth-child(2) > a"
click "#cphPage_UserAccountsCompactControl1_gvData > tbody > tr > td:first-child > table > tbody > tr > td:nth-child(2) > a"

let rec submitInBonusForm(bonusReq : BonusReqType)(tries: int) =
    // go into the bonus from the user details
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
            printfn "Succeeded awarding the bonus amount %M %s" bonusReq.Amount bonusReq.Currency
        | false ->
            failwith "Failed in setting the bonus amount."

    if not <| checkAwardingResult bonusReq.Amount then
        printfn "Failed to award the right amount: %M" bonusReq.Amount
        click "#cphPage_btnReturn"
        click "#cphPage_CasinoWalletAccountDataControl1_btnBonusDetails"
        let baseWindow = browser.CurrentWindowHandle
        let sndWindow = browser.WindowHandles |> Seq.find(fun w -> w <> baseWindow)
        switchToWindow sndWindow
        //click cancel
        click "#cphPage_gvData_lnkCancel_0"
        acceptAlert()
        click "#cphPage_btnCloseWindow"
        switchToWindow baseWindow
        submitInBonusForm bonusReq (tries - 1)

       //submitInBonusForm bonusReq (tries - 1)