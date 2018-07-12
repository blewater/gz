module Segmentation

open System
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Table
open Newtonsoft.Json
open NLog
open GzBatchCommon
open ExcelSchemas
open GmRptFiles
open FsUtils

let logger = LogManager.GetCurrentClassLogger()

// Connect via configuration file with named connection string.
type AzStorage = AzureTypeProvider<"DefaultEndpointsProtocol=https;AccountName=gzazurefunctionsstorage;AccountKey=70GYx9psHLDTh+y4TBIYNb2OSkXRWy5JTIdFFGL7GY3EsrTnebs8ztsyEu3ro5ZEnOm1g8RmgbrlORpSQuJEjQ==;EndpointSuffix=core.windows.net", tableSchema="Emailing.json">

let openDepositsRptSchemaFile excelFilename = 
    let depositsExcelSchemaFile = PastDepositsExcelSchema(excelFilename)
    logger.Info ""
    logger.Info (sprintf "************ Importing Segmentation Users from Deposits Report %s excel file" excelFilename)
    logger.Info ""
    depositsExcelSchemaFile
    
/// Open an excel file and console out the filename
let openCustomRptSchemaFile excelFilename = 
    let customExcelSchemaFile = new CustomExcelSchema(excelFilename)
    logger.Info ""
    logger.Info (sprintf "************ Importing Segmentation Users from Custom Report %s excel file" excelFilename)
    logger.Info ""
    customExcelSchemaFile

let daysSinceJoining(yyyyMmDd : string) (excelCustomRow : CustomExcelSchema.Row) : float =
    let joinDateObj = excelCustomRow.``Join date``
    let userJoinDate =
        match DateTime.TryParseExact(joinDateObj, "yyyy-MM-dd", null, Globalization.DateTimeStyles.None) with
        | true, date -> date
        | false, _ -> invalidArg "Cannot parse a Date in this string" (sprintf "this string %A." joinDateObj)

    let procDate = yyyyMmDd.ToDateWithDay.AddDays(1.0)

    printfn "Join Date: %s, Processing Date: %s" (userJoinDate.ToString()) yyyyMmDd

    let joinElapsedDays = (procDate - userJoinDate).TotalDays
    joinElapsedDays
    
type DepositTrx = 
    {
        Amount : float;
        CompletedDate : DateTime;
        DepositTrx : bool;
    }
type DepositTrxs = DepositTrx seq

let parseCompletedDate (dateString : string) : DateTime =
    match DateTime.TryParseExact(dateString, "dd/MM/yyyy HH:mm", null, Globalization.DateTimeStyles.None) with
    | true, date -> date
    | false, _ -> invalidArg "Cannot parse a Date in this string" (sprintf "this string %A." dateString)

let getUserDeposits(depositsExcel : PastDepositsExcelSchema)(username : string) : DepositTrxs option = 
    let usernameLower = username.ToLower()

    let userDeposits =
        depositsExcel.Data
        |> Seq.filter (fun (r:PastDepositsExcelSchema.Row) -> objIsNotNull r.Username && r.Username.ToLower() = usernameLower)

    let userDepositsLen = userDeposits |> Seq.length
    match userDepositsLen with
    | 0 -> None
    | _ ->
        userDeposits
        |> Seq.map(fun (r:PastDepositsExcelSchema.Row) -> 
            { 
                Amount = r.``Credit real  amount``; 
                CompletedDate = parseCompletedDate r.Completed;
                DepositTrx = (r.``Trans type`` = "Deposit") 
            })
        |> Seq.sortByDescending (fun r-> r.CompletedDate)
        |> Seq.cache
        |> Some

let findLastUserDepositDate (userDeposits : DepositTrxs)(depositTrx : bool) : DateTime option =
    let lastDepositDate =
        let userDepTrxs =
            userDeposits
            |> Seq.filter (fun r-> r.DepositTrx = depositTrx)
        let userDepTrxsLen = userDepTrxs |> Seq.length
        match userDepTrxsLen with
        | 0 -> None
        | _ ->
            userDepTrxs
            |> Seq.head
            |> fun r -> Some r.CompletedDate
    lastDepositDate

let getLastUserDepositDate(userDeposits : DepositTrxs option) : DateTime option= 

    let lastUserDepositDate =
        match userDeposits with
        | Some deposits ->
            findLastUserDepositDate deposits true
        | None -> None
                
    printfn "Last deposit date %s" <| if lastUserDepositDate.IsSome then lastUserDepositDate.ToString() else "None"

    lastUserDepositDate

let getLastBonusDate(userDeposits : DepositTrxs option) : DateTime option= 

    let lastUserBonusDate =
        match userDeposits with
        | Some deposits ->
            findLastUserDepositDate deposits false
        | None -> None

    printfn "Last bonus date %s" <| if lastUserBonusDate.IsSome then lastUserBonusDate.ToString() else "None"
                
    lastUserBonusDate

/// Process a single custom user row
let processUser (yyyyMmDd : string) (excelCustomRow : CustomExcelSchema.Row)(depositsExcel : PastDepositsExcelSchema option) =

    let joiningDays = daysSinceJoining yyyyMmDd excelCustomRow

    let lastDate =
        match depositsExcel with
        | Some openDeposits -> 
            let userDeposits = getUserDeposits openDeposits excelCustomRow.Username 
            let lastdepositDate = getLastUserDepositDate userDeposits
            let lastBonusDate = getLastBonusDate userDeposits
            lastBonusDate
        | _ -> None

    logger.Info(
        sprintf "Processing email %s on %s/%s/%s" 
            excelCustomRow.``Email address`` 
            <| yyyyMmDd.Substring(6, 2) 
            <| yyyyMmDd.Substring(4, 2) 
            <| yyyyMmDd.Substring(0, 4))

/// Process all excel lines except Totals and upsert them
let private setDbExcelRows 
            (customExcelSchemaFile : CustomExcelSchema)
            (depositsExcel : PastDepositsExcelSchema option)
            (yyyyMmDd : string) 
            (emailToProcAlone: string option) : unit =

    // Loop through all excel rows
    for excelRow in customExcelSchemaFile.Data do
        let isActive = 
            match excelRow.``Player status`` with 
            | "Active" -> true
            | "Blocked" -> true // They may be unblocked in the future
            | _ -> false
        let excelEmailAddress = excelRow.``Email address``
        let okEmail =
            match excelEmailAddress with
            | null -> false
            | email -> email |> allowedPlayerEmail
        if 
            isActive && okEmail then

            // Normal all users processing mode
            if emailToProcAlone.IsNone then
                processUser yyyyMmDd excelRow depositsExcel
    
            //// single user processing
            elif emailToProcAlone.IsSome && emailToProcAlone.Value = excelEmailAddress then
                processUser yyyyMmDd excelRow depositsExcel

/// Process the custom excel file: Extract each row and upsert the database PlayerRevRpt table customer rows.
let loadUsers 
        (customRptFullPath: string) 
        (depositsRptFullPath: string option) 
        (emailToProcOnly: string option) : unit =

    // Open excel report file for the memory schema
    let openCustomExcelFile = customRptFullPath |> openCustomRptSchemaFile
    let yyyyMmDd = customRptFullPath |> getCustomDtStr

    let openDepositExcelFile = 
        match depositsRptFullPath with
        | Some excelFilename -> Some (openDepositsRptSchemaFile excelFilename)
        | _ -> None

    try 
        setDbExcelRows openCustomExcelFile openDepositExcelFile yyyyMmDd emailToProcOnly
    with _ -> 
        reraise()

let segmentUsers(inRpt : InRptFolder)(userEmail : string option) =
    let reportFilenames = getExcelFilenames inRpt
    loadUsers reportFilenames.customFilename reportFilenames.PastDaysDepositsFilename userEmail