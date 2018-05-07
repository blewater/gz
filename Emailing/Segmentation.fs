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

let logger = LogManager.GetCurrentClassLogger()

// Connect via configuration file with named connection string.
type AzStorage = AzureTypeProvider<"DefaultEndpointsProtocol=https;AccountName=gzazurefunctionsstorage;AccountKey=70GYx9psHLDTh+y4TBIYNb2OSkXRWy5JTIdFFGL7GY3EsrTnebs8ztsyEu3ro5ZEnOm1g8RmgbrlORpSQuJEjQ==;EndpointSuffix=core.windows.net", tableSchema="Emailing.json">

let private openDepositsRptSchemaFile excelFilename = 
    let depositsExcelSchemaFile = DepositsExcelSchema(excelFilename)
    logger.Info ""
    logger.Info (sprintf "************ Importing Segmentation Users from Deposits Report %s excel file" excelFilename)
    logger.Info ""
    depositsExcelSchemaFile
    
/// Open an excel file and console out the filename
let private openCustomRptSchemaFile excelFilename = 
    let customExcelSchemaFile = new CustomExcelSchema(excelFilename)
    logger.Info ""
    logger.Info (sprintf "************ Importing Segmentation Users from Custom Report %s excel file" excelFilename)
    logger.Info ""
    customExcelSchemaFile

/// Process a single custom user row
let processUser (yyyyMmDd : string) (excelCustomRow : CustomExcelSchema.Row)(depositsExcel : DepositsExcelSchema option) =
    let joinDate = excelCustomRow.``Join date``
    printfn "%O" joinDate
    logger.Info(
        sprintf "Processing email %s on %s/%s/%s" 
            excelCustomRow.``Email address`` <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

/// Process all excel lines except Totals and upsert them
let private setDbExcelRows 
            (customExcelSchemaFile : CustomExcelSchema)
            (depositsExcel : DepositsExcelSchema option)
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