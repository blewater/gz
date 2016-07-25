namespace CpcDataServices

open System
open System.IO
open System.Data
open System.Data.Linq
open FSharp.Data.TypeProviders
open FSharp.Linq
open FSharp.ExcelProvider
open System.Text.RegularExpressions

module public Etl=

    // Compile type
    type excelSchema = ExcelFile<"GM Trx 20160719.xlsx">

    // Use for compile time memory schema representation
    [<Literal>]
    let compileTimeDbString = @"Data Source=.\SQLEXPRESS;Initial Catalog=gzDevDb;Persist Security Info=True;Integrated Security=SSPI;"
    type dbSchema = SqlDataConnection<ConnectionString = compileTimeDbString>

    /// <summary>
    ///
    /// Read dir files
    ///
    /// </summary>
    /// <param name="folderName">The input excel folder name.</param>
    /// <returns>list of excel filenames with extension .xlsx</returns>
    let getDirExcelList folderName = 
        Directory.GetFiles(folderName, "*.xlsx")
            |> Array.toList


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
        filename.Substring(len-13, 8)

    /// <summary>
    ///
    /// Check excel file for correctness of their name
    ///
    /// </summary>
    /// <param name="excelFiles">The collection of read excel files.</param>
    /// <returns>list of excel filenames with date mask YYYYMMDD at the end of their names before .xlsx</returns>
    let getDatedExcelfiles excelFiles =
        [
            for excelFile in excelFiles do 
                if Regex.Match(excelFile, "\d{8}\.xlsx$", RegexOptions.Compiled &&& RegexOptions.CultureInvariant).Success then
                    let datePart = datePartofFilename excelFile
                    let parsed, _ = System.DateTime.TryParseExact(datePart, "yyyymmdd", null, Globalization.DateTimeStyles.None) 
                    if parsed then
                        yield excelFile
                    else
                        failwithf "No date part in file %s. Cannot continue" excelFile
        ] 

    /// <summary>
    ///
    /// Update all values of db Row but not the id or insert time stamp.
    ///
    /// </summary>
    /// <param name="yearMonthDay">The yyyyMMdd mask of the filename</param>
    /// <param name="excelRow">The excel row as the source input</param>
    /// <param name="playerRow">The db row matching the id of the excel row</param>
    /// <returns>Updated row of type dbSchema.ServiceTypes.PlayerRevRpt</returns>
    let updateDbRowValues
        (yearMonthDay : string)
        (excelRow : excelSchema.Row)
        (playerRow : dbSchema.ServiceTypes.PlayerRevRpt) =
    
        if excelRow.``Block reason`` <> null then
            playerRow.BlockReason <- excelRow.``Block reason``.ToString()
        playerRow.Currency <- excelRow.Currency
        playerRow.EmailAddress <- excelRow.``Email address``
        playerRow.GrossRevenue <- Convert.ToDecimal(excelRow.``Gross revenue``)
        let excelLastLogin = excelRow.``Last login``
        let parsedLastLogin, lastLogin = DateTime.TryParseExact(excelLastLogin, "dd/MM/yyyy HH:mm:ss", null, Globalization.DateTimeStyles.None)
        if parsedLastLogin then 
            playerRow.LastLogin <- Nullable lastLogin
        playerRow.NetRevenue <- Convert.ToDecimal(excelRow.``Net revenue``)
        playerRow.PlayerStatus <- excelRow.``Player status``
        playerRow.RealMoneyBalance <- Convert.ToDecimal(excelRow.``Real money balance``)
        if excelRow.Role <> null then
            playerRow.Role <- excelRow.Role.ToString()
        playerRow.TotalDepositsAmount <- Convert.ToDecimal(excelRow.``Total deposits amount``)
        playerRow.Username <- excelRow.Username
        playerRow.YearMonth <- yearMonthDay.Substring(0, 6)
        playerRow.YearMonthDay <- yearMonthDay
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow

    /// <summary>
    ///
    /// Insert excel row values in db Row but don't touch the id and set the insert time stamp.
    ///
    /// </summary>
    /// <param name="yearMonthDay">The yyyyMMdd mask of the filename</param>
    /// <param name="excelRow">The excel row as the source input</param>
    /// <param name="playerRow">The db row matching the id of the excel row</param>
    /// <returns>Updated row ready for insertion of type dbSchema.ServiceTypes.PlayerRevRpt</returns>
    let insertDbRowValues
        (yearMonthDay : string)
        (excelRow : excelSchema.Row) =

        let newPlayerRow = new dbSchema.ServiceTypes.PlayerRevRpt(
                            UserID = Convert.ToInt32(excelRow.``User ID``)
                            , CreatedOnUtc = DateTime.UtcNow
                            )
        updateDbRowValues yearMonthDay excelRow newPlayerRow

    /// <summary>
    ///
    /// Save excel row values in db Row by updating or inserting a row.
    ///
    /// </summary>
    /// <param name="db">The database db context object to update</param>
    /// <param name="yearMonthDay">The yyyyMMdd mask of the filename</param>
    /// <param name="excelRow">The excel row as the source input</param>
    /// <returns>Unit</returns>
    let SavePlayerRow 
        (db : dbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb) 
        datePart 
        (excelRow : excelSchema.Row) 
        =

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
                if playerRow = null then
                    let newPlayerRow = insertDbRowValues datePart excelRow
                    db.PlayerRevRpt.InsertOnSubmit(newPlayerRow)
                else
                    updateDbRowValues datePart excelRow playerRow |> ignore
                )
            db.DataContext.SubmitChanges()

    /// <summary>
    ///
    /// Get a database object
    ///
    /// </summary>
    /// <param name="dbConnectionString">The database connection string to use</param>
    /// <returns>A newly created database object</returns>
    let getOpenDb connectionString= 
        // Open db
        let db = dbSchema.GetDataContext(connectionString)
        //db.DataContext.Log <- System.Console.Out
        db

    /// <summary>
    ///
    /// Process excel files
    ///
    /// </summary>
    /// <param name="dbConnectionString">The database connection string to use to update the database</param>
    /// <param name="datedExcelFilesNames">The list of dated (filename containing YYYYMMDD in their filename)</param>
    /// <returns>Unit</returns>
    let processExcelFiles dbConnectionString datedExcelFilesNames=
        let db = getOpenDb dbConnectionString
        // Open each excel file
        for excelFileName in datedExcelFilesNames do
            let openFile = new excelSchema(excelFileName)
            let datePart = datePartofFilename excelFileName
            printfn ""
            printfn "************ Processing %s excel file" excelFileName
            printfn ""
            // Loop through all excel rows
            for excelRow in openFile.Data do
                printfn "Processing email %s on %s/%s/%s" excelRow.``Email address`` <| datePart.Substring(6,2) <| datePart.Substring(4,2) <| datePart.Substring(0,4)
                SavePlayerRow db datePart excelRow


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
    let Phase1Processing dbConnectionString inFolder=
    //------------ Read filenames
        printfn "Reading the %s folder" inFolder
        let dirExcelFileList = getDirExcelList inFolder

        if dirExcelFileList.Length > 0 then 
            printfn "Checking read files for date in their name"
        else
            printfn "No excel files were found!"
        let datedExcelFilenames = getDatedExcelfiles dirExcelFileList
    //------------ Save excel files
        processExcelFiles dbConnectionString datedExcelFilenames