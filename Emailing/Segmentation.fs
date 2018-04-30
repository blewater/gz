module Segmentation

open System
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Table
open Newtonsoft.Json
open NLog

let logger = LogManager.GetCurrentClassLogger()

// Connect via configuration file with named connection string.
type AzStorage = AzureTypeProvider<"DefaultEndpointsProtocol=https;AccountName=gzazurefunctionsstorage;AccountKey=70GYx9psHLDTh+y4TBIYNb2OSkXRWy5JTIdFFGL7GY3EsrTnebs8ztsyEu3ro5ZEnOm1g8RmgbrlORpSQuJEjQ==;EndpointSuffix=core.windows.net", tableSchema="Registrations.json">
