module ArgumentsProcessor

open Argu
open DbImport
open GzBatchCommon
open NLog

    type Gm2GzArguments =
        | Balance_Files_Usage of ConfigArgs.BalanceFilesUsageType
        | User_Email_Proc_Alone of ConfigArgs.UserEmailProcOnlyType
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Balance_Files_Usage _ -> "specify \"skipbalancefiles\" for replacing both balance amounts (beg & end) with the custom report balance numbers, \"skipendbalancefile\" for replacing the end balance amounts or \"usebothbalancefiles\" for using both balance files."
                | User_Email_Proc_Alone _ -> "specify an email address of a user to process that alone."
    
    [<Literal>]
    let DefNumOfTradeDays = 22 // Roughly a month
    let logger = LogManager.GetCurrentClassLogger()

    let parseCmdArgs (argv : string[]) : ConfigArgs.CmdArgs =
        try 
            let parser = ArgumentParser.Create<Gm2GzArguments>(programName = "Gm2Gz.exe")
            let results = parser.Parse(argv)
            if not <| results.Contains(<@ Balance_Files_Usage @>) then
                failwith "Please provide a runtime value for the --Balance-Files-Usage argument."
            if results.Contains(<@ User_Email_Proc_Alone @>) then
                let emailAddressToProc = results.GetResult(<@ User_Email_Proc_Alone @>)
                logger.Warn(sprintf "*** Running in exclusive single user processing mode for %s ***" <| emailAddressToProc.ToString())
                { 
                    BalanceFilesUsage = results.GetResult(<@ Balance_Files_Usage @>);
                    UserEmailProcAlone = results.GetResult(<@ User_Email_Proc_Alone @>);
                }
            else
                { 
                    BalanceFilesUsage = results.GetResult(<@ Balance_Files_Usage @>);
                    UserEmailProcAlone = None
                }
                    

        with _ -> 
            reraise()
