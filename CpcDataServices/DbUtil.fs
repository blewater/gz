namespace CpcDataServices

open FSharp.Data.TypeProviders

module DbUtil =

    // Use for compile time memory schema representation
    [<Literal>]
    let CompileTimeDbString = 
        @"Data Source=.\SQLEXPRESS;Initial Catalog=gzDevDb;Persist Security Info=True;Integrated Security=SSPI;"
    
    type DbSchema = SqlDataConnection< ConnectionString=CompileTimeDbString >

    /// <summary>
    ///
    /// Get a database object by creating a new DataContext and opening a database connection
    ///
    /// </summary>
    /// <param name="dbConnectionString">The database connection string to use</param>
    /// <returns>A newly created database object</returns>
    let getOpenDb (dbConnectionString : string) : DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb= 
        // Open db
        let db = DbSchema.GetDataContext(dbConnectionString)
        //db.DataContext.Log <- System.Console.Out
        db.Connection.Open()
        db