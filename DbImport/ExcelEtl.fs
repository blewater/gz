﻿namespace DbImport

module ErrorHandling =
    open System
    open NLog

    type DomainException = 
    | DbUpdateFailure of Exception
    | MissingCustomReport of Exception
    | Missing1stBalanceReport of Exception
    | Missing2ndBalanceReport of Exception
    | MissingWithdrawalReport of Exception

    let logger = LogManager.GetCurrentClassLogger()

    let private logError taskExc (logInfo : 'I Option) (ex : Exception) =
        let logMsg = 
            match logInfo with
            | Some info -> sprintf "Operation failed with %A. Here's additional info: %A" taskExc info
            | None -> sprintf "Operation failed with %A." taskExc
        logger.Fatal logMsg
        logger.Error ("**Exception: " + ex.Message)
    
    let tryF f taskExc (logInfo : 'I Option)=
        try 
            f() 
        with ex ->
            logError taskExc logInfo ex
            raise ex

module ExcelFilesValidation =
    open Chessie.ErrorHandling
    open System
    open System.IO
    open ErrorHandling
    open System.Text.RegularExpressions
    
    type InputExcelFolder = { isProd : bool; folderName : string }

    let folderTryF isProd f domainException=
        let logInfo = "isProd", isProd
        tryF f domainException (Some logInfo)

    /// <summary>
    ///
    /// Read dir files
    ///
    /// </summary>
    /// <param name="folderName">The input excel folder name.</param>
    /// <returns>list of excel filenames with extension .xlsx</returns>
    let get1stCustomExcelRptFilename (isProd : bool) (folderName : string) : string=
        let fileMask = if isProd then "Custom Prod*.xlsx" else "Custom Stage*.xlsx"
        let readDir () = 
            Directory.GetFiles(folderName, fileMask) 
            |> Array.toList 
            |> List.min
        //let logInfo = "isProd", isProd
        //tryF readDir Need1CustomReportFailure (Some logInfo)
        folderTryF isProd readDir MissingCustomReport

    
    // Get first 2 balance filenames
    let get1stBalanceRptExcelDirList (isProd : bool) (folderName : string) : string = 
        let fileMask = if isProd then "Balance Prod*.xlsx" else "Balance Stage*.xlsx"
        let readDir () =
            Directory.GetFiles(folderName, fileMask) 
            |> Array.toList 
            |> List.sort
            |> List.min
        folderTryF isProd readDir Missing1stBalanceReport

    let get2ndBalanceRptExcelDirList (isProd : bool) (folderName : string) : string = 
        let fileMask = if isProd then "Balance Prod*.xlsx" else "Balance Stage*.xlsx"
        let readDir () =
            Directory.GetFiles(folderName, fileMask) 
            |> Array.toList 
            |> List.sort
            |> List.item 2
        folderTryF isProd readDir Missing2ndBalanceReport

    // Get first withdrawal filename
    let getFirstWithdrawalExcelRptFilename (isProd : bool) (folderName : string) : string =
        let fileMask = if isProd then "withdraw Prod*.xlsx" else "withdraw Stage*.xlsx"
        let readDir () =
            Directory.GetFiles(folderName, fileMask) 
                |> Array.toList 
                |> List.min 
        folderTryF isProd readDir MissingWithdrawalReport


    /// <summary>
    ///
    /// Read the date part of the excel file.
    ///
    /// Precondition that the filename has a date part on the filename
    ///
    /// </summary>
    /// <param name="filename">The excel filename</param>
    /// <returns>the date part YYYYMMDD</returns>
    let datePartofFilename (filename : string) = 
        let len = filename.Length
        filename.Substring(len - 13, 8)
    
    let parsedDateOnExcel (filename : string) : string option=
        if Regex.Match(filename, "\d{8}\.xlsx$", RegexOptions.Compiled &&& RegexOptions.CultureInvariant).Success then 
            let dateStrPart = datePartofFilename filename
            let parsed, _ = DateTime.TryParseExact(dateStrPart, "yyyymmdd", null, Globalization.DateTimeStyles.None)
            if parsed then 
                Some dateStrPart
            else
                None
        else
            None

    let combinedValidation (isProd : bool) (folderName : string) =
        [ get1stCustomExcelRptFilename ; get1stBalanceRptExcelDirList ; get1stBalanceRptExcelDirList ; getFirstWithdrawalExcelRptFilename ]
        |> List.map (fun f -> f isProd folderName)


module Etl = 
    open System
    open System.IO
    open FSharp.ExcelProvider
    open DbUtil
    open CurrencyRates
    open NLog
    open gzCpcLib.Task
    open System.Text.RegularExpressions

    // Compile type
    type CustomExcelSchema = ExcelFile< "Custom Prod 20160930.xlsx" >
    type BalanceExcelSchema = ExcelFile< "Balance Prod 20161001.xlsx" >
    type WithdrawalsExcelSchema = ExcelFile< "pendingwithdraw prod 201609.xlsx" >
    let logger = LogManager.GetCurrentClassLogger()

    /// <summary>
    /// Take floats (excel's default type) and convert them to Db nullable decimals
    /// </summary>
    let float2NullableDecimal (n : float) : Nullable<decimal>= 
        Convert.ToDecimal n 
        |> Nullable<decimal> 

    /// <summary>
    ///
    /// Read dir files
    ///
    /// </summary>
    /// <param name="folderName">The input excel folder name.</param>
    /// <returns>list of excel filenames with extension .xlsx</returns>
    let getDirExcelList (isProd : bool) (folderName : string) : string list= 
        let fileMask = if isProd then "Custom Prod*.xlsx" else "Custom Stage*.xlsx"
        Directory.GetFiles(folderName, fileMask) |> Array.toList |> List.sort
    
    /// <summary>
    ///
    /// Read the date part of the excel file.
    ///
    /// Precondition that the filename has a date part on the filename
    ///
    /// </summary>
    /// <param name="filename">The excel filename</param>
    /// <returns>the date part YYYYMMDD</returns>
    let datePartofFilename (filename : string) = 
        let len = filename.Length
        filename.Substring(len - 13, 8)
    
    /// <summary>
    ///
    /// Check excel file for correctness of their name
    ///
    /// </summary>
    /// <param name="excelFiles">The collection of read excel files.</param>
    /// <returns>list of excel filenames with date mask YYYYMMDD at the end of their names before .xlsx</returns>
    let getDatedExcelfiles excelFiles = 
        [ for excelFile in excelFiles do
              if Regex.Match(excelFile, "\d{8}\.xlsx$", RegexOptions.Compiled &&& RegexOptions.CultureInvariant).Success then 
                  let datePart = datePartofFilename excelFile
                  let parsed, _ = 
                      System.DateTime.TryParseExact(datePart, "yyyymmdd", null, Globalization.DateTimeStyles.None)
                  if parsed then yield excelFile
                  else failwithf "No date part in file %s. Cannot continue" excelFile ]

    /// <summary>
    ///
    /// Update all values of db Row but not the id or insert time stamp.
    ///
    /// -> unit
    ///
    /// </summary>
    /// <param name="yearMonthDay">The yyyyMMdd mask of the filename</param>
    /// <param name="excelRow">The excel row as the source input</param>
    /// <param name="playerRow">The db row matching the id of the excel row</param>
    let setPlayerDbRowValues (yearMonthDay : string) (excelRow : CustomExcelSchema.Row) 
            (playerRow : DbUtil.DbSchema.ServiceTypes.PlayerRevRpt) = 

        if not <| isNull excelRow.``Block reason`` then playerRow.BlockReason <- excelRow.``Block reason``.ToString()
        let excelCurrency = excelRow.Currency.ToString()
        playerRow.Currency <- excelCurrency
        playerRow.EmailAddress <- excelRow.``Email address``
        playerRow.GrossRevenue <- float2NullableDecimal excelRow.``Gross revenue``
        
        // TODO: Handle Bonus out of Cash
        //playerRow.Acceptsbonuses <- Convert.ToBoolean(excelRow.``Accepts bonuses``)
        //let b = excelRow.``Last played date``

        let excelLastLogin = excelRow.``Last login``
        let parsedLastLogin, lastLogin = 
            DateTime.TryParseExact(excelLastLogin, "dd/MM/yyyy HH:mm:ss", null, Globalization.DateTimeStyles.None)
        if parsedLastLogin then playerRow.LastLogin <- Nullable lastLogin
        playerRow.NetRevenue <- float2NullableDecimal excelRow.``Net revenue``
        playerRow.PlayerStatus <- excelRow.``Player status``
        playerRow.RealMoneyBalance <- float2NullableDecimal excelRow.``Real money balance``
        if not <| isNull excelRow.Role then playerRow.Role <- excelRow.Role.ToString()
        playerRow.TotalDepositsAmount <- float2NullableDecimal excelRow.``Total deposits amount``
        //------------- TODO Last played date
        playerRow.Username <- excelRow.Username
        playerRow.YearMonth <- yearMonthDay.Substring(0, 6)
        playerRow.YearMonthDay <- yearMonthDay
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
    
    /// <summary>
    ///
    /// Insert excel row values in db Row but don't touch the id and set the insert time stamp.
    ///
    /// -> unit
    ///
    /// </summary>
    /// <param name="yearMonthDay">The yyyyMMdd mask of the filename</param>
    /// <param name="excelRow">The excel row as the source input</param>
    /// <param name="playerRow">The db row matching the id of the excel row</param>
    let setPlayerNewDbRowValues
            (db : DbContext) 
            (yearMonthDay : string) 
            (excelRow : CustomExcelSchema.Row) = 

        let newPlayerRow = 
            new DbUtil.DbSchema.ServiceTypes.PlayerRevRpt(UserID = Convert.ToInt32(excelRow.``User ID``), 
                                                   CreatedOnUtc = DateTime.UtcNow)
        setPlayerDbRowValues yearMonthDay excelRow newPlayerRow
        db.PlayerRevRpt.InsertOnSubmit(newPlayerRow)
    
    /// <summary>
    ///
    /// Save excel row values in a db PlayerRevRpt Row by updating or inserting a row.
    ///
    /// -> unit
    ///
    /// </summary>
    /// <param name="db">The database db context object to update</param>
    /// <param name="yyyyMmDd">The year month day mask of the filename</param>
    /// <param name="excelRow">The excel row as the source input</param>
    let setDbPlayerRow (db : DbContext) 
                        (yyyyMmDd :string) 
                        (excelRow : CustomExcelSchema.Row) = 

        let gmUserId = 
            try 
                Convert.ToInt32(excelRow.``User ID``)
            with e -> 0
        if gmUserId > 0 then 
            query { 
                for playerRow in db.PlayerRevRpt do
                    where (playerRow.YearMonthDay = yyyyMmDd && playerRow.UserID = gmUserId)
                    select playerRow
                    exactlyOneOrDefault
            }
            |> (fun playerRow -> 
                if isNull playerRow then 
                    setPlayerNewDbRowValues db yyyyMmDd excelRow
                else 
                    setPlayerDbRowValues yyyyMmDd excelRow playerRow
            )
            db.DataContext.SubmitChanges()

    let setGzTrxDbRowValues 
            (amount : decimal)
            (creditPcntApplied : float32)
            (trxRow : DbUtil.DbSchema.ServiceTypes.GzTrxs)=

        trxRow.Amount <- amount
        trxRow.CreditPcntApplied <- Nullable creditPcntApplied
        

    /// <summary>
    /// 
    /// Create & Insert a GzTrxs row
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="yearMonth"></param>
    /// <param name="amount"></param>
    /// <param name="creditPcntApplied"></param>
    /// <param name="gzUserId"></param>
    let setGzTrxNewDbRowValues
            (db : DbContext) 
            (yearMonth : string)
            (amount : decimal)
            (creditPcntApplied : float32)
            (gzUserId : int) = 

        let newGzTrxRow = 
            new DbUtil.DbSchema.ServiceTypes.GzTrxs(
                CustomerId=gzUserId,
                YearMonthCtd = yearMonth,
                CreatedOnUTC = DateTime.UtcNow,
                TypeId = int DbUtil.GzTransactionType.CreditedPlayingLoss)

        setGzTrxDbRowValues amount creditPcntApplied newGzTrxRow
        db.GzTrxs.InsertOnSubmit(newGzTrxRow)

    /// <summary>
    /// 
    /// Get the greenzorro used id by email
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="gmUserEmail"></param>
    let getGzUserId (db : DbContext) (gmUserEmail : string) : int =
        query {
            for user in db.AspNetUsers do
            where (user.Email = gmUserEmail)
            select user.Id
            exactlyOneOrDefault 
        }
        |> (fun userId ->
            if userId = 0 then
                failwithf "Everymatrix email %s not found in db: %s. Cannot continue..." gmUserEmail db.DataContext.Connection.DataSource
            else
                userId
        )

    /// <summary>
    /// 
    /// Get the credited loss Percentage in human form % i.e. 50 from gzConfiguration
    /// 
    /// </summary>
    /// <param name="db"></param>
    let getCreditLossPcnt (db : DbContext) : float32 =
        query {
            for c in db.GzConfigurations do
            exactlyOne
        }
        |> (fun conf -> conf.CREDIT_LOSS_PCNT)

    /// <summary>
    /// 
    /// Main formula calculating the amount that will be credited to the users account
    /// 
    /// </summary>
    /// <param name="excelRow"></param>
    /// <param name="creditLossPcnt"></param>
    /// <param name="rates"></param>
    /// <param name="playerRawGrossRevenue"></param>
    let getCreditedPlayerAmount (excelRow : CustomExcelSchema.Row)
                                (creditLossPcnt : float32)
                                (rates : CurrencyRatesValues)
                                (playerRawGrossRevenue : float) : decimal=

        if playerRawGrossRevenue > 0.0 then

            let curKey = excelRow.Currency + "USD"
            let convRate = rates.Item (curKey) |> fst

            (****  50% of Positive Gross Revenue ***)
            let usdAmount = (decimal creditLossPcnt / 100m) * decimal playerRawGrossRevenue * convRate
            usdAmount
        else
            0m
    
    /// <summary>
    /// 
    /// Upsert a GzTrxs transaction row with the credited amount
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="yyyyMm"></param>
    /// <param name="excelRow"></param>
    /// <param name="rates"></param>
    let setDbGzTrxRow   (db : DbContext) 
                        (yyyyMm :string) 
                        (excelRow : CustomExcelSchema.Row)
                        (rates : CurrencyRatesValues) = 

        let playerRawGrossRevenue = excelRow.``Gross revenue``
        if playerRawGrossRevenue > 0.0 then
            let gmEmail = excelRow.``Email address``
            let gzUserId = getGzUserId db gmEmail
            query { 
                for trxRow in db.GzTrxs do
                    where (
                        trxRow.YearMonthCtd = yyyyMm 
                        && trxRow.CustomerId = gzUserId
                        && trxRow.GzTrxTypes.Code = int DbUtil.GzTransactionType.CreditedPlayingLoss
                    )
                    select trxRow
                    exactlyOneOrDefault
            }
            |> (fun trxRow ->
                let creditLossPcnt = getCreditLossPcnt db 
                let usdAmount = getCreditedPlayerAmount excelRow creditLossPcnt rates playerRawGrossRevenue

                if isNull trxRow then 
                    setGzTrxNewDbRowValues db yyyyMm usdAmount creditLossPcnt gzUserId 
                else 
                    setGzTrxDbRowValues usdAmount creditLossPcnt trxRow
            )
            db.DataContext.SubmitChanges()
    
    /// <summary>
    ///
    /// Upsert all excel files rows
    ///
    /// </summary>
    /// <param name="db">The database context object</param>
    /// <param name="ExcelSchema">The excel schema object to read all rows</param>
    /// <param name="yyyyMmDd">the date string</param>
    /// <returns>unit</returns>
    let setDbExcelRows 
                (db : DbContext) (openFile : CustomExcelSchema) 
                (yyyyMmDd : string)
                (rates : CurrencyRatesValues) = 

        // Loop through all excel rows
        for excelRow in openFile.Data do
            if not <| isNull excelRow.``Email address`` then
                logger.Info(
                    sprintf "Processing email %s on %s/%s/%s" 
                        excelRow.``Email address`` <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

                setDbPlayerRow db yyyyMmDd excelRow
                // Send string date wout day
                setDbGzTrxRow db (yyyyMmDd.Substring(0, 6)) excelRow rates
    
    /// <summary>
    ///
    /// Open an excel file and console out the filename
    ///
    /// </summary>
    /// <param name="excelFilename">the dated excel filename</param>
    /// <returns>the open file excel schema</returns>
    let openExcelSchemaFile excelFilename = 
        let openFile = new CustomExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing %s excel file" excelFilename)
        logger.Info ""
        openFile
    
    /// <summary>
    ///
    /// Process excel files: Extract each row and update database customer amounts
    ///
    /// </summary>
    /// <param name="db">The database context</param>
    /// <param name="outFolder">The processed file folder value</param>
    /// <param name="dbConnectionString">The database connection string to use to update the database</param>
    /// <param name="datedExcelFilesNames">The list of dated (filename containing YYYYMMDD in their filename)</param>
    /// <returns>Unit</returns>
    let processExcelFiles 
            (db : DbContext) 
            (outFolder : string)
            (datedExcelFilesNames: string seq) 
            (rates : CurrencyRatesValues) = 

        // Open each excel file
        for excelFilename in datedExcelFilesNames do
            let openFile = openExcelSchemaFile excelFilename
            let yyyyMmDd = datePartofFilename excelFilename
            let transaction = db.Connection.BeginTransaction()
            db.DataContext.Transaction <- transaction
            try 
                setDbExcelRows db openFile yyyyMmDd rates
                // ********* Commit once per excel File
                transaction.Commit()
                (* Move processed file to out folder*)
                File.Move(excelFilename, outFolder)
            with _ -> 
                transaction.Rollback()
                reraise()
    
    /// <summary>
    ///
    /// Phase 1 Processing:
    ///     Read all excel files from input folder
    ///     Check if they have date YYYYMMDD in their names
    ///     Save files into the database
    ///
    /// </summary>
    /// <param name="db">The database context</param>
    /// <param name="inFolder">The input file folder value</param>
    /// <param name="outFolder">The processed file folder value</param>
    /// <param name="rates">The currency rate values for converting Everymatrix amounts to $</param>
    /// <returns>Unit</returns>
    let ProcessExcelFolder
            (isProd : bool) 
            (db : DbContext) 
            (inFolder : string)
            (outFolder : string)
            (rates : CurrencyRatesValues) = 
        
        //------------ Read filenames
        logger.Info (sprintf "Reading the %s folder" inFolder)
        let dirExcelFileList = getDirExcelList isProd inFolder
        if dirExcelFileList.Length > 0 then 
            logger.Debug "Checking read files for date in their name"
            let datedExcelFilenames = getDatedExcelfiles dirExcelFileList

            (new CustomerBalanceUpdTask(isProd)).DoTask()

            //------------ Save excel value to database
            processExcelFiles db outFolder datedExcelFilenames rates

        else 
            logger.Trace "No excel files were found!"
