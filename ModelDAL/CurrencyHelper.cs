using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace gzDAL
{
    public class CurrencyInfo
    {
        public string ISOSymbol { get; set; }
        public string Symbol { get; set; }
        public NumberFormatInfo NumberFormat { get; set; }
    }

    public static class CurrencyHelper
    {
        private static readonly Dictionary<string, CurrencyInfo> SymbolsByCode;

        public static CurrencyInfo GetSymbol(string code) { return SymbolsByCode[code]; }

        static CurrencyHelper()
        {
            SymbolsByCode = new Dictionary<string, CurrencyInfo>();
            
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var region = new RegionInfo(culture.LCID);
                if (!SymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
                {
                    SymbolsByCode.Add(region.ISOCurrencySymbol, new CurrencyInfo
                                                                {
                                                                        ISOSymbol = region.ISOCurrencySymbol,
                                                                        Symbol = region.CurrencySymbol,
                                                                        NumberFormat = culture.NumberFormat
                                                                });
                }
            }
        }
    }
}