using System;

namespace gzWeb.Utilities
{
    public static class ExtensionMethods
    {
        public static string ToMoneyString(this decimal amount, string currency)
        {
            return String.Format("{0} {1}", currency, amount.ToString("N0"));
        }
    }
}
