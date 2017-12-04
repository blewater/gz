namespace GzBalances

module WithdrawnVintageBonusGen =
    open System
    open System.IO
    open FSharp.Data
    open NLog
    open GzDb.DbUtil
    open GzBatchCommon
    open ConfigArgs

    type VintageDeposits = CsvProvider<"everymatrix_deposits.csv", ";", HasHeaders=false>

    let logger = LogManager.GetCurrentClassLogger()

    /// get the invBalance row of the desired month in the format of yyyyMm
    let private withdrawnVintages (db : DbContext) = 
        query {
            for invb in db.InvBalances do
            join u in db.AspNetUsers 
                on (invb.CustomerId = u.Id)
            where (
                invb.Sold
                && invb.AwardedSoldAmount = false
            )
            sortBy u.UserName
            thenBy invb.YearMonth
            select (invb, u)
        }

    /// Send email to admins of the csv content
    let private sendCashBonusCsvToAdmins
            (db : DbContext)
            (adminEmailUserArgs : EverymatrixPortalArgsType)
            (folder : ReportsFoldersType)
            (dayToProcess : DateTime)
            (depositsCsvStrContent : string) : unit = 

        if depositsCsvStrContent.Length > 0 then
        
            logger.Info "*** Generating csv to email bonuses ***"

            let yyyyMmDd = dayToProcess.ToYyyyMmDd
            let drive = Path.GetPathRoot  __SOURCE_DIRECTORY__
            let depositsCsvFilename = Path.Combine(drive, folder.BaseFolder, folder.ExcelOutFolder, sprintf "Vintage Deposits for %s.csv" yyyyMmDd)
            File.WriteAllText(depositsCsvFilename, depositsCsvStrContent)

            // Email deposits
            let gmailClient = EmailAccess(dayToProcess, adminEmailUserArgs.EmailReportsUser, adminEmailUserArgs.EmailReportsPwd)
            gmailClient.SendWithdrawnVintagesCashBonusCsv depositsCsvStrContent depositsCsvFilename dayToProcess

            db.DataContext.SubmitChanges()
            logger.Debug "*** Emailed bonuses csv ***"

    /// award deposit bonus for withdrawn vintages
    let updDbRewSoldVintages
            (adminEmailUserArgs : EverymatrixPortalArgsType)
            (folder : ReportsFoldersType)
            (db : DbContext)
            // Contrary to other dayToProcess this the current day
            (dayToProcess : DateTime) =

        logger.Info "*** Checking if there are pending withdrawn vintages ***"
        use depositsInMem = new StringWriter()

        db 
        |> withdrawnVintages
        |> Seq.iter (fun (invb, u) -> 
            let depLine = sprintf "%d;CasinoWallet;%s;%M;vintage month %s cash for U:%s L:%s F:%s" u.GmCustomerId.Value u.Currency (Math.Round(invb.SoldAmount.Value - invb.SoldFees.Value, 1)) invb.YearMonth u.UserName u.LastName u.FirstName

            depositsInMem.WriteLine(depLine)
            logger.Warn (depLine)

            invb.AwardedSoldAmount <- true
        )
        let depositsCsvStrContent = depositsInMem.ToString()
        depositsInMem.Close()

        if depositsCsvStrContent.Length = 0 then
            logger.Info "No withdrawn vintages today."
