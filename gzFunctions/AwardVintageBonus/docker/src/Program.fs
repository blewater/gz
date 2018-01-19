open canopy
open System
open OpenQA.Selenium

[<Literal>]
let everymatrixUsername = "admin"
[<Literal>]
let everymatrixPassword = "player888"
[<Literal>]
let everymatrixSecureToken = "3DFEC757D808494"

// Need to use --no-sandbox or chrome wont start
// https://github.com/elgalu/docker-selenium#chrome-not-reachable-or-timeout-after-60-secs
let chromeOptions = OpenQA.Selenium.Chrome.ChromeOptions()
chromeOptions.AddArgument("--no-sandbox")
chromeOptions.AddArgument("--disable-extensions")
chromeOptions.AddArgument("--headless")
chromeOptions.AddArgument("--disable-gpu")
chromeOptions.AddArgument("--port=4444")
let chromeNoSandbox = ChromeWithOptions(chromeOptions)
canopy.configuration.chromeDir <- "."
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

[<EntryPoint>]
let main argv = 

    uiAutomateLoginEverymatrixReports everymatrixUsername everymatrixPassword everymatrixSecureToken
    searchCustomer()
    click "#cphPage_UsersControl1_gvData > tbody > tr:nth-child(2) > td:nth-child(2) > a"
    click "#cphPage_UserAccountsCompactControl1_gvData > tbody > tr > td:first-child > table > tbody > tr > td:nth-child(2) > a"
    click "#cphPage_CasinoWalletAccountDataControl1_btnGiveManualBonus"

    selCashBackBonusinSelectList()
    let bonusAmountEl = element "#ctl00_cphPage_rtbBonusAmount_text"
    bonusAmountEl << "0.10"
    let amount = read bonusAmountEl

    quit()

    printfn "Amount: %s" amount
    0 // return an integer exit code