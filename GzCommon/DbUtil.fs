namespace GzDb

[<AutoOpen>]
module DbUtil =
    open NLog
    open FSharp.Data.TypeProviders
    open System

    [<Literal>]
    let WaitBefRetryinMillis = 500 // milliseconds

    // Use for compile time memory schema representation
    [<Literal>]
    // let CompileTimeDbString = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=gzDbDev;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True"
    let CompileTimeDbString = "Server=tcp:gzdbdev.database.windows.net,1433;Database=gzDbDev;User ID=gzDevReader;Password=Life is good wout writing8!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    let logger = LogManager.GetCurrentClassLogger()

//-- Types

    type DbSchema = SqlDataConnection<ConnectionString=CompileTimeDbString >
    type DbContext = DbSchema.ServiceTypes.SimpleDataContextTypes.GzDbDev
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
        | DepositsUpd = 2
        | BegBalanceRptUpd = 3
        | EndBalanceRptUpd = 4
        | WithdrawsRptUpd = 5
        | GainLossRptUpd = 6
            
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
        logger.Fatal(ex, "Database Runtime Exception:")

    /// Enclose db operation within a transaction
    let tryDbTransOperation (db : DbContext) (dbOperation : (unit -> unit)) : unit =
        let transaction = startDbTransaction db
        try

            dbOperation()

            commitTransaction transaction

        with ex ->
            handleFailure transaction ex
            reraise ()

    /// retry x times a function (fn)
    let rec retry times fn = 
        if times > 0 then
            try
                if times < 3 then
                    logger.Info (sprintf "** DbUtil remaining efforts: %d" times)
                fn()
            with 
            | _ -> System.Threading.Thread.Sleep(WaitBefRetryinMillis); retry (times - 1) fn
        else
            fn()
    
    /// Try a database operation within a transaction 3 times with a delay of 50ms before each commit.
    let tryDBCommit3Times (db : DbContext) (dbOperation : (unit -> unit)) : unit =
        let dbTransOperation() = (db, dbOperation) ||> tryDbTransOperation 
        retry 3 dbTransOperation

/// Credit to http://www.fssnip.net/7PJ/title/Way-to-wrap-methods-to-transactions
module Trx =
    open System
    open System.Threading
    open System.Threading.Tasks
    open System.Transactions

    let logmsg act threadid transId =
        let tidm = match String.IsNullOrEmpty threadid with | true -> "" | false -> " at thread " + threadid
        let msg = "Transaction " + transId + " " + act + tidm
        Console.WriteLine msg
        //Logary.Logger.log (Logary.Logging.getCurrentLogger()) (Logary.Message.eventDebug msg) |> start

    let getTransactionId() =
        if not <| isNull Transaction.Current then
            Transaction.Current.TransactionInformation.LocalIdentifier
        else
            ""

    let transactionWithManualComplete<'T>() =
        if not <| isNull (Type.GetType ("Mono.Runtime")) then
            new Transactions.TransactionScope()
        else
            // Mono would fail to compilation, so we have to construct this via reflection:
            // new Transactions.TransactionScope(Transactions.TransactionScopeAsyncFlowOption.Enabled)
            let transactionAssembly = System.Reflection.Assembly.GetAssembly typeof<TransactionScope>
            let asynctype = transactionAssembly.GetType "System.Transactions.TransactionScopeAsyncFlowOption"
            let transaction = typeof<TransactionScope>.GetConstructor [|asynctype|]
            transaction.Invoke [|1|] :?> TransactionScope

    let transactionWith<'T> (func: unit -> 'T) =
        use scope = transactionWithManualComplete()
        let transId = getTransactionId()
        //logmsg "started" Thread.CurrentThread.Name transId
        let res = func()
        match box res with
        | :? Task as task -> 
            let commit = Action<Task>(fun a -> 
                //logmsg "completed" Thread.CurrentThread.Name transId
                scope.Complete()
                )
            let commitTran1 = task.ContinueWith(commit, TaskContinuationOptions.OnlyOnRanToCompletion)
            let commitTran2 = task.ContinueWith((fun _ -> 
                logmsg "failed" Thread.CurrentThread.Name transId), TaskContinuationOptions.NotOnRanToCompletion)
            res
        | item when not <| isNull item && item.GetType().Name = "FSharpAsync`1" ->
            let msg = "Use transactionWithAsync"
            //Logary.Logger.log (Logary.Logging.getCurrentLogger()) (Logary.Message.eventError msg) |> start
            failwith msg 
        | _ -> 
            //logmsg "completed" System.Threading.Thread.CurrentThread.Name transId
            scope.Complete()
            res

    let transactionWithAsync<'T> (func: unit -> Async<'T>) =
        async {
            use scope = transactionWithManualComplete()
            let transId = getTransactionId()
            logmsg "started" Thread.CurrentThread.Name transId
            let! res = func()
            logmsg "completed" Thread.CurrentThread.Name transId
            scope.Complete()
            return res
        }

    (*
    let ``example usage`` =
        transactionWith <| fun () -> 
            Console.WriteLine "Normal source code in transaction"
            Console.WriteLine "e.g. database operations!"
            Console.WriteLine "Return values and "
            Console.WriteLine "C# Task classes supported."

    let ``example usage FSharpAsync`` =
        transactionWithAsync <| fun () -> async {
            Console.WriteLine "Async source code in transaction"
            return "hello!"
        }
    exampleusageFSharpAsync |> Async.RunSynchronously;;
    *)
