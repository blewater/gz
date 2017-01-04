using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzDAL.DTO
{

    /// <summary>
    /// Customer Values buffer for values communication between controller views
    /// </summary>
    public class UserSummaryDTO {
        public CurrencyInfo Currency { get; set; }
        public decimal InvestmentsBalance { get; set; }
        public decimal BegGmBalance { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawals { get; set; }
        public decimal GamingGainLoss { get; set; }
        public decimal EndGmBalance { get; set; }
        public decimal TotalInvestments { get; set; }
        public decimal TotalInvestmentsReturns { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public DateTime NextInvestmentOn { get; set; }
        public decimal LastInvestmentAmount { get; set; }
        public DateTime StatusAsOf { get; set; }
        public IEnumerable<VintageDto> Vintages { get; set; }

        // Withdrawal eligibility props
        public int LockInDays { get; set; }
        public bool OkToWithdraw { get; set; }
        public DateTime EligibleWithdrawDate { get; set; }
        public string Prompt { get; set; }
    }
}