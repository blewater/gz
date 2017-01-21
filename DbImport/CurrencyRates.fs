namespace DbImport

open System
open FSharp.Data
open NLog
open System.Collections.Generic
open DbUtil

module public CurrencyRates = 
    type UsdRates = JsonProvider<"../DbImport/openexchangerates.json">

    let logger = LogManager.GetCurrentClassLogger()

    /// <summary>
    /// A map (immutable dict) ->
    ///     "From" (Currency Name) "To" (Currency), conversion rate*epoch time-stamp
    /// </summary>
    type CurrencyRatesValues = Map<string, (decimal*int64)>

    /// <summary>
    ///
    /// httpGet currency rates based on the UsdRates type
    /// -> Map of rates
    /// 
    /// </summary>
    /// <param name="currencyApiUrl"></param>
    let getCurrencyRates (currencyApiUrl : string) : CurrencyRatesValues = 
        let usdRateTo = UsdRates.Load(currencyApiUrl)
        let usdRates = [
                        "USDAUD", (usdRateTo.Rates.Aud, int64 usdRateTo.Timestamp) ;
                        "USDCAD", (usdRateTo.Rates.Cad, int64 usdRateTo.Timestamp) ;
                        "USDCHF", (usdRateTo.Rates.Chf, int64 usdRateTo.Timestamp) ;
                        "USDDKK", (usdRateTo.Rates.Dkk, int64 usdRateTo.Timestamp) ;
                        "USDEUR", (usdRateTo.Rates.Eur, int64 usdRateTo.Timestamp) ;
                        "USDGBP", (usdRateTo.Rates.Gbp, int64 usdRateTo.Timestamp) ;
                        "USDNOK", (usdRateTo.Rates.Nok, int64 usdRateTo.Timestamp) ;
                        "USDSEK", (usdRateTo.Rates.Sek, int64 usdRateTo.Timestamp) ;
                        "AUDUSD", (1m/usdRateTo.Rates.Aud, int64 usdRateTo.Timestamp) ;
                        "CADUSD", (1m/usdRateTo.Rates.Cad, int64 usdRateTo.Timestamp) ;
                        "CHFUSD", (1m/usdRateTo.Rates.Chf, int64 usdRateTo.Timestamp) ;
                        "DKKUSD", (1m/usdRateTo.Rates.Dkk, int64 usdRateTo.Timestamp) ;
                        "EURUSD", (1m/usdRateTo.Rates.Eur, int64 usdRateTo.Timestamp) ;
                        "GBPUSD", (1m/usdRateTo.Rates.Gbp, int64 usdRateTo.Timestamp) ;
                        "NOKUSD", (1m/usdRateTo.Rates.Nok, int64 usdRateTo.Timestamp) ;
                        "SEKUSD", (1m/usdRateTo.Rates.Sek, int64 usdRateTo.Timestamp) ;
                        "USDUSD", (1m,int64 usdRateTo.Timestamp)
                        ] |> Map.ofList
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
            (db : DbContext)
            (tradeDateTime:DateTime) 
            (marketRate:KeyValuePair<string, (decimal*int64)>) =
        let newRateRow = new  DbUtil.DbSchema.ServiceTypes.CurrencyRates(TradeDateTime=tradeDateTime, FromTo=marketRate.Key)
        setRateDbRowValues newRateRow <| fst marketRate.Value
        db.CurrencyRates.InsertOnSubmit(newRateRow)

    /// <summary>
    /// 
    /// Insert to database CurrencyRates table only if no rates exist within the same hour.
    /// 
    /// </summary>
    /// <param name="db">db context</param>
    /// <param name="usdRates">Map of rates to update</param>
    let setDbRates (db : DbContext) (usdRates : CurrencyRatesValues) : unit = 

        for marketRate in usdRates do

            if marketRate.Key <> "USDUSD" then

                let tradeTm = DateTimeOffset.FromUnixTimeSeconds(snd marketRate.Value).UtcDateTime

                // Round within an hour
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
    (* No point in updating currency rates
                    else 
                        setRateDbRowValues rateRow <| fst marketRate.Value *)
                )

        // Commit for all rates once!
        db.DataContext.SubmitChanges()

    /// <summary>
    /// 
    /// Get the latest exchange rates and update the database rates table with them
    ///
    /// </summary>
    /// <param name="currencyApiUrl"></param>
    /// <param name="db"></param>
    let updCurrencyRates (currencyApiUrl : string)(db : DbContext) : unit =

        logger.Info("Retrieving last hour's currency values...")

        // setDbCurrencyRates curried
        let setDbCurrencyRates = setDbRates db

        getCurrencyRates currencyApiUrl
        |> setDbCurrencyRates
