namespace DbImport

open NLog
open FSharp.Data.TypeProviders

module DbUtil =
    open System

    // Use for compile time memory schema representation
    [<Literal>]
    let CompileTimeDbString = 
        @"Data Source=.\SQLEXPRESS;Initial Catalog=gzDevDb;Persist Security Info=True;Integrated Security=SSPI;"
    
    let logger = LogManager.GetCurrentClassLogger()

//-- Types

    type DbSchema = SqlDataConnection< ConnectionString=CompileTimeDbString >
    type DbContext = DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb
    type DbPlayerRevRptRow = DbSchema.ServiceTypes.PlayerRevRpt

//----Extensions

    /// From (excel's default decimal type) to Db nullable decimals
    let float2NullableDecimal (excelFloatExpr : float) : decimal Nullable =
        excelFloatExpr |> Convert.ToDecimal |> Nullable<decimal> 

    /// Convert string boolean literal to bool Nullable
    let string2NullableBool (excelFloatExpr : string) : bool Nullable = 
        excelFloatExpr |> Convert.ToBoolean |> Nullable<bool> 

    /// Convert DateTime object expression to DateTime Nullable
    let excelObj2NullableDt (excelObjDt : obj) : DateTime System.Nullable = 
        let nullableDate = System.Nullable<DateTime>()
        if not <| isNull excelObjDt then
            match DateTime.TryParseExact(excelObjDt.ToString(), "yyyy-mm-dd", null, Globalization.DateTimeStyles.None) with
                | true, dtRes -> Nullable dtRes
                | false, _ -> nullableDate
        else nullableDate
            
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