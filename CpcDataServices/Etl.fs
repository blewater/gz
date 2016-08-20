namespace CpcDataServices

open System
open System.IO
open FSharp.Data.TypeProviders
open FSharp.ExcelProvider
open System.Text.RegularExpressions

module Etl = 
    // Compile type
    type ExcelSchema = ExcelFile< "GM Trx 20160719.xlsx" >
    
    /// <summary>
    ///
    /// Read dir files
    ///
    /// </summary>
    /// <param name="folderName">The input excel folder name.</param>
    /// <returns>list of excel filenames with extension .xlsx</returns>
    let getDirExcelList folderName = Directory.GetFiles(folderName, "*.xlsx") |> Array.toList
    
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
    let setPlayerDbRowValues (yearMonthDay : string) (excelRow : ExcelSchema.Row) 
        (playerRow : DbUtil.DbSchema.ServiceTypes.PlayerRevRpt) = 
        if not <| isNull excelRow.``Block reason`` then playerRow.BlockReason <- excelRow.``Block reason``.ToString()
        playerRow.Currency <- excelRow.Currency
        playerRow.EmailAddress <- excelRow.``Email address``
        playerRow.GrossRevenue <- Convert.ToDecimal(excelRow.``Gross revenue``)
        let excelLastLogin = excelRow.``Last login``
        let parsedLastLogin, lastLogin = 
            DateTime.TryParseExact(excelLastLogin, "dd/MM/yyyy HH:mm:ss", null, Globalization.DateTimeStyles.None)
        if parsedLastLogin then playerRow.LastLogin <- Nullable lastLogin
        playerRow.NetRevenue <- Convert.ToDecimal(excelRow.``Net revenue``)
        playerRow.PlayerStatus <- excelRow.``Player status``
        playerRow.RealMoneyBalance <- Convert.ToDecimal(excelRow.``Real money balance``)
        if not <| isNull excelRow.Role then playerRow.Role <- excelRow.Role.ToString()
        playerRow.TotalDepositsAmount <- Convert.ToDecimal(excelRow.``Total deposits amount``)
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
            (db : DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb) 
            (yearMonthDay : string) 
            (excelRow : ExcelSchema.Row) = 

        let newPlayerRow = 
            new DbUtil.DbSchema.ServiceTypes.PlayerRevRpt(UserID = Convert.ToInt32(excelRow.``User ID``), 
                                                   CreatedOnUtc = DateTime.UtcNow)
        setPlayerDbRowValues yearMonthDay excelRow newPlayerRow
        db.PlayerRevRpt.InsertOnSubmit(newPlayerRow)
    
    /// <summary>
    ///
    /// Save excel row values in db Row by updating or inserting a row.
    ///
    /// -> unit
    ///
    /// </summary>
    /// <param name="db">The database db context object to update</param>
    /// <param name="yearMonthDay">The yyyyMMdd mask of the filename</param>
    /// <param name="excelRow">The excel row as the source input</param>
    let SavePlayerRow (db : DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb) 
                        (datePart :string) 
                        (excelRow : ExcelSchema.Row) = 

        let gmUserId = 
            try 
                Convert.ToInt32(excelRow.``User ID``)
            with e -> 0
        if gmUserId > 0 then 
            query { 
                for playerRow in db.PlayerRevRpt do
                    where (playerRow.YearMonthDay = datePart && playerRow.UserID = gmUserId)
                    select playerRow
                    exactlyOneOrDefault
            }
            |> (fun playerRow -> 
                if isNull playerRow then 
                    setPlayerNewDbRowValues db datePart excelRow
                else 
                    setPlayerDbRowValues datePart excelRow playerRow
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
    let saveExcelRows (db : DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb) (openFile : ExcelSchema) (yyyyMmDd : string) = 
        // Loop through all excel rows
        for excelRow in openFile.Data do
            printfn "Processing email %s on %s/%s/%s" excelRow.``Email address`` <| yyyyMmDd.Substring(6, 2) 
            <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4)
            SavePlayerRow db yyyyMmDd excelRow
    
    /// <summary>
    ///
    /// Open an excel file and console out the filename
    ///
    /// </summary>
    /// <param name="excelFilename">the dated excel filename</param>
    /// <returns>the open file excel schema</returns>
    let openExcelSchemaFile excelFilename = 
        let openFile = new ExcelSchema(excelFilename)
        printfn ""
        printfn "************ Processing %s excel file" excelFilename
        printfn ""
        openFile
    
    /// <summary>
    ///
    /// Process excel files
    ///
    /// </summary>
    /// <param name="dbConnectionString">The database connection string to use to update the database</param>
    /// <param name="datedExcelFilesNames">The list of dated (filename containing YYYYMMDD in their filename)</param>
    /// <returns>Unit</returns>
    let processExcelFiles (db : DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb) (datedExcelFilesNames: string seq) = 
        // Open each excel file
        for excelFilename in datedExcelFilesNames do
            let openFile = openExcelSchemaFile excelFilename
            let yyyyMmDd = datePartofFilename excelFilename
            let transaction = db.Connection.BeginTransaction()
            db.DataContext.Transaction <- transaction
            try 
                saveExcelRows db openFile yyyyMmDd
                // ********* Commit once per excel File
                transaction.Commit()
            with _ -> 
                transaction.Rollback()
                reraise()
    
    /// <summary>
    ///
    /// Phase 1 Processing:
    ///     Read all excel files
    ///     Check if they have date YYYYMMDD in their names
    ///     Save files into the database
    ///
    /// </summary>
    /// <param name="dbConnectionString">The database connection string to use to update the database</param>
    /// <param name="inFolder">The input file folder parameter</param>
    /// <returns>Unit</returns>
    let Phase1Processing (db : DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb) (inFolder:string) = 
        //------------ Read filenames
        printfn "Reading the %s folder" inFolder
        let dirExcelFileList = getDirExcelList inFolder
        if dirExcelFileList.Length > 0 then printfn "Checking read files for date in their name"
        else printfn "No excel files were found!"
        let datedExcelFilenames = getDatedExcelfiles dirExcelFileList
        //------------ Save excel files
        processExcelFiles db datedExcelFilenames
