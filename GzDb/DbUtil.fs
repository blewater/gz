﻿namespace GzDb

module DbUtil =
    open NLog
    open FSharp.Data.TypeProviders
    open System


    // Use for compile time memory schema representation
    [<Literal>]
    let CompileTimeDbString = 
        @"Data Source=.\SQLEXPRESS;Initial Catalog=gzDevDb;Persist Security Info=True;Integrated Security=SSPI;"
    
    let logger = LogManager.GetCurrentClassLogger()

//-- Types

    type DbSchema = SqlDataConnection< ConnectionString=CompileTimeDbString >
    type DbContext = DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb
    type DbPlayerRevRpt = DbSchema.ServiceTypes.PlayerRevRpt
    type DbGzTrx = DbSchema.ServiceTypes.GzTrxs
    type DbFunds = DbSchema.ServiceTypes.Funds
    type DbPortfolios = DbSchema.ServiceTypes.Portfolios
    type DbPortfolioFunds = DbSchema.ServiceTypes.PortFunds
    type DbPortfolioPrices = DbSchema.ServiceTypes.PortfolioPrices
    type DbVintageShares = DbSchema.ServiceTypes.VintageShares
    type DbInvBalances = DbSchema.ServiceTypes.InvBalances
    type DbCustoPortfolios = DbSchema.ServiceTypes.CustPortfolios

    /// PlayerRevRpt update status
    type GmRptProcessStatus =
        | CustomRptUpd = 1
        | BegBalanceRptUpd = 2
        | EndBalanceRptUpd = 3
        | WithdrawsRptUpd = 4
        | GainLossRptUpd = 5
            
    type GzTransactionType =
        /// <summary>
        /// 
        /// "Reserved for future use. Customer withdrawal from their greenzorro account 
        /// to their banking account." 
        /// Generate fees transactions: FundFee, Commission (4%)
        /// 
        /// </summary>
        | InvWithdrawal = 1

        /// <summary>
        /// 
        /// Sell portfolio shares and transfer cash to their casino account.
        /// 
        /// </summary>
        | TransferToGaming = 2

        /// <summary>
        /// 
        /// Liquidate all customer's funds to cash. 
        /// Typical Transaction when closing a customer's account.
        /// 
        /// </summary>
        | FullCustomerFundsLiquidation = 3

        /// <summary>
        /// 
        /// The %50 pcnt changes we credit to the players investment account.
        /// the whole amount "Playing Loss"
        /// <see cref="GmTransactionTypeEnum.PlayingLoss"/>
        /// 
        /// </summary>
        | CreditedPlayingLoss = 4

        /// <summary>
        /// 
        /// Fund fees: 2.5%
        /// Deducted from the customer investment when withdrawing cash.
        /// 
        /// </summary>
        | FundFee = 5

        /// <summary>
        /// 
        /// greenzorro fees: 1.5%
        /// Deducted from the customer investment when withdrawing cash.
        /// 
        /// </summary>
        | GzFees = 6

        /// <summary>
        /// 
        /// The realized greenzorro profit or loss by the actual purchase of customer shares 
        /// after the month's end.
        /// It is the total difference between 
        /// 
        /// "Bought Funds Prices" * "Monthly Customer Shares"
        ///         - 
        /// "Customer Shares" * "Funds Prices" credited to the customer's Account
        /// 
        /// </summary>
        | GzActualTrxProfitOrLoss = 7

    /// <summary>
    ///
    /// Get a database object by creating a new DataContext and opening a database connection
    ///
    /// </summary>
    /// <param name="dbConnectionString">The database connection string to use</param>
    /// <returns>A newly created database object</returns>
    let getOpenDb (dbConnectionString : string) : DbContext= 

        logger.Debug("Attempting to open the database connection...")
        // Open db
        let db = DbSchema.GetDataContext(dbConnectionString)
        //db.DataContext.Log <- System.Console.Out
        db.Connection.Open()
        db

    /// Start a Db Transaction
    let private startDbTransaction (db : DbContext) = 
        let transaction = db.Connection.BeginTransaction()
        db.DataContext.Transaction <- transaction
        transaction

    /// Commit a transaction
    let private commitTransaction (transaction : Data.Common.DbTransaction) = 
            // ********* Commit once per excel File
            transaction.Commit()

    /// Rollback transaction and raise exception
    let private handleFailure (transaction : Data.Common.DbTransaction) (ex : exn) = 
        transaction.Rollback()
        logger.Fatal(ex, "Runtime Exception at main")

    /// Enclose db operation within a transaction
    let tryDbTransOperation (db : DbContext) (dbOperation : (unit -> unit)) : unit =
        let transaction = startDbTransaction db
        try

            dbOperation()

            commitTransaction transaction

        with ex ->
            handleFailure transaction ex
            reraise ()

