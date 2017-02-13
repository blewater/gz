module ArgumentsProcessor

open Argu

type HandleShares =
    | Store of int
    | Skip
    | OneDay

type Gm2GzArguments =
    | [<First>] Store_Shares of days:int
    | [<First>] Skip_Shares
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
                | Store_Shares _ -> "specify number of days to store shares 1 - 300."
                | Skip_Shares _ -> "specify number of days to store shares 1 - 300."
    

let parseCmdArgs (argv : string[]) : HandleShares=

    let parseDays (d :int)  = 
        if d < 1 || d > 300 then 
            failwith (sprintf "invalid days [1-300] number. Instead given %d." d)
        else d

    let getDays =
        function 
        | Some d -> d
        | None -> 1

    let parser = ArgumentParser.Create<Gm2GzArguments>(programName = "Gm2Gz.exe")
    let results = parser.Parse(argv)
    let skipShares = results.Contains(<@ Skip_Shares @>)
    if skipShares then
        Skip
    else 
        Store(
            results.TryGetResult(<@ Store_Shares @>) 
            |> getDays 
            |> parseDays
        )