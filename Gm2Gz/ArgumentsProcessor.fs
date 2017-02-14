module ArgumentsProcessor

open Argu

    /// Any value of other than 0 or 1 proceeds with the remaining processing
    type HandleShares =
        | Store of int // 0 or 1 only
        | StoreOnlyAction of int // Values of >= 2

    type Gm2GzArguments =
        | Store_Shares of days:int
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Store_Shares _ -> "specify number of days to store shares 0 - 300."
    
    let parseCmdArgs (argv : string[]) : HandleShares=
        let getDays =
            function
            | Some d -> d
            | None -> 1
        let parseDays (d :int)  = 
            if d < 0 || d > 300 then 
                failwith (sprintf "invalid days [1-300] number. Instead given %d." d)
            else d
        let parser = ArgumentParser.Create<Gm2GzArguments>(programName = "Gm2Gz.exe")
        let results = parser.Parse(argv)
        if results.Contains(<@ Store_Shares @>) then
            results.TryGetResult(<@ Store_Shares @>) 
            |> getDays
            |> parseDays
            |> function
                | 0 -> Store(0)
                | 1 -> Store(1)
                | days -> StoreOnlyAction(days)
        else Store(1)