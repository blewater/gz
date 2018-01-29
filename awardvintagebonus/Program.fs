#if INTERACTIVE
#r "./packages/canopy/lib/canopy.dll"
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/NLog/lib/net45/NLog.dll"
#r "./packages/Selenium.WebDriver/lib/net45/WebDriver.dll"
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#endif
open canopy
open System
open OpenQA.Selenium
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Queue

// Connect via configuration file with named connection string.
type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName="App.config">
let bonusQueue = FunctionsQueue.Queues.``withdrawn-vintages-bonus``
printfn "Queue length is %d." (bonusQueue.GetCurrentLength())

// Connect to local storage emulator localstorageConnString
(*
type FunctionsQueueLocal = AzureTypeProvider<"UseDevelopmentStorage=true">
//let qc = FunctionsQueueLocal.Queues.CloudQueueClient
//let lq = qc.GetQueueReference(Settings.QueueName)
//let res = lq.CreateIfNotExists()
let localBonusQueue = FunctionsQueueLocal.Queues.``withdrawn-vintages-bonus``
printfn "Local Queue length is %d." (localBonusQueue.GetCurrentLength())
*)

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

//let initQueue (queueConnString : string)(queueName : string) : CloudQueue=
//    // Parse the connection string and return a reference to the storage account.
//    let storageAccount = CloudStorageAccount.Parse(queueConnString)
//    let queueClient = storageAccount.CreateCloudQueueClient()
//    let queue = queueClient.GetQueueReference(queueName)
//    queue

[<EntryPoint>]
let main argv = 
        
    //let queue = initQueue queueConnString queueName

    uiAutomateLoginEverymatrixReports everymatrixUsername everymatrixPassword everymatrixSecureToken
    searchCustomer()
    // go to the portfolio page
    click "#cphPage_UsersControl1_gvData > tbody > tr:nth-child(2) > td:nth-child(2) > a"
    click "#cphPage_UserAccountsCompactControl1_gvData > tbody > tr > td:first-child > table > tbody > tr > td:nth-child(2) > a"
    // go into the bonus
    click "#cphPage_CasinoWalletAccountDataControl1_btnGiveManualBonus"

    selCashBackBonusinSelectList()

    let bonusAmountEl = element "#ctl00_cphPage_rtbBonusAmount_text"
    bonusAmountEl << "0.10"
    let amount = read bonusAmountEl

    quit()

    printfn "Amount: %s" amount

    0 // return an integer exit code