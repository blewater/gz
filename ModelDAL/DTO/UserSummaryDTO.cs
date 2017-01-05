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

        //-- New monthly gaming amounts imported by the reports
        public decimal BegMonthlyGmBalance { get; set; }
        public decimal MonthlyDeposits { get; set; }
        public decimal MonthlyWithdrawals { get; set; }
        public decimal MonthlyGamingGainLoss { get; set; }
        public decimal EndMonthlyGmBalance { get; set; }
        //--
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