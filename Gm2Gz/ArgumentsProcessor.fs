module ArgumentsProcessor

open Argu

    /// Any value of other than 0 or 1 proceeds with the remaining processing
    type HandleShares =
        | GetShares of int // 0 or 1 only
        | StoreOnlyShares of int // Values of >= 2

    type Gm2GzArguments =
        | Get_Shares of days:int
        | Store_Only_Shares of days:int
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Get_Shares _ -> "specify number of days to store and continue processing 0 - 300."
                | Store_Only_Shares _ -> "specify number of days to store shares 1 - 300 and halt further processing."
    
    [<Literal>]
    let DefNumOfTradeDays = 22 // Roughly a month

    let parseCmdArgs (argv : string[]) : HandleShares=
        try 
            let parseDays (d :int)  = 
                if d < 0 || d > 300 then 
                    failwith (sprintf "invalid days [0-300] number. Instead given %d." d)
                else d
            let parser = ArgumentParser.Create<Gm2GzArguments>(programName = "Gm2Gz.exe")
            let results = parser.Parse(argv)
            if results.Contains(<@ Get_Shares @>) then
                results.GetResult(<@ Get_Shares @>) 
                |> parseDays
                |> GetShares
            elif results.Contains(<@ Store_Only_Shares @>) then
                results.GetResult(<@ Store_Only_Shares @>) 
                |> parseDays
                |> StoreOnlyShares
            else
                GetShares(DefNumOfTradeDays)
        with _ -> 
            reraise()
