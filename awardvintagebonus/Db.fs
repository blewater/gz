module Db

open FSharp.Configuration
open FSharp.Data.TypeProviders
open System
open NLog

let logger = LogManager.GetCurrentClassLogger()

[<Literal>]
let WaitBefRetryinMillis = 500 // milliseconds

//-- Db Types / entities

type DbSchema = DbmlFile<"gzdbdev.dbml", ContextTypeName="GzRunTimeDb">

type DbContext = DbSchema.GzRunTimeDb
type DbInvBalances = DbSchema.InvBalances

type Settings = AppSettings<"App.config">

/// <summary>
///
/// Get a database object by creating a new DataContext and opening a database connection
///
/// </summary>
/// <param name="dbConnectionString">The database connection string to use</param>
/// <returns>A newly created database object</returns>
let getOpenDb (dbConnectionString : string) : DbContext= 

    //logger.Debug("Attempting to open the database connection...")
    let db = new DbSchema.GzRunTimeDb(dbConnectionString)
    //db.DataContext.Log <- System.Console.Out
    db.Connection.Open()
    db

/// Start a Db Transaction
let private startDbTransaction (db : DbContext) = 
    let newTransaction = db.Connection.BeginTransaction()
    db.Transaction <- newTransaction
    newTransaction

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
                logger.Info (sprintf "** retry remaining efforts: %d" times)
            fn()
        with 
        | _ -> System.Threading.Thread.Sleep(WaitBefRetryinMillis); retry (times - 1) fn
    
/// Try a database operation within a transaction 3 times with a delay of 50ms before each commit.
let tryDBCommit3Times (db : DbContext) (dbOperation : (unit -> unit)) : unit =
    let dbTransOperation() = (db, dbOperation) ||> tryDbTransOperation 
    retry 3 dbTransOperation

/// Credit to http://www.fssnip.net/7PJ/title/Way-to-wrap-methods-to-transactions
module Trx=

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


