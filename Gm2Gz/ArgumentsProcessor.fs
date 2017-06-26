module ArgumentsProcessor

open Argu
open DbImport
open ExcelSchemas

    type Gm2GzArguments =
        | Balance_Files_Usage of BalanceFilesUsageType
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Balance_Files_Usage _ -> "specify \"skipbalancefiles\" for replacing both balance amounts (beg & end) with the custom report balance numbers, \"skipendbalancefile\" for replacing the end balance amounts or \"usebothbalancefiles\" for using both balance files."
    
    [<Literal>]
    let DefNumOfTradeDays = 22 // Roughly a month

    let parseCmdArgs (argv : string[]) : BalanceFilesUsageType =
        try 
            let parser = ArgumentParser.Create<Gm2GzArguments>(programName = "Gm2Gz.exe")
            let results = parser.Parse(argv)
            if not <| results.Contains(<@ Balance_Files_Usage @>) then
                failwith "Please provide a runtime value for the --Balance-Files-Usage argument."
            else
                results.GetResult(<@ Balance_Files_Usage @>)
        with _ -> 
            reraise()
