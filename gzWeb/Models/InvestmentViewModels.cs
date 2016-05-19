using System;
using System.Collections.Generic;

namespace gzWeb.Models
{
    public class VintageViewModel
    {
        public string YearMonthStr { get; set; }
        public decimal InvestAmount { get; set; }
        public decimal SellingValue { get; set; }
        public bool Locked { get; set; }
        public bool Sold { get; set; }
        public bool Selected { get; set; }
    }

    public class PlanViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double ROI { get; set; }
        public string Color { get; set; }
        public decimal UserBalance { get; set; }
        public double AllocationPercent { get; set; }
        public bool Selected { get; set; }
        public IEnumerable<HoldingViewModel> Holdings { get; set; }
    }

    public class HoldingViewModel
    {
        public string Name { get; set; }
        public double Weight { get; set; }
    }

    public class ReturnOnInvestmentViewModel
    {
        public string Title { get; set; }
        public double Percent { get; set; }
    }

    public class SummaryDataViewModel
    {
        //public string Currency { get; set; }
        //public string Culture { get; set; }
        public decimal InvestmentsBalance { get; set; }
        public decimal TotalInvestments { get; set; }
        public decimal TotalInvestmentsReturns { get; set; }
        //public decimal GamingBalance { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public DateTime NextInvestmentOn { get; set; }
        public decimal LastInvestmentAmount { get; set; }
        public DateTime StatusAsOf { get; set; }
        public IList<VintageViewModel> Vintages { get; set; }

        // Withdrawal eligibility props
        public int LockInDays { get; set; }
        public bool OkToWithdraw { get; set; }
        public DateTime EligibleWithdrawDate { get; set; }
        public string Prompt { get; set; }
    }

    public class PortfolioDataViewModel
    {
        //public string Currency { get; set; }
        public DateTime NextInvestmentOn { get; set; }
        public decimal NextExpectedInvestment { get; set; }
        //public ReturnOnInvestmentViewModel ROI { get; set; }
        public IEnumerable<PlanViewModel> Plans { get; set; }
    }

    public class PerformanceDataViewModel
    {
        public string Currency { get; set; }
        public IEnumerable<PlanViewModel> Plans { get; set; }
    }
}