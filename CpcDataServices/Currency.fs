namespace CpcDataServices

open FSharp.Data
open FSharp.Data.HttpRequestHeaders

module public Currency = 
    let CurrencyUrl = "https://openexchangerates.org/api/latest.json?app_id=b26776ee05a74ed9817e8da69e80a16a"

    let currencyJSON (currencyApiUrl:string) (apiKey:string) =
        Http.RequestString
          ( currencyApiUrl,
            query   = [ "api_key", apiKey ],
            headers = [ Accept HttpContentTypes.Json ])
