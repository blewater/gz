module Segmentation

open System
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Table
open Newtonsoft.Json
open NLog
open FSharp.ExcelProvider

type BalanceExcelSchema = ExcelFile< "Balance Prod 20161001.xlsx" >
type CustomExcelSchema = ExcelFile< "Custom Prod 20180309.xlsx" >
//type DepositsExcelSchema = ExcelFile< "Vendor2User Prod 20170606.xlsx" >
//type BonusExcelSchema = ExcelFile< "Bonus Prod 20171212.xlsx" >

let logger = LogManager.GetCurrentClassLogger()

// Connect via configuration file with named connection string.
type AzStorage = AzureTypeProvider<"DefaultEndpointsProtocol=https;AccountName=gzazurefunctionsstorage;AccountKey=70GYx9psHLDTh+y4TBIYNb2OSkXRWy5JTIdFFGL7GY3EsrTnebs8ztsyEu3ro5ZEnOm1g8RmgbrlORpSQuJEjQ==;EndpointSuffix=core.windows.net", tableSchema="Emailing.json">

/// Open an excel file and return its memory schema
let private openBalanceRptSchemaFile (excelFilename : string) : BalanceExcelSchema =
    let balanceFileExcelSchema = new BalanceExcelSchema(excelFilename)
    logger.Info ""
    logger.Info (sprintf "************ Processing Segmentation Report from filename: %s " excelFilename)
    logger.Info ""
    balanceFileExcelSchema
    
