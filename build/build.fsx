//**********************************************************************
//Perform a build and deployment to greenzorro dev or production site
//
//Requirements:
//    First timers: Run build.cmd to set up fake, paket dependencies.
//    Install azure powershell if you intent to deploy from stage to greenzorro.com
//
//FAKE build script:
//For Gz Web site to build develop or prod and deploy to Azure.
//
//Steps based on mode=<env> parameter:
//
//Dev
//    -> checkout develop
//    -> build 
//    ->    if build fails open azure deployment result page
//    -> deploy to dev site https://www.greenzorrodev.azurewebsites.net
//    -> open stage site in browser
//    ->      --or if the build fails open azure deployment status page 
//prod 
//    -> checkout develop
//    -> pull develop [unique step in this mode]
//    -> merge with develop [unique step in this mode]
//    -> build 
//    ->    if buld fails open azure deployment result page
//    -> deploy to stage site https://www.greenzorro-sgn.azurewebsites.net
//    -> open stage site in browser if new "develop"" branch changes resulted in stage build
//        -- or open production site in browser if production is uptodate 
//    -> prompt user to deploy to production, if there's a new stage build
//    ** Note **  check now result of stage build before answering Y
//    [Requires azure powershell] 
//    -> Swap stage with production, if user answered Y in previous step
//
//Usage :
//
//Go to source directory
//If Fake is installed use fake or fsi (fsharp script interpreter)
//Fake build.fsx mode=<prod or dev>
//
//Examples:
//fake
//    Runs build.fsx with default options for the stage site and prod branch. See steps above.
//fake "build.fsx"
//--or fsi build.fsx
//    Same as before.
//
//fake build.fsx mode=dev
//--or fsi build.fsx mode=dev
//Runs build for dev site using the develop branch.
//
//***********************************************************************
#r @"packages/FAKE/tools/FakeLib.dll"
#r @"packages/Fake.Azure.WebApps/lib/net451/Fake.Azure.WebApps.dll"
#r @"packages\FSharp.Text.RegexProvider\lib\net40\FSharp.Text.RegexProvider.dll"
#r @"packages\System.Management.Automation.dll\lib\net40\System.Management.Automation.dll"
open System
open System.Net
open System.Management.Automation
open FSharp.Data
open FSharp.Text.RegexProvider

open Fake
open Fake.Git
open Fake.ZipHelper
open Fake.Azure.WebApps

type GitShaRegex = Regex< @"\.Sha\.(?<SHA>\w+)</" >

(*----------------  Properties  -------------------*)
[<Literal>]
let Solution = @"../gz.sln"
let GitRepo = __SOURCE_DIRECTORY__ @@ ".."

(* Stage *)
[<Literal>]
let StageGzUrl = "https://greenzorro-sgn.azurewebsites.net"
[<Literal>]
//let StageAzDepUrl = "https://portal.azure.com/#resource/subscriptions/d92ca232-a672-424c-975d-1dcf45a58b0b/resourceGroups/GreenzorroBizSpark/providers/Microsoft.Web/sites/greenzorro/slots/sgn/DeploymentSource"
// Temp url during sites transition
let StageAzDepUrl = "https://portal.azure.com/#resource/subscriptions/500c96ff-15a2-4861-8a33-8872bdcb6b58/resourceGroups/2ndSub_All_BizSpark_RG/providers/Microsoft.Web/sites/greenzorro/slots/sgn/DeploymentSource"
let StageAzureProdSlots = "https://portal.azure.com/#resource/subscriptions/d92ca232-a672-424c-975d-1dcf45a58b0b/resourceGroups/GreenzorroBizSpark/providers/Microsoft.Web/sites/greenzorro/deploymentSlots"
let ProdGzUrl = "https://www.greenzorro.com"

(* Dev *)
[<Literal>]
let DevGzUrl = "https://greenzorrodev.azurewebsites.net"
[<Literal>]
let DevAzDepUrl = "https://portal.azure.com/#resource/subscriptions/d92ca232-a672-424c-975d-1dcf45a58b0b/resourceGroups/GreenzorroBizSpark/providers/Microsoft.Web/sites/greenzorrodev/DeploymentSource"

let mode = getBuildParamOrDefault "mode" "prod"

/// Whether azure stage site is updated with the latest local git source version
let mutable stageIsUpdated = false
/// Whether azure production site is updated with the latest local git source version
let mutable productionUpToDate = false


let projectName = "gzWeb.csproj"

let baseDir = __SOURCE_DIRECTORY__ @@ @"..\"
let gzWebProj = baseDir @@ @"gzWeb\gzWeb.csproj"

// Following contains Azure password and it's git-ignored
let gzWebDevPublishProfile = __SOURCE_DIRECTORY__ @@ "greenzorroDev.pubxml"

(*-------------------  End of property declarations   ---------------------------------*)
Target "IsModeParamOk" (fun _ ->
    match mode with
    | "dev" -> trace "Running in dev mode"
    | "prod" -> trace  "Running in prod mode"
    | _ -> failwithf "Unknown mode %s passed in" mode
)
Target "CheckoutDevelop" (fun _ ->
    checkoutBranch GitRepo "develop"
)
Target "EndWithDevelop" (fun _ ->
    checkoutBranch GitRepo "develop"
)
Target "PullDevelop" (fun _ ->
    pull GitRepo "origin" "develop"
    trace "pulled develop..."
)
Target "MergeMaster" (fun _ ->
    checkoutBranch GitRepo "master"
    trace "checked out master..."

    // Attempt to merge with master
    try
        merge GitRepo NoFastForwardFlag "develop"
    with
        ex -> checkoutBranch GitRepo "develop"; trace "Can't merge with master.\nNeed to checkin your local changes below:" ; raise ex
    trace "merging with develop"
)

Target "BuildGzWeb" (fun _ ->
    !!gzWebProj |>
      match mode with
          | "dev" ->   
                MSBuild "" "build" [ ("Configuration", "Debug"); ("RestorePackages", "True") ; ("PublishDir", "") ]
          | _ -> 
                MSBuild "" "build"  [ ("Configuration", "Release"); ("RestorePackages", "True") ]
      |> Log "Build-Output: "
      tracefn "built %s..." mode
)

Target "DeployAzDev" (fun () ->

    let result =
        ExecProcess (fun info -> 
            info.FileName <- @"c:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"
            info.Arguments <- "-verb:sync"
            info.Arguments <- "-source:contentPath=" + @""
            info.Arguments <- "-verb:sync"
            info.Arguments <- "-verb:sync"
            info.Arguments <- "-verb:sync"
        ) (System.TimeSpan.FromMinutes 1.0)     
    if result <> 0 then failwith "DeployAzDev timed out!"
)

Target "PushMaster" (fun _ ->
    tracefn "about to push to %s" <| if mode = "prod" then "master" else mode
    if mode = "prod" then
        push GitRepo
        trace "pushed to prod"
)

let userReply2Bool (userAns : string) : bool=
    match userAns.Trim() with
    | "Y" | "y" | "Υ" | "υ" -> true
    | "N" | "n" | "Ν" | "ν" -> false
    | null -> trace "Null answer"; false
    | _ -> tracefn "Unknown response %s" userAns; false

Target "OpenResultInBrowser" (fun _ ->

    ///<summary>
    /// Get html content by Http Get
    ///</summary>
    let getHtml (url : string) : string =
        let req = WebRequest.Create(Uri(url)) 
        use resp = req.GetResponse()
        use stream = resp.GetResponseStream() 
        use reader = new IO.StreamReader(stream) 
        reader.ReadToEnd()

    ///<summary>
    /// Get the parsed sha value from a string.
    ///</summary>
    let getDeployedSha (indexHtml : string) : string =
        let htmlSha = GitShaRegex().TypedMatch(indexHtml).SHA.Value
        htmlSha

    ///<summary>
    /// Compare the local git sha with the newly built sha value on azure site (dev or stage) or whether it matches the one already in production.
    ///<summary>
    ///<returns>
    /// Whether the newly built Sha matches the local git sha. True means the Azure build succeeded of the latest git code version.
    ///</returns>
    let AzureBuildSuccess (mode : string): bool=

        trace "Http getting the freshly built home page. Azure may take a while to answer..."
        let htmlStageOrDevSha = 
            match mode with
                | "dev" -> DevGzUrl
                | _ -> StageGzUrl
            |> getHtml 
            |> getDeployedSha

        let gitSha = getSHA1 GitRepo "HEAD"
        tracefn "The stage html, master git Sha1 hashes are %s,%s" htmlStageOrDevSha gitSha

        if 
            gitSha = htmlStageOrDevSha
        then
            trace "** Successful build in Azure! ** Sha1 tags match. Opening site in browser..."
            match mode with
                | "dev" -> DevGzUrl
                | _ -> stageIsUpdated <- true; StageGzUrl
            |> Diagnostics.Process.Start 
            |> ignore
            true
        else
            if 
                mode = "prod" 
            then
                let prodHtmlSha = ProdGzUrl |> getHtml |> getDeployedSha

                if 
                    gitSha = prodHtmlSha 
                then
                    tracefn "This build has been previously deployed to production. The production Sha1 matches the local git Sha1 -> %s,%s" prodHtmlSha gitSha
                    productionUpToDate <- true
                    Diagnostics.Process.Start ProdGzUrl 
                    |> ignore
                    true
                else
                    trace "Sha1 tags do not match with either Stage or Production so the build failed. Opening azure deployment page in browser..."
                    false
            else
                trace "Sha1 tags do not match so the build failed. Opening azure deployment page in browser..."
                false

    //******** Start Here in this target by httpGet the site home page
    match mode with
        | "dev" -> Diagnostics.Process.Start DevAzDepUrl
        | _ -> Diagnostics.Process.Start StageAzDepUrl
        |> ignore
    let proceedBuildStatus = getUserInput "Please check build status in opened page.\nHas the build completed successfully (Y/N)?"
                                |> userReply2Bool
    if
        proceedBuildStatus
    then
        if not <| AzureBuildSuccess mode then
            // Open Azure build/deployment page and abort this build script
            failwith "Build deployment to Azure failed." 
)
Target "SwapStageLive" (fun _ ->

    (**** run a powershell script by providing the script filename without extension .ps1 *)
    let runPowershellScript (ps1FileName : string) : unit =
        PowerShell.Create()
            .AddScript(__SOURCE_DIRECTORY__ @@ ps1FileName+ ".ps1")
            .Invoke()
            |> Seq.iter (printfn "%O")            
          
//            let p = new Diagnostics.Process();              
//            p.StartInfo.FileName <- "cmd.exe";
//            p.StartInfo.Arguments <- ("/c powershell -ExecutionPolicy Unrestricted .\\" + ps1FileName + ".ps1")
//            p.StartInfo.RedirectStandardOutput <- true
//            p.StartInfo.UseShellExecute <- false
//            p.Start() |> ignore
//            printfn "Processing"
//            printfn "%A" (p.StandardOutput.ReadToEnd())
//            printfn "Finished"

    if stageIsUpdated then
        trace "Stage was updated with a new build and is cleared to swap with live:"
    else if productionUpToDate then
        trace "*** Production is already up to date. No need to swap. ***"
    else
        trace "Stage and production do not appear to have the latest git source version built.\nCheck build status"
    
    let proceedSwap = 
        not productionUpToDate && getUserInput "Are you sure you want to swap stage with production (Y/N)?"
        |> userReply2Bool
    if proceedSwap then
        runPowershellScript "SwapStageLive"
)

// Dependencies
"IsModeParamOk"
  ==> "CheckoutDevelop"
  =?> ("PullDevelop", mode = "prod")
  =?> ("MergeMaster", mode = "prod")
  ==> "BuildGzWeb"
  =?> ("Zip", mode = "dev")
  =?> ("DeployAzDev", mode.Equals "dev")
  =?> ("PushMaster", mode = "prod")
  ==> "OpenResultInBrowser"
  =?> ("SwapStageLive", mode = "prod")
  ==> "EndWithDevelop"

// start build
RunTargetOrDefault "EndWithDevelop"