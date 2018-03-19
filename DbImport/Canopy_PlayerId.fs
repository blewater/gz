namespace DbImport

open System
open canopy
open OpenQA.Selenium
open System.Text.RegularExpressions
open NLog

type Canopy_PlayerId(visualSession : bool, everymatrixUsername : string, everymatrixPassword : string, everymatrixSecureToken : string)=

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
        //chromeOptions.AddArgument("--remote-debugging-port=9222");
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