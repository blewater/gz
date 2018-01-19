#I __SOURCE_DIRECTORY__
#r "./packages/Selenium.WebDriver/lib/net40/WebDriver.dll"
#r "./packages/canopy/lib/canopy.dll"

open canopy
open runner
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
// let chromeOptions = OpenQA.Selenium.Chrome.ChromeOptions()
// chromeOptions.AddArgument("--no-sandbox")
// let chromeNoSandbox = ChromeWithOptions(chromeOptions)
// start chromeNoSandbox
canopy.configuration.chromeDir <- "."
start chrome

"Should check home page h1" &&& fun _ ->
  url "https://stackoverflow.com/"

  "h1#h-top-questions" == "Top Questions"

"Should check questions page h1" &&& fun _ ->
  url "https://stackoverflow.com/questions"

  "h1#h-all-questions" == "All Questions"

run()

let uiAutomateLoginEverymatrixReports 
        (everyMatrixUserName : string)
        (everymatrixPassword : string)
        (everymatrixLoginToken : string) : unit =

    url "https://admin3.gammatrix.com/Admin/Login.aspx"
    "#rtbTop_text" << everyMatrixUserName
    "#rtbMid_text" << everymatrixPassword
    "#rtbBottom_text" << everymatrixLoginToken
    click "#btnLogin" 

quit()