using System;

namespace gzWeb.Areas.Mvc.Models
{
    public class SummaryViewModel
    {
        public string Currency { get; set; }
        public string Culture { get; set; }
        public decimal InvestmentsBalance { get; set; }
        public decimal TotalInvestments { get; set; }
        public decimal TotalInvestmentsReturns { get; set; }
        public decimal GamingBalance { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public DateTime NextInvestmentOn { get; set; }
        public decimal LastInvestmentAmount { get; set; }
    }
}