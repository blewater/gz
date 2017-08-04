namespace GzBalances

module WithdrawnVintageBonusGen =
    open System
    open System.IO
    open FSharp.Data
    open NLog
    open GzDb.DbUtil
    open GzBatchCommon
    open PortfolioTypes
    open System.Net.Mail
    open System.Net
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
                && not <| invb.AwardedSoldAmount
            )
            select (invb, u)
        }

    /// award deposit bonus for withdrawn vintages
    let updDbRewSoldVintages
            (adminEmailUserArgs : EverymatrixPortalArgsType)
            (folder : ReportsFoldersType)
            (db : DbContext)
            (dayToProcess : DateTime) =

        use depositsInMem = new StringWriter()
        let depositsCsvFilename = Path.Combine(folder.BaseFolder, folder.ExcelInFolder, "Vintage Deposits for yyyyMmDd.csv")
        use depositsCsv = new StreamWriter(depositsCsvFilename)
        let yyyyMmDd = dayToProcess.ToYyyyMmDd

        db 
        |> withdrawnVintages
        |> Seq.sortBy (fun (i, u) -> i.CustomerId, i.SoldYearMonth)
        |> Seq.iter (fun (invb, u) -> 
            let depLine = sprintf "%d,CasinoWallet;%s;%M;vintage month %s cash for %s" u.GmCustomerId.Value u.Currency (invb.SoldAmount.Value - invb.SoldFees.Value) invb.YearMonth u.Email

            depositsInMem.WriteLine(depLine)
            depositsCsv.WriteLine(depLine)
            logger.Info (depLine)

            invb.AwardedSoldAmount <- true
        )
        depositsCsv.Close()
        
        // Email deposits
        let gmailClient = EmailAccess(dayToProcess, adminEmailUserArgs.EmailReportsUser, adminEmailUserArgs.EmailReportsPwd)
        (depositsInMem.ToString(), depositsCsvFilename) 
            ||> gmailClient.SendWithdrawnVintagesCashBonusCsv

        db.DataContext.SubmitChanges()