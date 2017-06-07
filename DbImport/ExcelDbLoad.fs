﻿namespace DbImport

module DbGzTrx =
    open System
    open NLog
    open GzDb.DbUtil

    let logger = LogManager.GetCurrentClassLogger()

    /// Update a trx row with PlayerRevRpt as the source
    let updDbGzTrxRowValues 
            (amount : decimal)
            (creditPcntApplied : float32)
            (playerRevRpt : DbPlayerRevRpt)
            (trxRow : DbGzTrx) =

        trxRow.PlayerRevRptId <- Nullable playerRevRpt.Id
        trxRow.Amount <- amount
        trxRow.CreditPcntApplied <- Nullable creditPcntApplied
        trxRow.BegGmBalance <- playerRevRpt.BegGmBalance
        trxRow.EndGmBalance <- playerRevRpt.EndGmBalance
        trxRow.Deposits <- playerRevRpt.TotalDepositsAmount
        trxRow.Withdrawals <- Nullable <| playerRevRpt.WithdrawsMade.Value + playerRevRpt.PendingWithdrawals.Value
        trxRow.GmGainLoss <- playerRevRpt.GmGainLoss

    /// Create & Insert a GzTrxs row
    let insDbGzTrxRowValues
            (db : DbContext) 
            (yearMonth : string)
            (amount : decimal)
            (creditPcntApplied : float32)
            (gzUserId : int)
            (playerRevRpt : DbPlayerRevRpt) =

        let newGzTrxRow = 
            new DbGzTrx(
                CustomerId=gzUserId,
                YearMonthCtd = yearMonth,
                CreatedOnUTC = DateTime.UtcNow,
                TypeId = int GzTransactionType.CreditedPlayingLoss)

        updDbGzTrxRowValues amount creditPcntApplied playerRevRpt newGzTrxRow
        db.GzTrxs.InsertOnSubmit(newGzTrxRow)

    /// Get the greenzorro used id by email
    let getGzUserId (db : DbContext) (gmUserEmail : string) : int option =
        query {
            for user in db.AspNetUsers do
            where (user.Email = gmUserEmail)
            select user.Id
            exactlyOneOrDefault 
        }
        |> (fun userId ->
            if userId = 0 then
                logger.Warn (sprintf "*** Everymatrix email %s not found in the AspNetUsers table of db: %s. Cannot award..." gmUserEmail db.DataContext.Connection.DataSource)
                None
            else
                Some userId
        )

    /// Get the credited loss Percentage in human form % i.e. 50 from gzConfiguration
    /// <param name="db"></param>
    let getCreditLossPcnt (db : DbContext) : float32 =
        query {
            for c in db.GzConfigurations do
            exactlyOne
        }
        |> (fun conf -> conf.CREDIT_LOSS_PCNT)

    /// Main formula calculating the amount that will be credited to the users account
    /// (**** Player Loss in GzTrx is a positive amount. No point with negatives in there. ***)
    let getCreditedPlayerAmount (creditLossPcnt : float32)
                                (playerGainLoss : decimal) : decimal =

        let (|Gain|Loss|) (amount : decimal) = if amount >= 0M then Gain 0M else Loss amount
        match playerGainLoss with
        | Gain _ -> 0M
        | Loss lossAmount -> (decimal creditLossPcnt / 100m) * -lossAmount
    
    /// Upsert a GzTrxs transaction row with the credited amount: 1 user row per month
    let private setDbGzTrxRow(db : DbContext)(yyyyMmDd :string)(playerRevRpt : DbPlayerRevRpt) =

        let playerGainLoss = playerRevRpt.GmGainLoss.Value
        let yyyyMm = yyyyMmDd.Substring(0, 6) 
        let gmEmail = playerRevRpt.EmailAddress
        let gzUserId = getGzUserId db gmEmail

        if gzUserId.IsSome then
            query { 
                for trxRow in db.GzTrxs do
                    where (
                        trxRow.YearMonthCtd = yyyyMm
                        && trxRow.CustomerId = gzUserId.Value
                        && trxRow.GzTrxTypes.Code = int GzTransactionType.CreditedPlayingLoss
                    )
                    select trxRow
                    exactlyOneOrDefault
            }
            |> (fun trxRow ->
                let creditLossPcnt = getCreditLossPcnt db 
                let playerLossToInvest = getCreditedPlayerAmount creditLossPcnt playerGainLoss

                if isNull trxRow then
                    insDbGzTrxRowValues db yyyyMm playerLossToInvest creditLossPcnt gzUserId.Value playerRevRpt
                else 
                    updDbGzTrxRowValues playerLossToInvest creditLossPcnt playerRevRpt trxRow
            )
            db.DataContext.SubmitChanges()

    /// Read all the playerRevRpt latest monthly row and Upsert them as monthly GzTrxs transaction rows
    let setDbPlayerRevRpt2GzTrx (db : DbContext)(yyyyMmDd : string) =

        let yyyyMm = yyyyMmDd.Substring(0, 6)
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonth = yyyyMm)
                select playerDbRow
        }
        |> Seq.iter (fun playerDbRow -> setDbGzTrxRow db yyyyMmDd playerDbRow)

module DbPlayerRevRpt =
    open System
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas
    open GzCommon
    open ExcelUtil

    let logger = LogManager.GetCurrentClassLogger()

    /// Updated db player Gain Loss
    let private setDbPlayerGainLoss (row : DbSchema.ServiceTypes.PlayerRevRpt) : unit =
        let totalWithdrawals = row.WithdrawsMade.Value + row.PendingWithdrawals.Value
        // Formula to get player losses as negative amounts
        let gainLoss = 
            row.EndGmBalance.Value
            + totalWithdrawals
            - row.TotalDepositsAmount.Value 
            - row.Vendor2UserDeposits.Value
            - row.BegGmBalance.Value 
        row.GmGainLoss <- Nullable gainLoss
        row.UpdatedOnUtc <- DateTime.UtcNow
        row.Processed <- int GmRptProcessStatus.GainLossRptUpd

    /// Query non zero balance affecting amounts and update the player GainLoss
    let setDbMonthyGainLossAmounts
                        (db : DbContext)
                        (yyyyMmDd :string) =

        let yyyyMm = yyyyMmDd.Substring(0, 6)
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonth = yyyyMm
                        && (
                            playerDbRow.BegGmBalance <> Nullable 0M 
                            || playerDbRow.EndGmBalance <> Nullable 0M
                            || playerDbRow.TotalDepositsAmount <> Nullable 0M
                            || playerDbRow.Vendor2UserDeposits <> Nullable 0m
                            // Withdrawals that deduct balance but have not completed yet
                            || playerDbRow.PendingWithdrawals <> Nullable 0M
                            // Completed Withdrawals like TotalDepositsAmount is for Deposits
                            || playerDbRow.WithdrawsMade <> Nullable 0M
                        ))
                select playerDbRow
        }
        |> Seq.iter setDbPlayerGainLoss
        db.DataContext.SubmitChanges()

    /// Update the vendor2User amounts
    let private updDbRowVendor2UserValues 
                    (vendor2UserAmountType : Vendor2UserAmountType)
                    (vendor2UserExcelRow : Vendor2UserExcelSchema.Row) 
                    (playerRow : DbPlayerRevRpt) = 

        let dbVendor2UserCashBonusAmount = playerRow.CashBonusAmount.Value
        let dbVendor2UserDepositsAmount = playerRow.Vendor2UserDeposits.Value
        match vendor2UserAmountType with
        | V2UDeposit ->
            let newV2UDepositAmount = dbVendor2UserDepositsAmount + decimal vendor2UserExcelRow.``Debit real amount``
            playerRow.Vendor2UserDeposits <- Nullable newV2UDepositAmount
        | V2UCashBonus ->
            let newV2UCashBonusAmount = dbVendor2UserCashBonusAmount + decimal vendor2UserExcelRow.``Debit real amount``
            playerRow.CashBonusAmount <- Nullable newV2UCashBonusAmount
        //Non-excel content
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.Vendor2UserUpd

    /// Update the withdrawal amount with an addition for pending withdrawal amounts or deducting rollback withdrawal amounts
    /// Note: they may be multiple pending / rollback withdrawal transactions per user/month
    let private updDbRowWithdrawalsValues 
                    (withdrawalType : WithdrawalType) 
                    (withdrawalsExcelRow : WithdrawalsPendingExcelSchema.Row) 
                    (playerRow : DbPlayerRevRpt) = 

        let dbWithdrawalAmount = playerRow.PendingWithdrawals.Value
        if withdrawalType = Pending then
            let newWithdrawalAmount = dbWithdrawalAmount + decimal withdrawalsExcelRow.``Debit real amount``
            playerRow.PendingWithdrawals <- Nullable newWithdrawalAmount
        else
            let newWithdrawalAmount = dbWithdrawalAmount - decimal withdrawalsExcelRow.``Debit real amount``
            playerRow.PendingWithdrawals <- Nullable newWithdrawalAmount
        //Non-excel content
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.WithdrawsRptUpd

    /// Update begining balance amount of the selected month
    let private updDbRowBegBalanceValues 
            (begBalanceExcelRow : BalanceExcelSchema.Row) 
            (playerRow : DbPlayerRevRpt) = 

        playerRow.BegGmBalance <- begBalanceExcelRow.``Account balance`` |> float2NullableDecimal
        //Non-excel content
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.BegBalanceRptUpd
    
    /// Update ending balance amount of the selected month
    let private updDbRowEndBalanceValues 
                (endBalanceExcelRow : BalanceExcelSchema.Row) (playerRow : DbPlayerRevRpt) = 

        // Zero out balance amounts, playerloss
        playerRow.EndGmBalance <- endBalanceExcelRow.``Account balance`` |> float2NullableDecimal
        //Non-excel content
        playerRow.Processed <- int GmRptProcessStatus.EndBalanceRptUpd
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
    
    /// Update most PlayerRevRpt row values from the CustomRpt values but without touching the id or insert time stamp.
    let private setDbRowCustomValues 
            (yearMonthDay : string) 
            (customExcelRow : CustomExcelSchema.Row) 
            (playerRow : DbPlayerRevRpt) : unit =

        playerRow.Username <- customExcelRow.Username
        playerRow.PlayerStatus <- customExcelRow.``Player status``
        if not <| isNull customExcelRow.``Block reason`` then playerRow.BlockReason <- customExcelRow.``Block reason``.ToString()
        playerRow.EmailAddress <- customExcelRow.``Email address``
        playerRow.TotalDepositsAmount <- customExcelRow.``Total deposits amount`` |> float2NullableDecimal
        playerRow.WithdrawsMade <- customExcelRow.``Withdraws made`` |> float2NullableDecimal
        playerRow.Currency <- customExcelRow.Currency.ToString()

        // Zero out gaming deposit cashBonus
        playerRow.Vendor2UserDeposits <- Nullable 0m
        playerRow.CashBonusAmount <- Nullable 0m

        // Zero out gaming balance amounts, playerloss
        playerRow.BegGmBalance <- Nullable 0m
        playerRow.GmGainLoss <- Nullable 0m

        // Zero this now in case the subsequent balance file misses an entry for this player due to 0 balance
        playerRow.EndGmBalance <- Nullable 0m

        // Withdrawals that deduct balance but have not completed yet they come from the pending report
        playerRow.PendingWithdrawals <- Nullable 0m

        playerRow.Mobile <- customExcelRow.Mobile
        playerRow.Phone <- customExcelRow.Phone |> excelObjNullableString
        playerRow.City <- customExcelRow.City
        playerRow.Country <- customExcelRow.Country

        //Non-excel content
        playerRow.YearMonth <- yearMonthDay.Substring(0, 6)
        playerRow.YearMonthDay <- yearMonthDay
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.CustomRptUpd
    
    /// Insert custom excel row values in db Row but don't touch the id and set the createdOnUtc time stamp.
    let insDbNewRowCustomValues
            (db : DbContext) 
            (yearMonthDay : string) 
            (excelRow : CustomExcelSchema.Row) : unit =

        let newPlayerRow = 
            new DbPlayerRevRpt(UserID = (int) excelRow.``User ID``, CreatedOnUtc = DateTime.UtcNow)
        setDbRowCustomValues yearMonthDay excelRow newPlayerRow
        db.PlayerRevRpt.InsertOnSubmit(newPlayerRow)

    /// Set withdrawal amount in a db PlayerRevRpt Row
    let updDbWithdrawalsPlayerRow 
                        (withdrawalType : WithdrawalType)
                        (db : DbContext)
                        (yyyyMmDd :string)
                        (withdrawalRow : WithdrawalsPendingExcelSchema.Row) =

        let gmUserId = (int) withdrawalRow.UserID
        let yyyyMm = yyyyMmDd.Substring(0, 4)
        logger.Info(sprintf "Importing %A withdrawal user id %d on %s/%s/%s" 
                withdrawalType <| gmUserId <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonth = yyyyMm && playerDbRow.UserID = gmUserId)
                select playerDbRow
                exactlyOneOrDefault
        }
        |> (fun playerDbRow -> 
            if isNull playerDbRow then 
                let warningMsg = sprintf "Couldn't find user Id %d from %A withdrawals excel in the PlayerRevRpt table." <| gmUserId <| withdrawalType
                logger.Warn warningMsg
            else
                updDbRowWithdrawalsValues withdrawalType withdrawalRow playerDbRow
        )
        db.DataContext.SubmitChanges()

    /// Set beginning or ending amount in a db PlayerRevRpt Row
    let updDbBalances 
            (balanceType : BalanceType)
            (db : DbContext)
            (yyyyMmDd :string) 
            (balanceRow : BalanceExcelSchema.Row) = 

        let gmUserId = (int) balanceRow.``User ID``
        let yyyyMm = yyyyMmDd.Substring(0, 6)
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonth = yyyyMm && playerDbRow.UserID = gmUserId)
                select playerDbRow
                exactlyOneOrDefault
        }
        |> (fun playerDbRow -> 
            if isNull playerDbRow then 
                let failMsg = sprintf "When importing a balance file you can't have a user Id %d that is not found in the PlayerRevRpt table!" gmUserId
                failwith failMsg
            else
                match balanceType with
                | BeginingBalance -> updDbRowBegBalanceValues balanceRow playerDbRow
                | EndingBalance -> updDbRowEndBalanceValues balanceRow playerDbRow
        )
        db.DataContext.SubmitChanges()

    /// Set Vendor2User deposit Cash bonus amount in a db PlayerRevRpt Row
    let updDbVendor2UserPlayerRow 
                        (vendor2UserAmountType : Vendor2UserAmountType)
                        (db : DbContext)
                        (yyyyMmDd :string)
                        (vendor2UserRow : Vendor2UserExcelSchema.Row) =

        let gmUserId = (int) vendor2UserRow.UserID
        let yyyyMm = yyyyMmDd.Substring(0, 4)
        logger.Info(sprintf "Importing vendor2User user id %d on %s/%s/%s" 
                gmUserId <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonth = yyyyMm && playerDbRow.UserID = gmUserId)
                select playerDbRow
                exactlyOneOrDefault
        }
        |> (fun playerDbRow -> 
            if isNull playerDbRow then 
                let warningMsg = sprintf "Couldn't find user Id %d from vendor2User excel in the PlayerRevRpt table." gmUserId
                logger.Warn warningMsg
            else
                updDbRowVendor2UserValues vendor2UserAmountType vendor2UserRow playerDbRow
        )
        db.DataContext.SubmitChanges()

    /// Upsert excel row values in a db PlayerRevRpt Row
    let setDbCustomPlayerRow (db : DbContext) 
                        (yyyyMmDd :string) 
                        (customExcelRow : CustomExcelSchema.Row) : unit =

        let gmUserId = (int) customExcelRow.``User ID``
        let yyyyMm = yyyyMmDd.Substring(0, 6)
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonth = yyyyMm && playerDbRow.UserID = gmUserId)
                select playerDbRow
                exactlyOneOrDefault
        }
        |> (fun playerDbRow -> 
            if isNull playerDbRow then 
                insDbNewRowCustomValues db yyyyMmDd customExcelRow
            else 
                setDbRowCustomValues yyyyMmDd customExcelRow playerDbRow
        )
        db.DataContext.SubmitChanges()