namespace DbImport

module DbGzTrx =
    open System
    open NLog
    open FSharp.ExcelProvider
    open DbUtil
    open ExcelSchemas
    let logger = LogManager.GetCurrentClassLogger()

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
    /// <param name="playerRawGrossRevenue"></param>
    let getCreditedPlayerAmount (excelRow : CustomExcelSchema.Row)
                                (creditLossPcnt : float32)
                                (playerRawGrossRevenue : float) : decimal=

        if playerRawGrossRevenue > 0.0 then

            let userCurrency = excelRow.Currency

            (****  50% of Positive Gross Revenue ***)
            let creditAmount = (decimal creditLossPcnt / 100m) * decimal playerRawGrossRevenue
            creditAmount
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
    let setDbGzTrxRow   (db : DbContext) 
                        (yyyyMm :string) 
                        (excelRow : CustomExcelSchema.Row) =

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
                let usdAmount = getCreditedPlayerAmount excelRow creditLossPcnt playerRawGrossRevenue

                if isNull trxRow then 
                    setGzTrxNewDbRowValues db yyyyMm usdAmount creditLossPcnt gzUserId 
                else 
                    setGzTrxDbRowValues usdAmount creditLossPcnt trxRow
            )
            db.DataContext.SubmitChanges()
    
module DbPlayerRevRpt =
    open System
    open NLog
    open FSharp.ExcelProvider
    open DbUtil
    open ExcelSchemas

    let logger = LogManager.GetCurrentClassLogger()

    /// Update all db player values Row with Beginning Balance Rpt values but without touching the id or insert time stamp.
    let setDbRowBegBalanceValues (begBalanceExcelRow : BalanceExcelSchema.Row) (playerRow : DbPlayerRevRptRow) = 
        // Zero out balance amounts, playerloss
        playerRow.BegBalance <- begBalanceExcelRow.``Account balance`` |> float2NullableDecimal
        //Non-excel content
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.BegBalanceRptUpd
    
    /// Update all db player values Row with Ending Balance Rpt but without touching the id or insert time stamp.
    let setDbRowEndBalanceValues (endBalanceExcelRow : BalanceExcelSchema.Row) (playerRow : DbPlayerRevRptRow) = 
        // Zero out balance amounts, playerloss
        playerRow.EndBalance <- endBalanceExcelRow.``Account balance`` |> float2NullableDecimal
        //Non-excel content
        playerRow.Processed <- int GmRptProcessStatus.EndBalanceRptUpd
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
    
    /// Update all db player values Row with CustomRpt values but without touching the id or insert time stamp.
    let setDbRowCustomValues (yearMonthDay : string) (customExcelRow : CustomExcelSchema.Row) (playerRow : DbPlayerRevRptRow) = 

        playerRow.Username <- customExcelRow.Username
        if not <| isNull customExcelRow.Role then playerRow.Role <- customExcelRow.Role.ToString()
        playerRow.PlayerStatus <- customExcelRow.``Player status``
        if not <| isNull customExcelRow.``Block reason`` then playerRow.BlockReason <- customExcelRow.``Block reason``.ToString()
        playerRow.EmailAddress <- customExcelRow.``Email address``
        playerRow.LastLogin <- customExcelRow.``Last login`` |> excelObj2NullableDt
        playerRow.AcceptsBonuses <- customExcelRow.``Accepts bonuses`` |> string2NullableBool
        playerRow.TotalDepositsAmount <- customExcelRow.``Total deposits amount`` |> float2NullableDecimal
        playerRow.WithdrawsMade <- customExcelRow.``Withdraws made`` |> float2NullableDecimal
        playerRow.LastPlayedDate <- customExcelRow.``Last played date`` |> excelObj2NullableDt
        playerRow.Currency <- customExcelRow.Currency.ToString()
        playerRow.TotalBonusesAcceptedByThePlayer <- customExcelRow.``Total bonuses accepted by the player`` |> float2NullableDecimal
        playerRow.NetRevenue <- customExcelRow.``Net revenue`` |> float2NullableDecimal 
        playerRow.GrossRevenue <- customExcelRow.``Gross revenue`` |> float2NullableDecimal
        playerRow.RealMoneyBalance <- customExcelRow.``Real money balance`` |> float2NullableDecimal
        // Zero out balance amounts, playerloss
        playerRow.BegBalance <- Nullable 0m
        playerRow.EndBalance <- Nullable 0m
        playerRow.PlayerLoss <- Nullable 0m
        playerRow.PendingWithdrawals <- Nullable 0m
        playerRow.TotalWithdrawals <- Nullable 0m
        //Non-excel content
        playerRow.YearMonth <- yearMonthDay.Substring(0, 6)
        playerRow.YearMonthDay <- yearMonthDay
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.CustomRptUpd
    
    /// Insert custom excel row values in db Row but don't touch the id and set the createdOnUtc time stamp.
    let setDbNewRowCustomValues
            (db : DbContext) 
            (yearMonthDay : string) 
            (excelRow : CustomExcelSchema.Row) = 

        let newPlayerRow = 
            new DbPlayerRevRptRow(UserID = Convert.ToInt32(excelRow.``User ID``), CreatedOnUtc = DateTime.UtcNow)
        setDbRowCustomValues yearMonthDay excelRow newPlayerRow
        db.PlayerRevRpt.InsertOnSubmit(newPlayerRow)
    
    /// Save excel row values in a db PlayerRevRpt Row by upserting a custom excel row.
    let setDbBalancePlayerRow 
                        (balanceType : BalanceType)
                        (db : DbContext)
                        (yyyyMmDd :string) 
                        (balanceRow : BalanceExcelSchema.Row) = 

        let gmUserId = 
            try 
                Convert.ToInt32(balanceRow.``User ID``)
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
                    let failMsg = sprintf "When processing a balance file you can't have a user Id %d that is not found in the PlayerRevRpt table!" gmUserId
                    failwith failMsg
                else
                    match balanceType with
                    | BeginingBalance -> setDbRowBegBalanceValues balanceRow playerRow
                    | EndingBalance -> setDbRowEndBalanceValues balanceRow playerRow
            )
            db.DataContext.SubmitChanges()

    /// Save excel row values in a db PlayerRevRpt Row by upserting a row.
    let setDbCustomPlayerRow (db : DbContext) 
                        (yyyyMmDd :string) 
                        (customExcelRow : CustomExcelSchema.Row) = 

        let gmUserId = 
            try 
                Convert.ToInt32(customExcelRow.``User ID``)
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
                    setDbNewRowCustomValues db yyyyMmDd customExcelRow
                else 
                    setDbRowCustomValues yyyyMmDd customExcelRow playerRow
            )
            db.DataContext.SubmitChanges()

module BalanceRpt2Db =
    open System.IO
    open NLog
    open DbUtil
    open ExcelSchemas
    open GmRptFiles

    let logger = LogManager.GetCurrentClassLogger()

    /// Process all excel lines except Totals and upsert them
    let processBalanceExcelRptRows 
                (balanceType : BalanceType)
                (db : DbContext) 
                (balanceExcelFile : BalanceExcelSchema) 
                (yyyyMmDd : string) =

        // Loop through all excel rows
        for excelRow in balanceExcelFile.Data do
            // Skip totals line
            if not <| isNull excelRow.Email then
                logger.Info(
                    sprintf "Processing balance email %s on %s/%s/%s" 
                        excelRow.Email <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

                DbPlayerRevRpt.setDbBalancePlayerRow balanceType db yyyyMmDd excelRow
    
    /// Open an excel file and return its memory schema
    let openBalanceRptSchemaFile (balanceType : BalanceType) (excelFilename : string) : BalanceExcelSchema =
        let balanceFileExcelSchema = new BalanceExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing %A Report from filename: %s " balanceType excelFilename)
        logger.Info ""
        balanceFileExcelSchema
    
    /// Process the balance excel files: Extract each row and update database customer amounts
    let processBalanceRpt 
                (balanceType : BalanceType)
                (db : DbContext) 
                (balanceRptFullPath: string) 
                (customYyyyMmDd : string ) =

        //Curry with balance Type
        let balanceTypedOpenSchema = openBalanceRptSchemaFile balanceType
        // Open excel report file for the memory schema
        let openFile = balanceRptFullPath |> balanceTypedOpenSchema
        try 
            processBalanceExcelRptRows balanceType db openFile customYyyyMmDd
        with _ -> 
            reraise()

module WithdrawalRpt2Db =
    open System.IO
    open NLog
    open DbUtil
    open ExcelSchemas
    open GmRptFiles
    let logger = LogManager.GetCurrentClassLogger()

module CustomRpt2Db =
    open System.IO
    open NLog
    open DbUtil
    open ExcelSchemas
    open GmRptFiles
    let logger = LogManager.GetCurrentClassLogger()

    /// Process all excel lines except Totals and upsert them
    let processCustomExcelRptRows 
                (db : DbContext) (customExcelSchemaFile : CustomExcelSchema) 
                (yyyyMmDd : string) =

        // Loop through all excel rows
        for excelRow in customExcelSchemaFile.Data do
            if not <| isNull excelRow.``Email address`` then
                logger.Info(
                    sprintf "Processing email %s on %s/%s/%s" 
                        excelRow.``Email address`` <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

                DbPlayerRevRpt.setDbCustomPlayerRow db yyyyMmDd excelRow
    
    /// <summary>
    ///
    /// Open an excel file and console out the filename
    ///
    /// </summary>
    /// <param name="excelFilename">the dated excel filename</param>
    /// <returns>the open file excel schema</returns>
    let openCustomRptSchemaFile excelFilename = 
        let customExcelSchemaFile = new CustomExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing Custom Report %s excel file" excelFilename)
        logger.Info ""
        customExcelSchemaFile
    
    /// <summary>
    ///
    /// Process the custom excel file: Extract each row and update database customer amounts
    ///
    /// </summary>
    /// <param name="db">The database context</param>
    /// <param name="outFolder">The processed file folder value</param>
    /// <param name="dbConnectionString">The database connection string to use to update the database</param>
    /// <param name="datedExcelFilesNames">The list of dated (filename containing YYYYMMDD in their filename)</param>
    /// <returns>Unit</returns>
    let processCustomRpt 
            (db : DbContext) 
            (customRptFullPath: string) =

        // Open excel report file for the memory schema
        let openFile = customRptFullPath |> openCustomRptSchemaFile
        let yyyyMmDd = customRptFullPath |> getCustomDtStr
        try 
            processCustomExcelRptRows db openFile yyyyMmDd
        with _ -> 
            reraise()

module Etl = 
    open System
    open System.IO
    open FSharp.ExcelProvider
    open DbUtil
    open CurrencyRates
    open NLog
    open gzCpcLib.Task
    open GmRptFiles
    open ExcelSchemas
    open CustomRpt2Db
    open BalanceRpt2Db
    open WithdrawalRpt2Db
    let logger = LogManager.GetCurrentClassLogger()

    /// Start a Db Transaction
    let startDbTransaction (db : DbContext) = 
        let transaction = db.Connection.BeginTransaction()
        db.DataContext.Transaction <- transaction
        transaction

    /// Commit a transaction
    let commitTransaction (transaction : Data.Common.DbTransaction) = 
            // ********* Commit once per excel File
            transaction.Commit()

    let handleFailure (transaction : Data.Common.DbTransaction) (ex : exn) = 
        transaction.Rollback()
        logger.Fatal(ex, "Runtime Exception at main")

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
            (outFolder : string) =
        
        //------------ Read filenames
        logger.Info (sprintf "Reading the %s folder" inFolder)
        
        let { 
            customFilename = customFilename; 
            withdrawalFilename = withdrawalFilename; 
            begBalanceFilename = begBalanceFilename; 
            endBalanceFilename = endBalanceFilename 
            } = 
            { isProd = isProd ; folderName = inFolder } 
            |> getExcelFilenames

        logger.Debug "Starting processing excel report files"

        let transaction = startDbTransaction db
        try
            //-- Save custom excel value to database
            processCustomRpt db customFilename
            processBalanceRpt BeginingBalance db begBalanceFilename <| getCustomDtStr customFilename
            processBalanceRpt EndingBalance db endBalanceFilename <| getCustomDtStr customFilename

            // Process GzTrx
            //DbGzTrx.setDbGzTrxRow db (yyyyMmDd.Substring(0, 6)) excelRow

            (* Move processed file to out folder*)
            File.Move(customFilename, customFilename.Replace(inFolder, outFolder))
            File.Move(begBalanceFilename, begBalanceFilename.Replace(inFolder, outFolder))
            File.Move(endBalanceFilename, endBalanceFilename.Replace(inFolder, outFolder))
            File.Move(withdrawalFilename, withdrawalFilename.Replace(inFolder, outFolder))

            commitTransaction transaction

        with ex -> 
            handleFailure transaction ex
            reraise ()

        // Process investment balance
        //(new CustomerBalanceUpdTask(isProd)).DoTask()


