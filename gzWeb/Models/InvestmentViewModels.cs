using System;
using System.Collections.Generic;
using gzDAL.Models;

namespace gzWeb.Models
{
    public class VintagesWithSellingValuesViewModel
    {
        public IList<VintageViewModel> Vintages { get; set; }
        public decimal WithdrawnAmount { get; set; }
    }

    public class VintageViewModel
    {
        public int InvBalanceId { get; set; }
        public string YearMonthStr { get; set; }
        public decimal InvestmentAmount { get; set; }
        public decimal SellingValue { get; set; }
        public bool Locked { get; set; }
        public bool Selected { get; set; }
        public bool Sold { get; set; }
        public decimal SoldAmount { get; set; }
        public decimal SoldFees { get; set; }
        public decimal SoldYearMonth { get; set; }
    }

    public class PlanViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double ROI { get; set; }
        public string Color { get; set; }
        public bool Selected { get; set; }
        public RiskToleranceEnum Risk { get; set; }
        public float AllocatedPercent { get; set; }
        public decimal AllocatedAmount { get; set; }
        public List<HoldingViewModel> Holdings { get; set; }
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
        public decimal InvestmentsBalance { get; set; }

        //-- New monthly gaming amounts imported by the reports
        public decimal BegGmBalance { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawals { get; set; }
        public decimal GmGainLoss { get; set; }
        public decimal EndGmBalance { get; set; }
        public decimal CashBonus { get; set; }

        /// <summary>
        /// Total monthly player losses summed up
        /// </summary>
        public decimal TotalInvestments { get; set; }
        /// <summary>
        /// Total net gain or net loss from investing the player losses
        /// </summary>
        public decimal TotalInvestmentsReturns { get; set; }
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
        public List<PlanViewModel> Plans { get; set; }
    }

    public class PerformanceDataViewModel
    {
        public decimal InvestmentsBalance { get; set; }
        public decimal NextExpectedInvestment { get; set; }
        public List<PlanViewModel> Plans { get; set; }
    }
}