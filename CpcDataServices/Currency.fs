namespace CpcDataServices

open FSharp.Data
open System.Collections.Generic

module public CurrencyRates = 
    type UsdRates = JsonProvider<"../CpcDataServices/openexchangerates.json">

    /// <summary>
    ///
    /// httpGet currency rates based on the UsdRates type
    /// 
    /// </summary>
    /// <param name="currencyApiUrl"></param>
    /// <return>dict of rates</return>
    let getCurrencyRates (currencyApiUrl : string) : IDictionary<string, decimal> = 
        let usdRateTo = UsdRates.Load(currencyApiUrl)
        let usdRates = dict [ 
                            "USDAUD", usdRateTo.Rates.Aud
                            ; "USDCHF", usdRateTo.Rates.Chf
                            ; "USDDKK", usdRateTo.Rates.Dkk
                            ; "USDEUR", usdRateTo.Rates.Eur
                            ; "USDGBP", usdRateTo.Rates.Gbp
                            ; "USDNOK", usdRateTo.Rates.Nok
                            ; "USDSEK", usdRateTo.Rates.Sek
                            ; "AUDUSD", 1m/usdRateTo.Rates.Aud
                            ; "CHFUSD", 1m/usdRateTo.Rates.Chf
                            ; "DKKUSD", 1m/usdRateTo.Rates.Dkk
                            ; "EURUSD", 1m/usdRateTo.Rates.Eur
                            ; "GBPUSD", 1m/usdRateTo.Rates.Gbp
                            ; "NOKUSD", 1m/usdRateTo.Rates.Nok
                            ; "SEKUSD", 1m/usdRateTo.Rates.Sek]
        usdRates
    
    let setDbRates (usdRates : IDictionary<string, decimal>) = 
        usdRates
