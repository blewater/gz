using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace gzDAL
{
    public static class CurrencyHelper
    {
        private static readonly Dictionary<string, string> SymbolsByCode;

        public static string GetSymbol(string code) { return SymbolsByCode[code]; }

        static CurrencyHelper()
        {
            SymbolsByCode = new Dictionary<string, string>();

            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                          .Select(x => new RegionInfo(x.LCID));

            foreach (var region in regions)
                if (!SymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
                    SymbolsByCode.Add(region.ISOCurrencySymbol, region.CurrencySymbol);
        }
    }
}