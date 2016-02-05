using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

/// <summary>
/// Credit to jarloo
/// http://www.jarloo.com/get-yahoo-finance-api-data-via-yql/
/// modified, simplified
/// </summary>

/// Alternative for historical data
/// https://query.yahooapis.com/v1/public/yql?q=select%20Symbol%2C%20Date%2C%20Close%20from%20yahoo.finance.historicaldata%20where%20symbol%20IN%20(%22XLE%22%2C%22VTI%22)%20and%20startDay%20%3D%201%20and%20startDate%20%3D%20%222015-12-01%22%20and%20endDate%20%3D%20%222016-02-03%22&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys
namespace gzWeb.Helpers {

    public class PQuote {
        public string Symbol { get; set; }
        public float? LastTradePrice { get; set; }
        public DateTime? LastTradeDate { get; set; }
        public DateTime UpdatedOnUTC { get; set; }
    }

    public class Quote {
        public string Symbol { get; set; }
        public decimal? Ask { get; set; }
        public decimal? Bid { get; set; }
        public decimal? AverageDailyVolume { get; set; }
        public decimal? BookValue { get; set; }
        public decimal? Change { get; set; }
        public decimal? DividendShare { get; set; }
        public DateTime? LastTradeDate { get; set; }
        public decimal? EarningsShare { get; set; }
        public decimal? EpsEstimateCurrentYear { get; set; }
        public decimal? EpsEstimateNextYear { get; set; }
        public decimal? EpsEstimateNextQuarter { get; set; }
        public decimal? DailyLow { get; set; }
        public decimal? DailyHigh { get; set; }
        public decimal? YearlyLow { get; set; }
        public decimal? YearlyHigh { get; set; }
        public decimal? MarketCapitalization { get; set; }
        public decimal? Ebitda { get; set; }
        public decimal? ChangeFromYearLow { get; set; }
        public decimal? PercentChangeFromYearLow { get; set; }
        public decimal? ChangeFromYearHigh { get; set; }
        public decimal? LastTradePrice { get; set; }
        public decimal? PercentChangeFromYearHigh { get; set; }
        public decimal? FiftyDayMovingAverage { get; set; }
        public decimal? TwoHunderedDayMovingAverage { get; set; }
        public decimal? ChangeFromTwoHundredDayMovingAverage { get; set; }
        public decimal? PercentChangeFromTwoHundredDayMovingAverage { get; set; }
        public decimal? PercentChangeFromFiftyDayMovingAverage { get; set; }
        public string Name { get; set; }
        public decimal? Open { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? ChangeInPercent { get; set; }
        public decimal? PriceSales { get; set; }
        public decimal? PriceBook { get; set; }
        public DateTime? ExDividendDate { get; set; }
        public decimal? PeRatio { get; set; }
        public DateTime? DividendPayDate { get; set; }
        public decimal? PegRatio { get; set; }
        public decimal? PriceEpsEstimateCurrentYear { get; set; }
        public decimal? PriceEpsEstimateNextYear { get; set; }
        public decimal? ShortRatio { get; set; }
        public decimal? OneYearPriceTarget { get; set; }
        public decimal? Volume { get; set; }
        public string StockExchange { get; set; }
        public DateTime UpdatedOnUTC { get; set; }
    }

    /// <summary>
    /// Credit to jarloo
    /// http://www.jarloo.com/get-yahoo-finance-api-data-via-yql/
    /// modified, simplified
    /// </summary>
    public class YahooStockEngine {


        private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q=" +
                                        "select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20({0})" +
                                        "&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";

        /// <summary>
        /// Partial Quote version
        /// </summary>
        /// <param name="quotes"></param>
        public static List<PQuote> Fetch(List<PQuote> quotes) {
            string symbolList = String.Join("%2C", quotes.Select(w => "%22" + w.Symbol + "%22").ToArray());
            string url = string.Format(BASE_URL, symbolList);

            XDocument doc = XDocument.Load(url);
            Parse(quotes, doc);

            return quotes;
        }

        /// <summary>
        /// Partial Quote version
        /// </summary>
        /// <param name="quotes"></param>
        /// <param name="doc"></param>
        private static void Parse(IEnumerable<PQuote> quotes, XDocument doc) {
            XElement results = doc.Root.Element("results");

            foreach (PQuote quote in quotes) {
                XElement q = results.Elements("quote").First(w => w.Attribute("symbol").Value == quote.Symbol);

                quote.LastTradePrice = GetFloat(q.Element("LastTradePriceOnly").Value);
                quote.LastTradeDate = GetDateTime(q.Element("LastTradeDate").Value + " " + q.Element("LastTradeTime").Value);
                quote.UpdatedOnUTC = DateTime.UtcNow;
            }
        }

        public static void Fetch(IEnumerable<Quote> quotes) {
            string symbolList = String.Join("%2C", quotes.Select(w => "%22" + w.Symbol + "%22").ToArray());
            string url = string.Format(BASE_URL, symbolList);

            XDocument doc = XDocument.Load(url);
            Parse(quotes, doc);
        }

        private static void Parse(IEnumerable<Quote> quotes, XDocument doc) {
            XElement results = doc.Root.Element("results");

            foreach (Quote quote in quotes) {
                XElement q = results.Elements("quote").First(w => w.Attribute("symbol").Value == quote.Symbol);

                quote.Ask = GetDecimal(q.Element("Ask").Value);
                quote.Bid = GetDecimal(q.Element("Bid").Value);
                quote.AverageDailyVolume = GetDecimal(q.Element("AverageDailyVolume").Value);
                quote.BookValue = GetDecimal(q.Element("BookValue").Value);
                quote.Change = GetDecimal(q.Element("Change").Value);
                quote.DividendShare = GetDecimal(q.Element("DividendShare").Value);
                quote.LastTradeDate = GetDateTime(q.Element("LastTradeDate").Value + " " + q.Element("LastTradeTime").Value);
                quote.EarningsShare = GetDecimal(q.Element("EarningsShare").Value);
                quote.EpsEstimateCurrentYear = GetDecimal(q.Element("EPSEstimateCurrentYear").Value);
                quote.EpsEstimateNextYear = GetDecimal(q.Element("EPSEstimateNextYear").Value);
                quote.EpsEstimateNextQuarter = GetDecimal(q.Element("EPSEstimateNextQuarter").Value);
                quote.DailyLow = GetDecimal(q.Element("DaysLow").Value);
                quote.DailyHigh = GetDecimal(q.Element("DaysHigh").Value);
                quote.YearlyLow = GetDecimal(q.Element("YearLow").Value);
                quote.YearlyHigh = GetDecimal(q.Element("YearHigh").Value);
                quote.MarketCapitalization = GetDecimal(q.Element("MarketCapitalization").Value);
                quote.Ebitda = GetDecimal(q.Element("EBITDA").Value);
                quote.ChangeFromYearLow = GetDecimal(q.Element("ChangeFromYearLow").Value);
                quote.PercentChangeFromYearLow = GetDecimal(q.Element("PercentChangeFromYearLow").Value);
                quote.ChangeFromYearHigh = GetDecimal(q.Element("ChangeFromYearHigh").Value);
                quote.LastTradePrice = GetDecimal(q.Element("LastTradePriceOnly").Value);
                quote.PercentChangeFromYearHigh = GetDecimal(q.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
                quote.FiftyDayMovingAverage = GetDecimal(q.Element("FiftydayMovingAverage").Value);
                quote.TwoHunderedDayMovingAverage = GetDecimal(q.Element("TwoHundreddayMovingAverage").Value);
                quote.ChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("ChangeFromTwoHundreddayMovingAverage").Value);
                quote.PercentChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
                quote.PercentChangeFromFiftyDayMovingAverage = GetDecimal(q.Element("PercentChangeFromFiftydayMovingAverage").Value);
                quote.Name = q.Element("Name").Value;
                quote.Open = GetDecimal(q.Element("Open").Value);
                quote.PreviousClose = GetDecimal(q.Element("PreviousClose").Value);
                quote.ChangeInPercent = GetDecimal(q.Element("ChangeinPercent").Value);
                quote.PriceSales = GetDecimal(q.Element("PriceSales").Value);
                quote.PriceBook = GetDecimal(q.Element("PriceBook").Value);
                quote.ExDividendDate = GetDateTime(q.Element("ExDividendDate").Value);
                quote.PeRatio = GetDecimal(q.Element("PERatio").Value);
                quote.DividendPayDate = GetDateTime(q.Element("DividendPayDate").Value);
                quote.PegRatio = GetDecimal(q.Element("PEGRatio").Value);
                quote.PriceEpsEstimateCurrentYear = GetDecimal(q.Element("PriceEPSEstimateCurrentYear").Value);
                quote.PriceEpsEstimateNextYear = GetDecimal(q.Element("PriceEPSEstimateNextYear").Value);
                quote.ShortRatio = GetDecimal(q.Element("ShortRatio").Value);
                quote.OneYearPriceTarget = GetDecimal(q.Element("OneyrTargetPrice").Value);
                quote.Volume = GetDecimal(q.Element("Volume").Value);
                quote.StockExchange = q.Element("StockExchange").Value;

                quote.UpdatedOnUTC = DateTime.UtcNow;
            }
        }

        private static float? GetFloat(string input) {
            if (input == null) return null;

            input = input.Replace("%", "");

            float value;

            if (float.TryParse(input, out value)) return value;
            return null;
        }

        private static decimal? GetDecimal(string input) {
            if (input == null) return null;

            input = input.Replace("%", "");

            decimal value;

            if (Decimal.TryParse(input, out value)) return value;
            return null;
        }

        private static DateTime? GetDateTime(string input) {
            if (input == null) return null;

            DateTime value;

            if (DateTime.TryParse(input, out value)) return value;
            return null;
        }
    }
}