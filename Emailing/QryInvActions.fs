module QryInvActions

    open System
    open NLog
    open GzDb.DbUtil

    let logger = LogManager.GetCurrentClassLogger()

    let getDbLastInvWithdrawalDate(db : DbContext)(gmUserId : int Nullable) =

        query { 
            for user in db.AspNetUsers do
            join invB in db.InvBalances
                on (user.Id = invB.CustomerId)
            where (user.GmCustomerId = gmUserId && invB.SoldOnUtc.HasValue)
            sortByNullableDescending invB.SoldOnUtc
            select (invB.SoldOnUtc)
            take 1
            exactlyOneOrDefault
        }
