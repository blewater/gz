namespace CpcDataServices

open System
open FSharp.Data
open System.Collections.Generic

module public CurrencyRates = 
    type UsdRates = JsonProvider<"../CpcDataServices/openexchangerates.json">

    /// <summary>
    ///
    /// httpGet currency rates based on the UsdRates type
    /// -> Map of rates
    /// 
    /// </summary>
    /// <param name="currencyApiUrl"></param>
    let getCurrencyRates (currencyApiUrl : string) : Map<string, (decimal * int64)> = 
        let usdRateTo = UsdRates.Load(currencyApiUrl)
        let usdRates = [
                        "USDAUD", (usdRateTo.Rates.Aud, int64 usdRateTo.Timestamp) ;
                        "USDCHF", (usdRateTo.Rates.Chf, int64 usdRateTo.Timestamp) ;
                        "USDDKK", (usdRateTo.Rates.Dkk, int64 usdRateTo.Timestamp) ;
                        "USDEUR", (usdRateTo.Rates.Eur, int64 usdRateTo.Timestamp) ;
                        "USDGBP", (usdRateTo.Rates.Gbp, int64 usdRateTo.Timestamp) ;
                        "USDNOK", (usdRateTo.Rates.Nok, int64 usdRateTo.Timestamp) ;
                        "USDSEK", (usdRateTo.Rates.Sek, int64 usdRateTo.Timestamp) ;
                        "AUDUSD", (1m/usdRateTo.Rates.Aud, int64 usdRateTo.Timestamp) ;
                        "CHFUSD", (1m/usdRateTo.Rates.Chf, int64 usdRateTo.Timestamp) ;
                        "DKKUSD", (1m/usdRateTo.Rates.Dkk, int64 usdRateTo.Timestamp) ;
                        "EURUSD", (1m/usdRateTo.Rates.Eur, int64 usdRateTo.Timestamp) ;
                        "GBPUSD", (1m/usdRateTo.Rates.Gbp, int64 usdRateTo.Timestamp) ;
                        "NOKUSD", (1m/usdRateTo.Rates.Nok, int64 usdRateTo.Timestamp) ;
                        "SEKUSD", (1m/usdRateTo.Rates.Sek, int64 usdRateTo.Timestamp)] |> Map.ofList
        usdRates

    /// <summary>
    ///
    /// set new values in Currency Rates Db Row
    /// -> unit
    /// 
    /// </summary>
    /// <param name="rateDbRow">Currency Rate row</param>
    /// <param name="rateValue">rate value</param>
    /// <return>Map of rates</return>
    let setRateDbRowValues 
            (rateDbRow : DbUtil.DbSchema.ServiceTypes.CurrencyRates) 
            (rateValue : decimal) =

        rateDbRow.Rate <- rateValue
        rateDbRow.UpdatedOnUTC <- DateTime.UtcNow
    
    /// <summary>
    ///
    /// Create and initialize a new Currency Rates Db Row
    /// -> unit
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tradeDateTime"></param>
    /// <param name="marketRate"></param>
    let setRateNewDbRowValues 
            (db : DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb)
            (tradeDateTime:DateTime) 
            (marketRate:KeyValuePair<string, (decimal*int64)>) =
        let newRateRow = new  DbUtil.DbSchema.ServiceTypes.CurrencyRates(TradeDateTime=tradeDateTime, FromTo=marketRate.Key)
        setRateDbRowValues newRateRow <| fst marketRate.Value
        db.CurrencyRates.InsertOnSubmit(newRateRow)

    let setDbRates (db : DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDevDb) (usdRates : Map<string, (decimal*int64)>) = 

        for marketRate in usdRates do
            let tradeTm = DateTimeOffset.FromUnixTimeSeconds(snd marketRate.Value).UtcDateTime
            let roundedTradeTm = new DateTime(tradeTm.Year, tradeTm.Month, tradeTm.Day, tradeTm.Hour, 0, 0)
            let rateFromTo = marketRate.Key
            query { 
            for rate in db.CurrencyRates do
                where (rate.TradeDateTime = roundedTradeTm && rate.FromTo = rateFromTo)
                select rate
                exactlyOneOrDefault
            } 
            |> (fun rateRow ->
                if isNull rateRow then
                    setRateNewDbRowValues db roundedTradeTm marketRate
                else 
                    setRateDbRowValues rateRow <| fst marketRate.Value
            )
        // Commit for all rates once!
        db.DataContext.SubmitChanges()
